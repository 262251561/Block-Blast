using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.EventSystems;

public class BlockSprite : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
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

    public interface IInput
    {
        void OnBeginDrag(BlockSprite bs, PointerEventData eventData);
        void OnDrag(BlockSprite bs, PointerEventData eventData);
        void OnEndDrag(BlockSprite bs, PointerEventData eventData);
    }

    private IInput __input;
    private BoxCollider2D __collider2D;

    [HideInInspector]
    public int ownerIndex;

    private void Awake()
    {
        ownerIndex = -1;
        __collider2D = gameObject.GetComponent<BoxCollider2D>();
    }

    static private List<Transform> s_TempFree = new List<Transform>();
    public void Free(Transform freeRoot)
    {
        s_TempFree.Clear();
        for (int i = 0; i < transform.childCount; ++i)
            s_TempFree.Add(transform.GetChild(i));

        for(int i=0; i<s_TempFree.Count; ++i)
            __FreeSP(freeRoot, s_TempFree[i].gameObject.GetComponent<SpriteRenderer>());

        gameObject.SetActive(false);
    }

    public void ApplyBlock(
        SpriteRenderer spPrefab,
        Sprite sprite, 
        float gridSize,
        IInput inputHandler,
        int index, 
        GameDataMeta.BlockData blockData)
    {
        ownerIndex = index;
        __input = inputHandler;

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
            }
        }

        __collider2D.size = localSize;
    }

    public Vector3 GetLeftBottomCornerPosition(float gridSize)
    {
        var pos = transform.position;
        var halfSize = __collider2D.size * 0.5f;
        var halfGridSize = gridSize * 0.5f;
        pos.x -= halfSize.x;
        pos.y -= halfSize.y;

        pos.x += halfGridSize;
        pos.y += halfGridSize;

        return pos;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (__input != null)
            __input.OnBeginDrag(this, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (__input != null)
            __input.OnDrag(this, eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (__input != null)
            __input.OnEndDrag(this, eventData);
    }
}