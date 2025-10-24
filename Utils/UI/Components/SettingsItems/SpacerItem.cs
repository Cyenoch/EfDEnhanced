using UnityEngine;
using UnityEngine.UI;

namespace EfDEnhanced.Utils.UI.Components.SettingsItems
{
    /// <summary>
    /// Spacer item for adding vertical space between settings
    /// </summary>
    public class SpacerItem : MonoBehaviour
    {
        public void Initialize(float height = 20f)
        {
            var rectTransform = gameObject.GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);

            var layoutElement = gameObject.AddComponent<LayoutElement>();
            layoutElement.minHeight = height;
            layoutElement.preferredHeight = height;
        }
    }
}

