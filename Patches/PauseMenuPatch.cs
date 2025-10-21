using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EfDEnhanced.Utils;
using EfDEnhanced.Features;

namespace EfDEnhanced.Patches
{
    /// <summary>
    /// Adds "Mod Settings" button to the pause menu
    /// </summary>
    [HarmonyPatch(typeof(PauseMenu))]
    public class PauseMenuPatch
    {
        private static ModSettingsPanel? settingsPanel;
        private static bool buttonAdded = false;

        /// <summary>
        /// Patch PauseMenu.Show to add our settings button (only once)
        /// </summary>
        [HarmonyPatch("Show")]
        [HarmonyPostfix]
        public static void Show_Postfix()
        {
            try
            {
                // Only add button once
                if (buttonAdded)
                {
                    return;
                }

                // Get PauseMenu instance
                PauseMenu instance = PauseMenu.Instance;
                if (instance == null)
                {
                    ModLogger.LogError("PauseMenuPatch: PauseMenu.Instance is null");
                    return;
                }

                ModLogger.Log("PauseMenuPatch", "Adding Mod Settings button to pause menu");

                // Create settings panel as independent UI (not child of pause menu)
                // It needs its own Canvas to stay visible when pause menu closes
                var panelObj = new GameObject("EfDEnhanced_SettingsPanel");
                GameObject.DontDestroyOnLoad(panelObj);
                settingsPanel = panelObj.AddComponent<ModSettingsPanel>();

                // Panel starts hidden (via canvasGroup.alpha = 0 in BuildUI)

                // Find the pause menu's button container
                Transform? buttonContainer = FindButtonContainer(instance.transform);
                if (buttonContainer == null)
                {
                    ModLogger.LogError("PauseMenuPatch: Could not find button container");
                    return;
                }

                // Find the "设置" (Settings) button to insert after it
                Button? settingsButton = FindSettingsButton(buttonContainer);
                if (settingsButton == null)
                {
                    ModLogger.LogWarning("PauseMenuPatch: Could not find Settings button, will add to end");
                }

                // Find an existing button to copy its style
                Button? templateButton = buttonContainer.GetComponentInChildren<Button>();
                if (templateButton == null)
                {
                    ModLogger.LogError("PauseMenuPatch: Could not find template button");
                    return;
                }

                // Create our settings button by cloning template
                GameObject modSettingsButtonObj = GameObject.Instantiate(templateButton.gameObject, buttonContainer);
                modSettingsButtonObj.name = "ModSettingsButton";

                // Position after "设置" button
                if (settingsButton != null)
                {
                    int settingsButtonIndex = settingsButton.transform.GetSiblingIndex();
                    modSettingsButtonObj.transform.SetSiblingIndex(settingsButtonIndex + 1);
                    ModLogger.Log("PauseMenuPatch", $"Positioned Mod Settings button at index {settingsButtonIndex + 1}");
                }

                // Update button text and disable any dynamic text components
                TextMeshProUGUI? buttonText = modSettingsButtonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = LocalizationHelper.Get("Settings_ModSettings_Button");

                    // Disable any localization components that might change the text
                    var localizationComponents = modSettingsButtonObj.GetComponents<MonoBehaviour>();
                    foreach (var component in localizationComponents)
                    {
                        if (component != null && component.GetType().Name.Contains("Locali"))
                        {
                            component.enabled = false;
                            ModLogger.Log("PauseMenuPatch", $"Disabled component: {component.GetType().Name}");
                        }
                    }
                }

                // Setup button click handler
                Button modSettingsButton = modSettingsButtonObj.GetComponent<Button>();
                modSettingsButton.onClick.RemoveAllListeners();
                modSettingsButton.onClick.AddListener(() =>
                {
                    ModLogger.Log("PauseMenuPatch", "Mod Settings button clicked");
                    if (settingsPanel != null)
                    {
                        // Close pause menu first, then open settings panel
                        PauseMenu.Hide();

                        // Direct Open() call - no parent-child relationship needed
                        settingsPanel.Open();
                    }
                });

                buttonAdded = true;
                ModLogger.Log("PauseMenuPatch", "Mod Settings button added successfully");
            }
            catch (System.Exception ex)
            {
                ModLogger.LogError($"PauseMenuPatch.Show_Postfix failed: {ex}");
            }
        }

        /// <summary>
        /// Find the button container in pause menu hierarchy
        /// </summary>
        private static Transform? FindButtonContainer(Transform root)
        {
            // Try common names for button containers
            string[] possibleNames = { "Buttons", "ButtonContainer", "Content", "Panel", "Body", "Layout" };

            foreach (string name in possibleNames)
            {
                Transform? found = root.Find(name);
                if (found != null)
                {
                    ModLogger.Log("PauseMenuPatch", $"Found button container: {name}");
                    return found;
                }
            }

            // If not found by name, look for any transform with multiple Button children
            foreach (Transform child in root)
            {
                int buttonCount = child.GetComponentsInChildren<Button>().Length;
                if (buttonCount >= 2)
                {
                    ModLogger.Log("PauseMenuPatch", $"Found button container with {buttonCount} buttons: {child.name}");
                    return child;
                }
            }

            ModLogger.LogWarning("PauseMenuPatch: Could not find button container, using root");
            return root;
        }

        /// <summary>
        /// Find the "设置" (Settings) button in the button container
        /// </summary>
        private static Button? FindSettingsButton(Transform buttonContainer)
        {
            Button[] buttons = buttonContainer.GetComponentsInChildren<Button>();

            foreach (Button button in buttons)
            {
                TextMeshProUGUI? buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    string text = buttonText.text;
                    ModLogger.Log("PauseMenuPatch", $"Found button with text: {text}");

                    // Check for "设置" (Settings) in Chinese or "Settings" in English
                    if (text.Contains("设置") || text.Contains("Settings"))
                    {
                        ModLogger.Log("PauseMenuPatch", $"Found Settings button at index {button.transform.GetSiblingIndex()}");
                        return button;
                    }
                }
            }

            return null;
        }
    }
}
