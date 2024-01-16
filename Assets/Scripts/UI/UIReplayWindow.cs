using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TH;
using UnityEngine;
using UnityEngine.UI;

public class UIReplayWindow : UIBase
{
    private Text __localScore;
    private Text __bestScore;

    private void Start()
    {
        GetCT<Button>("btnMainPage").onClick.AddListener(OnMainPage);
        GetCT<Button>("btnRestart").onClick.AddListener(OnRestart);
    }

    private void Update()
    {
        if(__localScore == null)
        {
            __localScore = GetCT<Text>("txtCurrent");
            __bestScore = GetCT<Text>("txtBest");

            var currentScore = GameSceneLogic.s_Instance.coreLogic.score;
            var bestScore = GamePlayerData.s_Instance.bestScore;

            __localScore.text = IntToStringMap.instance.GetCacheString(currentScore);
            __bestScore.text = IntToStringMap.instance.GetCacheString(bestScore);

            if (currentScore > bestScore)
                GamePlayerData.s_Instance.ApplyBestScore(currentScore);
        }
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