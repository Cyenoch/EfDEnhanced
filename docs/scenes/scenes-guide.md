# Escape from Duckov - Game Scenes Guide

This guide documents the game's scene structure and level organization based on extracted Unity assets.

## Overview

The game uses Unity's scene system to organize levels, with each major area consisting of one or more scene files. Scenes contain lighting settings, object placements, and environmental data.

## Scene Organization

Scenes are located in `extracted_assets/Assets/Scenes/` and organized by level/area.

### Scene Types

1. **Main Menu** - Entry point and UI
2. **Prologue** - Tutorial/introduction
3. **Guide Levels** - Early game progression
4. **Main Game Areas** - Primary gameplay locations
5. **Cutscenes** - Story/narrative sequences

## Complete Level List

### 1. MainMenu
**Path**: `Scenes/MainMenu/MainMenu/`

- Main menu interface
- Contains lightmap textures (MainMenu_LM*.png)
- Entry point for game

**Assets**:
- 6 lightmap textures for menu lighting
- UI and background elements

---

### 2. Prologue
**Path**: `Scenes/Prologue/Prologue_1/`

- Tutorial/introduction level
- Player's first experience with game mechanics

**Configuration**:
- Lighting: `New Lighting Settings 1.json`

**Mod Considerations**:
- Ideal for testing basic player mechanics
- Relatively small, controlled environment
- Good for tutorial modifications

---

### 3. Level_Guide (Guide Levels)
**Paths**: 
- `Scenes/Level_Guide/Level_Guide_1/`
- `Scenes/Level_Guide/Level_Guide_2/`

Early game levels that guide players through core mechanics.

**Configuration**:
- Each has dedicated `LightingSettings.json`

**Structure**:
- Two sequential areas
- Introduces gameplay systems progressively

---

### 4. Level_DemoChallenge
**Path**: `Scenes/Level_DemoChallenge/Level_DemoChallenge_Main/`

Demo or challenge-focused level.

**Configuration**:
- `LightingSettings.json`
- `LightProbes.json` - Dynamic lighting probes

**Features**:
- Light probe system for dynamic lighting
- Likely focused on specific gameplay challenges

---

### 5. Level_JLab (Laboratory Complex)
**Paths**:
- `Scenes/Level_JLab/Level_JLab_Main/` - Main laboratory
- `Scenes/Level_JLab/Level_JLab_2/` - Secondary area

**Configuration**:
- JLab_Main: `LightingSettings.json`
- JLab_2: `LightProbes.json`

**Structure**:
- Two-part facility
- Indoor/research environment
- Connected areas with different lighting setups

**Mod Potential**:
- Laboratory experiments
- Item spawning
- Scientific equipment interactions

---

### 6. Level_OpenWorldTest (Farm Areas)
**Paths**:
- `Scenes/Level_OpenWorldTest/Level_Farm_Main/`
- `Scenes/Level_OpenWorldTest/Level_Farm_JLab_Facility/`

Open world test areas with farm setting.

**Configuration**:
- Farm_Main: `LightingSettings.json`
- Farm_JLab_Facility: `LightProbes.json`

**Features**:
- Open outdoor environment
- Connected to JLab facility
- Likely outdoor/indoor transition area

**Terrain**:
- `TerrainData/TR_Farm.glb`
- Outdoor landscape with farm structures

---

### 7. Level_GroundZero (Main Mission Area)
**Paths**:
- `Scenes/Level_GroundZero/Level_GroundZero_1/`
- `Scenes/Level_GroundZero/Level_GroundZero_Cave/`

Major gameplay area with surface and underground sections.

**Configuration**:
- Both areas have `LightingSettings.json`

**Structure**:
- Two-part level: surface and cave
- Significant story/gameplay location

**Terrain**:
- `TerrainData/Terrain_GroundZero.glb` - Surface terrain
- `TerrainData/Terrain_GrounZero_Cave.glb` - Cave system
- `TerrainData/Terrain_GroundZero_Cutscene.glb` - Cutscene-specific terrain

**Mod Potential**:
- Large explorable area
- Surface/underground transitions
- Environmental storytelling

---

### 8. Level_StormZone (Multi-Floor Complex)
**Paths**:
- `Scenes/Level_StormZone/Level_StormZone_1/` - Main entrance
- `Scenes/Level_StormZone/Level_StormZone_B0/` - Basement level 0
- `Scenes/Level_StormZone/Level_StormZone_B1/` - Basement level 1
- `Scenes/Level_StormZone/Level_StormZone_B2/` - Basement level 2
- `Scenes/Level_StormZone/Level_StormZone_B3/` - Basement level 3
- `Scenes/Level_StormZone/Level_StormZone_B4/` - Basement level 4

**Configuration**:
- All areas have individual `LightingSettings.json`

**Structure**:
- Large vertical complex with 6 distinct floors
- Storm-themed environment
- Progressive descent through basement levels

**Terrain**:
- `TerrainData/Terrain_StormZone.glb`

**Mod Potential**:
- Multi-level exploration
- Progressive difficulty through floors
- Complex navigation systems

---

### 9. Level_HiddenWarehouse
**Path**: `Scenes/Level_HiddenWarehouse/Level_HiddenWarehouse_Main/`

Secret or hidden location.

**Configuration**:
- `LightingSettings.json`

**Terrain**:
- `TerrainData/TR_HiddenWarehouse.glb`

**Features**:
- Likely secret/bonus area
- Warehouse-themed environment

