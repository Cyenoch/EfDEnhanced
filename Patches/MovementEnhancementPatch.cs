using System;
using System.Reflection;
using ECM2;
using EfDEnhanced.Utils;
using HarmonyLib;
using UnityEngine;

namespace EfDEnhanced.Patches
{
    /// <summary>
    /// Patch for Movement class to enhance character movement responsiveness
    /// Addresses "sticky" movement feel by optimizing acceleration, deceleration, and input response
    /// </summary>
    [HarmonyPatch(typeof(Movement), "UpdateNormalMove")]
    public class MovementEnhancementPatch
    {
        // Cache reflection fields for better performance
        private static FieldInfo? _characterMovementField;
        private static FieldInfo? _moveInputField;
        private static FieldInfo? _movingField;
        private static FieldInfo? _runningField;
        private static FieldInfo? _currentMoveDirectionXZField;
        private static FieldInfo? _targetAimDirectionField;

        // Movement preset configurations
        private struct MovementPreset
        {
            public float AccelerationMultiplier;
            public float BrakingMultiplier;
            public float LerpFactor; // For direct lerp interpolation
            public bool UseDirectLerp;

            public MovementPreset(float accel, float braking, float lerp, bool useDirectLerp)
            {
                AccelerationMultiplier = accel;
                BrakingMultiplier = braking;
                LerpFactor = lerp;
                UseDirectLerp = useDirectLerp;
            }
        }

        private static readonly MovementPreset[] Presets = new MovementPreset[]
        {
            new MovementPreset(1.0f, 1.0f, 0f, false),      // 0: Disabled (never used, fallback in Prefix)
            new MovementPreset(2.0f, 2.0f, 0.05f, false),      // 1: Light - 3x faster
            new MovementPreset(4.0f, 4.0f, 0.15f, true),    // 2: Medium - 6x with instant direction change
            new MovementPreset(8.0f, 8.0f, 0.35f, true)    // 3: Heavy - 10x ultra responsive
        };

        private static bool _hasLoggedActivation = false;
        private static int _lastLoggedLevel = -1;

        /// <summary>
        /// Initialize reflection fields on first use
        /// </summary>
        static MovementEnhancementPatch()
        {
            try
            {
                Type movementType = typeof(Movement);
                
                _characterMovementField = movementType.GetField("characterMovement", 
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                _moveInputField = movementType.GetField("moveInput", 
                    BindingFlags.Instance | BindingFlags.NonPublic);
                _movingField = movementType.GetField("moving", 
                    BindingFlags.Instance | BindingFlags.NonPublic);
                _runningField = movementType.GetField("running", 
                    BindingFlags.Instance | BindingFlags.NonPublic);
                _currentMoveDirectionXZField = movementType.GetField("currentMoveDirectionXZ", 
                    BindingFlags.Instance | BindingFlags.NonPublic);
                _targetAimDirectionField = movementType.GetField("targetAimDirection",
                    BindingFlags.Instance | BindingFlags.Public);

                if (_characterMovementField == null || _moveInputField == null || 
                    _movingField == null || _runningField == null || _currentMoveDirectionXZField == null ||
                    _targetAimDirectionField == null)
                {
                    ModLogger.LogError("MovementEnhancementPatch: Failed to find one or more required fields");
                }
                else
                {
                    ModLogger.Log("MovementEnhancementPatch", "Successfully initialized all reflection fields");
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"MovementEnhancementPatch initialization failed: {ex}");
            }
        }

        /// <summary>
        /// Prefix patch to replace UpdateNormalMove logic when enhancement is enabled
        /// </summary>
        [HarmonyPrefix]
        static bool Prefix(Movement __instance)
        {
            try
            {
                // Only apply to player-controlled character, not NPCs
                if (__instance.characterController == null || !__instance.characterController.IsMainCharacter)
                {
                    return true; // Run original method for NPCs
                }

                int enhancementLevel = ModSettings.MovementEnhancement.Value;
                
                // Level 0 = disabled, use original logic
                if (enhancementLevel == 0)
                {
                    return true; // Run original method
                }

                // Validate level
                if (enhancementLevel < 0 || enhancementLevel >= Presets.Length)
                {
                    return true;
                }

                // Get required fields via reflection
                if (!TryGetFields(__instance, out var characterMovement, out var moveInput, 
                    out var moving, out var running))
                {
                    return true; // Fallback to original on error
                }

                // Log activation when level changes
                if (!_hasLoggedActivation || _lastLoggedLevel != enhancementLevel)
                {
                    _hasLoggedActivation = true;
                    _lastLoggedLevel = enhancementLevel;
                    string[] presetNames = { "Disabled", "Light", "Medium", "Heavy" };
                    ModLogger.Log("MovementEnhancementPatch", $"Movement enhancement ACTIVE - Level: {presetNames[enhancementLevel]} " +
                        $"(Accel: {Presets[enhancementLevel].AccelerationMultiplier}x, Brake: {Presets[enhancementLevel].BrakingMultiplier}x, " +
                        $"Lerp: {Presets[enhancementLevel].LerpFactor}, DirectLerp: {Presets[enhancementLevel].UseDirectLerp})");
                    ModLogger.Log("MovementEnhancementPatch", $"Base stats - WalkAcc: {__instance.walkAcc}, RunAcc: {__instance.runAcc}, " +
                        $"WalkSpeed: {__instance.walkSpeed}, RunSpeed: {__instance.runSpeed}");
                    
                    // Log actual calculated values for verification
                    float enhancedWalkAcc = __instance.walkAcc * Presets[enhancementLevel].AccelerationMultiplier;
                    float enhancedRunAcc = __instance.runAcc * Presets[enhancementLevel].AccelerationMultiplier;
                    ModLogger.Log("MovementEnhancementPatch", $"Enhanced values - WalkAcc: {enhancedWalkAcc}, RunAcc: {enhancedRunAcc}");
                }

                // Apply enhanced movement logic
                ApplyEnhancedMovement(__instance, characterMovement, moveInput, moving, running, 
                    Presets[enhancementLevel]);

                return false; // Skip original method
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"MovementEnhancementPatch.Prefix failed: {ex}");
                return true; // Fallback to original on error
            }
        }

