using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.EventSystems;

public class BlockSprite : MonoBehaviour
{
    public static void FreeSPPool()
    {
        s_SpPool.Clear();
    }

    private static Stack<SpriteRenderer> s_SpPool;
    private static SpriteRenderer __AllocateSP(Sprite sp, SpriteRenderer spPrefab)
    {
        if (s_SpPool == null)
            s_SpPool = new Stack<SpriteRenderer>();

        SpriteRenderer spNode = null;
        if (s_SpPool.Count > 0)
        {
            spNode = s_SpPool.Pop();
            spNode.gameObject.SetActive(true);
        }
        else
            spNode = SpriteRenderer.Instantiate(spPrefab);

        spNode.sprite = sp;

        return spNode;
    }

    private static void __FreeSP(Transform freeRoot, SpriteRenderer sp)
    {
        if (s_SpPool == null)
        {
            GameObject.Destroy(sp.gameObject);
            return;
        }

        sp.gameObject.SetActive(false);
        sp.transform.SetParent(freeRoot, false);
        s_SpPool.Push(sp);
    }

    private Vector2 __localSize;
    private List<SpriteRenderer> __spriteRenderers;

    [HideInInspector]
    public int ownerIndex;

    private void Awake()
    {
        ownerIndex = -1;
        __spriteRenderers = new List<SpriteRenderer>();
    }

    public SpriteRenderer GetChildSpriteRender(int index)
    {
        return __spriteRenderers[index];
    }

    public void Free(Transform freeRoot)
    {
        int i, length = __spriteRenderers.Count;
        for(i=0; i< length; ++i)
            __FreeSP(freeRoot, __spriteRenderers[i]);

        __spriteRenderers.Clear();

        gameObject.SetActive(false);
    }
    
    public void ApplyBlock(
        SpriteRenderer spPrefab,
        Sprite sprite, 
        float gridSize,
        int index, 
        GameDataMeta.BlockData blockData)
    {
        ownerIndex = index;

        if(__spriteRenderers.Count > 0)
            __spriteRenderers.Clear();

        float halfGridSize = gridSize * 0.5f;
        var localSize = new Vector2(blockData.width * gridSize, blockData.height * gridSize);
        var halfLocalSize = localSize * 0.5f;
        for (int x=0; x<blockData.width; ++x)
        {
            for(int y=0; y<blockData.height; ++y)
            {
                if (blockData.dataArray[y * blockData.width + x] == 0)
                    continue;

                var spNode = __AllocateSP(sprite, spPrefab);
                spNode.transform.SetParent(transform, false);
                var localPos = new Vector3(x * gridSize + halfGridSize, y * gridSize + halfGridSize, 0.0f);
                localPos.x -= halfLocalSize.x;
                localPos.y -= halfLocalSize.y;
                spNode.transform.localPosition = localPos;

                __spriteRenderers.Add(spNode);
            }
        }

        __localSize = localSize;
    }

    public Vector3 GetLeftBottomCornerPosition(float gridSize)
    {
        var pos = transform.position;
        var halfSize = __localSize * 0.5f;
        var halfGridSize = gridSize * 0.5f;
        pos.x -= halfSize.x;
        pos.y -= halfSize.y;

        pos.x += halfGridSize;
        pos.y += halfGridSize;

        return pos;
    }
}