using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TH;
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
        var sceneLogic = GameSceneLogic.s_Instance;
        if (sceneLogic == null || sceneLogic.coreLogic == null)
            return;

        __uiScoreShow.text = IntToStringMap.instance.GetCacheString(sceneLogic.coreLogic.score);
    }
}