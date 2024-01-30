using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AutoHide : MonoBehaviour
{
    public float showTime;

    private void OnEnable()
    {
        Invoke("Hide", showTime);
    }
    
    void Hide()
    {
        gameObject.SetActive(false);
    }
}