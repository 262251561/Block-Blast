using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UISCInit : MonoBehaviour
{
    private void Start()
    {
        var canvasRoot = gameObject.GetComponent<Canvas>();
        canvasRoot.renderMode = RenderMode.ScreenSpaceCamera;
        canvasRoot.worldCamera = UICanvasConfig.s_Instance.uiCamera;

        var canvasScaler = gameObject.GetComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.referenceResolution = new Vector2(UICanvasConfig.s_Instance.canvasWidth, UICanvasConfig.s_Instance.canvasHeight);
    }
}