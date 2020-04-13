using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace uClicker.Editor
{
    /// <summary>
    /// Utility for rebuilding Unity AssetDB when things go out of whack (sometimes common when using SOs)
    /// </summary>
    public static class ClickerComponentAssetRebuilder
    {
        [MenuItem("uClicker/Rebuild")]
        public static void RebuildAssets()
        {
            RebuildAssets(typeof(ClickerComponent));
        }

        private static void RebuildAssets(Type baseType)
        {
            IEnumerable<MonoScript> clickerScripts = AssetDatabase.FindAssets("t:MonoScript")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<MonoScript>).Where(script =>
                    script.GetClass() != null && script.GetClass().IsSubclassOf(baseType));
            IEnumerable<string> scriptGuids =
                clickerScripts.Select(AssetDatabase.GetAssetPath).Select(AssetDatabase.AssetPathToGUID);
            HashSet<string> guidHash = new HashSet<string>(scriptGuids);

            AssetDatabase.StartAssetEditing();
            foreach (string assetFile in Directory.EnumerateFiles(Application.dataPath, "*.asset",
                SearchOption.AllDirectories))
            {
                foreach (string yamlLine in File.ReadLines(assetFile))
                {
                    if (yamlLine.StartsWith("  m_Script: {fileID: 11500000, guid: "))
                    {
                        string guid = (yamlLine.Substring(37, 32));
                        if (guidHash.Contains(guid))
                        {
                            Debug.LogFormat("Reimporting asset: {0}", assetFile);
                            AssetDatabase.ImportAsset(assetFile.Replace(Application.dataPath, "Assets"),
                                ImportAssetOptions.ForceUpdate);
                        }
                    }
                }
            }

            AssetDatabase.StopAssetEditing();
        }
    }
}