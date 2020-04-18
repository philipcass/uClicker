using System;
using System.IO;
using UnityEngine;

namespace uClicker
{
    [Serializable]
    public class ManagerSaveSettings
    {
        public enum SaveTypeEnum
        {
            SaveToPlayerPrefs,
            SaveToFile
        }

        public SaveTypeEnum SaveType = SaveTypeEnum.SaveToPlayerPrefs;
        public string SaveName = "save";
        public string SavePath = "";

        public string FullSavePath
        {
            get { return Path.Combine(Application.persistentDataPath, SavePath, SaveName); }
        }
    }
}