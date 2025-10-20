# Escape from Duckov - Assets System Guide

Complete guide to Unity assets extracted from "Escape from Duckov" for mod development reference.

## Overview

The `extracted_assets/Assets/` directory contains exported Unity assets from the game, organized by asset type. These assets provide insight into game structure, art assets, and configuration for mod development.

## Asset Directory Structure

```
extracted_assets/Assets/
├── Scenes/              # Level configurations (21 files)
├── Mesh/                # 3D models (2762 GLB files)
├── Texture2D/           # Image textures (3096 PNG files)
├── Sprite/              # UI sprites (1028 JSON files)
├── PrefabHierarchyObject/ # Prefab definitions (2195 GLB files)
├── TerrainData/         # Terrain meshes (13 GLB files)
├── TerrainLayer/        # Terrain materials (14 JSON files)
├── Resources/           # Runtime loadable assets (324 files)
├── Cubemap/             # Environment maps (50+ PNG files)
├── Shader/              # Shader definitions (171 JSON files)
├── Font/                # Text fonts (12 TTF/OTF files)
├── PhysicMaterial/      # Physics properties (9 JSON files)
├── LightingSettings/    # Global lighting (3 JSON files)
├── LightProbes/         # Dynamic lighting (6 JSON files)
├── OcclusionCullingData/ # Optimization data (3 JSON files)
└── [Settings & Config files]
```

## Asset Categories

### 1. Visual Assets

#### Meshes (3D Models)
**Location**: `extracted_assets/Assets/Mesh/`  
**Count**: 2762 files  
**Format**: GLB (binary glTF)

**Contents**:
- Character models
- Weapon models
- Environmental objects
- Building structures
- Item models
- Vehicle/equipment meshes

**Usage in Mods**:
```csharp
// Reference mesh assets by name in Unity
// Meshes are typically assigned to MeshFilter components
GameObject obj = new GameObject("CustomObject");
MeshFilter filter = obj.AddComponent<MeshFilter>();
// Load mesh from Resources or AssetBundle
```

**Finding Meshes**:
```bash
# Search for specific mesh names
grep -r "weapon" extracted_assets/Assets/Mesh/ -i

# Count mesh files
ls extracted_assets/Assets/Mesh/*.glb | wc -l
```

---

#### Textures (2D Images)
**Location**: `extracted_assets/Assets/Texture2D/`  
**Count**: 3096 files  
**Format**: PNG

**Contents**:
- Material textures (diffuse, normal, metallic, etc.)
- UI textures
- Character textures
- Environment textures
- Effect textures
- Icon images

**Common Texture Naming**:
- `*_albedo` - Base color/diffuse maps
- `*_normal` - Normal maps for surface detail
- `*_metallic` - Metallic/smoothness maps
- `*_AO` - Ambient occlusion
- `icon_*` - UI icons
- `UI_*` - Interface textures

**Usage**:
```csharp
// Load texture at runtime
Texture2D texture = Resources.Load<Texture2D>("TextureName");

// Apply to material
Material material = renderer.material;
material.mainTexture = texture;
```

---

#### Sprites (2D Graphics)
**Location**: `extracted_assets/Assets/Sprite/`  
**Count**: 1028 files  
**Format**: JSON definitions

**Contents**:
- UI elements
- Icons
- Mini-map graphics
- HUD elements
- Menu graphics

**Notable Sprites**:
- `MiniMap_*.json` - Mini-map images for all levels
- `pictoicon_exit.json` - Exit/door icon
- `skull.json` - Death/danger indicator
- `Tex_skill_*.json` - Skill icons
- `DropdownArrow.json` - UI dropdown element

**Mini-Map Sprites**:
```
MiniMap_Base_SceneV2.json           - Base map
MiniMap_Level_DemoChallenge_1.json  - Demo area
MiniMap_Level_Farm_01.json          - Farm map
MiniMap_Level_GroundZero_1.json     - Ground Zero
MiniMap_Level_Guide_1.json          - Guide level 1
MiniMap_Level_Guide_2.json          - Guide level 2
MiniMap_Level_HiddenWarehouse.json  - Warehouse
MiniMap_Level_JLab_1.json           - JLab main
MiniMap_Level_JLab_2.json           - JLab 2
MiniMap_Level_StormZone_1.json      - Storm Zone entrance
MiniMap_Level_StormZone_B0-B4.json  - Storm Zone basements
MiniMap_Prologue_1.json             - Tutorial map
```

