using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFix : MonoBehaviour
{
    private void Start()
    {
        float width = UICanvasConfig.s_Instance.canvasWidth;
        float height = UICanvasConfig.s_Instance.canvasHeight;

        gameObject.GetComponent<Camera>().aspect = width / height;
    }
}