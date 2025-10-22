# Raid Ready Check Documentation

Complete documentation for "Escape from Duckov" mod development and the Raid Ready Check mod.

## Quick Links

### Getting Started
- **[Main Guide (CLAUDE.md)](../CLAUDE.md)** - Complete project overview and AI assistant guide
- **[Mod Development Guide](assets/mod-development-guide.md)** - Comprehensive mod development tutorial
- **[Main README](../README.md)** - User-facing mod documentation (English)

### Game Resources
- **[Scenes Guide](scenes/scenes-guide.md)** - All game levels and scene structure
- **[Assets Guide](assets/assets-guide.md)** - Complete Unity assets reference

### Development Resources
- **[Cursor Rules](.cursor/rules/)** - AI coding assistant rules for this project
- **[ModLogger Guide](logging-guide.md)** - Logging system usage (if exists)

## Documentation Structure

```
docs/
├── README.md                           # This file
├── assets/
│   ├── assets-guide.md                 # Complete asset system documentation
│   └── mod-development-guide.md        # Mod development best practices
├── scenes/
│   └── scenes-guide.md                 # Game scenes and levels
├── item-wheel-menu.md                  # Item wheel menu feature guide
├── keybinding-system.md                # Keybinding system documentation
├── localization-guide.md               # Localization system guide
├── quest-tracking-feature.md           # Quest tracking feature guide
└── weapon-comparison.md                # Weapon comparison feature guide
```

## Documentation Overview

### 1. [Mod Development Guide](assets/mod-development-guide.md)
**Complete tutorial for mod development**

Topics covered:
- Development environment setup
- Project structure and organization
- Harmony patching techniques
- Unity asset manipulation
- Scene handling
- Best practices and patterns
- Testing and debugging
- Troubleshooting common issues

**Who should read**: Anyone developing mods for Escape from Duckov

---

### 2. [Scenes Guide](scenes/scenes-guide.md)
**Game levels and scene structure**

Content:
- Complete list of all game scenes
- Level progression flow
- Scene configuration (lighting, terrain)
- Modding considerations per scene
- Runtime scene access methods

**Who should read**: Modders working with scenes, levels, or environment

---

### 3. [Assets Guide](assets/assets-guide.md)
**Unity assets system reference**

Content:
- Asset directory structure (9,000+ files)
- Visual assets (meshes, textures, sprites)
- Prefabs and scene objects
- Terrain system
- Physics materials
- Shaders and rendering
- Lighting system
- UI and fonts
- Resources folder
- Configuration assets

**Who should read**: Anyone working with game assets, textures, models, or resources

---

### 4. [Item Wheel Menu](item-wheel-menu.md)
**物品轮盘菜单功能指南**

Content:
- 快速访问物品栏的径向菜单
- 使用方法和操作说明
- 输入处理和屏蔽机制
- 技术实现细节
- 自定义设置

**Who should read**: 用户想了解如何使用物品轮盘，开发者想了解实现原理

---

### 5. [Keybinding System](keybinding-system.md)
**按键绑定系统文档**

Content:
- 自定义按键绑定系统
- KeyCode 设置条目
- UI 组件实现
- 事件系统

**Who should read**: 开发者需要实现可自定义热键功能

---

### 6. [Quest Tracking Feature](quest-tracking-feature.md)
**任务追踪功能文档**

Content:
- 游戏内任务追踪显示
- 实时任务状态更新
- UI 集成方式

**Who should read**: 开发者需要了解任务系统集成

---

### 7. [Weapon Comparison](weapon-comparison.md)
**武器对比功能文档**

Content:
- 物品对比显示
- 属性极性映射
- UI 实现

**Who should read**: 开发者需要实现物品对比功能

---

### 8. [Localization Guide](localization-guide.md)
**多语言支持指南**

Content:
- 本地化系统使用
- 多语言文本管理
- LocalizationHelper 工具

**Who should read**: 开发者需要添加多语言支持

---

## Quick Reference

### Common Tasks

