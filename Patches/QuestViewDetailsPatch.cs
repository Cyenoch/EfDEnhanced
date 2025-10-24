using System;
using System.Collections.Generic;
using Duckov.Quests;
using Duckov.Quests.UI;
using EfDEnhanced.Utils;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace EfDEnhanced.Patches;

/// <summary>
/// 为任务详情视图添加追踪按钮（Button版本）
/// </summary>
[HarmonyPatch(typeof(QuestViewDetails))]
public class QuestViewDetailsPatch
{
    // 为每个 QuestViewDetails 实例维护独立的按钮
    private static readonly Dictionary<QuestViewDetails, ButtonData> _trackButtons = [];
    private static Quest? _currentQuest;

    private class ButtonData
    {
        public GameObject ButtonObject = null!;
        public Image StatusImage = null!;
        public Sprite UncheckedSprite = null!;
        public Sprite CheckedSprite = null!;
        public bool UsingNativeSprites;
        public GameObject IconContainer = null!;
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
            // Check for null parameters
            if (__instance == null)
            {
                ModLogger.LogError("QuestViewDetailsPatch.Setup_Postfix: __instance is null");
                return;
            }

            if (quest == null)
            {
                ModLogger.Log("QuestTracker", "Setup called with null quest, cleaning up existing button");

                // Clean up existing button if quest is null
                if (_trackButtons.ContainsKey(__instance) && _trackButtons[__instance]?.ButtonObject != null)
                {
                    UnityEngine.Object.Destroy(_trackButtons[__instance].ButtonObject);
                    _trackButtons.Remove(__instance);
                }

                _currentQuest = null;
                return;
            }

            _currentQuest = quest;

            // 销毁旧按钮（如果存在）
            if (_trackButtons.ContainsKey(__instance) && _trackButtons[__instance]?.ButtonObject != null)
            {
                UnityEngine.Object.Destroy(_trackButtons[__instance].ButtonObject);
                _trackButtons.Remove(__instance);
            }

