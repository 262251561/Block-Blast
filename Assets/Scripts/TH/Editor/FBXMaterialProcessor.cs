using System;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

using Object = UnityEngine.Object;

namespace TH
{
#if UNITY_EDITOR

    public class FBXMaterialProcessor : AssetPostprocessor
    {
        /// <summary>
        /// 批量删除模型上的材质
        /// </summary>
        public static void DelModelMats(Object[] models)
        {
            foreach (Object obj in models)
            {
                DelModelMat(obj as GameObject);
            }
        }

        /// <summary>
        /// 删除模型上绑定的材质
        /// </summary>
        /// <param name="model">模型对象</param>
        public static void DelModelMat(GameObject model)
        {
            if (null == model)
                return;

            string assetPath = AssetDatabase.GetAssetPath(model);
            ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (null == importer)
                return;

            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            AssetDatabase.ImportAsset(assetPath);
        }

        [MenuItem("Assets/FBX/删除选中模型的材质")]
        static void DelSelectedModelMat()
        {
            Object[] objs = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
            if (null == objs)
                return;

            DelModelMats(objs);
        }

        private void OnPostprocessModel(GameObject model)
        {
            if (null == model)
                return;

            Renderer[] renders = model.GetComponentsInChildren<Renderer>(true);

            if (null == renders)
                return;

            foreach (Renderer render in renders)
            {
                render.sharedMaterials = new Material[render.sharedMaterials.Length];
            }
        }
    }
#endif
}
