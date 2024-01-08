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

    private Queue<EnterNode> __queueNodes;

    private void Awake()
    {
        s_Instance = this;        
    }
    
    void Start()
    {
        GameObject.DontDestroyOnLoad(gameObject);

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