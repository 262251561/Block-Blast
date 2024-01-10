using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

namespace TH
{
    public static class EditorHelper
    {
        public static void CreateAsset(UnityEngine.Object asset)
        {
            if (asset == null)
                return;

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
                path = "Assets";
            else if (Path.GetExtension(path) != "")
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + asset.name + ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }

        public static string GetAssetPathWithoutName(UnityEngine.Object asset)
        {
            var path = AssetDatabase.GetAssetPath(asset);
            return path.Substring(0, path.Length -  (asset.name + ".asset").Length);
        }

        public static T CreateAsset<T>(string assetName) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
            if (asset != null)
            {
                asset.name = assetName;

                CreateAsset(asset);
            }

            return asset;
        }
    }
}