**Mod Applications**:
- Custom UI elements
- Mini-map modifications
- Icon replacements
- HUD customization

---

#### Cubemaps (Environment Maps)
**Location**: `extracted_assets/Assets/Cubemap/`  
**Count**: 50+ files  
**Format**: PNG (cubemap faces)

**Contents**:
- Skybox textures
- Reflection probes
- Environment lighting
- HDR environment maps

**Key Cubemaps**:
- `HDR_Deserts_01.png` - Desert environment
- `rosendal_mountain_midmorning_1k.png` - Mountain skybox
- `ReflectionProbe-*.png` - Reflection probes (numbered 0-10)
- Scene-specific skyboxes

**Usage**:
```csharp
// Cubemaps used for reflections and skyboxes
RenderSettings.skybox = skyboxMaterial;
RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;
```

---

### 2. Prefabs & Scene Objects

#### Prefab Hierarchy Objects
**Location**: `extracted_assets/Assets/PrefabHierarchyObject/`  
**Count**: 2195 files  
**Format**: GLB (hierarchy data)

**Contents**:
- Character prefabs
- Item prefabs
- Enemy/NPC prefabs
- Interactive objects
- Environmental props
- Effect prefabs

**Purpose**:
- Reusable game objects
- Pre-configured components
- Spawnable entities
- Complex object hierarchies

**Mod Usage**:
```csharp
// Instantiate prefabs at runtime
// Usually loaded via Resources or AssetBundles
GameObject prefab = Resources.Load<GameObject>("PrefabName");
GameObject instance = Instantiate(prefab, position, rotation);
```

---

#### Scene Hierarchy Objects
**Location**: `extracted_assets/Assets/SceneHierarchyObject/`  
**Count**: 46 files  
**Format**: GLB

**Contents**:
- Scene-specific object hierarchies
- Static scene elements
- Level structure data

---

### 3. Terrain System

#### Terrain Data
**Location**: `extracted_assets/Assets/TerrainData/`  
**Count**: 13 files  
**Format**: GLB

**Terrain Files**:
```
Terrain_DemoChallenge_1.glb          - Demo challenge area
Terrain_GroundZero.glb               - Ground Zero surface
Terrain_GroundZero_Cutscene.glb      - Cutscene variant
Terrain_GrounZero_Cave.glb           - Cave system
Terrain_Guide_1.glb                  - Guide level 1
Terrain_Guide_2.glb                  - Guide level 2
Terrain_StormZone.glb                - Storm Zone
TerrainDream.glb                     - Dream sequence?
TR_Farm.glb                          - Farm area
TR_HiddenWarehouse.glb               - Warehouse area
```

**Contents**:
- Heightmap data
- Terrain mesh geometry
- Splat map references
- Detail object placement

---

#### Terrain Layers
**Location**: `extracted_assets/Assets/TerrainLayer/`  
**Count**: 14 files  
**Format**: JSON

**Layer Types**:
```
TL_Asphalt_01.json      - Road surfaces
TL_Asphalt_02.json      - Alternate asphalt
TL_Ceramic_02.json      - Ceramic/tile surfaces
TL_DreamWhite.json      - Dream sequence terrain
TL_DryMud_02.json       - Dry mud/dirt
TL_Grass_StormZone.json - Storm Zone grass
TL_Mud.json             - Wet mud
TL_Snow.json            - Snow coverage
TL_StormZone.json       - Storm Zone surface
NewLayer.json           - Default/test layer
```

**Purpose**:
- Define terrain surface textures
- Control texture tiling and offsets
- Set terrain material properties

**Mod Applications**:
- Terrain visual modifications
- Surface type changes
- Custom terrain textures

---

### 4. Physics & Materials

#### Physic Materials
**Location**: `extracted_assets/Assets/PhysicMaterial/`  
**Count**: 9 files  
**Format**: JSON

**Materials**:
```
Character.json          - Character physics
DropItem.json          - Dropped item behavior
Grenade.json           - Grenade physics
BasketBallPhy.json     - Basketball (easter egg?)
PhyElectonics.json     - Electronics
PhyFabric.json         - Cloth/fabric
PhyGlass.json          - Glass surfaces
PhyMetal.json          - Metal objects
PhyPlastic.json        - Plastic materials
```

