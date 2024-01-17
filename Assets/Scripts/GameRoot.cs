using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRoot : MonoBehaviour
{
    public enum State
    {
        NONE,
        LOGIN,
        GAME
    }

    private struct EnterNode
    {
        public State state;
        public object userData;
    }

    public static GameRoot s_Instance;

    public State currentState;
    private Queue<EnterNode> __queueNodes;

    private void Awake()
    {
        s_Instance = this;        
    }
    
    void Start()
    {
        GameObject.DontDestroyOnLoad(gameObject);

        var playerData = new GamePlayerData();
        playerData.Load();

        currentState = State.NONE;

        __queueNodes = new Queue<EnterNode>();
        StartCoroutine(__RunLogic());

        EnterState(State.LOGIN, null);
    }

    public void EnterState( State state, object userData)
    {
        __queueNodes.Enqueue( new EnterNode { state = state, userData = userData } );
    }

    IEnumerator __LoadScene(int sceneIndex)
    {
        switch(currentState)
        {
            case State.LOGIN:
                yield return SceneManager.UnloadSceneAsync(0);
                break;
            case State.GAME:
                yield return SceneManager.UnloadSceneAsync(1);
                break;
        }

        var loader = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Single);
        while(!loader.isDone)
        {
            yield return null;
        }
    }
    
    IEnumerator __RunLogic()
    {
        while(true)
        {
            if(__queueNodes.Count > 0)
            {
                var headNode = __queueNodes.Dequeue();
                switch(headNode.state)
                {
                    case State.LOGIN:
                        yield return __LoadScene(1);
                        break;
                    case State.GAME:
                        yield return __LoadScene(2);
                        break;
                }
            }

            yield return null;
        }
    }
}
