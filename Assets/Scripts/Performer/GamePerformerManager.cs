using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.EventSystems;

public class GamePerformerManager : MonoBehaviour
{
    private class InputManager : BlockSprite.IInput
    {
        private Vector3 __sourcePos;
        private Transform __sourceParent;
        private float __targetZ;
        private List<int> __hightLights;
        private GamePerformerManager __owner;
        public InputManager(GamePerformerManager owner)
        {
            __owner = owner;
            __hightLights = new List<int>();
        }

        public void OnBeginDrag(BlockSprite bs, PointerEventData eventData)
        {
            var srcPos = bs.transform.position;
            __sourcePos = srcPos;
            __targetZ = Camera.main.transform.position.z + Camera.main.nearClipPlane + 0.001f;
            srcPos.z = __targetZ;
            __sourceParent = bs.transform.parent;
            bs.transform.position = srcPos;
            bs.transform.SetParent(null);
            bs.transform.localScale = Vector3.one;
        }

        public void OnDrag(BlockSprite bs, PointerEventData eventData)
        {
            var worldPos  = Camera.main.ScreenToWorldPoint(eventData.position);
            worldPos.z = __targetZ;
            bs.transform.position = worldPos;

            int mapIndex = GetMapIndex(bs.GetLeftBottomCornerPosition(__owner.gridSize));

            //Debug.LogError( (mapIndex % GameDataMeta.s_Instance.mapWidth) + " " + (mapIndex / GameDataMeta.s_Instance.mapWidth) );
            if (mapIndex == -1)
                return;

            __hightLights.Clear();
            __owner.__coreLogic.CheckPush(bs.ownerIndex, mapIndex, __hightLights);
        }

        int GetMapIndex(Vector3 pos)
        {
            var gameMeta = GameDataMeta.s_Instance;
            var gridPos = new Vector2(pos.x, pos.y);
            var halfScene = new Vector2((gameMeta.mapWidth >> 1) * __owner.gridSize, (gameMeta.mapHeight >> 1) * __owner.gridSize);

            if (pos.x < -halfScene.x || pos.x > halfScene.x || pos.y < -halfScene.y || pos.y > halfScene.y)
                return -1;

            gridPos += halfScene;
            gridPos -= new Vector2(__owner.gridSize * 0.5f, __owner.gridSize * 0.5f);

            return Mathf.RoundToInt(gridPos.y / __owner.gridSize ) * gameMeta.mapWidth + Mathf.RoundToInt(gridPos.x / __owner.gridSize);
        }

        Vector3 ComputePosition(int x, int y, float z)
        {
            float halfGridSize = __owner.gridSize * 0.5f;
            var vecPos = new Vector3(x * __owner.gridSize + halfGridSize, y * __owner.gridSize + halfGridSize, z);

            var gameMeta = GameDataMeta.s_Instance;
            var halfScene = new Vector2((gameMeta.mapWidth >> 1) * __owner.gridSize, (gameMeta.mapHeight >> 1) * __owner.gridSize);
            vecPos.x -= halfScene.x;
            vecPos.y -= halfScene.y;

            return vecPos;
        }

        void __RestoreBS(BlockSprite bs)
        {
            bs.transform.position = __sourcePos;
            bs.transform.SetParent(__sourceParent);
            bs.transform.localScale = Vector3.one;
        }

