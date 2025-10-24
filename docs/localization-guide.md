# EfDEnhanced 本地化指南

## 概述

EfDEnhanced 使用游戏内置的 `SodaCraft.Localizations.LocalizationManager` 系统实现多语言支持。

## 支持的语言

- **简体中文** (`SystemLanguage.ChineseSimplified`)
- **繁体中文** (`SystemLanguage.ChineseTraditional`) 
- **英语** (`SystemLanguage.English`)
- **日语** (`SystemLanguage.Japanese`)

## 架构设计

### LocalizationHelper

核心本地化辅助类位于 `Utils/LocalizationHelper.cs`，提供：

1. **初始化系统** - 在 Mod 加载时自动初始化
2. **语言切换监听** - 自动响应游戏语言切换
3. **文本覆盖** - 使用 `LocalizationManager.SetOverrideText()` 覆盖显示文本
4. **便捷方法** - 简化的API用于获取本地化文本

### 工作原理

```csharp
// 1. Mod启动时初始化
LocalizationHelper.Initialize();

// 2. 加载所有语言的翻译数据（存储在内存中）
LoadTranslations();

// 3. 根据当前语言应用翻译
ApplyTranslations(LocalizationManager.CurrentLanguage);

// 4. 监听语言切换事件
LocalizationManager.OnSetLanguage += OnLanguageChanged;
```

## 使用方法

### 获取本地化文本

```csharp
// 简单获取
string text = LocalizationHelper.Get("Warning_NoWeapon");

// 格式化文本（带参数）
string text = LocalizationHelper.GetFormatted("Warning_QuestItem", 
    itemName, currentCount, requiredCount, questName);
```

### 添加新的本地化键

在 `LocalizationHelper.cs` 的 `LoadTranslations()` 方法中添加：

```csharp
LocalizationData[SystemLanguage.ChineseSimplified] = new Dictionary<string, string>
{
    // 现有键...
    { "NewFeature_Title", "新功能标题" },
    { "NewFeature_Description", "新功能描述" },
};

LocalizationData[SystemLanguage.English] = new Dictionary<string, string>
{
    // 现有键...
    { "NewFeature_Title", "New Feature Title" },
    { "NewFeature_Description", "New Feature Description" },
};

// 其他语言...
```

## 现有本地化键

### Raid 检查系统

| 键名 | 简体中文 | 英语 |
|------|---------|------|
| `RaidCheck_Title` | Raid 准备检查 | Raid Preparation Check |
| `RaidCheck_AllClear` | 装备检查通过 | Equipment check passed |
| `RaidCheck_HasIssues` | 检测到以下问题：\n | The following issues detected:\n |
| `RaidCheck_Confirm` | 继续进入 | Continue Anyway |
| `RaidCheck_Cancel` | 返回准备 | Go Back |

### 警告信息

| 键名 | 简体中文 | 英语 |
|------|---------|------|
| `Warning_NoWeapon` | ⚠ 未携带枪支 | ⚠ No weapon equipped |
| `Warning_NoAmmo` | ⚠ 未携带弹药 | ⚠ No ammunition |
| `Warning_NoMedicine` | ⚠ 未携带药品 | ⚠ No medical supplies |
| `Warning_NoFood` | ⚠ 未携带食物 | ⚠ No food or drinks |
| `Warning_StormyWeather` | ⚠ 当前为风暴天气 | ⚠ Stormy weather conditions |
| `Warning_QuestItem` | ⚠ 任务物品不足: {0} ({1}/{2}) - {3} | ⚠ Quest item insufficient: {0} ({1}/{2}) - {3} |

## 技术细节

### 键名规范

- 使用前缀 `EfDEnhanced_` 避免与游戏原有键冲突
- 使用 PascalCase 命名
- 按功能分组（如 `RaidCheck_*`, `Warning_*`）

### 语言后备

如果当前语言没有翻译，系统会自动回退到英语：

```csharp
if (!LocalizationData.ContainsKey(language))
{
    language = SystemLanguage.English;
}
```

### 动态更新

当玩家在游戏设置中切换语言时，Mod 会自动：

1. 接收 `OnSetLanguage` 事件
2. 清除旧的文本覆盖
3. 应用新语言的翻译
4. 已显示的UI会通过游戏的 `TextLocalizor` 组件自动刷新

## UI 组件的动态本地化更新

### LocalizationHelper 公共事件

从最新版本开始，`LocalizationHelper` 提供了一个公共事件供 UI 组件订阅：

