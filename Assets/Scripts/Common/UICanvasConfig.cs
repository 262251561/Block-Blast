using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class UICanvasConfig : MonoBehaviour
{
    public static UICanvasConfig s_Instance;

    public int canvasWidth;
    public int canvasHeight;

    public Camera uiCamera
    {
        private set;
        get;
    }

    private void Awake()
    {
        uiCamera = gameObject.GetComponent<Camera>();
        s_Instance = this;
    }
}