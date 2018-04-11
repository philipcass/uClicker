using UnityEditor;
using UnityEngine;

namespace uClicker.Editor
{
    [CreateAssetMenu(menuName = "uClicker/Settings")]
    public class ClickerSettings : ScriptableObject
    {
        private static ClickerSettings _instance;

        public static ClickerSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    string findAsset = AssetDatabase.FindAssets("t:ClickerSettings")[0];
                    string guidToAssetPath = AssetDatabase.GUIDToAssetPath(findAsset);
                    _instance = AssetDatabase.LoadAssetAtPath<ClickerSettings>(guidToAssetPath);
                }

                return _instance;
            }
        }

        public bool MapNameToFileNames = true;
    }
}