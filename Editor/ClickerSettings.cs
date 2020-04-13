using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace uClicker.Editor
{
    [Serializable]
    public class ClickerSettings : ScriptableObject
    {
        public bool UseCustomInspector = true;
        public List<GUIDContainer> ClickerComponentGUIDContainers = new List<GUIDContainer>();
        public List<string> ClickerComponentAssetGUIDs = new List<string>();

        private const string SettingsSaveKey = "_clickerSettings";
        private static ClickerSettings _instance;
        [SerializeField] [HideInInspector] private bool _init;

        public static ClickerSettings Instance
        {
            get
            {
                _instance = _instance
                    ? _instance
                    : Resources.FindObjectsOfTypeAll<ClickerSettings>().FirstOrDefault() ??
                      CreateInstance<ClickerSettings>();
                _instance.hideFlags = HideFlags.DontSave;
                return _instance;
            }
        }

        public void Save()
        {
            EditorPrefs.SetString(SettingsSaveKey, EditorJsonUtility.ToJson(_instance));
        }

        public void Load()
        {
            EditorJsonUtility.FromJsonOverwrite(EditorPrefs.GetString(SettingsSaveKey, "{}"), _instance);
        }

        [InitializeOnLoadMethod]
        private static void EditorLoad()
        {
            if (!Instance._init)
            {
                Debug.Log("Loading ClickerSettings...");
                Instance.Load();
                Instance._init = true;
            }
        }
    }
}