#### Find a Game Class or Method
```bash
# Search decompiled code
grep -r "class QuestManager" decompiled/
grep -r "CompleteQuest" decompiled/QuestManager.cs
```

#### Find a Scene or Level
```bash
# Search extracted scenes
grep -r "StormZone" extracted_assets/Assets/Scenes/
```

#### Find an Asset
```bash
# Search by asset name
grep -ri "minimap" extracted_assets/Assets/Sprite/

# Find terrain data
ls extracted_assets/Assets/TerrainData/
```

#### Build and Deploy Mod
```bash
# Build
dotnet build

# Deploy to game
./scripts/deploy.sh

# Watch logs
./scripts/rlog.sh
```

### Development Workflow

1. **Plan Feature** - Read relevant documentation
2. **Search Code** - Use grep on decompiled/ for game code
3. **Create Patch** - Add Harmony patch in Patches/
4. **Build & Test** - Build, deploy, and test in-game
5. **Debug** - Use rlog.sh to monitor logs
6. **Iterate** - Refine based on testing

---

## Resource Locations

### Project Files
- **Source Code**: Root directory (`ModBehaviour.cs`, `Utils/`, `Patches/`)
- **Build Output**: `output/` directory
- **Documentation**: `docs/` directory
- **Scripts**: `scripts/` directory

### Game References
- **Decompiled Code**: `decompiled/` directory (if generated)
- **Extracted Assets**: `extracted_assets/Assets/`
- **Game Installation**: Path specified in `.csproj`

### Generated Files
- **Mod DLL**: `output/EfDEnhanced.dll`
- **Harmony**: `output/0Harmony.dll`
- **Config**: `info.ini`
- **Preview**: `preview.png`

---

## Documentation Guidelines

When adding new documentation:

1. **Use Markdown** - All docs in `.md` format
2. **Link Thoroughly** - Cross-reference related docs
3. **Code Examples** - Include working code samples
4. **Search Examples** - Show grep/search patterns
5. **Update README** - Add to this index
6. **English Language** - Keep docs in English for AI compatibility

---

## External Resources

### Official Unity Documentation
- [Unity Scripting Reference](https://docs.unity3d.com/ScriptReference/)
- [Unity Manual](https://docs.unity3d.com/Manual/)

### Harmony Library
- [Harmony Documentation](https://harmony.pardeike.net/)
- [Harmony GitHub](https://github.com/pardeike/Harmony)

### .NET Documentation
- [C# Programming Guide](https://docs.microsoft.com/en-us/dotnet/csharp/)
- [.NET Standard 2.1](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)

### Tools
- [dnSpy](https://github.com/dnSpy/dnSpy) - .NET debugger/decompiler
- [ILSpy](https://github.com/icsharpcode/ILSpy) - .NET decompiler
- [AssetRipper](https://github.com/AssetRipper/AssetRipper) - Unity asset extractor

---

## Contributing to Documentation

To improve or add documentation:

1. Follow existing structure and style
2. Include code examples where helpful
3. Add cross-references to related docs
4. Update this README index
5. Test all code samples
6. Use clear, concise language

---

## Getting Help

### Documentation Issues
- Check [CLAUDE.md](../CLAUDE.md) main guide
- Search existing documentation
- Review code examples in `Patches/`

### Development Issues
- Check [Troubleshooting](assets/mod-development-guide.md#troubleshooting) section
- Review logs with `./scripts/rlog.sh`
- Examine decompiled code in `decompiled/`

### Game Understanding
- Read [Scenes Guide](scenes/scenes-guide.md) for level structure
- Read [Assets Guide](assets/assets-guide.md) for asset system
- Search extracted assets for specific elements

---

## Document Versions

All documentation is based on:
- **Game Version**: Latest (check game for version)
- **Framework**: .NET Standard 2.1
- **Unity Version**: Check game's Unity version
- **Harmony Version**: 2.2.2

---

**Last Updated**: October 2025  
**Maintained By**: Raid Ready Check Development Team

For the most up-to-date information, always refer to the latest version of this repository.

