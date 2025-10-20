# EfDEnhanced - Escape from Duckov Enhancement Mod

**Enhanced raid experience with pre-raid checks and in-raid quest tracking**

Never die from forgetting your meds, ammo, or quest items again! Track your active quests in real-time during raids with a clean, non-intrusive HUD overlay.

---

## 目前功能 | Current Features

### 🎯 任务追踪 | Quest Tracking

在任务面板中可以追踪任务，并在游戏中于右上角显示。

Quests can be tracked in the quest panel, and will be displayed in the upper right corner of the game.

**功能特性 | Features:**
- **选择性追踪** | Selective Tracking - 使用任务面板中的复选框选择要追踪的任务
- **局内显示** | In-Raid Display - 仅追踪的任务会显示在右上角
- **原生UI风格** | Native UI Style - 使用游戏原生任务图标实现无缝集成
- **进度追踪** | Progress Tracking - 显示每个任务的完成/总数（例如 "2/3"）
- **任务状态** | Task Status - ✓ 表示已完成，○ 表示待完成
- **持久化设置** | Persistent Settings - 追踪偏好会在会话间保存
- **自动显隐** | Auto Show/Hide - 仅在突袭时可见，基地中隐藏
- **多语言支持** | Multi-language - 完整支持中文、英文和日文

**使用方法 | How to use:**
1. 打开任务面板（Tab 键）| Open quest panel (Tab key)
2. 点击任意任务查看详情 | Click any quest to view details
3. 勾选任务标题下方的"局内追踪"复选框 | Check the "Track in Raid" checkbox below the quest title
4. 进入突袭后在右上角查看追踪的任务！| Enter raid and see your tracked quests in top-right corner!

---

### ✅ 传送台进图检查 | Pre-Raid Checks

使用传送台进入地图前检查：

Before entering a map using the teleporter, check:

- **武器检查** | Weapon - 是否携带了武器？| Are you carrying weapons?
- **弹药检查** | Ammunition - 是否携带了弹药？| Are you carrying ammunition?
- **药品检查** | Medicine - 是否携带了药品？| Are you carrying medicine?
- **食物检查** | Food/Drink - 是否携带了食物？| Are you carrying food?
- **天气检查** | Weather - 是否处于风暴天气？| Are you in a storm?
- **任务物品检查** | Quest Items - 是否携带了任务所需的物品？| Are you carrying the necessary items for the quest?
- **风暴预警** | Storm Warning - 是否有即将到来的风暴？| Is a storm approaching within 24 hours?

**智能警告对话框 | Smart Warning Dialog:**

如果检测到任何问题，会显示清晰的对话框：

If any issues are detected, a clear dialog shows you:
- 缺少的装备内容（带颜色标记）| What's missing from your loadout (color-coded)
- 需要的任务物品（带任务名称）| Which quest items you need (with quest names)
- 当前天气状况和风暴警告 | Current weather conditions and storm warnings
- 可选择继续或返回准备 | Option to continue anyway or go back to prepare

---

### 🔧 无缝集成 | Seamless Integration

- 适用于所有突袭入口（传送台、楼梯、下水道）| Works with all raid entry points
- 使用游戏原生物品检测系统以确保准确性 | Uses game's native item detection systems for accuracy
- 失效保护设计：出错时不会阻止你 | Fail-safe design: won't block you if something goes wrong
- 完全本地化UI（简体中文、繁体中文、英文、日文）| Fully localized UI in Chinese Simplified, Traditional, English, and Japanese

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
[EfDEnhanced] [RaidCheck] Found medicine: 急救包
[EfDEnhanced] [RaidCheck] Found food: 罐头
[EfDEnhanced] [RaidCheck] Current weather: Sunny, Safe: True
[EfDEnhanced] [QuestCheck] Found 2 active quests
[EfDEnhanced] [QuestCheck] ✓ Quest: 解救人质, Item: 钥匙卡, Required: 1, Current: 1
[EfDEnhanced] [RaidCheck] All checks passed, allowing entry
```

## Known Limitations

- Doesn't check if ammo matches your gun type
- Doesn't verify minimum quantities (just checks if you have any)
- Quest item checks only verify `RequiredItemID` (items you must bring)
- Doesn't check quest scene requirements (you might have items but wrong map)
- Doesn't check `SubmitItems` (items to find and turn in later)

## Technical Details

### Architecture

- **Built with**: .NET Standard 2.1, HarmonyLib 2.4.1
- **Game Framework**: Official Duckov.Modding API
- **Patches**: Runtime method interception on `MapSelectionView.NotifyEntryClicked`
- **UI**: Custom Unity UI integrated with game's view system

### Code Structure

```
EfDEnhanced/
├── ModBehaviour.cs           # Mod entry point and initialization
├── Patches/
│   └── RaidEntryPatches.cs  # Intercepts raid entry clicks
├── Features/
│   ├── ActiveQuestTracker.cs # In-raid quest tracker HUD (NEW!)
│   ├── RaidCheckDialog.cs    # Warning dialog UI
│   └── RaidPreparationView.cs # Preparation screen view
└── Utils/
    ├── RaidCheckUtility.cs   # Core check logic
    ├── LocalizationHelper.cs # Multi-language support
    └── ModLogger.cs          # Logging utilities
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

