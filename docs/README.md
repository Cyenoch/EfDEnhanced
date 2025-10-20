# EfDEnhanced Documentation

Complete documentation for "Escape from Duckov" mod development using the EfDEnhanced framework.

## Quick Links

### Getting Started
- **[Main Guide (CLAUDE.md)](../CLAUDE.md)** - Complete project overview and AI assistant guide
- **[Mod Development Guide](assets/mod-development-guide.md)** - Comprehensive mod development tutorial

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
└── scenes/
    └── scenes-guide.md                 # Game scenes and levels
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
**Maintained By**: EfDEnhanced Development Team

For the most up-to-date information, always refer to the latest version of this repository.