---

### 10. Base Scene
**Path**: `Scenes/Base/`

**Configuration**:
- `LightingSettings.json`

**Purpose**:
- Possibly base/hub area
- Or shared scene template

---

### 11. LevelCutScene
**Path**: `Scenes/LevelCutScene/Cutscene_Ending/`

**Configuration**:
- `New Lighting Settings.json`
- `LightProbes.json`

**Purpose**:
- Ending cutscene
- Story conclusion

## Scene Assets Reference

### Lighting Settings
Each scene has `LightingSettings.json` defining:
- Ambient lighting
- Light baking parameters
- Reflection settings
- Fog and atmospheric effects

### Light Probes
Some scenes include `LightProbes.json`:
- Dynamic lighting information
- Real-time indirect lighting
- Used in scenes with moving objects/characters

### Terrain Data
Large outdoor areas use terrain meshes:
- `Terrain_GroundZero.glb` - Ground Zero surface
- `Terrain_StormZone.glb` - Storm Zone area
- `TR_Farm.glb` - Farm area
- `TR_HiddenWarehouse.glb` - Warehouse area
- Multiple terrain variants for different areas

## Level Progression Flow

Based on scene names and structure:

```
MainMenu
    ↓
Prologue_1 (Tutorial)
    ↓
Level_Guide_1 → Level_Guide_2 (Early game)
    ↓
Level_DemoChallenge (Optional?)
    ↓
[Open World Exploration]
    ├─ Level_OpenWorldTest (Farm areas)
    ├─ Level_JLab (Laboratory)
    ├─ Level_GroundZero (Main area)
    ├─ Level_StormZone (B0→B1→B2→B3→B4)
    └─ Level_HiddenWarehouse (Secret)
    ↓
Cutscene_Ending
```

## Modding Considerations

### Testing Locations

**Small/Controlled Environments**:
- Prologue_1 - Tutorial area, good for basic testing
- Level_Guide levels - Simple, focused areas

**Large Open Areas**:
- Level_OpenWorldTest - Open world testing
- Level_GroundZero - Large explorable area

**Complex Multi-Area**:
- Level_StormZone - Multi-floor structure
- Level_JLab - Connected facility areas

### Scene Loading

When modding scene-related features:
- Unity uses `SceneManager` for scene loading
- Additive loading allows multiple scenes active
- Scene transitions may load/unload scenes dynamically

### Finding Scene References

Use grep to search extracted assets:

```bash
# Find lighting settings
grep -r "LightingSettings" extracted_assets/Assets/Scenes/

# Find specific scene configuration
grep -r "StormZone" extracted_assets/Assets/Scenes/

# Search for terrain references
grep -r "TerrainData" extracted_assets/
```

### Runtime Scene Access

In mod code, access scenes via Unity's Scene API:

```csharp
using UnityEngine.SceneManagement;

// Get active scene
Scene currentScene = SceneManager.GetActiveScene();
string sceneName = currentScene.name;

// Scene loaded event
SceneManager.sceneLoaded += OnSceneLoaded;

void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    ModLogger.Log("Scene", $"Loaded: {scene.name}");
    
    // Scene-specific mod logic
    if (scene.name == "Level_StormZone_B0")
    {
        // Apply B0-specific modifications
    }
}
```

### Terrain Modifications

Terrain systems in outdoor levels:

```csharp
// Find terrain in scene
Terrain terrain = Terrain.activeTerrain;
if (terrain != null)
{
    TerrainData data = terrain.terrainData;
    // Modify terrain as needed
}
```

### Lighting Modifications

Modify scene lighting at runtime:

```csharp
// Adjust ambient lighting
RenderSettings.ambientLight = Color.white;
RenderSettings.ambientIntensity = 1.0f;

// Modify fog
RenderSettings.fog = true;
RenderSettings.fogColor = Color.gray;
RenderSettings.fogDensity = 0.01f;
```

## Scene Asset Statistics

| Asset Type | Count | Location |
|------------|-------|----------|
| Scene Configurations | 21+ | `Scenes/*/` |
| Lighting Settings | 16 | Various scenes |
| Light Probes | 5 | Dynamic lighting scenes |
| Terrain Meshes | 13 | `TerrainData/` |
| Terrain Layers | 14 | `TerrainLayer/` |
| Lightmap Textures | 6 | MainMenu only |

## Related Documentation

- **[assets-guide.md](../assets/assets-guide.md)** - Complete asset system documentation
- **[mod-development-guide.md](../assets/mod-development-guide.md)** - Mod development best practices
- **[CLAUDE.md](../../CLAUDE.md)** - Main project documentation

## Search Examples

Find scene-specific assets:

```bash
# Light probes in specific scene
grep -r "LightProbes" extracted_assets/Assets/Scenes/Level_JLab/

# All Storm Zone configurations
grep -r "StormZone" extracted_assets/Assets/Scenes/

# Terrain layer references
grep -r "TerrainLayer" extracted_assets/Assets/TerrainLayer/
```

## Notes

- Scene files themselves are binary Unity assets (not in extracted_assets)
- Extracted data shows scene configuration (lighting, probes)
- Actual scene hierarchy requires runtime inspection or full YAML export
- Use UnityExplorer mod for runtime scene inspection
- Lighting settings impact performance and visual quality

---

**Last Updated**: Based on extracted assets structure
**For More Information**: See main documentation and use grep to explore specific scenes

