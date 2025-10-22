# Keybinding System Documentation

## Overview

The keybinding system allows users to customize hotkeys for mod features through an intuitive visual interface. This document describes the implementation and usage of the keybinding system.

## Core Components

### KeyCodeSettingsEntry

**Location**: `Utils/Settings/KeyCodeSettingsEntry.cs`

A settings entry type for storing Unity KeyCode values with built-in validation and user-friendly display names.

**Features**:
- Inherits from `SettingsEntry<KeyCode>`
- Validates keybindings to prevent invalid keys
- Converts KeyCode enums to human-readable names
- Persists settings using ES3 serialization

**Key Display Names**:
```csharp
KeyCode.Space           → "Space"
KeyCode.F1              → "F1"
KeyCode.Alpha0          → "0"
KeyCode.Keypad5         → "Num 5"
KeyCode.UpArrow         → "↑"
KeyCode.LeftShift       → "L-Shift"
KeyCode.JoystickButton0 → "A (Gamepad)"
KeyCode.JoystickButton4 → "LB (Gamepad)"
```

**Gamepad Button Mapping** (Xbox Controller Standard):
```
JoystickButton0  = A button
JoystickButton1  = B button
JoystickButton2  = X button
JoystickButton3  = Y button
JoystickButton4  = LB (Left Bumper)
JoystickButton5  = RB (Right Bumper)
JoystickButton6  = Back/View button
JoystickButton7  = Start/Menu button
JoystickButton8  = LS (Left Stick Click)
JoystickButton9  = RS (Right Stick Click)
JoystickButton10-19 = Additional gamepad buttons
```

**Validation Rules**:
- ✅ **Allowed**: A-Z, 0-9, F1-F15, arrow keys, numpad, modifiers (Shift/Ctrl/Alt), control keys (Space/Enter/Tab), mouse middle/side buttons (Mouse2-Mouse6), **gamepad buttons** (JoystickButton0-19)
- ❌ **Blocked**: Mouse left/right buttons (Mouse0/Mouse1), None, unknown keys

### ModKeybindingButton

**Location**: `Utils/UI/Components/ModKeybindingButton.cs`

A visual UI component for rebinding keys, similar to the game's native `UIKeybindingEntry`.

**Features**:
- Click-to-rebind interaction pattern
- Visual feedback during listening state
- ESC to cancel rebinding
- Automatic subscription to settings changes
- Localized UI text
- Consistent styling with game's native UI

**UI States**:
1. **Default**: Shows current bound key
2. **Listening**: Shows "Press Any Key..." prompt with blue accent color
3. **Cancelled**: Reverts to default state with original key

## Usage

### Creating a Keybinding Setting

```csharp
using EfDEnhanced.Utils.Settings;
using UnityEngine;

public static class ModSettings
{
    public static KeyCodeSettingsEntry OpenMenuHotkey = new KeyCodeSettingsEntry(
        prefix: "EfDEnhanced",
        key: "OpenMenuHotkey",
        name: "Settings_OpenMenuHotkey",  // Localization key
        defaultValue: KeyCode.H,
        category: "Keybindings",
        description: "Hotkey to open the menu"
    );
}
```

### Creating a Keybinding UI

```csharp
using EfDEnhanced.Utils.UI.Components;

// Create keybinding button
ModKeybindingButton keybindingButton = ModKeybindingButton.Create(
    parentTransform,
    ModSettings.OpenMenuHotkey
);
```

### Using FormBuilder

```csharp
using EfDEnhanced.Utils.UI.Builders;

var formBuilder = new FormBuilder(parentTransform);
formBuilder.AddKeybinding(ModSettings.OpenMenuHotkey);
formBuilder.Build();
```

### Checking Key Input

```csharp
private void Update()
{
    if (Input.GetKeyDown(ModSettings.OpenMenuHotkey.Value))
    {
        // Trigger hotkey action
        OpenMenu();
    }
}
```

### Listening to Keybinding Changes

```csharp
private void Awake()
{
    ModSettings.OpenMenuHotkey.ValueChanged += OnHotkeyChanged;
}

private void OnHotkeyChanged(object sender, ValueChangedEventArgs<KeyCode> e)
{
    ModLogger.Log("Keybinding", $"Hotkey changed from {e.OldValue} to {e.NewValue}");
}

private void OnDestroy()
{
    ModSettings.OpenMenuHotkey.ValueChanged -= OnHotkeyChanged;
}
```

