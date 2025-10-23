using UnityEngine;

namespace EfDEnhanced.Utils.UI.Constants
{
    /// <summary>
    /// UI常量定义 - 集中管理所有UI相关的魔法数字
    /// 包括尺寸、间距、字体大小、颜色等
    /// </summary>
    public static class UIConstants
    {
        #region Panel Sizes

        /// <summary>
        /// 任务追踪器面板宽度
        /// </summary>
        public const int QUEST_PANEL_WIDTH = 280;

        /// <summary>
        /// 任务追踪器最大高度占屏幕高度的比例
        /// </summary>
        public const float QUEST_PANEL_SCREEN_HEIGHT_RATIO = 0.4f;

        /// <summary>
        /// 设置面板宽度
        /// </summary>
        public const int SETTINGS_PANEL_WIDTH = 800;

        /// <summary>
        /// 设置面板高度
        /// </summary>
        public const int SETTINGS_PANEL_HEIGHT = 900;

        /// <summary>
        /// Raid准备检查面板宽度
        /// </summary>
        public const int RAID_CHECK_PANEL_WIDTH = 800;

        /// <summary>
        /// Raid准备检查面板高度
        /// </summary>
        public const int RAID_CHECK_PANEL_HEIGHT = 600;

        #endregion

        #region Spacing

        /// <summary>
        /// 任务条目之间的间距
        /// </summary>
        public const int QUEST_ENTRY_SPACING = 4;

        /// <summary>
        /// 任务内部子任务之间的间距
        /// </summary>
        public const int QUEST_TASK_SPACING = 2;

        /// <summary>
        /// 任务条目内边距
        /// </summary>
        public const int QUEST_ENTRY_PADDING = 8;

        /// <summary>
        /// 设置条目之间的间距
        /// </summary>
        public const int SETTINGS_ENTRY_SPACING = 4;

        /// <summary>
        /// 设置字段高度
        /// </summary>
        public const int SETTINGS_FIELD_HEIGHT = 42;

        /// <summary>
        /// 分节标题之间的间距
        /// </summary>
        public const int SECTION_SPACING = 6;

        #endregion

        #region Font Sizes

        /// <summary>
        /// 任务标题字体大小
        /// </summary>
        public const int QUEST_TITLE_FONT_SIZE = 16;

        /// <summary>
        /// 任务描述字体大小
        /// </summary>
        public const int QUEST_DESC_FONT_SIZE = 12;

        /// <summary>
        /// 任务子任务字体大小
        /// </summary>
        public const int QUEST_TASK_FONT_SIZE = 13;

        /// <summary>
        /// 任务进度徽章字体大小
        /// </summary>
        public const int QUEST_PROGRESS_FONT_SIZE = 15;

        /// <summary>
        /// 设置面板标题字体大小
        /// </summary>
        public const int SETTINGS_TITLE_FONT_SIZE = 28;

        /// <summary>
        /// 设置分节标题字体大小
        /// </summary>
        public const int SETTINGS_SECTION_FONT_SIZE = 22;

        /// <summary>
        /// 设置标签字体大小
        /// </summary>
        public const int SETTINGS_LABEL_FONT_SIZE = 18;

        /// <summary>
        /// 设置普通文本字体大小
        /// </summary>
        public const int SETTINGS_FONT_SIZE = 16;

        /// <summary>
        /// 按钮文本字体大小
        /// </summary>
        public const int BUTTON_TEXT_FONT_SIZE = 20;

        /// <summary>
        /// Raid检查标题字体大小
        /// </summary>
        public const int RAID_CHECK_TITLE_FONT_SIZE = 48;

        /// <summary>
        /// Raid检查警告文本字体大小
        /// </summary>
        public const int RAID_CHECK_WARNING_FONT_SIZE = 28;

        #endregion

        #region Colors

        /// <summary>
        /// 任务标题颜色（金黄色）
        /// </summary>
        public static readonly Color QUEST_TITLE_COLOR = new(1f, 0.9f, 0.4f, 1f);

        /// <summary>
        /// 任务描述颜色（浅灰色）
        /// </summary>
        public static readonly Color QUEST_DESC_COLOR = new(0.85f, 0.85f, 0.85f, 1f);

        /// <summary>
        /// 任务完成颜色（绿色）
        /// </summary>
        public static readonly Color QUEST_COMPLETE_COLOR = new(0.6f, 1f, 0.6f, 1f);

        /// <summary>
        /// 任务未完成颜色（白色）
        /// </summary>
        public static readonly Color QUEST_INCOMPLETE_COLOR = new(1f, 1f, 1f, 1f);

        /// <summary>
        /// 任务进度徽章颜色
        /// </summary>
        public static readonly Color QUEST_PROGRESS_COLOR = new(0.9f, 0.9f, 0.9f, 1f);

        /// <summary>
        /// 设置文本颜色
        /// </summary>
        public static readonly Color SETTINGS_TEXT_COLOR = new(0.9f, 0.9f, 0.9f, 1f);

        /// <summary>
        /// 分隔线颜色
        /// </summary>
        public static readonly Color SEPARATOR_COLOR = new(0.6f, 0.6f, 0.6f, 0.3f);

