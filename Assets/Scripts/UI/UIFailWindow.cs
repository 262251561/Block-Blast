using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UIFailWindow : UIBase
{
    private void Start()
    {
        GetCT<Button>("btnMainPage").onClick.AddListener(OnMainPage);
        GetCT<Button>("btnRestart").onClick.AddListener(OnRestart);
    }

    public void OnMainPage()
    {
        GameRoot.s_Instance.EnterState(GameRoot.State.LOGIN, null);
    }

    public void OnRestart()
    {
        GameRoot.s_Instance.EnterState(GameRoot.State.GAME, null);
    }
}