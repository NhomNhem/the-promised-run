using UnityEngine;

#if UNITY_EDITOR
namespace RaycastPro.Editor
{
    [CreateAssetMenu(fileName = "RCProColorProfile", menuName = "RCPRO/ColorProfile", order = 1)]
    public class RCPROColorProfile : ScriptableObject
    {
        // Note: Do NOT reference EditorGUIUtility.isProSkin (via RCProEditor.Aqua/RCProPanel.DarkMode)
        // from a ScriptableObject field initializer, otherwise Unity may throw:
        // "get_skinIndex is not allowed to be called from a ScriptableObject constructor".
        // Use a constant fallback here and apply theme-aware defaults in OnEnable if desired.
        public Color DefaultColor = new Color(0.17f, 0.87f, 0.92f, 1f);
        public Color DetectColor = new Color(.3f, 1, .3f, 1f);
        public Color HelperColor = new Color(1f, .7f, .0f, 1f);
        public Color BlockColor = new Color(1f, .2f, .2f, 1f);

        private void OnEnable()
        {
            // If the asset is freshly created (or reset) we can safely apply the theme-aware default.
            // Avoid overwriting user-customized colors.
            if (DefaultColor.a <= 0f)
                DefaultColor = RCProEditor.Aqua;
        }
    }
}
#endif
