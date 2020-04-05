using UnityEditor;

namespace uClicker.Editor
{
    public class ClickerSettings : UnityEditor.EditorWindow
    {
        public bool UseCustomInspector = true;

        private static ClickerSettings _instance;
        private UnityEditor.Editor _cachedEditor;
        private const string SettingsSaveKey = "_clickerSettings";

        [UnityEditor.Callbacks.DidReloadScripts]
        [InitializeOnLoadMethod]
        private static void Load()
        {
            _instance = _instance ?? CreateInstance<ClickerSettings>();
            DontDestroyOnLoad(_instance);
            EditorJsonUtility.FromJsonOverwrite(EditorPrefs.GetString(SettingsSaveKey, "{}"), _instance);
        }

        public static ClickerSettings Instance
        {
            get { return _instance; }
        }

        [MenuItem("uClicker/Settings")]
        static void Init()
        {
            ClickerSettings window = (ClickerSettings) GetWindow(typeof(ClickerSettings));
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            _cachedEditor = _cachedEditor ?? UnityEditor.Editor.CreateEditor(Instance);
            _cachedEditor.DrawDefaultInspector();
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetString(SettingsSaveKey, EditorJsonUtility.ToJson(Instance));
            }
        }
    }
}