**Properties**:
- Friction coefficients
- Bounciness/restitution
- Collision behaviors

**Usage**:
```csharp
// Apply physics material to collider
PhysicMaterial physicMat = Resources.Load<PhysicMaterial>("PhyMetal");
collider.material = physicMat;
```

---

### 5. Shaders & Rendering

#### Shaders
**Location**: `extracted_assets/Assets/Shader/`  
**Count**: 171 files  
**Format**: JSON

**Notable Shaders**:
- `CloudShadowRT.json` - Cloud shadows
- `DarkRoom.json` - Dark room effect
- Custom material shaders
- Effect shaders

**Shader Types**:
- Surface shaders (PBR materials)
- Effect shaders (particles, post-processing)
- UI shaders
- Custom rendering shaders

---

#### Custom Render Textures
**Location**: `extracted_assets/Assets/CustomRenderTexture/`  
**Files**: `CloudShadowRT.json`

**Purpose**:
- Runtime texture generation
- Dynamic effects
- Cloud shadow projection

---

#### Render Textures
**Location**: `extracted_assets/Assets/RenderTexture/`  
**Files**: `GamingConsoleRT.json`

**Purpose**:
- In-game screen rendering
- Camera to texture rendering
- UI display surfaces

---

### 6. Lighting System

#### Lighting Settings
**Location**: `extracted_assets/Assets/LightingSettings/`  
**Count**: 3 files  
**Format**: JSON

**Files**:
- `demoSettings.json` - Demo lighting
- `New Lighting Settings.json` - Default settings
- Scene-specific settings in Scenes/ directories

**Properties**:
- Lightmap baking settings
- Ambient lighting
- Reflection settings
- Light probe settings

---

#### Light Probes
**Location**: `extracted_assets/Assets/LightProbes/`  
**Count**: 6 files  
**Format**: JSON

**Files**:
- `LightProbes.json` - Default probe data
- `LightProbes_0.json` through `LightProbes_4.json` - Variants

**Purpose**:
- Dynamic object lighting
- Real-time indirect lighting
- Character/object light sampling

---

### 7. UI & Text

#### Fonts
**Location**: `extracted_assets/Assets/Font/`  
**Count**: 12 files  
**Format**: TTF, OTF

**Font Files**:
```
fusion-pixel-12px-monospaced.ttf    - Pixel font
Inconsolata-SemiBold.ttf            - Monospace code font
Jua-Regular.ttf                     - Rounded font
LiberationSans.ttf                  - Sans-serif
Orbitron-Bold.ttf                   - Sci-fi font
PerfectDOSVGA437.ttf                - DOS-style font
PressStart2P-Regular.ttf            - Retro game font
ResourceHanRoundedCN-Medium.ttf     - Chinese font
SourceHanSansCN-Heavy.otf           - Chinese sans-serif
UnifontExMono.ttf                   - Unicode monospace
```

**Languages Supported**:
- English
- Chinese (Simplified & Traditional)
- Japanese

---

### 8. Resources Folder

#### Runtime Resources
**Location**: `extracted_assets/Assets/Resources/`  
**Count**: 324 files  
**Formats**: JSON, PNG, BYTES, GLB, TTF

**Contents**:
- Localization data folders:
  - `chinesesimplified/`
  - `chinesetraditional/`
  - `english/`
  - `japanese/`
- Shape primitives (sphere, cube, cylinder, etc.)
- Line rendering resources (Aline library)
- Procedural UI elements
- Shader resources
- Various Unity packages:
  - Easy Performant Outline
  - SSR (Screen Space Reflections)
  - RadiantGI
  - Umbra occlusion

**Special Files**:
- `Quest_Proxy.png` - Quest system proxy texture
- `DustParticles.png` - Particle effects
- `True Shadow Blue Noise.png` - Shadow dithering
- Line breaking character data (Chinese/Japanese text)

---

### 9. Configuration Assets

#### Build Settings
**Location**: `extracted_assets/Assets/BuildSettings/`  
**File**: `BuildSettings.json`

**Purpose**:
- Build configuration
- Scene list
- Platform settings

---

