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

    private void Awake()
    {
        s_Instance = this;
    }
}