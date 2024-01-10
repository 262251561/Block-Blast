using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TH;
using System.Collections;

public class GridSpriteHideLogic : CoroutineQueuePool.ICoroutNode
{
    private float __totalTime;
    private List<GridSprite> __sprites;
    private GamePerformerManager __performerManager;

    public GridSpriteHideLogic()
    {
        __sprites = new List<GridSprite>();
    }

    public void Refresh(GamePerformerManager pool)
    {
        __sprites.Clear();
        __performerManager = pool;
    }

    public void AddSprite(GridSprite sp)
    {
        __sprites.Add(sp);
    }

    public IEnumerator Run()
    {
        float totalTime = __performerManager.hideAnimTime;
        float time = 0.0f;
        int i, length = __sprites.Count;
        while(time <= totalTime)
        {
            yield return null;
            var progress = Mathf.Pow(time / totalTime, 3.0f);

            for (i = 0; i < length; ++i)
            {
                var sp = __sprites[i];
                sp.transform.localScale = Vector3.Lerp(sp.sourceScale, Vector3.zero, progress);
                sp.ApplyAlpha(Mathf.Lerp(1.0f, 0.0f, progress));
            }

            time += Time.deltaTime;
        }

        for(i=0; i<length; ++i)
            __performerManager.FreeGridSprite(__sprites[i]);

        ObjectPool<GridSpriteHideLogic>.s_instance.Free(this);
    }
}