#### Editor Settings
**Location**: `extracted_assets/Assets/EditorSettings/`  
**File**: `EditorSettings.json`

**Purpose**:
- Unity Editor configuration
- Asset serialization settings

---

#### Graphics Settings
**Location**: `extracted_assets/Assets/GraphicsSettings/`  
**File**: `GraphicsSettings.json`

**Purpose**:
- Rendering pipeline settings
- Quality tier definitions
- Shader stripping rules

---

#### Player Settings
**Location**: `extracted_assets/Assets/PlayerSettings/`  
**File**: `PlayerSettings.json`

**Purpose**:
- Application settings
- Platform-specific config
- Build versioning

---

#### Quality Settings
**Location**: `extracted_assets/Assets/QualitySettings/`  
**File**: `QualitySettings.json`

**Purpose**:
- Quality presets (Low, Medium, High, etc.)
- Graphics quality levels
- Performance settings

---

#### Input Manager
**Location**: `extracted_assets/Assets/InputManager/`  
**File**: `InputManager.json`

**Purpose**:
- Input axis definitions
- Button mappings
- Controller configurations

---

#### Physics Settings
**Locations**:
- `extracted_assets/Assets/PhysicsManager/` - 3D physics
- `extracted_assets/Assets/Physics2DSettings/` - 2D physics

**Purpose**:
- Gravity settings
- Collision layers
- Physics simulation parameters

---

#### Tag Manager
**Location**: `extracted_assets/Assets/TagManager/`  
**File**: `TagManager.json`

**Purpose**:
- GameObject tags
- Layer definitions
- Sorting layers

---

### 10. Occlusion & Optimization

#### Occlusion Culling Data
**Location**: `extracted_assets/Assets/OcclusionCullingData/`  
**Count**: 3 files  
**Format**: JSON

**Files**:
- `OcclusionCullingData.json`
- `OcclusionCullingData_0.json`
- `OcclusionCullingData_1.json`

**Purpose**:
- Pre-computed visibility data
- Rendering optimization
- Scene-specific culling

---

### 11. Misc Assets

#### Audio Manager
**Location**: `extracted_assets/Assets/AudioManager/`  
**File**: `AudioManager.json`

**Purpose**:
- Audio system configuration
- Global audio settings

---

#### Text Assets
**Location**: `extracted_assets/Assets/TextAsset/`  
**Count**: 11 files  
**Format**: BYTES

**Contents**:
- Configuration data
- Text-based data files
- Serialized data

---

#### Texture3D
**Location**: `extracted_assets/Assets/Texture3D/`  
**Count**: 3 files  
**Format**: PNG

**Purpose**:
- 3D textures for volumetric effects
- Lookup tables (LUTs)
- Volume data

---

## Asset Statistics Summary

| Asset Type | Count | Format | Primary Use |
|------------|-------|--------|-------------|
| Meshes | 2,762 | GLB | 3D models |
| Textures | 3,096 | PNG | Material textures |
| Sprites | 1,028 | JSON | UI graphics |
| Prefabs | 2,195 | GLB | Game objects |
| Shaders | 171 | JSON | Rendering |
| Scenes | 21+ | JSON | Level config |
| Terrain Data | 13 | GLB | Landscapes |
| Terrain Layers | 14 | JSON | Surface types |
| Fonts | 12 | TTF/OTF | Text rendering |
| Cubemaps | 50+ | PNG | Environment |

**Total Asset Files**: 9,000+ files

---

## Modding With Assets

### Asset Search Strategies

#### Find Specific Asset Types
```bash
# Find all terrain-related assets
grep -r "Terrain" extracted_assets/Assets/

# Find Storm Zone assets
grep -ri "stormzone" extracted_assets/Assets/

# Find mini-map sprites
grep -r "MiniMap" extracted_assets/Assets/Sprite/

# Find physics materials
ls extracted_assets/Assets/PhysicMaterial/
```

#### Search Asset Contents
```bash
# Search JSON files for specific properties
grep -r "metallic" extracted_assets/Assets/ --include="*.json"

# Find shader references
grep -r "Shader" extracted_assets/Assets/Shader/

# Find texture references
grep -r "albedo" extracted_assets/Assets/Texture2D/
```

### Runtime Asset Access

