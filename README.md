# EfDEnhanced - Escape from Duckov Enhancement Mod

**Enhanced raid experience with pre-raid checks and in-raid quest tracking**

Never die from forgetting your meds, ammo, or quest items again! Track your active quests in real-time during raids with a clean, non-intrusive HUD overlay.

---

## 目前功能 | Current Features

### 🎯 任务追踪 | Quest Tracking

在任务面板中可以追踪任务，并在游戏中于左上角显示。

Quests can be tracked in the quest panel, and will be displayed in the upper left corner of the game.

**功能特性 | Features:**
- **选择性追踪** | Selective Tracking - 使用任务面板中的复选框选择要追踪的任务
- **局内显示** | In-Raid Display - 仅追踪的任务会显示在左上角
- **原生UI风格** | Native UI Style - 使用游戏原生任务图标实现无缝集成
- **进度追踪** | Progress Tracking - 显示每个任务的完成/总数（例如 "2/3"）
- **任务状态** | Task Status - ✓ 表示已完成，○ 表示待完成
- **地图过滤** | Map Filtering - 自动只显示当前地图相关的任务
- **可调外观** | Customizable - 调整位置、缩放、是否显示描述
- **快捷键切换** | Hotkey Toggle - 按住设置的快捷键（默认 `.`）即可快速折叠/展开追踪器
- **智能帮助文本** | Smart Help Text - 首次使用时显示快捷键提示，使用后自动隐藏
- **持久化设置** | Persistent Settings - 追踪偏好会在会话间保存
- **自动显隐** | Auto Show/Hide - 仅在突袭时可见，打开菜单时隐藏
- **多语言支持** | Multi-language - 完整支持中文、英文和日文

**使用方法 | How to use:**
1. 打开任务面板（Tab 键）| Open quest panel (Tab key)
2. 点击任意任务查看详情 | Click any quest to view details
3. 勾选任务标题下方的"局内追踪"复选框 | Check the "Track in Raid" checkbox below the quest title
4. 进入突袭后在左上角查看追踪的任务！| Enter raid and see your tracked quests in top-left corner!
5. 按住快捷键（默认 `.` 键）快速折叠或展开追踪器 | Hold the hotkey (default `.`) to quickly collapse or expand the tracker!

---

### 🏃 移动增强 | Movement Enhancement

优化角色移动响应速度，告别"粘滞感"。

Optimize character movement response, say goodbye to "sticky" feeling.

**4档位可调 | 4 Adjustment Levels:**
- **禁用** | Disabled - 保持原版手感
- **轻度** | Light - 2倍加速/制动，1.5倍转向
- **中度** | Medium - 4倍加速/制动，2.5倍转向，支持瞬时方向切换
- **重度** | Heavy - 8倍加速/制动，4倍转向，极致响应速度

**特性 | Features:**
- 仅影响玩家角色，不影响NPC | Only affects player character, not NPCs
- 加速度和制动力同步提升 | Synchronized acceleration and braking enhancement
- 转向速度独立优化 | Independent turning speed optimization
- 保持垂直速度（跳跃/下落）| Preserves vertical velocity (jumping/falling)
- 实时切换无需重启 | Real-time switching without restart

**使用场景 | Use Cases:**
- 轻度：略微改善操作手感，接近原版体验
- 中度：显著提升响应速度，适合大多数玩家（推荐）
- 重度：极致灵敏操作，适合高手玩家和快节奏战斗

---

### 🎡 物品轮盘菜单 | Item Wheel Menu

快速访问物品栏的径向菜单，单手即可完成选择和使用操作。

Quick access radial menu for inventory items, designed for one-handed operation.