## Implementation Details

### Validation System

The `KeyCodeSettingsEntry.Validate()` method uses pattern matching to categorize valid keys:

```csharp
protected override bool Validate(KeyCode value)
{
    return value switch
    {
        KeyCode.None => false,
        >= KeyCode.JoystickButton0 and <= KeyCode.JoystickButton19 => true, // ✅ Allow gamepad
        KeyCode.Mouse0 => false,
        KeyCode.Mouse1 => false,
        >= KeyCode.Mouse2 and <= KeyCode.Mouse6 => true,
        >= KeyCode.A and <= KeyCode.Z => true,
        >= KeyCode.Alpha0 and <= KeyCode.Alpha9 => true,
        >= KeyCode.F1 and <= KeyCode.F15 => true,
        // ... more patterns
        _ => false
    };
}
```

### Listening Coroutine

The `ModKeybindingButton` uses a coroutine to listen for key input:

```csharp
private IEnumerator ListenForKeyCoroutine()
{
    yield return null; // Skip click frame
    
    while (!keyReceived)
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopListening(false); // Cancel
            yield break;
        }
        
        foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode) && !IsKeyExcluded(keyCode))
            {
                _settingsEntry.Value = keyCode; // Validated by setter
                keyReceived = true;
                break;
            }
        }
        
        yield return null;
    }
    
    StopListening(true); // Success
}
```

## Best Practices

1. **Use Descriptive Localization Keys**: Name your keybinding settings clearly
   ```csharp
   name: "Settings_OpenMenu"
   name: "Settings_ToggleTracker"
   ```

2. **Choose Appropriate Default Keys**: Avoid common game keys
   ```csharp
   defaultValue: KeyCode.H              // ✅ Good - uncommon keyboard key
   defaultValue: KeyCode.JoystickButton6 // ✅ Good - gamepad Back button
   defaultValue: KeyCode.W              // ❌ Bad - movement key
   defaultValue: KeyCode.Mouse0         // ❌ Bad - reserved for clicking
   ```

3. **Group Related Keybindings**: Use categories for organization
   ```csharp
   category: "Keybindings"
   ```

4. **Validate User Input**: Let `KeyCodeSettingsEntry` handle validation
   ```csharp
   // No need to manually validate - the entry does it automatically
   ModSettings.OpenMenuHotkey.Value = userInput;
   ```

5. **Provide Visual Feedback**: Use the standard `ModKeybindingButton` component
   ```csharp
   // Don't create custom keybinding UI - use the provided component
   ModKeybindingButton.Create(parent, settingsEntry);
   ```

## Localization

The keybinding system supports localization through `LocalizationHelper`:

**Required Localization Keys**:
- `Settings_PressAnyKey` - Displayed during listening state
- `{settingsEntry.NameKey}` - Label for the keybinding

**Example**:
```csharp
LocalizationHelper.RegisterText("Settings_PressAnyKey", "Press Any Key...");
LocalizationHelper.RegisterText("Settings_OpenMenuHotkey", "Open Menu");
```

## Example Implementation

See `Features/ModSettingsContent.cs` and `Patches/ItemWheelMenuPatch.cs` for complete examples of the keybinding system in action.

## Technical Notes

- **Performance**: The listening coroutine runs only when rebinding, not during gameplay
- **Conflicts**: The system does not automatically detect conflicts between keybindings - implement conflict detection if needed
- **Persistence**: Keybindings are saved via ES3 along with other settings
- **Thread Safety**: All keybinding operations run on the Unity main thread

## Migration Guide

### Adding Keybinding to Existing Feature

1. Define the keybinding in `ModSettings.cs`:
   ```csharp
   public static KeyCodeSettingsEntry FeatureHotkey = new KeyCodeSettingsEntry(
       prefix: "EfDEnhanced",
       key: "FeatureHotkey",
       name: "Settings_FeatureHotkey",
       defaultValue: KeyCode.F,
       category: "Keybindings"
   );
   ```

2. Add UI in `ModSettingsContent.cs`:
   ```csharp
   formBuilder.AddKeybinding(ModSettings.FeatureHotkey);
   ```

3. Check input in your feature:
   ```csharp
   if (Input.GetKeyDown(ModSettings.FeatureHotkey.Value))
   {
       ToggleFeature();
   }
   ```

4. Add localization:
   ```csharp
   LocalizationHelper.RegisterText("Settings_FeatureHotkey", "Toggle Feature");
   ```

