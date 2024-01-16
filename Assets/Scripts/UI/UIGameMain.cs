using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TH;
using UnityEngine;
using UnityEngine.UI;

public class UIGameMain : UIBase
{
    private bool __isDirty;
    private Text __uiScoreShow;

    private IEnumerator Start()
    {
        __uiScoreShow = GetCT<Text>("txtScoreShow");

        while(GameSceneLogic.s_Instance == null || GameSceneLogic.s_Instance.coreLogic == null)
        {
            yield return null;
        }

        var coreLogic = GameSceneLogic.s_Instance.coreLogic;
        coreLogic.scoreChangedEvent += OnScoreChanged;

        __isDirty = true;
    }

    void OnScoreChanged()
    {
        __isDirty = true;
    }

    private void LateUpdate()
    {
        if (!__isDirty)
            return;

        __uiScoreShow.text = IntToStringMap.instance.GetCacheString(GameSceneLogic.s_Instance.coreLogic.score);
        __isDirty = false;
    }
}