**特性 | Features:**
- **单手操作** | One-Handed - 按住热键 → 移动鼠标选择 → 释放热键使用
- **快速切换** | Fast Switching - 无需离开游戏视角即可使用物品
- **智能屏蔽** | Smart Blocking - 菜单打开时自动屏蔽射击和视角旋转，防止误操作
- **实时同步** | Real-time Sync - 显示当前物品栏实际绑定的物品图标
- **可自定义** | Customizable - 支持自定义热键和菜单缩放（0.5-2.0倍）
- **自动取消** | Auto Cancel - 打开其他界面或游戏暂停时自动取消

**使用方法 | How to use:**
1. **按住** 配置的热键（默认 `~` 键）打开菜单 | **Hold** the configured hotkey (default `~`) to open menu
2. **保持按住** 热键，移动鼠标选择物品 | **Keep holding** and move mouse to select item
3. **释放** 热键，自动使用或装备选中的物品 | **Release** to use or equip the selected item

**支持的物品类型 | Supported Item Types:**
- ✅ 消耗品（药品、食物等）| Consumables (medicine, food, etc.)
- ✅ 技能物品 | Skill items
- ✅ 可手持的物品和武器 | Hand-held items and weapons

**设置选项 | Settings:**
- 在 MOD 设置中可自定义热键 | Customize hotkey in MOD Settings
- 调整轮盘菜单的缩放大小 | Adjust menu scale size
- 详细文档：[物品轮盘菜单指南](docs/item-wheel-menu.md) | Detailed docs: [Item Wheel Menu Guide](docs/item-wheel-menu.md)

---

### 💣 投掷物轮盘菜单 | Throwable Wheel Menu

专门用于快速切换投掷物（手雷等）的径向菜单。

Dedicated radial menu for quick throwable (grenades, etc.) selection.

**特性 | Features:**
- **快速装备** | Quick Equip - 按住 G 键（可配置）→ 选择 → 释放装备
- **自动扫描** | Auto Scan - 自动检测背包中所有投掷物
- **智能堆叠** | Smart Stacking - 相同类型投掷物自动堆叠显示总数量
- **输入屏蔽** | Input Blocking - 菜单打开时自动屏蔽射击和视角旋转
- **可自定义** | Customizable - 支持自定义热键和菜单缩放
- **自动取消** | Auto Cancel - 打开其他界面或游戏暂停时自动取消

**使用方法 | How to use:**
1. **按住** G 键（可在设置中修改）打开菜单 | **Hold** G key (customizable) to open menu
2. **保持按住** 热键，移动鼠标选择投掷物 | **Keep holding** and move mouse to select throwable
3. **释放** 热键，自动装备选中的投掷物 | **Release** to equip the selected throwable

**支持的投掷物类型 | Supported Throwable Types:**
- ✅ 手雷（破片、震撼等）| Grenades (frag, stun, etc.)
- ✅ 闪光弹 | Flashbangs
- ✅ 烟雾弹 | Smoke grenades
- ✅ 所有投掷技能物品（Skill_Grenade 类型）| All throwable skill items (Skill_Grenade type)

**设置选项 | Settings:**
- 在 MOD 设置中可启用/禁用功能 | Enable/disable in MOD Settings
- 自定义热键（默认 G 键）| Customize hotkey (default G)
- 调整轮盘菜单的缩放大小（与物品轮盘共用）| Adjust menu scale size (shared with item wheel)
- 详细文档：[投掷物轮盘菜单指南](docs/throwable-wheel-menu.md) | Detailed docs: [Throwable Wheel Menu Guide](docs/throwable-wheel-menu.md)

---

### 🔫 武器对比 | Weapon Comparison

在库存中选中武器后，悬停到其他武器会自动显示属性对比。支持枪支和近战武器。

When a weapon is selected in inventory, hovering over other weapons shows automatic stat comparison. Supports both guns and melee weapons.

**特性 | Features:**
- **自动对比** | Automatic Comparison - 选中 + 悬停 = 自动显示差异
- **颜色标识** | Color Coded - 绿色表示更好，红色表示更差
- **智能判断** | Smart Analysis - 自动识别属性的正向性（伤害高好）和负向性（后坐力低好）
- **可配置** | Configurable - 可在设置中开关，默认启用
- **全属性支持** | All Stats - 支持所有武器属性
- **双武器类型** | Dual Weapon Types - 支持枪支（射速、后坐力等）和近战武器（攻击速度、攻击距离等）

