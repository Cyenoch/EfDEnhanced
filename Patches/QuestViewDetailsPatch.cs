using System;
using System.Collections.Generic;
using Duckov.Quests;
using Duckov.Quests.UI;
using EfDEnhanced.Utils;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EfDEnhanced.Patches;

/// <summary>
/// 为任务详情视图添加追踪按钮（Button版本）
/// </summary>
[HarmonyPatch(typeof(QuestViewDetails))]
public class QuestViewDetailsPatch
{
    // 为每个 QuestViewDetails 实例维护独立的按钮
    private static Dictionary<QuestViewDetails, ButtonData> _trackButtons = new Dictionary<QuestViewDetails, ButtonData>();
    private static Quest? _currentQuest;
    
    private class ButtonData
    {
        public GameObject ButtonObject;
        public Image StatusImage;
        public Sprite UncheckedSprite;
        public Sprite CheckedSprite;
        public bool UsingNativeSprites;
    }
    
    private class IconData
    {
        public Image StatusImage;
        public Sprite UncheckedSprite;
        public Sprite CheckedSprite;
        public bool UsingNativeSprites;
    }
    
    /// <summary>
    /// Patch Setup 方法来注入追踪按钮
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch("Setup")]
    private static void Setup_Postfix(QuestViewDetails __instance, Quest quest)
    {
        try
        {
            _currentQuest = quest;
            
            bool hasButton = _trackButtons.ContainsKey(__instance) && _trackButtons[__instance]?.ButtonObject != null;
            
            // 确保追踪UI已创建
            if (!hasButton)
            {
                CreateTrackButton(__instance);
            }
            
            // 更新按钮状态
            UpdateButtonState(__instance, quest);
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestViewDetailsPatch.Setup_Postfix failed: {ex}");
        }
    }
    
    /// <summary>
    /// 创建追踪按钮
    /// </summary>
    private static void CreateTrackButton(QuestViewDetails questViewDetails)
    {
        try
        {
            ModLogger.Log("QuestTracker", "Creating track button in QuestViewDetails");
            
            // 尝试获取游戏原生的任务图标
            Sprite uncheckedSprite = null;
            Sprite checkedSprite = null;
            
            var taskEntryPrefabField = typeof(QuestViewDetails).GetField("taskEntryPrefab", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (taskEntryPrefabField != null)
            {
                var taskEntryPrefab = taskEntryPrefabField.GetValue(questViewDetails) as TaskEntry;
                if (taskEntryPrefab != null)
                {
                    var unsatisfiedField = typeof(TaskEntry).GetField("unsatisfiedIcon", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var satisfiedField = typeof(TaskEntry).GetField("satisfiedIcon", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (unsatisfiedField != null) uncheckedSprite = unsatisfiedField.GetValue(taskEntryPrefab) as Sprite;
                    if (satisfiedField != null) checkedSprite = satisfiedField.GetValue(taskEntryPrefab) as Sprite;
                    
                    ModLogger.Log("QuestTracker", $"Got task sprites: unchecked={uncheckedSprite != null}, checked={checkedSprite != null}");
                }
            }
            
            // 使用反射获取 displayName 字段
            var displayNameField = typeof(QuestViewDetails).GetField("displayName", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (displayNameField == null)
            {
                ModLogger.LogError("QuestTracker: Failed to find displayName field");
                return;
            }
            
            var displayNameText = displayNameField.GetValue(questViewDetails) as TextMeshProUGUI;
            if (displayNameText == null)
            {
                ModLogger.LogError("QuestTracker: displayName is null");
                return;
            }
            
            // 获取 displayName 的父级
            Transform displayNameParent = displayNameText.transform.parent;
            if (displayNameParent == null)
            {
                ModLogger.LogError("QuestTracker: displayName parent is null");
                return;
            }
            
            ModLogger.Log("QuestTracker", $"Creating button below: {displayNameText.transform.name}");
            
            // 创建按钮容器（作为 displayName 的兄弟，显示在下方）
            GameObject buttonContainer = new GameObject("TrackQuestButton");
            buttonContainer.transform.SetParent(displayNameParent, false);
            
            // 设置为 displayName 的下一个兄弟（显示在下方）
            int displayNameIndex = displayNameText.transform.GetSiblingIndex();
            buttonContainer.transform.SetSiblingIndex(displayNameIndex + 1);
            
            RectTransform containerRect = buttonContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0, 1);
            containerRect.anchorMax = new Vector2(1, 1);
            containerRect.pivot = new Vector2(0, 1);
            containerRect.anchoredPosition = Vector2.zero;
            containerRect.sizeDelta = new Vector2(0, 40); // 增加高度到40
            
            // 添加水平布局
            HorizontalLayoutGroup layoutGroup = buttonContainer.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false; // 不强制扩展高度，保持图标原始大小
            layoutGroup.childControlHeight = false; // 不控制子元素高度
            layoutGroup.childAlignment = TextAnchor.MiddleLeft;
            layoutGroup.spacing = 12; // 增大图标和文字之间的间距
            layoutGroup.padding = new RectOffset(10, 10, 8, 8); // 增加内边距
            
            // 创建状态图标（使用游戏原生样式）
            GameObject iconObj = new GameObject("StatusIcon");
            iconObj.transform.SetParent(buttonContainer.transform, false);
            
            Image statusImage = iconObj.AddComponent<Image>();
            // 使用游戏原生图标，如果没有则使用简单方块
            statusImage.sprite = uncheckedSprite != null ? uncheckedSprite : CreateWhiteSquareSprite();
            statusImage.color = uncheckedSprite != null ? Color.white : new Color(0.6f, 0.6f, 0.6f, 1f);
            
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(32, 32); // 增大到32x32
            
            // 添加 LayoutElement 确保大小不被布局组改变
            LayoutElement iconLayout = iconObj.AddComponent<LayoutElement>();
            iconLayout.minWidth = 32;
            iconLayout.minHeight = 32;
            iconLayout.preferredWidth = 32;
            iconLayout.preferredHeight = 32;
            iconLayout.flexibleWidth = 0;
            iconLayout.flexibleHeight = 0;
            
            // 存储图标的引用和sprite
            var iconData = new IconData
            {
                StatusImage = statusImage,
                UncheckedSprite = uncheckedSprite != null ? uncheckedSprite : CreateWhiteSquareSprite(),
                CheckedSprite = checkedSprite != null ? checkedSprite : CreateWhiteSquareSprite(),
                UsingNativeSprites = uncheckedSprite != null && checkedSprite != null
            };
            
            // 创建文本标签
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(buttonContainer.transform, false);
            
            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = LocalizationHelper.Get("QuestTracker_CheckboxLabel");
            labelText.fontSize = 20;
            labelText.color = new Color(1f, 1f, 1f, 1f);
            labelText.alignment = TextAlignmentOptions.MidlineLeft;
            
            LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.flexibleWidth = 1;
            
            // 添加Button组件（整个容器可点击）
            Button button = buttonContainer.AddComponent<Button>();
            button.targetGraphic = statusImage;
            button.onClick.AddListener(() => OnTrackButtonClicked(questViewDetails));
            
            // 存储引用
            _trackButtons[questViewDetails] = new ButtonData
            {
                ButtonObject = buttonContainer,
                StatusImage = iconData.StatusImage,
                UncheckedSprite = iconData.UncheckedSprite,
                CheckedSprite = iconData.CheckedSprite,
                UsingNativeSprites = iconData.UsingNativeSprites
            };
            
            ModLogger.Log("QuestTracker", $"Track button created successfully (native sprites: {iconData.UsingNativeSprites})");
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestViewDetailsPatch.CreateTrackButton failed: {ex}");
        }
    }
    
    /// <summary>
    /// 创建白色方形 Sprite
    /// </summary>
    private static Sprite CreateWhiteSquareSprite()
    {
        Texture2D texture = new Texture2D(4, 4);
        Color32 white = new Color32(255, 255, 255, 255);
        
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                texture.SetPixel(x, y, white);
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
    }
    
    /// <summary>
    /// 更新按钮状态
    /// </summary>
    private static void UpdateButtonState(QuestViewDetails instance, Quest? quest)
    {
        try
        {
            if (!_trackButtons.ContainsKey(instance))
            {
                return;
            }
            
            var buttonData = _trackButtons[instance];
            if (buttonData == null || buttonData.ButtonObject == null)
            {
                return;
            }
            
            if (quest == null)
            {
                // 没有任务时隐藏按钮
                buttonData.ButtonObject.SetActive(false);
                return;
            }
            
            // 有任务时显示按钮
            buttonData.ButtonObject.SetActive(true);
            
            // 更新图标状态
            bool isTracked = QuestTrackingManager.IsQuestTracked(quest.ID);
            buttonData.StatusImage.sprite = isTracked ? buttonData.CheckedSprite : buttonData.UncheckedSprite;
            
            // 如果不使用原生sprite，用颜色区分
            if (!buttonData.UsingNativeSprites)
            {
                buttonData.StatusImage.color = isTracked ? new Color(0.3f, 1f, 0.3f, 1f) : new Color(0.6f, 0.6f, 0.6f, 1f);
            }
            
            ModLogger.Log("QuestTracker", $"Updated button for quest {quest.ID}: tracked={isTracked}");
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestViewDetailsPatch.UpdateButtonState failed: {ex}");
        }
    }
    
    /// <summary>
    /// 按钮点击事件
    /// </summary>
    private static void OnTrackButtonClicked(QuestViewDetails instance)
    {
        try
        {
            if (_currentQuest == null)
            {
                return;
            }
            
            // 切换追踪状态
            bool currentlyTracked = QuestTrackingManager.IsQuestTracked(_currentQuest.ID);
            QuestTrackingManager.SetQuestTracked(_currentQuest.ID, !currentlyTracked);
            
            // 立即更新UI
            UpdateButtonState(instance, _currentQuest);
            
            ModLogger.Log("QuestTracker", $"Quest {_currentQuest.ID} tracking toggled to: {!currentlyTracked}");
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestViewDetailsPatch.OnTrackButtonClicked failed: {ex}");
        }
    }
}

