# Escape from Duckov - Mod Development Guide

Comprehensive guide for developing mods for "Escape from Duckov" using the EfDEnhanced framework.

## Table of Contents

1. [Introduction](#introduction)
2. [Prerequisites](#prerequisites)
3. [Project Structure](#project-structure)
4. [Development Environment Setup](#development-environment-setup)
5. [Core Concepts](#core-concepts)
6. [Harmony Patching](#harmony-patching)
7. [Working with Unity Assets](#working-with-unity-assets)
8. [Scene Manipulation](#scene-manipulation)
9. [Best Practices](#best-practices)
10. [Testing & Debugging](#testing--debugging)
11. [Common Patterns](#common-patterns)
12. [Troubleshooting](#troubleshooting)

---

## Introduction

**Escape from Duckov** is a Unity-based game with official modding support via the `Duckov.Modding.ModBehaviour` framework. This guide covers mod development using:

- **.NET Standard 2.1** - Target framework
- **Unity Engine APIs** - Game engine integration
- **HarmonyLib** - Runtime code patching
- **Game's Modding Framework** - Official mod loader

**Important**: "Escape from Duckov" is NOT "Escape from Tarkov" - it's an independent game by TeamSoda.

---

## Prerequisites

### Required Knowledge
- C# programming (intermediate level)
- Basic Unity concepts (GameObjects, Components, Scenes)
- Understanding of .NET Standard 2.1
- Git basics (optional but recommended)

### Required Software
- **.NET SDK 6.0+** - For building .NET Standard 2.1 projects
- **IDE**: Visual Studio, Rider, or VS Code with C# extension
- **Game Installation** - "Escape from Duckov" installed
- **Git** (recommended) - Version control

### Optional Tools
- **dnSpy / ILSpy** - .NET decompiler (for game analysis)
- **AssetRipper** - Unity asset extractor
- **UnityExplorer** - Runtime game inspector (as mod)

---

## Project Structure

### Directory Layout

```
EfDEnhanced/
├── ModBehaviour.cs              # Mod entry point
├── Utils/                       # Utility classes
│   └── ModLogger.cs             # Logging system
├── Patches/                     # Harmony patches
│   ├── QuestPatches.cs          # Quest system patches
│   ├── InventoryPatches.cs      # Inventory patches
│   └── [SystemName]Patches.cs   # Group by game system
├── Features/                    # Feature implementations
│   ├── CustomFeature.cs         # Individual features
│   └── FeatureManager.cs        # Feature coordination
├── Config/                      # Configuration
│   └── ModConfig.cs             # Mod settings
├── docs/                        # Documentation
│   ├── assets/                  # Asset guides
│   ├── scenes/                  # Scene guides
│   └── ...                      # Other documentation
├── extracted_assets/            # Extracted Unity assets (reference)
├── scripts/                     # Development scripts
│   ├── deploy.sh                # Deploy to game
│   ├── rlog.sh                  # Real-time logging
│   └── decompile.sh             # Decompile game DLLs
├── info.ini                     # Mod metadata
├── preview.png                  # Mod preview (256x256)
└── EfDEnhanced.csproj           # Project file
```

### Key Files

#### ModBehaviour.cs
```csharp
using HarmonyLib;
using UnityEngine;
using EfDEnhanced.Utils;

namespace EfDEnhanced;

public class ModBehaviour : Duckov.Modding.ModBehaviour
{
    private const string HARMONY_ID = "com.efdenhanced.mod";
    private static Harmony? _harmonyInstance;
    
    public static ModBehaviour? Instance { get; private set; }
    
    void Awake()
    {
        if (Instance != null)
        {
            ModLogger.LogWarning("Multiple ModBehaviour instances detected!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        ModLogger.Log("=== EfDEnhanced Mod Loading ===");
        
        // Apply all Harmony patches
        _harmonyInstance = new Harmony(HARMONY_ID);
        _harmonyInstance.PatchAll();
        
        // Initialize features
        InitializeFeatures();
    }
    
    void OnDisable()
    {
        _harmonyInstance?.UnpatchAll(HARMONY_ID);
    }
    
    private void InitializeFeatures()
    {
        // Feature initialization logic
        ModLogger.Log("Features initialized");
    }
}
```

#### info.ini
```ini
name=EfDEnhanced                  # DLL name (without .dll)
displayName=EfD Enhanced          # Display name in mod manager
description=Enhanced gameplay     # Mod description
publishedFileId=3590346461        # Steam Workshop ID (optional)
```

---

## Development Environment Setup

### 1. Clone/Create Project
```bash
# Clone existing project
git clone https://github.com/yourusername/EfDEnhanced.git
cd EfDEnhanced

# Or create new project
dotnet new classlib -n YourModName -f netstandard2.1
```

### 2. Configure Project File

Edit `.csproj` to reference game assemblies:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    
    <!-- Game installation path (adjust for your system) -->
    <DuckovDataPath>~/Library/Application Support/Steam/steamapps/common/Duckov/Duckov.app/Contents/Resources/Data</DuckovDataPath>
  </PropertyGroup>

  <ItemGroup>
    <!-- Game assemblies -->
    <Reference Include="TeamSoda.Duckov.Core">
      <HintPath>$(DuckovDataPath)/Managed/TeamSoda.Duckov.Core.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="Assembly-CSharp">
      <HintPath>$(DuckovDataPath)/Managed/Assembly-CSharp.dll</HintPath>
      <Private>false</Private>
    </Reference>
    
    <!-- Unity assemblies -->
    <Reference Include="UnityEngine">
      <HintPath>$(DuckovDataPath)/Managed/UnityEngine.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="UnityEngine.CoreModule">
      <HintPath>$(DuckovDataPath)/Managed/UnityEngine.CoreModule.dll</HintPath>
      <Private>false</Private>
    </Reference>
    
    <!-- Harmony for patching -->
    <PackageReference Include="Lib.Harmony" Version="2.2.2" />
  </ItemGroup>
</Project>
```

### 3. Build & Deploy

```bash
# Build mod
dotnet build

# Deploy to game (using script)
./scripts/deploy.sh

# Or manually copy to:
# [GameFolder]/Duckov_Data/Mods/YourModName/
```

---

## Core Concepts

### 1. Mod Lifecycle

The game loads mods through this process:

```
Game Start
    ↓
Scan Duckov_Data/Mods/ for info.ini files
    ↓
Read 'name' parameter from info.ini
    ↓
Load {name}.dll assembly
    ↓
Find class inheriting Duckov.Modding.ModBehaviour
    ↓
Create GameObject with mod component
    ↓
Call Unity lifecycle methods:
    - Awake() → Start() → OnEnable()
    ↓
Mod active in game
```

### 2. Unity Lifecycle Methods

```csharp
public class ModBehaviour : Duckov.Modding.ModBehaviour
{
    // Called when component is created
    void Awake()
    {
        // Initialize core systems
        // Apply Harmony patches
        // Set up singleton
    }
    
    // Called after Awake, before first frame
    void Start()
    {
        // Initialize features that depend on game state
        // Register event handlers
    }
    
    // Called when component is enabled
    void OnEnable()
    {
        // Subscribe to events
        // Activate features
    }
    
    // Called when component is disabled
    void OnDisable()
    {
        // Unsubscribe from events
        // Clean up patches
    }
    
    // Called every frame
    void Update()
    {
        // Per-frame logic (use sparingly)
    }
    
    // Called at fixed intervals (physics)
    void FixedUpdate()
    {
        // Physics-related logic
    }
    
    // Called when component is destroyed
    void OnDestroy()
    {
        // Final cleanup
    }
}
```

### 3. Logging System

Always use `ModLogger` for consistent logging:

```csharp
using EfDEnhanced.Utils;

// Basic logging
ModLogger.Log("Initialization complete");
ModLogger.LogWarning("Configuration missing, using defaults");
ModLogger.LogError("Failed to load resource");

// Component-specific logging
ModLogger.Log("Inventory", "Item added to inventory");
ModLogger.Log("Quest", $"Quest {questId} completed");

// Logging with data
ModLogger.Log("Player", $"Position: {player.transform.position}");
```

**Log Location**:
- macOS: `~/Library/Logs/TeamSoda/Duckov/Player.log`
- Windows: `%USERPROFILE%\AppData\LocalLow\TeamSoda\Duckov\Player.log`
- Linux: `~/.config/unity3d/TeamSoda/Duckov/Player.log`

**View Logs in Real-Time**:
```bash
./scripts/rlog.sh
```

---

## Harmony Patching

### Basic Patch Structure

```csharp
using HarmonyLib;
using EfDEnhanced.Utils;

namespace EfDEnhanced.Patches;

[HarmonyPatch(typeof(TargetClass), "MethodName")]
public class MyPatch
{
    // Prefix: Runs before original method
    static bool Prefix(TargetClass __instance, ref int param1, out int __result)
    {
        try
        {
            ModLogger.Log("MyPatch", "Prefix executing");
            
            // Modify parameters
            param1 = param1 * 2;
            
            // Skip original method (return false)
            // __result = 42;
            // return false;
            
            // Run original method (return true)
            __result = 0; // Required when using 'out'
            return true;
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"MyPatch Prefix failed: {ex}");
            __result = 0;
            return true; // Always run original on error
        }
    }
    
    // Postfix: Runs after original method
    static void Postfix(TargetClass __instance, ref int __result)
    {
        try
        {
            ModLogger.Log("MyPatch", $"Original result: {__result}");
            
            // Modify return value
            __result = __result * 2;
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"MyPatch Postfix failed: {ex}");
        }
    }
}
```

### Patch Types

#### 1. Prefix Patch
Runs **before** the original method.

```csharp
[HarmonyPatch(typeof(Player), "TakeDamage")]
static bool Prefix(Player __instance, ref float damage)
{
    // Reduce damage by 50%
    damage *= 0.5f;
    
    ModLogger.Log("Damage", $"Reduced to {damage}");
    
    return true; // Run original method
}
```

**Use Cases**:
- Modify method parameters
- Skip method execution
- Add pre-conditions
- Log method calls

#### 2. Postfix Patch
Runs **after** the original method.

```csharp
[HarmonyPatch(typeof(Inventory), "AddItem")]
static void Postfix(Inventory __instance, Item item, bool __result)
{
    if (__result)
    {
        ModLogger.Log("Inventory", $"Added {item.name}");
        
        // Additional logic after successful add
        PlayItemPickupSound();
    }
}
```

**Use Cases**:
- Modify return values
- Add post-processing
- Trigger side effects
- Log results

#### 3. Transpiler Patch
Modifies IL code directly (advanced).

```csharp
[HarmonyPatch(typeof(Enemy), "CalculateDamage")]
static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
{
    // IL code modification
    // See Harmony documentation for details
}
```

**Use Cases**:
- Fine-grained control
- Performance optimization
- Complex modifications

#### 4. Finalizer Patch
Runs after method, even if exception thrown.

```csharp
[HarmonyPatch(typeof(GameManager), "SaveGame")]
static Exception? Finalizer(Exception __exception)
{
    if (__exception != null)
    {
        ModLogger.LogError($"Save failed: {__exception}");
        // Handle or suppress exception
        return null; // Suppress exception
    }
    return null;
}
```

### Special Patch Parameters

Harmony provides special parameter names:

```csharp
static void Postfix(
    TargetClass __instance,      // Instance being patched (non-static methods)
    ref int __result,             // Method return value (ref allows modification)
    Exception __exception,        // Exception thrown (if any)
    MethodBase __originalMethod,  // Original method info
    int normalParam              // Normal method parameters
)
{
    // Use special parameters
}
```

### Patch Organization

Group patches by game system:

```
Patches/
├── PlayerPatches.cs          # Player-related patches
├── InventoryPatches.cs       # Inventory system
├── QuestPatches.cs           # Quest system
├── CombatPatches.cs          # Combat mechanics
├── UIPatches.cs              # User interface
└── WorldPatches.cs           # World/scene patches
```

Example patch file:

```csharp
namespace EfDEnhanced.Patches;

// Multiple patches in one file, grouped by system
public class InventoryPatches
{
    [HarmonyPatch(typeof(Inventory), "AddItem")]
    public class AddItemPatch
    {
        static void Postfix(bool __result, Item item)
        {
            if (__result)
                ModLogger.Log("Inventory", $"Added: {item.name}");
        }
    }
    
    [HarmonyPatch(typeof(Inventory), "RemoveItem")]
    public class RemoveItemPatch
    {
        static void Postfix(bool __result, Item item)
        {
            if (__result)
                ModLogger.Log("Inventory", $"Removed: {item.name}");
        }
    }
}
```

---

## Working with Unity Assets

### Loading Assets at Runtime

#### From Resources Folder
```csharp
// Load prefab
GameObject prefab = Resources.Load<GameObject>("Prefabs/MyPrefab");
if (prefab != null)
{
    GameObject instance = Instantiate(prefab);
    ModLogger.Log("Assets", "Prefab loaded");
}

// Load texture
Texture2D texture = Resources.Load<Texture2D>("Textures/MyTexture");

// Load audio
AudioClip clip = Resources.Load<AudioClip>("Sounds/MySound");
```

#### Finding Scene Objects
```csharp
// Find by name
GameObject player = GameObject.Find("Player");

// Find by tag
GameObject enemy = GameObject.FindWithTag("Enemy");
GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

// Find by type
Player playerComponent = FindObjectOfType<Player>();
Player[] allPlayers = FindObjectsOfType<Player>();

// Find in children
Transform child = transform.Find("ChildName");
```

### Asset Modification

#### Replace Textures
```csharp
Renderer renderer = gameObject.GetComponent<Renderer>();
if (renderer != null && renderer.material != null)
{
    Texture2D newTexture = LoadCustomTexture();
    renderer.material.mainTexture = newTexture;
    
    ModLogger.Log("Assets", "Texture replaced");
}
```

#### Modify Materials
```csharp
Material material = renderer.material;
material.color = Color.red;
material.SetFloat("_Metallic", 0.5f);
material.SetFloat("_Smoothness", 0.8f);
```

#### Spawn Objects
```csharp
// Spawn at position
Vector3 spawnPos = new Vector3(10, 0, 5);
Quaternion rotation = Quaternion.identity;
GameObject obj = Instantiate(prefab, spawnPos, rotation);

// Spawn as child
GameObject child = Instantiate(prefab, parentTransform);
child.transform.localPosition = Vector3.zero;
```

---

## Scene Manipulation

### Scene Loading & Events

```csharp
using UnityEngine.SceneManagement;

void Awake()
{
    // Subscribe to scene events
    SceneManager.sceneLoaded += OnSceneLoaded;
    SceneManager.sceneUnloaded += OnSceneUnloaded;
}

void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    ModLogger.Log("Scene", $"Loaded: {scene.name}");
    
    // Scene-specific initialization
    switch (scene.name)
    {
        case "Level_GroundZero_1":
            InitializeGroundZero();
            break;
        case "Level_StormZone_B0":
            InitializeStormZone();
            break;
    }
}

void OnSceneUnloaded(Scene scene)
{
    ModLogger.Log("Scene", $"Unloaded: {scene.name}");
    // Cleanup scene-specific data
}
```

### Working with Terrain

```csharp
// Get active terrain
Terrain terrain = Terrain.activeTerrain;
if (terrain != null)
{
    TerrainData data = terrain.terrainData;
    
    // Get terrain info
    Vector3 size = data.size;
    int heightmapRes = data.heightmapResolution;
    
    ModLogger.Log("Terrain", $"Size: {size}, Resolution: {heightmapRes}");
    
    // Modify heightmap (advanced)
    float[,] heights = data.GetHeights(0, 0, heightmapRes, heightmapRes);
    // Modify heights...
    data.SetHeights(0, 0, heights);
}
```

### Lighting Modifications

```csharp
// Modify ambient lighting
RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.6f);
RenderSettings.ambientIntensity = 1.2f;

// Modify fog
RenderSettings.fog = true;
RenderSettings.fogColor = Color.gray;
RenderSettings.fogMode = FogMode.ExponentialSquared;
RenderSettings.fogDensity = 0.01f;

// Modify skybox
RenderSettings.skybox = customSkyboxMaterial;
```

---

## Best Practices

### Code Organization

#### 1. One Feature Per File
```
Features/
├── GodMode.cs
├── ItemSpawner.cs
├── TeleportSystem.cs
└── CustomUI.cs
```

#### 2. Feature Base Class
```csharp
public abstract class ModFeature
{
    protected ModBehaviour Mod => ModBehaviour.Instance;
    
    public abstract void Initialize();
    public abstract void Enable();
    public abstract void Disable();
    public virtual void Update() { }
}

public class GodMode : ModFeature
{
    public override void Initialize()
    {
        ModLogger.Log("GodMode", "Initialized");
    }
    
    public override void Enable()
    {
        // Apply god mode patches
    }
    
    public override void Disable()
    {
        // Remove god mode effects
    }
}
```

### Error Handling

#### Always Use Try-Catch in Patches
```csharp
[HarmonyPatch(typeof(Player), "TakeDamage")]
static bool Prefix(Player __instance, float damage)
{
    try
    {
        // Patch logic
        return true;
    }
    catch (Exception ex)
    {
        ModLogger.LogError($"TakeDamage patch failed: {ex}");
        return true; // Always run original on error
    }
}
```

#### Null Checks
```csharp
// Check before accessing
if (player != null && player.inventory != null)
{
    player.inventory.AddItem(item);
}

// Null-conditional operator
player?.inventory?.AddItem(item);

// Null-coalescing
var inventory = player?.inventory ?? defaultInventory;
```

### Performance

#### Cache Component References
```csharp
public class MyFeature : MonoBehaviour
{
    private Transform _cachedTransform;
    private Rigidbody _cachedRigidbody;
    
    void Awake()
    {
        // Cache once
        _cachedTransform = transform;
        _cachedRigidbody = GetComponent<Rigidbody>();
    }
    
    void Update()
    {
        // Use cached references
        _cachedTransform.position += Vector3.forward;
    }
}
```

#### Avoid Update() When Possible
```csharp
// Bad: Heavy logic every frame
void Update()
{
    FindAllEnemies(); // Expensive!
}

// Good: Use coroutines
IEnumerator UpdateEnemies()
{
    while (true)
    {
        FindAllEnemies();
        yield return new WaitForSeconds(1.0f); // Once per second
    }
}

void Start()
{
    StartCoroutine(UpdateEnemies());
}
```

#### Object Pooling
```csharp
public class ObjectPool
{
    private Queue<GameObject> _pool = new Queue<GameObject>();
    private GameObject _prefab;
    
    public GameObject Get()
    {
        if (_pool.Count > 0)
            return _pool.Dequeue();
        
        return Instantiate(_prefab);
    }
    
    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        _pool.Enqueue(obj);
    }
}
```

---

## Testing & Debugging

### Development Workflow

```bash
# 1. Make code changes
vim Patches/MyPatch.cs

# 2. Build mod
dotnet build

# 3. Deploy to game
./scripts/deploy.sh

# 4. Start log monitoring (in separate terminal)
./scripts/rlog.sh

# 5. Launch game and test

# 6. Check logs for errors
# Logs will appear in rlog.sh output
```

### Debugging Techniques

#### Extensive Logging
```csharp
[HarmonyPatch(typeof(QuestManager), "CompleteQuest")]
static void Postfix(Quest quest)
{
    try
    {
        ModLogger.Log("Quest", "=== CompleteQuest called ===");
        ModLogger.Log("Quest", $"Quest ID: {quest.id}");
        ModLogger.Log("Quest", $"Quest Name: {quest.name}");
        ModLogger.Log("Quest", $"Rewards: {quest.rewards.Count}");
        
        // Feature logic
        
        ModLogger.Log("Quest", "=== CompleteQuest finished ===");
    }
    catch (Exception ex)
    {
        ModLogger.LogError($"CompleteQuest failed: {ex}");
        ModLogger.LogError($"Stack trace: {ex.StackTrace}");
    }
}
```

#### Conditional Breakpoints
```csharp
// Debug flag
public static bool DebugMode = true;

if (DebugMode)
{
    ModLogger.Log("Debug", $"Variable value: {someVar}");
    ModLogger.Log("Debug", $"Object state: {JsonUtility.ToJson(obj)}");
}
```

#### Unity Console Integration
```csharp
// Use Unity's Debug class for in-game console
Debug.Log("This appears in Unity console");
Debug.LogWarning("Warning in Unity console");
Debug.LogError("Error in Unity console");

// Best practice: Use ModLogger which wraps Debug
ModLogger.Log("Works in both log file and console");
```

### Common Issues

#### Issue: Mod Not Loading
**Symptoms**: No log output, mod doesn't appear in game

**Solutions**:
1. Check `info.ini` name matches DLL name
2. Verify file structure:
   ```
   Duckov_Data/Mods/ModName/
   ├── ModName.dll
   ├── 0Harmony.dll
   ├── info.ini
   └── preview.png
   ```
3. Check Unity log for loading errors

#### Issue: Harmony Patches Not Applying
**Symptoms**: Patch methods never execute

**Solutions**:
1. Verify target class and method exist:
   ```bash
   grep -r "class QuestManager" decompiled/
   grep -r "CompleteQuest" decompiled/QuestManager.cs
   ```
2. Check method signature matches exactly
3. Ensure `PatchAll()` is called in `Awake()`
4. Add logging to verify patch application:
   ```csharp
   void Awake()
   {
       _harmonyInstance = new Harmony(HARMONY_ID);
       _harmonyInstance.PatchAll();
       
       var patches = _harmonyInstance.GetPatchedMethods();
       foreach (var method in patches)
       {
           ModLogger.Log("Harmony", $"Patched: {method.DeclaringType}.{method.Name}");
       }
   }
   ```

#### Issue: NullReferenceException
**Symptoms**: Exception in logs about null object

**Solutions**:
1. Add null checks:
   ```csharp
   if (obj == null)
   {
       ModLogger.LogWarning("Object is null!");
       return;
   }
   ```
2. Use null-conditional operators: `obj?.Method()`
3. Check Unity object lifecycle (may be destroyed)

---

## Common Patterns

### Singleton Pattern
```csharp
public class ModBehaviour : Duckov.Modding.ModBehaviour
{
    public static ModBehaviour? Instance { get; private set; }
    
    void Awake()
    {
        if (Instance != null)
        {
            ModLogger.LogWarning("Duplicate instance detected");
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
    }
}
```

### Event System
```csharp
// Define events
public class ModEvents
{
    public static event Action<Player>? OnPlayerSpawned;
    public static event Action<Quest>? OnQuestCompleted;
    public static event Action<Item>? OnItemPickup;
    
    public static void PlayerSpawned(Player player)
    {
        OnPlayerSpawned?.Invoke(player);
    }
}

// Subscribe to events
void OnEnable()
{
    ModEvents.OnPlayerSpawned += HandlePlayerSpawned;
}

void OnDisable()
{
    ModEvents.OnPlayerSpawned -= HandlePlayerSpawned;
}

void HandlePlayerSpawned(Player player)
{
    ModLogger.Log("Event", $"Player spawned: {player.name}");
}
```

### Configuration System
```csharp
[Serializable]
public class ModConfig
{
    public bool GodModeEnabled = false;
    public float DamageMultiplier = 1.0f;
    public int MaxInventorySize = 100;
}

public class ConfigManager
{
    private static string ConfigPath => Path.Combine(
        Application.persistentDataPath,
        "EfDEnhanced_config.json"
    );
    
    public static ModConfig Load()
    {
        if (File.Exists(ConfigPath))
        {
            string json = File.ReadAllText(ConfigPath);
            return JsonUtility.FromJson<ModConfig>(json);
        }
        
        return new ModConfig(); // Default
    }
    
    public static void Save(ModConfig config)
    {
        string json = JsonUtility.ToJson(config, true);
        File.WriteAllText(ConfigPath, json);
    }
}
```

### Coroutine Patterns
```csharp
// Delayed execution
IEnumerator ExecuteAfterDelay(float delay, Action action)
{
    yield return new WaitForSeconds(delay);
    action?.Invoke();
}

// Usage
StartCoroutine(ExecuteAfterDelay(2.0f, () => {
    ModLogger.Log("Delayed", "Executed after 2 seconds");
}));

// Repeated execution
IEnumerator RepeatAction(float interval, Action action)
{
    while (true)
    {
        action?.Invoke();
        yield return new WaitForSeconds(interval);
    }
}

// Usage
StartCoroutine(RepeatAction(5.0f, () => {
    ModLogger.Log("Repeat", "Every 5 seconds");
}));
```

---

## Troubleshooting

### Build Errors

**Error**: Cannot find game assemblies

**Solution**: Update `DuckovDataPath` in `.csproj`:
```xml
<DuckovDataPath>C:\Path\To\Game\Duckov_Data</DuckovDataPath>
```

**Error**: Nullable reference type warnings

**Solution**: Add null checks or disable nullable:
```xml
<Nullable>disable</Nullable>
```

### Runtime Errors

**Error**: `MissingMethodException`

**Cause**: Method signature doesn't match

**Solution**: Verify method signature in decompiled code

**Error**: `TypeLoadException`

**Cause**: Assembly version mismatch

**Solution**: Ensure using correct Unity/game versions

### Performance Issues

**Symptom**: Game lag/stuttering

**Causes**:
1. Heavy logic in `Update()`
2. Excessive logging
3. Memory leaks

**Solutions**:
1. Move logic to coroutines
2. Remove debug logging in production
3. Properly dispose objects

---

## Resources

### Documentation
- [CLAUDE.md](../../CLAUDE.md) - Main project guide
- [scenes-guide.md](../scenes/scenes-guide.md) - Game scenes
- [assets-guide.md](assets-guide.md) - Asset system

### External Resources
- [Harmony Documentation](https://harmony.pardeike.net/)
- [Unity Scripting Reference](https://docs.unity3d.com/ScriptReference/)
- [C# Programming Guide](https://docs.microsoft.com/en-us/dotnet/csharp/)

### Tools
- [dnSpy](https://github.com/dnSpy/dnSpy) - .NET debugger/decompiler
- [ILSpy](https://github.com/icsharpcode/ILSpy) - .NET decompiler
- [AssetRipper](https://github.com/AssetRipper/AssetRipper) - Unity asset extractor

---

## Next Steps

1. **Set up development environment**
2. **Study existing patches** in `Patches/` directory
3. **Explore game systems** using decompiled code
4. **Start with simple patches** (logging, value changes)
5. **Gradually add features** as you understand the game
6. **Test thoroughly** and handle errors gracefully
7. **Share your mod** via Steam Workshop

---

**Happy modding!** 🎮

For questions or issues, refer to the documentation or examine decompiled game code for insights into game systems.