**使用方法 | How to use:**
1. 左键点击选中一把武器 | Left-click to select a weapon
2. 鼠标悬停到另一把武器 | Hover over another weapon
3. 查看属性面板中的对比信息！| View comparison in the stats panel!

**显示格式 | Display Format:**
- `[选中武器值] → [悬停武器值]` | `[Selected Value] → [Hovered Value]`
- 绿色 Green: 悬停武器更好 | Hovered weapon is better
- 红色 Red: 选中武器更好 | Selected weapon is better
- 白色 White: 中性属性或相同值 | Neutral stat or equal values

---

### ✅ 传送前装备检查 | Pre-Raid Equipment Check

使用传送台进入地图前自动检查：

Automatically checks before entering a map via teleporter:

- **武器** | Weapon - 是否携带了枪支？| Are you carrying weapons?
- **弹药** | Ammunition - 是否携带了弹药？| Are you carrying ammunition?
- **弹药充足性** | Ammo Sufficiency - 每把枪的弹药是否足够一个弹匣？| Do you have at least one magazine worth of ammo for each gun?
- **医疗用品** | Medicine - 是否携带了急救包？| Are you carrying medical supplies?
- **食物饮水** | Food/Drink - 是否携带了食物和水？| Are you carrying food and water?
- **天气警告** | Weather - 当前是否为风暴天气？| Is it stormy weather?
- **风暴预警** | Storm Warning - 24小时内是否有风暴？| Is a storm approaching within 24 hours?
- **任务物品** | Quest Items - 是否携带了当前地图任务所需的物品？| Are you carrying quest items for this map?

**智能警告对话框 | Smart Warning Dialog:**

如果检测到任何问题，会显示清晰的对话框：

If any issues are detected, a clear dialog shows you:
- 缺少的装备内容（带颜色标记）| What's missing from your loadout (color-coded)
- 弹药不足的武器（显示当前数量/弹匣容量）| Weapons with low ammo (showing current/magazine capacity)
- 需要的任务物品（带任务名称）| Which quest items you need (with quest names)
- 当前天气状况和风暴警告 | Current weather conditions and storm warnings
- 可选择继续或返回准备 | Option to continue anyway or go back to prepare

**弹药充足性检查详情 | Ammo Sufficiency Check Details:**
- 自动检测所有携带的枪支（背包、装备栏、宠物）| Automatically detects all carried guns (inventory, equipment, pet)
- 按口径统计对应弹药总数 | Counts total ammo by caliber
- 包括装载在武器中的弹药 | Includes ammo loaded in weapons
- 如果某把枪的弹药总数 < 弹匣容量，则警告 | Warns if total ammo for a gun < magazine capacity
- 仅对枪支生效，不影响其他武器 | Only applies to guns, not other weapons

---

### 🎨 完整的设置面板 | Full Settings Panel

**在游戏设置菜单中添加"MOD 设置"标签页：**

**Adds "MOD Settings" tab to game settings menu:**

- **集成设计** | Integrated Design - 直接嵌入游戏设置界面，与游戏原生设置一致
- **传送前检查设置** | Pre-Raid Check Settings - 启用/禁用各项检查
- **移动增强设置** | Movement Enhancement Settings - 4档位调节移动手感
- **任务追踪器设置** | Quest Tracker Settings - 调整位置、大小、显示选项、地图过滤
- **追踪器快捷键** | Tracker Hotkey - 自定义快速切换追踪器的热键（默认 `.` 键）
- **界面增强设置** | UI Enhancement Settings - 武器对比等界面功能开关
- **按键绑定设置** | Keybinding Settings - 自定义模组功能的快捷键
- **一键重置** | Reset to Defaults - 恢复所有设置为默认值
- **实时生效** | Live Updates - 设置立即生效无需重启