```csharp
// 订阅语言变更事件
LocalizationHelper.OnLanguageChanged += (newLanguage) =>
{
    // 在这里更新 UI 文本
    UpdateMyUIText();
};

// 取消订阅
LocalizationHelper.OnLanguageChanged -= UpdateMyUIText;
```

### UI 组件集成

以下 UI 组件已支持语言变更时的动态文本更新：

#### ModButton
```csharp
var button = new ModButton()
    .SetText(localizationKey); // 语言变更时自动更新文本

// 在 OnDestroy 时自动取消订阅
```

#### ModToggle
```csharp
var toggle = new ModToggle()
    .SetLabel(localizationKey); // 语言变更时自动更新标签
```

#### BaseSettingsItem 及其子类
所有设置项都支持语言变更时的文本刷新：

```csharp
// 自动处理标签和描述的本地化
var settingsItem = new YourSettingsItem();
settingsItem.Initialize(settingsEntry);
// 语言变更时自动刷新标签和描述文本
```

#### SectionHeaderItem
```csharp
var header = sectionObj.AddComponent<SectionHeaderItem>();
header.Initialize(localizationKey); // 语言变更时自动刷新
```

### 为自定义组件添加语言变更支持

如果你创建了自己的 UI 组件，可以这样添加语言变更支持：

```csharp
public class MyCustomComponent : MonoBehaviour
{
    private TextMeshProUGUI _myText;
    private string _localizationKey;
    private Action<SystemLanguage>? _languageChangeHandler;

    public void Initialize(string localizationKey)
    {
        _localizationKey = localizationKey;
        
        // 初始化文本
        _myText.text = LocalizationHelper.Get(localizationKey);
        
        // 订阅语言变更事件
        _languageChangeHandler = OnLanguageChanged;
        LocalizationHelper.OnLanguageChanged += _languageChangeHandler;
    }

    private void OnLanguageChanged(SystemLanguage newLanguage)
    {
        if (_myText != null && !string.IsNullOrEmpty(_localizationKey))
        {
            _myText.text = LocalizationHelper.Get(_localizationKey);
        }
    }

    private void OnDestroy()
    {
        // 重要：取消订阅以防内存泄漏
        if (_languageChangeHandler != null)
        {
            LocalizationHelper.OnLanguageChanged -= _languageChangeHandler;
        }
    }
}
```

### 重要事项

1. **始终取消订阅** - 在 `OnDestroy()` 中取消语言变更事件的订阅，防止内存泄漏
2. **保存键值** - 保存本地化键以便在语言变更时重新查询
3. **错误处理** - 在事件处理中添加 try-catch 以确保稳定性

## 最佳实践

### 1. 统一管理

所有本地化文本都在 `LocalizationHelper.cs` 中集中管理，便于维护。

### 2. 格式化参数

对于动态内容，使用 C# 字符串格式化：

```csharp
// 定义
{ "Message_ItemCount", "你有 {0} 个 {1}" }

// 使用
LocalizationHelper.GetFormatted("Message_ItemCount", count, itemName);
```

### 3. 及时清理

在 Mod 卸载时清理覆盖的文本：

```csharp
void OnDestroy()
{
    LocalizationHelper.Cleanup();
}
```

### 4. 错误处理

所有本地化方法都包含错误处理，如果获取失败会返回键名：

```csharp
public static string Get(string key)
{
    return LocalizationManager.GetPlainText(GetFullKey(key));
    // 如果键不存在，返回 "*EfDEnhanced_KeyName*"
}
```

## 调试

查看本地化日志：

```
[EfDEnhanced] [Localization] Initializing localization system...
[EfDEnhanced] [Localization] Loaded translations for 4 languages
[EfDEnhanced] [Localization] Applied 11 translations for ChineseSimplified
[EfDEnhanced] [Localization] Language changed to: English
[EfDEnhanced] [Localization] Applied 11 translations for English
```

## 贡献翻译

如果你想为 Mod 贡献翻译：

1. Fork 项目
2. 在 `LocalizationHelper.LoadTranslations()` 中添加你的语言
3. 确保所有键都有对应的翻译
4. 测试语言切换功能
5. 提交 Pull Request

## 参考

- 游戏官方文档：`SodaCraft.Localizations.LocalizationManager`
- 源码位置：`extracted_assets/Scripts/SodaLocalization/`
- 实现示例：`Utils/LocalizationHelper.cs`

