using System;
using UnityEngine;

namespace EfDEnhanced.Utils.Settings
{
    /// <summary>
    /// Settings entry for storing a KeyCode value
    /// Used for customizable hotkeys
    /// </summary>
    public class KeyCodeSettingsEntry : SettingsEntry<KeyCode>
    {
        /// <summary>
        /// Get the localized display name of the current key
        /// </summary>
        public string KeyDisplayName => GetKeyDisplayName(Value);

        public KeyCodeSettingsEntry(
            string prefix,
            string key,
            string name,
            KeyCode defaultValue,
            string category = "General",
            string? description = null,
            int version = 1)
            : base(prefix, key, name, defaultValue, category, description, version)
        {
        }

        /// <summary>
        /// Get a user-friendly display name for a KeyCode
        /// </summary>
        public static string GetKeyDisplayName(KeyCode keyCode)
        {
            return keyCode switch
            {
                KeyCode.BackQuote => "~",
                KeyCode.Tilde => "~",
                KeyCode.Alpha0 => "0",
                KeyCode.Alpha1 => "1",
                KeyCode.Alpha2 => "2",
                KeyCode.Alpha3 => "3",
                KeyCode.Alpha4 => "4",
                KeyCode.Alpha5 => "5",
                KeyCode.Alpha6 => "6",
                KeyCode.Alpha7 => "7",
                KeyCode.Alpha8 => "8",
                KeyCode.Alpha9 => "9",
                KeyCode.Minus => "-",
                KeyCode.Equals => "=",
                KeyCode.LeftBracket => "[",
                KeyCode.RightBracket => "]",
                KeyCode.Backslash => "\\",
                KeyCode.Semicolon => ";",
                KeyCode.Quote => "'",
                KeyCode.Comma => ",",
                KeyCode.Period => ".",
                KeyCode.Slash => "/",
                KeyCode.Space => "Space",
                KeyCode.Return => "Enter",
                KeyCode.Backspace => "Backspace",
                KeyCode.Tab => "Tab",
                KeyCode.Escape => "Esc",
                KeyCode.LeftShift => "L-Shift",
                KeyCode.RightShift => "R-Shift",
                KeyCode.LeftControl => "L-Ctrl",
                KeyCode.RightControl => "R-Ctrl",
                KeyCode.LeftAlt => "L-Alt",
                KeyCode.RightAlt => "R-Alt",
                KeyCode.CapsLock => "Caps Lock",
                KeyCode.Insert => "Insert",
                KeyCode.Delete => "Delete",
                KeyCode.Home => "Home",
                KeyCode.End => "End",
                KeyCode.PageUp => "Page Up",
                KeyCode.PageDown => "Page Down",
                KeyCode.UpArrow => "↑",
                KeyCode.DownArrow => "↓",
                KeyCode.LeftArrow => "←",
                KeyCode.RightArrow => "→",
                KeyCode.Keypad0 => "Num 0",
                KeyCode.Keypad1 => "Num 1",
                KeyCode.Keypad2 => "Num 2",
                KeyCode.Keypad3 => "Num 3",
                KeyCode.Keypad4 => "Num 4",
                KeyCode.Keypad5 => "Num 5",
                KeyCode.Keypad6 => "Num 6",
                KeyCode.Keypad7 => "Num 7",
                KeyCode.Keypad8 => "Num 8",
                KeyCode.Keypad9 => "Num 9",
                KeyCode.KeypadPeriod => "Num .",
                KeyCode.KeypadDivide => "Num /",
                KeyCode.KeypadMultiply => "Num *",
                KeyCode.KeypadMinus => "Num -",
                KeyCode.KeypadPlus => "Num +",
                KeyCode.KeypadEnter => "Num Enter",
                KeyCode.F1 => "F1",
                KeyCode.F2 => "F2",
                KeyCode.F3 => "F3",
                KeyCode.F4 => "F4",
                KeyCode.F5 => "F5",
                KeyCode.F6 => "F6",
                KeyCode.F7 => "F7",
                KeyCode.F8 => "F8",
                KeyCode.F9 => "F9",
                KeyCode.F10 => "F10",
                KeyCode.F11 => "F11",
                KeyCode.F12 => "F12",
                // Joystick/Gamepad buttons (common names)
                KeyCode.JoystickButton0 => "A (Gamepad)",
                KeyCode.JoystickButton1 => "B (Gamepad)",
                KeyCode.JoystickButton2 => "X (Gamepad)",
                KeyCode.JoystickButton3 => "Y (Gamepad)",
                KeyCode.JoystickButton4 => "LB (Gamepad)",
                KeyCode.JoystickButton5 => "RB (Gamepad)",
                KeyCode.JoystickButton6 => "Back (Gamepad)",
                KeyCode.JoystickButton7 => "Start (Gamepad)",
                KeyCode.JoystickButton8 => "LS Click (Gamepad)",
                KeyCode.JoystickButton9 => "RS Click (Gamepad)",
                KeyCode.JoystickButton10 => "Gamepad 10",
                KeyCode.JoystickButton11 => "Gamepad 11",
                KeyCode.JoystickButton12 => "Gamepad 12",
                KeyCode.JoystickButton13 => "Gamepad 13",
                KeyCode.JoystickButton14 => "Gamepad 14",
                KeyCode.JoystickButton15 => "Gamepad 15",
                KeyCode.JoystickButton16 => "Gamepad 16",
                KeyCode.JoystickButton17 => "Gamepad 17",
                KeyCode.JoystickButton18 => "Gamepad 18",
                KeyCode.JoystickButton19 => "Gamepad 19",
                _ when keyCode >= KeyCode.A && keyCode <= KeyCode.Z => keyCode.ToString(),
                _ => keyCode.ToString()
            };
        }