**按键绑定特性 | Keybinding Features:**
- **可视化绑定** | Visual Binding - 点击按钮后按下任意键即可绑定
- **按键显示名** | Key Display Names - 友好的按键名称显示（如 "Space"、"F1"、"A (Gamepad)"）
- **手柄支持** | Gamepad Support - 支持 Xbox/PlayStation 等手柄按键（A/B/X/Y、LB/RB、Start/Back 等）
- **冲突避免** | Conflict Avoidance - 自动排除游戏保留按键（鼠标左右键）
- **验证系统** | Validation System - 允许绑定键盘、鼠标和手柄按键
- **ESC 取消** | ESC to Cancel - 按 ESC 键取消绑定操作

**访问方式 | How to Access:**
- 主菜单 → 设置 → MOD 设置标签页 | Main Menu → Settings → MOD Settings tab
- 暂停菜单 → 设置 → MOD 设置标签页 | Pause Menu (ESC) → Settings → MOD Settings tab

### 🌍 多语言支持 | Multi-Language Support

完整支持以下语言 | Fully supports the following languages:
- 🇨🇳 简体中文 | Simplified Chinese
- 🇹🇼 繁体中文 | Traditional Chinese
- 🇺🇸 English
- 🇯🇵 日本語 | Japanese

所有UI文本会根据游戏语言自动切换 | All UI text automatically switches based on game language

---

更多功能等我遇到忍不了的问题再说吧... 😄

More features will be added later when I encounter unbearable problems... 😄

## Installation

1. Download the latest release
2. Extract to `Duckov_Data/Mods/EfDEnhanced/` (or `Duckov.app/Contents/Mods/` on macOS)
3. Launch game - mod loads automatically

---

## 创意工坊注意事项 | Workshop Notes

### 防止介绍被覆盖 | Preventing Description Override

**此 Mod 已包含补丁，仅对自身防止上传时覆盖创意工坊介绍！**

**This mod includes a patch to prevent overwriting its own workshop description only!**

当你在游戏中上传 Mod 到创意工坊时，游戏默认会用 `info.ini` 中的 `description` 覆盖创意工坊页面的介绍。本 Mod 包含了自动补丁来防止 **EfDEnhanced 自身** 的创意工坊介绍被覆盖：

When uploading mods to the workshop in-game, the game normally overwrites your workshop page description with the `description` from `info.ini`. This mod includes automatic patches to prevent **EfDEnhanced's own** workshop description from being overwritten:

**仅对 EfDEnhanced 生效 | Only applies to EfDEnhanced:**
- ✅ **标题更新正常** | Title updates work normally
- ✅ **文件内容更新正常** | File content updates work normally  
- ✅ **预览图更新正常** | Preview image updates work normally
- ⛔ **介绍不会被覆盖** | Description will NOT be overwritten

**其他 Mod 不受影响 | Other mods are unaffected:**
- 如果你上传其他 Mod，描述仍会正常更新
- 补丁会通过 Workshop ID (3590346461) 和 Mod 名称识别 EfDEnhanced
- 其他 Mod 的上传行为保持游戏默认

- If you upload other mods, their descriptions will update normally
- The patch identifies EfDEnhanced by Workshop ID (3590346461) and mod name
- Other mods' upload behavior remains unchanged

这意味着你可以：
- 在创意工坊页面编写详细的 Markdown 介绍
- 在 `info.ini` 中保留简短的一句话描述
- 放心上传更新，不会丢失创意工坊上的详细介绍

This means you can:
- Write detailed Markdown descriptions on your workshop page
- Keep a short one-line description in `info.ini`
- Upload updates without losing your detailed workshop description

**注意**: `info.ini` 中的 `description` 仅用于游戏内 Mod 管理器显示，不会影响创意工坊页面。

**Note**: The `description` in `info.ini` is only used for the in-game mod manager, and will not affect your workshop page.

