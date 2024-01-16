using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class BlockDragGhostLogic
{
    private int __lastMapIndex;
    private List<int> __hightLightSprites;
    private List<GridSprite> __ghostGridSprites;

    private GameCoreLogic __coreLogic;
    private GamePerformerManager __performerManager;

    public BlockDragGhostLogic(GamePerformerManager performerManager, GameCoreLogic coreLogic)
    {
        __performerManager = performerManager;
        __coreLogic = coreLogic;
        __ghostGridSprites = new List<GridSprite>();
        __hightLightSprites = new List<int>();
    }

    public void OnBeginDrag(BlockSprite bs)
    {
        __lastMapIndex = -1;

        int i, length = bs.GetChildSpriteRenderCount();
        for(i=0; i<length; ++i)
        {
            var gridSp = __performerManager.AllocateGridSprite();
            gridSp.ApplySprite( bs.GetChildSpriteRender(i).sprite );
            gridSp.transform.SetParent(__performerManager.gridRoot, false);
            __ghostGridSprites.Add(gridSp);
        }

        __SetGhostActive(false);
    }

    void __SetGhostActive(bool isActive)
    {
        if (__ghostGridSprites[0].gameObject.activeSelf == isActive)
            return;

        int i, length = __ghostGridSprites.Count;
        for (i = 0; i < length; ++i)
            __ghostGridSprites[i].gameObject.SetActive(isActive);
    }

    public void OnDrag(
        BlockSprite bs, 
        int mapIndex, 
        bool isFilledOK, 
        List<int> hightLights)
    {
        if(__lastMapIndex != mapIndex)
        {
            //revert hightlights
            int i, length = __hightLightSprites.Count;
            for(i=0; i<length; ++i)
            {
                var gpSprite = __coreLogic.GetMapUserData(__hightLightSprites[i]) as GridSprite;
                if (gpSprite == null)
                    continue;

                gpSprite.RestoreSprite();
            }
            __hightLightSprites.Clear();

            if (hightLights.Count > 0)
            {
                //receive hightlights
                length = hightLights.Count;
                for(i=0; i<length; ++i)
                {
                    var gpSprite = __coreLogic.GetMapUserData(hightLights[i]) as GridSprite;
                    if (gpSprite == null)
                        continue;

                    gpSprite.ApplyTempSprite(bs.GetChildSpriteRender(0).sprite);
                    __hightLightSprites.Add(hightLights[i]);
                }
            }

            if (isFilledOK)
            {
                var gameMeta = GameDataMeta.s_Instance;
                var blockData = gameMeta.blockConfigArray[__coreLogic.currentRoungData.lineNodes[bs.ownerIndex].index];

                int startX = mapIndex % gameMeta.mapWidth;
                int startY = mapIndex / gameMeta.mapWidth;
                int childIndex = 0;
                for (int x = 0; x < blockData.width; ++x)
                {
                    for (int y = 0; y < blockData.height; ++y)
                    {
                        if (blockData.dataArray[y * blockData.width + x] == 0)
                            continue;

                        var gridSprite = __ghostGridSprites[childIndex++];
                        gridSprite.transform.localPosition = __performerManager.ComputePosition(x + startX, y + startY, 0.0f);
                        gridSprite.gameObject.SetActive(true);
                        gridSprite.ApplyAlpha(0.3f);
                    }
                }
            }
            else
                __SetGhostActive(false);

            __lastMapIndex = mapIndex;
        }
    }

    public void OnEndDrag(BlockSprite bs)
    {
        int i, length = __ghostGridSprites.Count;
        for(i=0; i<length; ++i)
            __performerManager.FreeGridSprite(__ghostGridSprites[i]);

        __ghostGridSprites.Clear();
    }
}