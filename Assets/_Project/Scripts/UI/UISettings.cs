using UnityEngine;
using TMPro;

namespace ProjectZero.UI
{
    /// <summary>
    /// ScriptableObject containing UI configuration settings for the UIManager
    /// Allows designers to configure UI appearance without code changes
    /// </summary>
    [CreateAssetMenu(fileName = "UISettings", menuName = "ProjectZero/UI/UI Settings")]
    public class UISettings : ScriptableObject
    {
        [Header("Canvas Configuration")]
        [SerializeField] public Vector2 referenceResolution = new Vector2(1920, 1080);
        [SerializeField] [Range(0.01f, 2f)] public float worldSpaceScaleFactor = 0.01f;

        [Header("Font Settings")]
        [SerializeField] public TMP_FontAsset defaultFont;
        [SerializeField] [Range(8f, 72f)] public float defaultFontSize = 12f;
        [SerializeField] public Color defaultTextColor = Color.white;

        [Header("Default Sizes")]
        [SerializeField] public Vector2 defaultLabelSize = new Vector2(200, 50);
        [SerializeField] public Vector2 defaultButtonSize = new Vector2(200, 50);
        [SerializeField] public Vector2 defaultPanelSize = new Vector2(400, 300);

        [Header("Button Configuration")]
        [SerializeField] public Sprite defaultButtonSprite;
        [SerializeField] public Color defaultButtonColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        [SerializeField] public Color buttonHighlightColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        [SerializeField] public Color buttonPressedColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        [SerializeField] public Color buttonDisabledColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);

        [Header("Panel Configuration")]
        [SerializeField] public Sprite defaultPanelSprite;
        [SerializeField] public Color defaultPanelColor = new Color(0.1f, 0.1f, 0.1f, 0.7f);

        [Header("Animation Settings")]
        [SerializeField] [Range(0.1f, 2f)] public float fadeInDuration = 0.5f;
        [SerializeField] [Range(0.1f, 2f)] public float fadeOutDuration = 0.5f;
        [SerializeField] public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("World Label Settings")]
        [SerializeField] [Range(0.1f, 10f)] public float worldLabelScale = 0.02f;
        [SerializeField] [Range(1f, 100f)] public float worldLabelMaxDistance = 50f;
        [SerializeField] public bool worldLabelsScaleWithDistance = true;
        [SerializeField] [Range(6f, 24f)] public float worldLabelFontSize = 8f;

        private void OnValidate()
        {
            // Ensure reasonable values
            referenceResolution = new Vector2(
                Mathf.Max(640, referenceResolution.x), 
                Mathf.Max(480, referenceResolution.y)
            );
            
            worldSpaceScaleFactor = Mathf.Max(0.001f, worldSpaceScaleFactor);
            defaultFontSize = Mathf.Max(6f, defaultFontSize);
        }

        /// <summary>
        /// Creates default UI settings with built-in Unity resources
        /// </summary>
        public static UISettings CreateDefault()
        {
            UISettings settings = CreateInstance<UISettings>();
            
            // Try to load default TextMeshPro font
            settings.defaultFont = Resources.GetBuiltinResource<TMP_FontAsset>("LegacyRuntime.fontsettings");
            
            // Use built-in sprites if available
            settings.defaultButtonSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            settings.defaultPanelSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
            
            return settings;
        }
    }
}
