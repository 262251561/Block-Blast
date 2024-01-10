using UnityEngine;
using System.Collections;

namespace TH
{
    public static class UnityExtension
    {
        public static void TrySetActive(this GameObject gameObject, bool value)
        {
            if (gameObject != null && gameObject.activeSelf != value)
            {
                gameObject.SetActive(value);
            }
        }

        public static void SetLayer(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            int i, length = gameObject.transform.childCount;
            for (i = 0; i < length; ++i)
                SetLayer(gameObject.transform.GetChild(i).gameObject, layer);
        }

        public static void SetChildLayer(this GameObject gameObject, int layer)
        {
            int i, length = gameObject.transform.childCount;
            for (i = 0; i < length; ++i)
                SetLayer(gameObject.transform.GetChild(i).gameObject, layer);
        }

        public static Transform FindChildDeep(this Transform root, string childName)
        {
            if (root.name == childName)
                return root;

            int i, length = root.childCount;
            for(i=0; i<length; ++i)
            {
                var child = FindChildDeep(root.GetChild(i), childName);
                if (child != null)
                    return child;
            }

            return null;
        }

        public static void SetSubCanvasToParent(this RectTransform rc, RectTransform parent)
        {
            rc.SetParent(parent, false);
            rc.anchorMin = new Vector2(0, 0);
            rc.anchorMax = new Vector2(1, 1);
            rc.offsetMin = new Vector2(0,0);
            rc.offsetMax = new Vector2(0, 0);
            rc.anchoredPosition3D = new Vector3(0, 0, 0);
            rc.localScale = Vector3.one;
        }

        public static void ReplaceChildrenToNewParent(Transform sourceParent, Transform destParent)
        {
            while (sourceParent.childCount > 0)
            { 
                sourceParent.GetChild(0).SetParent(destParent);
            }
        }

        public static IEnumerator WaitForSeconds(float seconds)
        {
            while(seconds > 0.0f)
            {
                yield return null;
                seconds -= Time.deltaTime;
            }
        }
    }
}