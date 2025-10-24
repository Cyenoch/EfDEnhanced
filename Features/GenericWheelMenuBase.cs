using System;
using System.Collections.Generic;
using UnityEngine;
using EfDEnhanced.Utils;
using EfDEnhanced.Utils.UI.Components;

namespace EfDEnhanced.Features
{
    /// <summary>
    /// Generic base class for wheel menus that eliminates template code duplication.
    /// Handles common lifecycle, event management, and PieMenuComponent initialization.
    /// Subclasses only need to implement RefreshItems() and OnItemInvoked() methods.
    /// 
    /// Type parameter T represents the item identifier type:
    /// - int for ItemWheelMenu and ThrowableWheelMenu
    /// - Item? for ContainerWheelMenu
    /// </summary>
    public abstract class GenericWheelMenuBase<T> : MonoBehaviour
    {
        protected static CharacterMainControl Character => CharacterMainControl.Main;
        protected PieMenuComponent? PieMenu { get; private set; }

        public bool IsOpen => PieMenu != null && PieMenu.IsOpen;

        protected virtual void Awake()
        {
            // Singleton pattern implementation in subclass if needed
            InitializePieMenu();

            // Subscribe to settings changes
            ModSettings.ItemWheelScale.ValueChanged += OnScaleChanged;

        }

        protected virtual void OnDestroy()
        {
            // Unsubscribe from settings changes
            ModSettings.ItemWheelScale.ValueChanged -= OnScaleChanged;

        }

        private void InitializePieMenu()
        {
            try
            {
                // Create pie menu GameObject
                GameObject pieMenuObj = new GameObject("PieMenu");
                pieMenuObj.transform.SetParent(transform, false);

                PieMenu = pieMenuObj.AddComponent<PieMenuComponent>();

                // Initialize with configuration
                var config = PieMenuConfig.Default;
                config.Scale = ModSettings.ItemWheelScale.Value;
                PieMenu.Initialize(config);

                // Subscribe to events
                PieMenu.OnItemInvoked += (itemId) => OnItemInvokedInternal(itemId);
                PieMenu.OnMenuShown += OnMenuShownInternal;
                PieMenu.OnMenuHidden += OnMenuHiddenInternal;

                ModLogger.Log(GetType().Name, "Pie menu initialized successfully");
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"{GetType().Name}: Failed to initialize pie menu: {ex}");
            }
        }

        private void OnScaleChanged(object? sender, Utils.Settings.SettingsValueChangedEventArgs<float> e)
        {
            if (PieMenu != null)
            {
                PieMenu.SetScale(e.NewValue);
                ModLogger.Log(GetType().Name, $"Scale changed to {e.NewValue:F2}");
            }
        }

        private void OnMenuShownInternal()
        {
            try
            {
                // Refresh items when menu is shown
                RefreshItems();

                // Call subclass hook
                OnMenuShown();
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"{GetType().Name}: Error in OnMenuShown: {ex}");
            }
        }

        private void OnMenuHiddenInternal()
        {
            try
            {
                // Call subclass hook
                OnMenuHidden();
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"{GetType().Name}: Error in OnMenuHidden: {ex}");
            }
        }

        private void OnItemInvokedInternal(string itemId)
        {
            try
            {
                T parsedId = ParseItemId(itemId);
                OnItemInvoked(parsedId);
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"{GetType().Name}: Error invoking item: {ex}");
            }
        }

        /// <summary>
        /// Convert string item ID from PieMenuComponent to type T
        /// Override in subclass if custom parsing is needed
        /// </summary>
        protected virtual T ParseItemId(string itemId)
        {
            if (typeof(T) == typeof(int))
            {
                if (int.TryParse(itemId, out int intId))
                {
                    return (T)(object)intId;
                }
                throw new FormatException($"Failed to parse item ID as int: {itemId}");
            }
            else if (typeof(T) == typeof(string))
            {
                return (T)(object)itemId;
            }
            else
            {
                throw new NotSupportedException($"ParseItemId does not support type {typeof(T).Name}");
            }
        }

        #region Public API

        public void Toggle()
        {
            if (PieMenu != null)
            {
                PieMenu.Toggle();
            }
        }

        public void Show()
        {
            if (PieMenu != null)
            {
                PieMenu.Show();
            }
        }

        public void Hide(bool invokeSelectedItem = true)
        {
            if (PieMenu != null)
            {
                PieMenu.Hide(invokeSelectedItem);
            }
        }

        public void Cancel()
        {
            if (PieMenu != null)
            {
                PieMenu.Cancel();
            }
        }

        public Vector2 GetOverrideMousePosition()
        {
            if (PieMenu != null)
            {
                return PieMenu.GetSavedMousePosition();
            }
            return Input.mousePosition;
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Refresh the items displayed in the pie menu.
        /// This is called automatically when the menu is shown.
        /// </summary>
        protected abstract void RefreshItems();

        /// <summary>
        /// Handle item selection in the pie menu.
        /// The T parameter is the parsed item ID.
        /// </summary>
        protected abstract void OnItemInvoked(T itemId);

        #endregion

        #region Virtual Hooks (optional)

        /// <summary>
        /// Called when the menu is shown (after RefreshItems is called)
        /// Override if additional setup is needed
        /// </summary>
        protected virtual void OnMenuShown()
        {
        }

        /// <summary>
        /// Called when the menu is hidden
        /// Override if cleanup is needed
        /// </summary>
        protected virtual void OnMenuHidden()
        {
        }

        #endregion

        /// <summary>
        /// Protected helper to set pie menu items
        /// </summary>
        protected void SetMenuItems(List<PieMenuItem> items)
        {
            if (PieMenu != null)
            {
                PieMenu.SetItems(items);
            }
        }
    }
}
