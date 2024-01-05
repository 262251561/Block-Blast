using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

public class UILogin : UIBase
{
    private void Start()
    {
        GetCT<Button>("btnStart").onClick.AddListener(OnStartClick);
    }

    void OnStartClick()
    {
        GameRoot.s_Instance.EnterState( GameRoot.State.GAME, null );
    }
}