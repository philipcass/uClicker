using UnityEditor;
using UnityEngine;

namespace uClicker.Editor
{
    [CreateAssetMenu(menuName = "uClicker/Settings")]
    public class ClickerSettings : ScriptableObject
    {
        private static ClickerSettings _instance;

        private void OnDisable()
        {
            _instance = null;
        }

        public static ClickerSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    var findAssets = AssetDatabase.FindAssets("t:ClickerSettings");
                    if (findAssets.Length == 0)
                    {
                        _instance = CreateInstance<ClickerSettings>();
                        return _instance;
                    }

                    string findAsset = findAssets[0];
                    string guidToAssetPath = AssetDatabase.GUIDToAssetPath(findAsset);
                    _instance = AssetDatabase.LoadAssetAtPath<ClickerSettings>(guidToAssetPath);
                }

                return _instance;
            }
        }

        public bool MapNameToFileNames = true;
    }
}