        /// <summary>
        /// 验证此KeyCode是否可以用于热键绑定。允许绝大多数常用键与功能键（包括手柄按键），排除None及Mouse0/1（左/右键）。
        /// </summary>
        protected override bool Validate(KeyCode value)
        {
            return value switch
            {
                // 不允许空、未知或无效键
                KeyCode.None => false,
                
                // 允许常见手柄按钮（JoystickButton0-19，对应Xbox/PlayStation等手柄）
                >= KeyCode.JoystickButton0 and <= KeyCode.Joystick8Button19 => true,

                // 禁用鼠标左/右键，允许其它按钮（Mouse2 ~ Mouse6 中键、侧键等）
                KeyCode.Mouse0 => false,
                KeyCode.Mouse1 => false,
                >= KeyCode.Mouse2 and <= KeyCode.Mouse6 => true,

                // 允许字母A-Z
                >= KeyCode.A and <= KeyCode.Z => true,

                // 允许数字键0-9
                >= KeyCode.Alpha0 and <= KeyCode.Alpha9 => true,

                // 允许标点符号键（键盘上排和主区域）
                KeyCode.Minus or KeyCode.Equals or KeyCode.LeftBracket or KeyCode.RightBracket => true,
                KeyCode.Backslash or KeyCode.Semicolon or KeyCode.Quote => true,
                KeyCode.Comma or KeyCode.Period or KeyCode.Slash => true,
                KeyCode.BackQuote or KeyCode.Tilde => true,

                // 允许功能键F1-F15（常用范围，F13-F15较少但某些键盘有）
                >= KeyCode.F1 and <= KeyCode.F15 => true,

                // 允许小键盘所有键（数字、运算符、回车、小数点等）
                >= KeyCode.Keypad0 and <= KeyCode.KeypadEquals => true,

                // 允许方向键
                KeyCode.UpArrow or KeyCode.DownArrow or KeyCode.LeftArrow or KeyCode.RightArrow => true,

                // 允许常见控制键
                KeyCode.Backspace or KeyCode.Tab or KeyCode.Clear or KeyCode.Return or KeyCode.Space => true,
                KeyCode.Escape or KeyCode.Pause or KeyCode.CapsLock => true,
                KeyCode.PageUp or KeyCode.PageDown or KeyCode.End or KeyCode.Home => true,
                KeyCode.Insert or KeyCode.Delete => true,

                // 允许修饰键 Shift/Ctrl/Alt/Command/Windows
                KeyCode.LeftShift or KeyCode.RightShift
                    or KeyCode.LeftControl or KeyCode.RightControl
                    or KeyCode.LeftAlt or KeyCode.RightAlt
                    or KeyCode.LeftCommand or KeyCode.RightCommand
                    or KeyCode.LeftWindows or KeyCode.RightWindows
                    or KeyCode.LeftApple or KeyCode.RightApple => true,

                // 允许其他功能键
                KeyCode.Menu or KeyCode.Numlock or KeyCode.ScrollLock or KeyCode.Print or KeyCode.SysReq => true,

                // 其余键不允许（包括特殊输入法键、保留键等）
                _ => false
            };
        }

        protected override KeyCode CoerceValue(KeyCode value)
        {
            if (!Validate(value))
            {
                return DefaultValue;
            }
            return value;
        }
    }
}