            // 为新任务创建按钮
            CreateTrackButton(__instance, quest);
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestViewDetailsPatch.Setup_Postfix failed: {ex}");
        }
    }

    /// <summary>
    /// 创建追踪按钮
    /// </summary>
    private static void CreateTrackButton(QuestViewDetails questViewDetails, Quest quest)
    {
        try
        {
            // Additional safety checks
            if (questViewDetails == null)
            {
                ModLogger.LogError("CreateTrackButton: questViewDetails is null");
                return;
            }

            if (quest == null)
            {
                ModLogger.LogError("CreateTrackButton: quest is null");
                return;
            }

            ModLogger.Log("QuestTracker", $"Creating track button for quest {quest.ID}");

            // 尝试获取游戏原生的任务图标
            Sprite? uncheckedSprite = null;
            Sprite? checkedSprite = null;

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
            GameObject buttonContainer = new("TrackQuestButton");
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

            // 创建复选框容器
            GameObject iconObj = new("StatusIcon");
            iconObj.transform.SetParent(buttonContainer.transform, false);

            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(32, 32);

            // 添加 LayoutElement 确保大小不被布局组改变
            LayoutElement iconLayout = iconObj.AddComponent<LayoutElement>();
            iconLayout.minWidth = 32;
            iconLayout.minHeight = 32;
            iconLayout.preferredWidth = 32;
            iconLayout.preferredHeight = 32;
            iconLayout.flexibleWidth = 0;
            iconLayout.flexibleHeight = 0;

            // 背景
            Image bgImage = iconObj.AddComponent<Image>();
            bgImage.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

            // 边框（使用Outline组件模拟）
            // 稍后会根据追踪状态设置颜色
            Outline borderOutline = iconObj.AddComponent<Outline>();
            borderOutline.effectDistance = new Vector2(2, -2);

            // 状态图标（内部的勾选标记）
            GameObject checkmarkObj = new("Checkmark");
            checkmarkObj.transform.SetParent(iconObj.transform, false);

            RectTransform checkmarkRect = checkmarkObj.AddComponent<RectTransform>();
            checkmarkRect.anchorMin = Vector2.zero;
            checkmarkRect.anchorMax = Vector2.one;
            checkmarkRect.offsetMin = new Vector2(4, 4);
            checkmarkRect.offsetMax = new Vector2(-4, -4);

            Image statusImage = checkmarkObj.AddComponent<Image>();

            // 确定是否使用原生图标
            bool usingNativeSprites = uncheckedSprite != null && checkedSprite != null;
            Sprite finalUncheckedSprite = usingNativeSprites ? uncheckedSprite! : CreateWhiteSquareSprite();
            Sprite finalCheckedSprite = usingNativeSprites ? checkedSprite! : CreateWhiteSquareSprite();

            // 根据当前追踪状态设置初始图标和颜色
            bool isTracked = QuestTrackingManager.IsQuestTracked(quest.ID);
            statusImage.sprite = isTracked ? finalCheckedSprite : finalUncheckedSprite;
            statusImage.color = usingNativeSprites ? Color.white : (isTracked ? new Color(0.3f, 1f, 0.3f, 1f) : new Color(0.6f, 0.6f, 0.6f, 1f));

            // 设置边框颜色
            Color borderColor = isTracked
                ? new Color(0.4f, 1f, 0.4f, 1f)  // 绿色边框
                : new Color(0.5f, 0.5f, 0.5f, 1f); // 灰色边框
            borderOutline.effectColor = borderColor;

            // 创建文本标签
            GameObject labelObj = new("Label");
            labelObj.transform.SetParent(buttonContainer.transform, false);

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = LocalizationHelper.Get("QuestTracker_CheckboxLabel");
            labelText.fontSize = 28;
            labelText.color = new Color(1f, 1f, 1f, 1f);
            labelText.alignment = TextAlignmentOptions.MidlineLeft;

            LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.flexibleWidth = 1;

            // 添加Button组件（整个容器可点击）
            Button button = buttonContainer.AddComponent<Button>();
            button.targetGraphic = statusImage;
            button.onClick.AddListener(() => OnTrackButtonClicked(questViewDetails, quest));

            // 存储引用（仅用于稍后销毁）
            _trackButtons[questViewDetails] = new ButtonData
            {
                ButtonObject = buttonContainer,
                StatusImage = statusImage,
                UncheckedSprite = finalUncheckedSprite,
                CheckedSprite = finalCheckedSprite,
                UsingNativeSprites = usingNativeSprites,
                IconContainer = iconObj
            };

            ModLogger.Log("QuestTracker", $"Track button created for quest {quest.ID} (tracked: {isTracked}, native sprites: {usingNativeSprites})");
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
        Texture2D texture = new(4, 4);
        Color32 white = new(255, 255, 255, 255);

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
    /// 按钮点击事件
    /// </summary>
    private static void OnTrackButtonClicked(QuestViewDetails instance, Quest quest)
    {
        try
        {
            // Safety checks
            if (instance == null)
            {
                ModLogger.LogError("OnTrackButtonClicked: instance is null");
                return;
            }

            if (quest == null)
            {
                ModLogger.LogError("OnTrackButtonClicked: quest is null");
                return;
            }

            // 切换追踪状态
            bool currentlyTracked = QuestTrackingManager.IsQuestTracked(quest.ID);
            QuestTrackingManager.SetQuestTracked(quest.ID, !currentlyTracked);

            // 重新创建按钮以反映新状态
            if (_trackButtons.ContainsKey(instance) && _trackButtons[instance]?.ButtonObject != null)
            {
                UnityEngine.Object.Destroy(_trackButtons[instance].ButtonObject);
                _trackButtons.Remove(instance);
            }
            CreateTrackButton(instance, quest);

            ModLogger.Log("QuestTracker", $"Quest {quest.ID} tracking toggled to: {!currentlyTracked}");
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"QuestViewDetailsPatch.OnTrackButtonClicked failed: {ex}");
        }
    }
}

/// <summary>
/// 新任务接受自动追踪补丁
/// 当玩家接受新任务时，自动将其添加到局内追踪列表中
/// Patches QuestManager.ActivateQuest() which is called when a player accepts a new quest
/// </summary>
[HarmonyPatch(typeof(QuestManager), "ActivateQuest")]
public class AutoTrackNewQuestPatch
{
    /// <summary>
    /// Patch ActivateQuest to auto-track newly accepted quests
    /// This is called right after the quest is added to activeQuests list
    /// </summary>
    [HarmonyPostfix]
    private static void ActivateQuest_Postfix(QuestManager __instance, int id)
    {
        try
        {
            // Check if auto-track setting is enabled
            if (!ModSettings.AutoTrackNewQuests.Value)
            {
                return;
            }

            // Find the quest that was just activated
            if (__instance == null || __instance.ActiveQuests == null || __instance.ActiveQuests.Count == 0)
            {
                return;
            }

            QuestTrackingManager.SetQuestTracked(id, true);

            // Get the quest with the matching ID
            var newQuest = __instance.ActiveQuests.FirstOrDefault(q => q != null && q.ID == id);
            if (newQuest == null)
            {
                return;
            }

            // Auto-track the newly accepted quest
            ModLogger.Log("QuestTracker", $"Auto-tracking newly accepted quest: {newQuest.DisplayName} (ID: {newQuest.ID})");
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"AutoTrackNewQuestPatch.ActivateQuest_Postfix failed: {ex}");
        }
    }
}

