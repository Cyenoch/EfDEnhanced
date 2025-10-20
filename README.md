# Raid Ready Check

**Pre-raid inspection system for Escape from Duckov**

Never die from forgetting your meds, ammo, or quest items again! This mod automatically checks your loadout before entering any raid and warns you if something is missing.

## Features

### Automatic Pre-Raid Checks

Before you enter a raid through teleport pads, stairs, or sewers, the mod checks:

- ✅ **Weapon** - Do you have at least one gun?
- ✅ **Ammunition** - Do you have bullets/magazines?
- ✅ **Medicine** - Do you have healing items?
- ✅ **Food/Drink** - Do you have sustenance?
- ✅ **Quest Items** - Do you have items required by active quests?
- ⚠️ **Weather** - Are you entering during a dangerous storm?
- ⚠️ **Storm Warning** - Is a storm approaching within 24 hours?

### Smart Warning Dialog

If any issues are detected, a clear dialog shows you:
- What's missing from your loadout (color-coded for visibility)
- Which quest items you need (with quest names)
- Current weather conditions and storm warnings
- Option to continue anyway or go back to prepare

**Warning Colors:**
- 🔴 **Red** - Critical issues (active storm, missing essential gear)
- 🟠 **Orange** - Important warnings (storm approaching)
- 🟡 **Gold** - Quest item reminders

### Seamless Integration

- Works with all raid entry points (teleport pads, stairs, sewers)
- Uses game's native item detection systems for accuracy
- Fail-safe design: won't block you if something goes wrong
- Fully localized UI in Chinese Simplified, Traditional, English, and Japanese

## Installation

1. Download the latest release
2. Extract to `Duckov_Data/Mods/EfDEnhanced/` (or `Duckov.app/Contents/Mods/` on macOS)
3. Launch game - mod loads automatically

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
- Duckov Modding Framework - Official mod API

---

**Support the mod:** If you find this useful, please star the repository and share with your squadmates!