        public void OnEndDrag(BlockSprite bs, PointerEventData eventData)
        {
            int mapIndex = GetMapIndex(bs.GetLeftBottomCornerPosition(__owner.gridSize));

            if(mapIndex != -1)
            {
                var gameMeta = GameDataMeta.s_Instance;
                var blockData = gameMeta.blockConfigArray[__owner.__coreLogic.currentRoungData.lineNodes[bs.ownerIndex].index];

                var gameState = __owner.__coreLogic.PushStep(
                    bs.ownerIndex, 
                    mapIndex, 
                    __hightLights, 
                    out var isPushOK, 
                    out var isRefreshEnable);

                if (isPushOK)
                {
                    //push to grid
                    int startX = mapIndex % gameMeta.mapWidth;
                    int startY = mapIndex / gameMeta.mapWidth;

                    for (int x = 0; x < blockData.width; ++x)
                    {
                        for (int y = 0; y < blockData.height; ++y)
                        {
                            if (blockData.dataArray[y*blockData.width + x] == 0)
                                continue;

                            int tmpMapIndex = x+startX + (y+startY) * gameMeta.mapWidth;
                            var gridSprite = __owner.__AllocateGridSprite();
                            gridSprite.transform.SetParent(__owner.gridRoot, false);
                            gridSprite.transform.localPosition = ComputePosition(x + startX, y + startY, 0.0f);
                            __owner.__coreLogic.SetMapUserData(tmpMapIndex, gridSprite);
                        }
                    }

                    //消失
                    int i, length = __hightLights.Count;
                    for (i = 0; i < length; ++i)
                    {
                        var node = __owner.__coreLogic.GetMapUserData(__hightLights[i]) as GridSprite;
                        if (node != null)
                        {
                            __owner.__FreeGridSprite(node);
                            __owner.__coreLogic.SetMapUserData(__hightLights[i], null);
                        }
                    }

                    //释放自己
                    __owner.__FreeBlockSprite(bs);

                    if (isRefreshEnable)
                        __owner.__coreLogic.RefreshLineRound();
                }
                else
                    __RestoreBS(bs);
            }
            else
                __RestoreBS(bs);
        }
    }

    public Sprite[] sprites;
    public BlockSprite blockSpritePrefab;
    public SpriteRenderer spPrefab;
    public GridSprite gridSpritePrefab;

    public float gridSize;

    public Transform gridRoot;
    public Transform blockRoot;

    private InputManager __inputManager;
    private Stack<BlockSprite> __blockSpritePool;
    private Stack<GridSprite> __gridSpritePool;


    private GameCoreLogic __coreLogic;

    void OnLineRoundChanged()
    {
        //apply block
        var gameMeta = GameDataMeta.s_Instance;
        var lineNodes = __coreLogic.currentRoungData.lineNodes;
        int i, length = lineNodes.Length;
        for(i=0; i<length; ++i)
        {
            var node = lineNodes[i];

            var bs = __AllocateBlockSprite();
            bs.ApplyBlock(
                spPrefab,
                sprites[UnityEngine.Random.Range(0, sprites.Length)], 
                gridSize, 
                __inputManager, 
                i,
                gameMeta.blockConfigArray[node.index]);

            bs.transform.SetParent(blockRoot.transform.GetChild(i), false);
            bs.transform.localPosition = Vector3.zero;
        }
    }

    GridSprite __AllocateGridSprite()
    {
        GridSprite bs = null;
        if (__gridSpritePool.Count > 0)
        {
            bs = __gridSpritePool.Pop();
            bs.gameObject.SetActive(true);
        }
        else
            bs = GridSprite.Instantiate(gridSpritePrefab);

        return bs;
    }

    void __FreeGridSprite(GridSprite gs)
    {
        gs.gameObject.SetActive(false);
        gs.transform.SetParent(gridRoot.transform, false);
        __gridSpritePool.Push(gs);
    }

    BlockSprite __AllocateBlockSprite()
    {
        BlockSprite bs = null;
        if (__blockSpritePool.Count > 0)
        {
            bs = __blockSpritePool.Pop();
            bs.gameObject.SetActive(true);
        }
        else
            bs = BlockSprite.Instantiate(blockSpritePrefab);

        return bs;
    }

    void __FreeBlockSprite(BlockSprite bs)
    {
        bs.Free(blockRoot);
        bs.transform.SetParent(blockRoot.transform, false);
        __blockSpritePool.Push(bs);
    }

    public IEnumerator Init(GameCoreLogic coreLogic)
    {
        __inputManager = new InputManager(this);

        __blockSpritePool = new Stack<BlockSprite>();
        __gridSpritePool = new Stack<GridSprite>();

        __coreLogic = coreLogic;
        __coreLogic.lineRoundChangedEvent += OnLineRoundChanged;

        //此处可以播放一些入场动画
        yield return null;
    }

    private void OnDisable()
    {
        __coreLogic.lineRoundChangedEvent -= OnLineRoundChanged;
        BlockSprite.FreeSPPool();
    }
}