## How It Works

### Usage

The mod runs automatically - no configuration needed!

1. Equip your gear and select a raid map
2. Click on a map entry to enter
3. If everything is ready: you'll enter immediately
4. If something is missing: a warning dialog appears

**In the warning dialog:**
- Press **Confirm** (Enter/Space/Gamepad A) to continue anyway
- Press **Cancel** (ESC/Gamepad B) to go back and prepare

### Item Detection

The mod uses the game's official item detection APIs:

- **Guns**: Uses `item.GetBool("IsGun")` marker
- **Ammo**: Uses `item.GetBool("IsBullet")` marker  
- **Ammo Caliber**: Reads `Caliber` constant from gun and ammo items
- **Magazine Capacity**: Reads `Capacity` stat from `ItemSetting_Gun` component
- **Loaded Ammo**: Reads `BulletCount` from `ItemSetting_Gun` component
- **Medicine**: Checks for `Drug` behavior component
- **Food**: Checks for `FoodDrink` behavior component
- **Quest Items**: Reads `RequiredItemID` from active quests

This ensures compatibility with all vanilla items and most modded items that follow game conventions.

## Logging

View logs to troubleshoot or see what's being checked:

**Windows:**
```powershell
Get-Content "$env:USERPROFILE\AppData\LocalLow\TeamSoda\Duckov\Player.log" -Wait
```

**macOS:**
```bash
tail -f ~/Library/Logs/TeamSoda/Duckov/Player.log
```

**Or use the included script:**
```bash
./scripts/rlog.sh
```

**Example log output:**
```
[EfDEnhanced] [RaidCheck] Starting raid readiness check...
[EfDEnhanced] [RaidCheck] Found weapon: AK-47
[EfDEnhanced] [RaidCheck] Found ammo: 5.45x39 弹药
[EfDEnhanced] [AmmoCheck] Found 2 guns to check
[EfDEnhanced] [AmmoCheck] Checking gun: AK-47, Caliber: 7.62x39, Capacity: 30
[EfDEnhanced] [AmmoCheck] Gun AK-47: Found 25 ammo vs capacity 30
[EfDEnhanced] [AmmoCheck] LOW AMMO: AK-47 (7.62x39) - 25/30
[EfDEnhanced] [RaidCheck] Found medicine: 急救包
[EfDEnhanced] [RaidCheck] Found food: 罐头
[EfDEnhanced] [RaidCheck] Current weather: Sunny, Safe: True
[EfDEnhanced] [QuestCheck] Found 2 active quests
[EfDEnhanced] [QuestCheck] ✓ Quest: 解救人质, Item: 钥匙卡, Required: 1, Current: 1
[EfDEnhanced] [RaidCheck] Check complete with 1 low ammo warning
```

## Known Limitations

### Pre-Raid Check:
- Ammo sufficiency check warns only if total ammo < one magazine capacity (not two or more magazines)
- Doesn't check if you have the right ammo type loaded in the gun (only checks total ammo of matching caliber)
- Quest item checks only verify `RequiredItemID` (items you must bring)
- Doesn't check `SubmitItems` (items to find and turn in later)

### Movement Enhancement:
- Changes base game movement mechanics (can be disabled anytime)
- May feel too sensitive on higher levels for some players
- Not recommended for precise aiming scenarios (use lower levels)

## Technical Details

### Architecture

- **Built with**: .NET Standard 2.1, HarmonyLib 2.4.1
- **Game Framework**: Official Duckov.Modding API
- **Patches**: Runtime method interception on `MapSelectionView.NotifyEntryClicked`
- **UI**: Custom Unity UI integrated with game's view system

### Code Structure