        /// <summary>
        /// Try to get all required fields from Movement instance
        /// </summary>
        private static bool TryGetFields(Movement instance, 
            out CharacterMovement characterMovement,
            out Vector3 moveInput,
            out bool moving,
            out bool running)
        {
            characterMovement = null!;
            moveInput = Vector3.zero;
            moving = false;
            running = false;

            try
            {
                if (_characterMovementField == null || _moveInputField == null || 
                    _movingField == null || _runningField == null)
                {
                    ModLogger.LogError("MovementEnhancementPatch: Required fields not initialized");
                    return false;
                }

                characterMovement = (CharacterMovement)_characterMovementField.GetValue(instance)!;
                moveInput = (Vector3)_moveInputField.GetValue(instance)!;
                moving = (bool)_movingField.GetValue(instance)!;
                running = (bool)_runningField.GetValue(instance)!;

                return characterMovement != null;
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Failed to get Movement fields: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Apply enhanced movement logic based on preset
        /// </summary>
        private static void ApplyEnhancedMovement(
            Movement instance,
            CharacterMovement characterMovement,
            Vector3 moveInput,
            bool moving,
            bool running,
            MovementPreset preset)
        {
            Vector3 velocity = characterMovement.velocity;
            Vector3 target = Vector3.zero;
            float baseAcc = instance.walkAcc;

            // Calculate target velocity
            if (moving)
            {
                target = moveInput * (running ? instance.runSpeed : instance.walkSpeed);
                baseAcc = running ? instance.runAcc : instance.walkAcc;
            }

            // Preserve Y component (vertical velocity)
            target.y = velocity.y;

            // Apply enhanced movement based on preset
            if (moving)
            {
                // Enhanced acceleration
                float enhancedAcc = baseAcc * preset.AccelerationMultiplier;

                if (preset.UseDirectLerp)
                {
                    // Direct lerp for instant response
                    // Combines MoveTowards for acceleration with Lerp for instant direction change
                    float maxDelta = enhancedAcc * Time.deltaTime;
                    velocity = Vector3.MoveTowards(velocity, target, maxDelta);
                    
                    // Additional lerp for instant direction changes when already moving
                    if (velocity.sqrMagnitude > 0.1f)
                    {
                        velocity = Vector3.Lerp(velocity, target, preset.LerpFactor);
                    }
                }
                else
                {
                    // Simple enhanced linear interpolation
                    velocity = Vector3.MoveTowards(velocity, target, enhancedAcc * Time.deltaTime);
                }
            }
            else
            {
                // Enhanced braking when no input
                float brakingForce = preset.BrakingMultiplier * baseAcc * Time.deltaTime;
                
                // Preserve Y component during braking
                float yVel = velocity.y;
                velocity.y = 0f;
                
                // Apply stronger braking
                if (velocity.sqrMagnitude > 0.001f)
                {
                    velocity = Vector3.MoveTowards(velocity, Vector3.zero, brakingForce);
                }
                else
                {
                    velocity = Vector3.zero;
                }
                
                // Restore Y component
                velocity.y = yVel;
            }

            // Update current move direction for animations
            Vector3 horizontalVelocity = velocity;
            horizontalVelocity.y = 0f;
            if (horizontalVelocity.magnitude > 0.02f)
            {
                Vector3 currentMoveDirectionXZ = horizontalVelocity.normalized;
                _currentMoveDirectionXZField?.SetValue(instance, currentMoveDirectionXZ);
            }

            // Apply the final velocity
            characterMovement.velocity = velocity;
        }
    }

    /// <summary>
    /// Patch for UpdateRotation to enhance turning speed
    /// </summary>
    [HarmonyPatch(typeof(Movement), "UpdateRotation")]
    public class MovementRotationEnhancementPatch
    {
        private static bool _hasLoggedActivation = false;
        private static int _lastLoggedLevel = -1;

        [HarmonyPrefix]
        static void Prefix(Movement __instance, ref float deltaTime)
        {
            try
            {
                // Only apply to player-controlled character
                if (__instance.characterController == null || !__instance.characterController.IsMainCharacter)
                {
                    return;
                }

                int enhancementLevel = ModSettings.MovementEnhancement.Value;
                if (enhancementLevel == 0)
                {
                    return;
                }

                // Get turn speed multiplier from movement preset
                float turnMultiplier = 1.0f;
                switch (enhancementLevel)
                {
                    case 1: turnMultiplier = 1.5f; break;  // Light
                    case 2: turnMultiplier = 2.5f; break;  // Medium
                    case 3: turnMultiplier = 4.0f; break;  // Heavy
                }

                // Enhance deltaTime for rotation to make turning faster
                deltaTime *= turnMultiplier;

                if (!_hasLoggedActivation || _lastLoggedLevel != enhancementLevel)
                {
                    _hasLoggedActivation = true;
                    _lastLoggedLevel = enhancementLevel;
                    ModLogger.Log("MovementRotationEnhancementPatch", 
                        $"Rotation enhancement ACTIVE - Turn speed multiplier: {turnMultiplier}x, Original deltaTime: {deltaTime / turnMultiplier}, Enhanced: {deltaTime}");
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"MovementRotationEnhancementPatch.Prefix failed: {ex}");
            }
        }
    }
}

