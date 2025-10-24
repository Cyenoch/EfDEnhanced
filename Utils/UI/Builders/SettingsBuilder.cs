using System;
using System.Collections.Generic;
using EfDEnhanced.Utils.Settings;
using EfDEnhanced.Utils.UI.Components;
using EfDEnhanced.Utils.UI.Components.SettingsItems;
using EfDEnhanced.Utils.UI.Constants;
using UnityEngine;
using UnityEngine.UI;

namespace EfDEnhanced.Utils.UI.Builders
{
    /// <summary>
    /// Builder for creating settings UI from SettingsEntry objects
    /// Automatically creates appropriate UI components based on entry type
    /// </summary>
    public class SettingsBuilder
    {
        private readonly Transform _parentTransform;
        private readonly List<GameObject> _createdItems = new();

        public SettingsBuilder(Transform parent)
        {
            _parentTransform = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        /// <summary>
        /// Add a section header
        /// </summary>
        public SettingsBuilder AddSection(string sectionKey)
        {
            var sectionObj = new GameObject($"Section_{sectionKey}");
            sectionObj.transform.SetParent(_parentTransform, false);

            var sectionItem = sectionObj.AddComponent<SectionHeaderItem>();
            sectionItem.Initialize(sectionKey);

            _createdItems.Add(sectionObj);
            return this;
        }

        /// <summary>
        /// Add a spacer
        /// </summary>
        public SettingsBuilder AddSpacer(float height = 10f)
        {
            var spacerObj = new GameObject("Spacer");
            spacerObj.transform.SetParent(_parentTransform, false);

            var spacerItem = spacerObj.AddComponent<SpacerItem>();
            spacerItem.Initialize(height);

            _createdItems.Add(spacerObj);
            return this;
        }

        /// <summary>
        /// Add a small spacer (5px)
        /// </summary>
        public SettingsBuilder AddSmallSpacer()
        {
            return AddSpacer(5f);
        }

        /// <summary>
        /// Add a medium spacer (10px)
        /// </summary>
        public SettingsBuilder AddMediumSpacer()
        {
            return AddSpacer(10f);
        }

        /// <summary>
        /// Add a large spacer (20px)
        /// </summary>
        public SettingsBuilder AddLargeSpacer()
        {
            return AddSpacer(20f);
        }

        /// <summary>
        /// Add spacing with custom height
        /// </summary>
        public SettingsBuilder AddSpacing(float height)
        {
            return AddSpacer(height);
        }

        /// <summary>
        /// Add a settings entry with optional visibility condition
        /// </summary>
        public SettingsBuilder AddSetting(ISettingsEntry entry, BoolSettingsEntry? visibilityCondition = null, int leftPadding = 0)
        {
            if (entry == null)
            {
                ModLogger.LogWarning("SettingsBuilder", "Attempted to add null settings entry");
                return this;
            }

            try
            {
                if (visibilityCondition != null)
                {
                    leftPadding += 16;
                }
                GameObject itemObj = CreateItemForEntry(entry, leftPadding);

                if (itemObj != null)
                {
                    // Handle visibility condition
                    if (visibilityCondition != null)
                    {
                        SetupVisibilityCondition(itemObj, visibilityCondition);
                    }

                    _createdItems.Add(itemObj);
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Failed to create settings item for {entry.Key}: {ex}");
            }

            return this;
        }

        /// <summary>
        /// Add a button
        /// </summary>
        public SettingsBuilder AddButton(string labelKey, Action onClick, UIStyles.ButtonStyle style = UIStyles.ButtonStyle.Primary)
        {
            var buttonObj = new GameObject($"Button_{labelKey}");
            buttonObj.transform.SetParent(_parentTransform, false);

            var buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0, 1);
            buttonRect.anchorMax = new Vector2(1, 1);
            buttonRect.pivot = new Vector2(0.5f, 1);

            // Add layout element
            var layoutElement = buttonObj.AddComponent<LayoutElement>();
            layoutElement.minHeight = 40;
            layoutElement.preferredHeight = 40;

            // Create button using ModButton
            var modButton = ModButton.Create(buttonObj.transform, "Button")
                .SetText(labelKey)
                .OnClick(() => onClick())
                .SetStyle(style);

            _createdItems.Add(buttonObj);
            return this;
        }

        /// <summary>
        /// Create appropriate item component based on entry type
        /// </summary>
        private GameObject CreateItemForEntry(ISettingsEntry entry, int leftPadding)
        {
            var itemObj = new GameObject($"Item_{entry.Key}");
            itemObj.transform.SetParent(_parentTransform, false);

            BaseSettingsItem itemComponent = entry switch
            {
                BoolSettingsEntry boolEntry => itemObj.AddComponent<BoolSettingsItem>(),
                RangedFloatSettingsEntry floatEntry => itemObj.AddComponent<RangedFloatSettingsItem>(),
                IndexedOptionsSettingsEntry optionsEntry => itemObj.AddComponent<IndexedOptionsSettingsItem>(),
                KeyCodeSettingsEntry keyCodeEntry => itemObj.AddComponent<KeyCodeSettingsItem>(),
                _ => throw new NotSupportedException($"Settings entry type {entry.GetType().Name} is not supported")
            };

            itemComponent.Initialize(entry, leftPadding);
            return itemObj;
        }

        /// <summary>
        /// Setup visibility condition for an item
        /// </summary>
        private void SetupVisibilityCondition(GameObject itemObj, BoolSettingsEntry visibilityCondition)
        {
            // Set initial visibility
            itemObj.SetActive(visibilityCondition.Value);

            // Subscribe to value changes
            visibilityCondition.ValueChanged += (sender, e) =>
            {
                if (itemObj != null)
                {
                    itemObj.SetActive(e.NewValue);
                }
            };
        }

        /// <summary>
        /// Get all created items
        /// </summary>
        public List<GameObject> GetCreatedItems()
        {
            return new List<GameObject>(_createdItems);
        }

        /// <summary>
        /// Clear all created items
        /// </summary>
        public void Clear()
        {
            foreach (var item in _createdItems)
            {
                if (item != null)
                {
                    UnityEngine.Object.Destroy(item);
                }
            }
            _createdItems.Clear();
        }
    }
}

