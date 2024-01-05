using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

public class UIGameMain : UIBase
{
    private Text __uiScoreShow;

    private void Start()
    {
        __uiScoreShow = GetCT<Text>("txtScoreShow");
    }

    private void LateUpdate()
    {
        if (GameSceneLogic.s_Instance == null || GameSceneLogic.s_Instance.coreLogic == null)
            return;

        __uiScoreShow.text = IntStringMap.instance.GetInt(GameSceneLogic.s_Instance.coreLogic.score);
    }
}