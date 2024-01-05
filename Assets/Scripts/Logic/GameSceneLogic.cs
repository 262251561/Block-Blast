using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

public class GameSceneLogic : MonoBehaviour
{
    public static GameSceneLogic s_Instance;

    public GameCoreLogic coreLogic { private set; get; }
    public GamePerformerManager performerManager { private set; get; }

    IEnumerator Start()
    {
        s_Instance = this;
        yield return null;

        coreLogic = new GameCoreLogic();
        coreLogic.Init();

        performerManager = gameObject.GetComponent<GamePerformerManager>();
        yield return performerManager.Init(coreLogic);

        coreLogic.RefreshLineRound();
    }

    private void OnDestroy()
    {
        s_Instance = null;
    }
}