        /// <summary>
        /// 背景半透明颜色（黑色）
        /// </summary>
        public static readonly Color BACKGROUND_DARK = new(0, 0, 0, 0.7f);

        /// <summary>
        /// 面板背景颜色（深灰色）
        /// </summary>
        public static readonly Color PANEL_BACKGROUND = new(0.1f, 0.1f, 0.1f, 0.95f);

        /// <summary>
        /// 复选框背景颜色（未选中）
        /// </summary>
        public static readonly Color CHECKBOX_BACKGROUND_UNCHECKED = new(0.15f, 0.15f, 0.15f, 1f);

        /// <summary>
        /// 复选框背景颜色（选中）
        /// </summary>
        public static readonly Color CHECKBOX_BACKGROUND_CHECKED = new(0.18f, 0.32f, 0.18f, 1f);

        /// <summary>
        /// 复选框勾选标记颜色
        /// </summary>
        public static readonly Color CHECKBOX_CHECKMARK = new(0.35f, 0.95f, 0.35f, 1f);

        /// <summary>
        /// 复选框边框颜色（未选中）
        /// </summary>
        public static readonly Color CHECKBOX_BORDER_UNCHECKED = new(0.45f, 0.45f, 0.45f, 1f);

        /// <summary>
        /// 复选框边框颜色（选中）
        /// </summary>
        public static readonly Color CHECKBOX_BORDER_CHECKED = new(0.45f, 0.85f, 0.45f, 1f);

        /// <summary>
        /// 按钮颜色 - Primary
        /// </summary>
        public static readonly Color BUTTON_PRIMARY = new(0.2f, 0.5f, 0.8f, 1f);

        /// <summary>
        /// 按钮颜色 - Secondary
        /// </summary>
        public static readonly Color BUTTON_SECONDARY = new(0.3f, 0.3f, 0.3f, 1f);

        /// <summary>
        /// 按钮颜色 - Success
        /// </summary>
        public static readonly Color BUTTON_SUCCESS = new(0.2f, 0.5f, 0.2f, 1f);

        /// <summary>
        /// 按钮颜色 - Danger
        /// </summary>
        public static readonly Color BUTTON_DANGER = new(0.5f, 0.2f, 0.2f, 1f);

        #endregion

        #region Button Sizes

        /// <summary>
        /// 标准按钮宽度
        /// </summary>
        public const int BUTTON_WIDTH = 220;

        /// <summary>
        /// 标准按钮高度
        /// </summary>
        public const int BUTTON_HEIGHT = 36;

        /// <summary>
        /// 复选框大小
        /// </summary>
        public const int CHECKBOX_SIZE = 24;

        /// <summary>
        /// Raid检查按钮宽度
        /// </summary>
        public const int RAID_CHECK_BUTTON_WIDTH = 280;

        /// <summary>
        /// Raid检查按钮高度
        /// </summary>
        public const int RAID_CHECK_BUTTON_HEIGHT = 60;

        #endregion

        #region Animation

        /// <summary>
        /// 标准淡入淡出动画时长
        /// </summary>
        public const float FADE_DURATION = 0.2f;

        /// <summary>
        /// 按钮点击缩放动画时长
        /// </summary>
        public const float BUTTON_SCALE_DURATION = 0.1f;

        /// <summary>
        /// 按钮点击缩放比例
        /// </summary>
        public const float BUTTON_SCALE_FACTOR = 0.95f;

        #endregion

        #region Canvas Sort Orders

        /// <summary>
        /// 任务追踪器Canvas层级
        /// </summary>
        public const int QUEST_TRACKER_SORT_ORDER = 100;

        /// <summary>
        /// 设置面板Canvas层级
        /// </summary>
        public const int SETTINGS_PANEL_SORT_ORDER = 1000;

        /// <summary>
        /// Raid检查面板Canvas层级
        /// </summary>
        public const int RAID_CHECK_SORT_ORDER = 1000;

        #endregion

        #region Shadow Settings

        /// <summary>
        /// 标题文字阴影大小
        /// </summary>
        public const float TITLE_SHADOW_SIZE = 16f;

        /// <summary>
        /// 标题文字阴影扩散
        /// </summary>
        public const float TITLE_SHADOW_SPREAD = 0.5f;

        /// <summary>
        /// 标题文字阴影距离
        /// </summary>
        public const float TITLE_SHADOW_DISTANCE = 3f;

        /// <summary>
        /// 普通文字阴影大小
        /// </summary>
        public const float TEXT_SHADOW_SIZE = 12f;

        /// <summary>
        /// 普通文字阴影扩散
        /// </summary>
        public const float TEXT_SHADOW_SPREAD = 0.4f;

        /// <summary>
        /// 普通文字阴影距离
        /// </summary>
        public const float TEXT_SHADOW_DISTANCE = 2f;

        /// <summary>
        /// 阴影颜色
        /// </summary>
        public static readonly Color SHADOW_COLOR = new(0, 0, 0, 0.8f);

        #endregion
    }
}