#### Loading Resources
```csharp
// Load from Resources folder
GameObject prefab = Resources.Load<GameObject>("PrefabName");
Texture2D texture = Resources.Load<Texture2D>("Textures/MyTexture");
AudioClip sound = Resources.Load<AudioClip>("Sounds/MySound");

// Check if resource exists
if (prefab != null)
{
    ModLogger.Log("Assets", "Loaded prefab successfully");
}
```

#### Finding Scene Assets
```csharp
// Find objects in current scene
GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

// Find by name
GameObject obj = GameObject.Find("ObjectName");

// Find by tag
GameObject[] tagged = GameObject.FindGameObjectsWithTag("Player");

// Find by component
MyComponent component = FindObjectOfType<MyComponent>();
```

#### Terrain Access
```csharp
// Get active terrain
Terrain terrain = Terrain.activeTerrain;
if (terrain != null)
{
    TerrainData data = terrain.terrainData;
    float height = data.size.y;
    int heightmapResolution = data.heightmapResolution;
    
    ModLogger.Log("Terrain", $"Size: {data.size}");
}

// Find all terrains in scene
Terrain[] terrains = FindObjectsOfType<Terrain>();
```

### Asset Modification Examples

#### Replace Texture
```csharp
// Find renderer and replace texture
Renderer renderer = gameObject.GetComponent<Renderer>();
if (renderer != null)
{
    Texture2D newTexture = Resources.Load<Texture2D>("MyCustomTexture");
    renderer.material.mainTexture = newTexture;
}
```

#### Modify Physics Material
```csharp
// Apply custom physics material
Collider collider = GetComponent<Collider>();
PhysicMaterial newMaterial = new PhysicMaterial();
newMaterial.dynamicFriction = 0.6f;
newMaterial.staticFriction = 0.6f;
newMaterial.bounciness = 0.0f;
collider.material = newMaterial;
```

#### Spawn Custom Prefab
```csharp
// Instantiate prefab at position
GameObject prefab = Resources.Load<GameObject>("MyPrefab");
if (prefab != null)
{
    Vector3 spawnPos = new Vector3(0, 1, 0);
    Quaternion rotation = Quaternion.identity;
    GameObject instance = Instantiate(prefab, spawnPos, rotation);
    ModLogger.Log("Spawn", $"Created {instance.name}");
}
```

---

## Best Practices

### Asset Search
1. **Use grep for targeted searches** - Don't read entire directories
2. **Search by asset name or type** - Be specific in queries
3. **Check JSON files for configuration** - Many assets have JSON definitions
4. **Reference scene structure** - Understand scene organization first

### Asset References in Code
1. **Use Resources.Load() for runtime loading** - Standard Unity pattern
2. **Check for null before using assets** - Assets may not exist
3. **Log asset operations** - Use ModLogger for debugging
4. **Cache loaded assets** - Don't reload repeatedly

### Performance
1. **Don't load too many assets at once** - Memory concerns
2. **Unload unused assets** - Call Resources.UnloadUnusedAssets()
3. **Use asset bundles for large mods** - Better than Resources
4. **Optimize texture sizes** - Consider memory usage

### Asset Discovery
1. **Start with scenes** - Understand level structure
2. **Examine prefabs** - See how objects are composed
3. **Check Resources folder** - Runtime-loadable assets
4. **Review terrain system** - For outdoor modifications

---

## Related Documentation

- **[scenes-guide.md](../scenes/scenes-guide.md)** - Game scenes and levels
- **[mod-development-guide.md](mod-development-guide.md)** - Mod development practices
- **[CLAUDE.md](../../CLAUDE.md)** - Main project documentation

---

## Tools & Utilities

### Asset Extraction
- **AssetRipper** - Extract Unity assets to readable formats
- **UnityExplorer** - Runtime asset inspection (as mod)
- **UABE (Unity Asset Bundle Extractor)** - Manual asset extraction

### Analysis Tools
- `grep` / `ripgrep` - Search asset contents
- `find` - Locate files by pattern
- Unity Editor - Full asset inspection (requires project setup)

---

**Note**: Asset files are references for mod development. Direct asset replacement requires AssetBundle creation or mod-specific loading mechanisms. Always use grep/search tools for targeted asset discovery rather than reading entire directories.

**Last Updated**: Based on extracted assets structure  
**Asset Count**: 9,000+ files across all categories