```
EfDEnhanced/
├── ModBehaviour.cs              # Mod entry point and initialization
├── Patches/
│   ├── ItemHoveringComparisonPatch.cs # Weapon comparison system
│   ├── ItemWheelMenuPatch.cs   # Item wheel menu input handling
│   ├── ThrowableWheelMenuPatch.cs # Throwable wheel menu input handling
│   ├── MovementEnhancementPatch.cs # Movement system optimization
│   ├── OptionsPanelPatch.cs    # Adds MOD Settings tab to game settings
│   ├── QuestViewDetailsPatch.cs # Adds tracking checkbox to quest details
│   ├── RaidEntryPatches.cs     # Intercepts raid entry for checks
│   ├── ItemSelectionDebugPatch.cs # Item selection debug helper (dev)
│   └── WorkshopUploadPatch.cs  # Prevents workshop description override
├── Features/
│   ├── ActiveQuestTracker.cs   # In-raid quest tracker HUD
│   ├── ItemWheelMenu.cs        # Item wheel menu implementation
│   ├── ThrowableWheelMenu.cs   # Throwable wheel menu implementation
│   ├── ModSettingsContent.cs   # Settings content for OptionsPanel
│   └── RaidPreparationView.cs  # Pre-raid warning dialog
└── Utils/
    ├── LocalizationHelper.cs   # Multi-language support
    ├── ModLogger.cs            # Logging utilities
    ├── ModSettings.cs          # Centralized settings manager
    ├── QuestTrackingManager.cs # Quest tracking persistence
    ├── RaidCheckUtility.cs     # Pre-raid check logic (weapon, ammo, meds, food, weather, quests)
    ├── StatPolarityMap.cs      # Stat polarity for weapon comparison
    ├── Settings/               # Settings entry classes
    │   ├── BoolSettingsEntry.cs
    │   ├── FloatSettingsEntry.cs
    │   ├── IntSettingsEntry.cs
    │   ├── KeyCodeSettingsEntry.cs  # Keybinding settings
    │   ├── OptionsSettingsEntry.cs
    │   ├── StringSettingsEntry.cs
    │   └── SettingsEntry.cs
    └── UI/                     # Reusable UI components
        ├── Animations/
        │   └── UIAnimations.cs # UI animation utilities
        ├── Builders/
        │   └── SettingsBuilder.cs  # Automatic settings UI generation
        ├── Components/
        │   ├── ModButton.cs
        │   ├── ModKeybindingButton.cs  # Keybinding UI component
        │   ├── ModSlider.cs
        │   ├── ModToggle.cs
        │   ├── PieMenuComponent.cs # Reusable pie/radial menu component
        │   └── SettingsItems/      # Auto-generated UI for settings
        │       ├── BaseSettingsItem.cs
        │       ├── BoolSettingsItem.cs
        │       ├── RangedFloatSettingsItem.cs
        │       ├── IndexedOptionsSettingsItem.cs
        │       ├── KeyCodeSettingsItem.cs
        │       ├── SectionHeaderItem.cs
        │       └── SpacerItem.cs
        ├── Constants/
        │   ├── ColorPalette.cs
        │   └── UIConstants.cs
        └── Core/
            └── UIHelpers.cs    # UI utility functions
```

## Development

### Building from Source

**Requirements:**
- Escape from Duckov installed

**Build steps:**
```bash
dotnet restore
dotnet build -c Release
```

The mod automatically deploys to your game's Mods folder on build.

### Contributing

Found a bug or have a feature request? Please open an issue on GitHub!

When reporting bugs, include:
- Your game version
- Mod version
- Relevant log excerpts
- Steps to reproduce

## License

This mod is provided as-is for Escape from Duckov modding community.

## Credits

Built with:
- [HarmonyLib](https://github.com/pardeike/Harmony) - Runtime patching
- [UniTask](https://github.com/Cysharp/UniTask) - Async operations
- [TrueShadow](https://github.com/LeaTai/TrueShadow) - High-quality UI shadows
- TextMeshPro - Advanced text rendering
- Duckov Modding Framework - Official mod API

---

**Support the mod:** If you find this useful, please star the repository and share with your squadmates!

