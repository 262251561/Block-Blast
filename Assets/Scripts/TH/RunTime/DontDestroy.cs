using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace TH
{
    public class DontDestroy : MonoBehaviour
    {
        private void OnEnable()
        {
            GameObject.DontDestroyOnLoad(gameObject);
        }
    }
}
