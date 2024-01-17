using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

public partial class GameCoreLogic
{
    public const int BEGIN_REFRESH_CRSH_ROUND = 5;
    public const int BEGIN_REFRESH_SEARCH_ROUND = 40;
    public const int SEARCH_ROUND_COUNT = 3;
    public const int NORMAL_ROUND_COUNT = 3;

    private enum RefreshRoundState
    {
        SEARCH,
        NORMAL,
    }

    private List<int> __ingoreIndices;
    private List<int> __emptyMapIndices;
    private List<int> __randomListIndices;
    private int[] __bestRoundConfigs;

    private int __currentRoundIndex;
    private RefreshRoundState __refreshRoundState;

    void __InitRefreshValues()
    {
        __ingoreIndices = new List<int>();
        __emptyMapIndices = new List<int>();
        __randomListIndices = new List<int>();
        __bestRoundConfigs = new int[RoundState.MAX_COUNT];
        __refreshRoundState = RefreshRoundState.SEARCH;
        __currentRoundIndex = 0;
    }

    int __TryGetFilledBlockIndex()
    {
        __randomListIndices.Clear();
        int i, length = GameDataMeta.s_Instance.blockConfigArray.Length;
        for (i = 0; i < length; ++i)
            __randomListIndices.Add(i);

        while (__randomListIndices.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, __randomListIndices.Count);

            var configIndex = __randomListIndices[index];
            if (__ingoreIndices.Contains(configIndex))
            {
                __randomListIndices.RemoveAtSwapBack(index);
                continue;
            }

            if (__CanFillBlockWithConfigIndex(configIndex))
            {
                __ingoreIndices.Add(configIndex);
                return configIndex;
            }

            __randomListIndices.RemoveAtSwapBack(index);
        }

        return 0;
    }

    bool __CanFillBlockWithConfigIndex(int configIndex)
    {
        //遍历所有的空格子，尝试放进去
        int i, count = __emptyMapIndices.Count;
        for (i = 0; i < count; ++i)
        {
            if (__CheckPushGridByConfigIndex(configIndex, __emptyMapIndices[i]))
                return true;
        }

        return false;
    }

    bool __CanCrushWithConfigIndex(int configIndex)
    {
        //遍历所有的空格子，尝试放进去
        int i, count = __emptyMapIndices.Count;
        for (i = 0; i < count; ++i)
        {
            __counterHandler.counter = 0;
            __TryPushGridWithConfigIndex(
                configIndex,
                __emptyMapIndices[i],
                true,
                __counterHandler);

            if (__counterHandler.counter > 0)
                return true;
        }

        return false;
    }

    int __GetBlockIndex()
    {
        var gameMeta = GameDataMeta.s_Instance;
        if (__round >= BEGIN_REFRESH_CRSH_ROUND)
        {
            __randomListIndices.Clear();
            int i, length = gameMeta.blockConfigArray.Length;
            for (i=0; i<length; ++i)
                __randomListIndices.Add(i);

            for (i = 0; i < length; ++i)
            {
                var index = UnityEngine.Random.Range(0, __randomListIndices.Count);
                var randomConfigIndex = __randomListIndices[index];
                __randomListIndices.RemoveAtSwapBack(index);

                if (__ingoreIndices.Contains(randomConfigIndex))
                    continue;

                if (__CanCrushWithConfigIndex(randomConfigIndex))
                {
                    __ingoreIndices.Add(randomConfigIndex);
                    return randomConfigIndex;
                }
            }
        }

        return __TryGetFilledBlockIndex();
    }

    void __NormalRefresh()
    {
        for (int i = 0; i < RoundState.MAX_COUNT; ++i)
            currentRoungData.lineNodes[i] = new BlockNode { index = __GetBlockIndex() };
    }

    void __AIStepRefresh()
    {
        float bestScore = 0.0f;
        currentRoungData.lineNodes[0] = new BlockNode { index = __GetBlockIndex() };

        __SearchDepth(
            1,
            RoundState.MAX_COUNT-1,
            GameDataMeta.s_Instance.blockConfigArray, 
            ref bestScore);

        for (int i = 1; i < RoundState.MAX_COUNT; ++i)
            currentRoungData.lineNodes[i] = new BlockNode { index = __bestRoundConfigs[i] };
    }

    void __SearchDepth(
        int depth, 
        int maxDepth,
        GameDataMeta.BlockData[] configArray, 
        ref float bestScore)
    {
        if (depth > maxDepth)
            return;

        //此处数量不太对，就不纠结了，下面可以判断是否能放入
        int i, length = Mathf.Min(16, __emptyMapIndices.Count), j, configCount = configArray.Length;
        for(i=0; i<length; ++i)
        {
            for(j=0; j<configCount; ++j)
            {
                var mapIndex = __emptyMapIndices[i];

                var searchStack = new NativeList<int>(Allocator.Temp);
                __counterHandler.counter = 0;
                if (__TryPushGridWithConfigIndex(
                    j, 
                    mapIndex,
                    false,
                    __counterHandler,
                    searchStack))
                {
                    float scaler = __counterHandler.counter * 10000.0f;
                    currentRoungData.lineNodes[depth] = new BlockNode { index = j };

                    if (depth < maxDepth)
                        __SearchDepth(
                            depth + 1,
                            maxDepth,
                            configArray,
                            ref bestScore);

                    // 如果是叶子节点 判断是否结束
                    if(depth == maxDepth)
                    {
                        //预估份值
                        float currentScore = scaler;
                        for(int k=0; k<RoundState.MAX_COUNT; ++k)
                        {
                            var index = currentRoungData.lineNodes[k].index;
                            currentScore += configArray[index].width * configArray[index].height;
                        }

                        if (currentScore >= bestScore)
                        {
                            bestScore = currentScore;
                            for (int k = 0; k < RoundState.MAX_COUNT; ++k)
                            {
                                __bestRoundConfigs[k] = currentRoungData.lineNodes[k].index;
                            }
                        }
                    }

                    //revert stack
                    {
                        for(int k=0, kCount = searchStack.Length; k<kCount; ++k)
                            __mapData[searchStack[k]] = new MapGridState { value = GRID_EMPTY };

                        currentRoungData.lineNodes[depth] = default;
                    }
                }
            }
        }
    }

    void __FillToEmptyMapIndices()
    {
        __emptyMapIndices.Clear();

        int i, j;
        for (i = 0; i < __width; ++i)
        {
            for (j = 0; j < __height; ++j)
            {
                int mapIndex = j * __width + i;
                if (__mapData[mapIndex].value != GRID_EMPTY)
                    continue;

                __emptyMapIndices.Add(mapIndex);
            }
        }
    }

    private void __RefreshRoundData()
    {
        __ingoreIndices.Clear();
        __FillToEmptyMapIndices();

        if (__round < BEGIN_REFRESH_SEARCH_ROUND)
            __NormalRefresh();
        else
        {
            ++__currentRoundIndex;
            switch(__refreshRoundState)
            {
                case RefreshRoundState.SEARCH:
                    __AIStepRefresh();
                    if (__currentRoundIndex >= SEARCH_ROUND_COUNT)
                        __refreshRoundState = RefreshRoundState.NORMAL;
                    break;
                case RefreshRoundState.NORMAL:
                    __NormalRefresh();
                    if (__currentRoundIndex >= NORMAL_ROUND_COUNT)
                        __refreshRoundState = RefreshRoundState.SEARCH;
                    break;
            }
        }
    }
}