using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TH;
using UnityEngine;
using UnityEngine.UI;

public class UIReplayWindow : UIBase
{
    public const float WAIT_TIME = 0.5f;

    private Text __localScore;
    private Text __bestScore;

    private int __type;

    private void Start()
    {
        __type = 0;
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

    IEnumerator __ContinueLogin()
    {
        gameObject.SetActive(false);
        yield return UnityExtension.WaitForSeconds(WAIT_TIME);
        GameRoot.s_Instance.EnterState(GameRoot.State.LOGIN, null);
    }

    IEnumerator __ContinueRefresh()
    {
        gameObject.SetActive(false);
        yield return UnityExtension.WaitForSeconds(WAIT_TIME);
        GameRoot.s_Instance.EnterState(GameRoot.State.GAME, null);
    }

    void OnPlayComplete()
    {

    }

    public void OnMainPage()
    {
        __type = 1;
        GameRoot.s_Instance.StartCoroutine(__ContinueLogin());
    }

    public void OnRestart()
    {
        __type = 2;
        GameRoot.s_Instance.StartCoroutine(__ContinueRefresh());
    }
}