using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;

public partial class GameCoreLogic
{
    public const int GRID_EMPTY = 0;
    public const int GRID_BLOCK = 1;
    public const int BEGIN_ITEM_INDEX = 2;

    public enum GameState
    {
        RUNNING,
        FAIL,
        SUCCESS,
    }
    
    public struct MapGridState
    {
        public int value;
        public object userData;
    }

    public struct BlockNode
    {
        public int index;
        public int starIndex;
    }

    public class RoundState
    {
        public const int MAX_COUNT = 3;

        public BlockNode[] lineNodes;

        public RoundState()
        {
            lineNodes = new BlockNode[MAX_COUNT];
            for (int i = 0; i < MAX_COUNT; ++i)
                SetNodeEmpty(i);
        }

        public void SetNodeEmpty(int index)
        {
            lineNodes[index] = new BlockNode { index = -1, starIndex = -1 };
        }

        public bool IsNodeEmpty(int index)
        {
            return lineNodes[index].index == -1;
        }
    }

    private interface IPushGridHandler
    {
        void OnCombineHorLine(int y, int startX, int endX);
        void OnCombineVerLine(int x, int startX, int endX);
    }

    private class CheckCounterHandler : IPushGridHandler
    {
        public int counter;

        public void OnCombineHorLine(int y, int startX, int endX)
        {
            ++counter;
        }

        public void OnCombineVerLine(int x, int startY, int endY)
        {
            ++counter;
        }
    }

    private class CheckPushHandler : IPushGridHandler
    {
        public GameCoreLogic owner;
        public List<int> highLightGrids;

        public void OnCombineHorLine(int y, int startX, int endX)
        {
            for (int x = startX; x < endX; ++x)
            {
                highLightGrids.Add(y * owner.__width + x);
            }
        }

        public void OnCombineVerLine(int x, int startY, int endY)
        {
            for (int y = startY; y < endY; ++y)
            {
                highLightGrids.Add(y * owner.__width + x);
            }
        }
    }

    private class PushGridHandler : IPushGridHandler
    {
        public GameCoreLogic owner;
        public int lineCount;
        public List<int> highLightGrids;

        void __OnChanged()
        {
            ++lineCount;
        }

        public void OnCombineHorLine(int y, int startX, int endX)
        {
            __OnChanged();

            for (int x = startX; x < endX; ++x)
            {
                int index = y * owner.__width + x;
                var srcData = owner.__mapData[index];
                srcData.value = GRID_EMPTY;
                owner.__mapData[index] = srcData;
            }

            if (highLightGrids == null)
                return;

            for (int x = startX; x < endX; ++x)
            {
                highLightGrids.Add(y * owner.__width + x);
            }
        }

        public void OnCombineVerLine(int x, int startY, int endY)
        {
            __OnChanged();
            for (int y = startY; y < endY; ++y)
            {
                int index = y * owner.__width + x;
                var srcData = owner.__mapData[index];
                srcData.value = GRID_EMPTY;
                owner.__mapData[index] = srcData;
            }

            if (highLightGrids == null)
                return;

            for (int y = startY; y < endY; ++y)
            {
                highLightGrids.Add(y * owner.__width + x);
            }
        }
    }

    public Action lineRoundChangedEvent;
    public Action scoreChangedEvent;

    private int __round;
    private int __width;
    private int __height;

    public int score
    {
        private set;
        get;
    }

    public int lastBlastStep
    {
        private set;
        get;
    }

    public int currentComboIndex
    {
        private set;
        get;
    }

    private MapGridState[] __mapData;
    public RoundState currentRoungData { get; }

    private CheckPushHandler __checkHandler;
    private PushGridHandler __pushHandler;
    private CheckCounterHandler __counterHandler;

    private List<int> __fillStack;

    public GameCoreLogic()
    {
        __fillStack = new List<int>();
        __checkHandler = new CheckPushHandler();
        __checkHandler.owner = this;

        __pushHandler = new PushGridHandler();
        __pushHandler.owner = this;

        __counterHandler = new CheckCounterHandler();
        currentRoungData = new RoundState();

        __InitRefreshValues();
    }

    public void Init()
    {
        __width = GameDataMeta.s_Instance.mapWidth;
        __height = GameDataMeta.s_Instance.mapHeight;

        lastBlastStep = 0;
        currentComboIndex = -1;
        int i, length = __width * __height;
        __mapData = new MapGridState[length];
        for (i = 0; i < length; ++i)
            __mapData[i] = new MapGridState { value = GRID_EMPTY };

        __round = 0;
    }

    public void RefreshLineRound()
    {
        ++__round;

        __RefreshRoundData();

        lineRoundChangedEvent?.Invoke();
    }

    bool __CheckPushGridByConfigIndex(int configIndex, int mapIndex)
    {
        var gameDataMeta = GameDataMeta.s_Instance;
        var blockData = gameDataMeta.blockConfigArray[configIndex];

        int startX = mapIndex % __width;
        int startY = mapIndex / __width;
        int endX = blockData.width + startX;
        int endY = blockData.height + startY;

        if (endX > __width || endY > __height || startX < 0 || startY < 0)
            return false;

        for (int x = 0; x < blockData.width; ++x)
        {
            for (int y = 0; y < blockData.height; ++y)
            {
                if (blockData.dataArray[x + y * blockData.width] == 0)
                    continue;

                var tmpMapIndex = (y + startY) * __width + x + startX;
                if (__mapData[tmpMapIndex].value != GRID_EMPTY)
                {
                    return false;
                }
            }
        }

        return true;
    }

    bool __CheckPushGrid(
        int index,
        int mapIndex)
    {
        return __CheckPushGrid(currentRoungData.lineNodes[index].index, mapIndex);
    }

    bool __TryPushGridWithConfigIndex(
        int configIndex,
        int mapIndex,
        bool isEndRevert,
        IPushGridHandler handler,
        NativeList<int> callbackStack = default)
    {
        var gameDataMeta = GameDataMeta.s_Instance;
        var blockData = gameDataMeta.blockConfigArray[configIndex];

        int startX = mapIndex % __width;
        int startY = mapIndex / __width;
        int endX = blockData.width + startX;
        int endY = blockData.height + startY;

        if (endX > __width || endY > __height || startX < 0 || startY < 0)
            return false;

        __fillStack.Clear();

        bool isFillEnable = true;
        for (int x = 0; x < blockData.width; ++x)
        {
            for (int y = 0; y < blockData.height; ++y)
            {
                if (blockData.dataArray[x + y * blockData.width] == 0)
                    continue;

                var tmpMapIndex = (y + startY) * __width + x + startX;
                if (__mapData[tmpMapIndex].value != GRID_EMPTY)
                {
                    isFillEnable = false;
                    break;
                }

                __mapData[tmpMapIndex].value = GRID_BLOCK;
                __fillStack.Add(tmpMapIndex);
                if (callbackStack.IsCreated)
                    callbackStack.Add(tmpMapIndex);
            }

            if (!isFillEnable)
                break;
        }

        int i, length = __fillStack.Count;
        if (!isFillEnable)
        {
            for (i = 0; i < length; ++i)
            {
                __mapData[__fillStack[i]] = new MapGridState { value = GRID_EMPTY };
            }

            if (callbackStack.IsCreated)
                callbackStack.Clear();

            return false;
        }

        //横线扫描
        startX = 0;
        endX = __width;
        startY = 0;
        endY = __height;
        for (int y = startY; y < endY; ++y)
        {
            bool isLine = true;
            for (int x = startX; x < endX; ++x)
            {
                var tmpMapIndex = y * __width + x;
                if (__mapData[tmpMapIndex].value == GRID_EMPTY)
                {
                    isLine = false;
                    break;
                }
            }

            if (isLine)
                handler.OnCombineHorLine(y, startX, endX);
        }

        //竖线扫描
        for (int x = startX; x < endX; ++x)
        {
            bool isLine = true;
            for (int y = startY; y < endY; ++y)
            {
                var tmpMapIndex = y * __width + x;
                if (__mapData[tmpMapIndex].value == GRID_EMPTY)
                {
                    isLine = false;
                    break;
                }
            }

            if (isLine)
                handler.OnCombineVerLine(x, startY, endY);
        }

        if (isEndRevert)
        {
            for (i = 0; i < length; ++i)
                __mapData[__fillStack[i]] = new MapGridState { value = GRID_EMPTY };
        }

        return true;
    }

    bool __TryPushGridWithLineIndex(
        int index, 
        int mapIndex, 
        bool isEndRevert, 
        IPushGridHandler handler)
    {
        return __TryPushGridWithConfigIndex(
            currentRoungData.lineNodes[index].index, 
            mapIndex, 
            isEndRevert, 
            handler);
    }

    public bool CheckPush(int index, int mapIndex, List<int> highLightGrids)
    {
        highLightGrids.Clear();

        __checkHandler.highLightGrids = highLightGrids;
        return __TryPushGridWithLineIndex(index, mapIndex, true, __checkHandler);
    }

    public void SetMapUserData(int mapIndex, object userData)
    {
        var srcData = __mapData[mapIndex];
        srcData.userData = userData;
        __mapData[mapIndex] = srcData;
    }

    public object GetMapUserData(int mapIndex)
    {
        return __mapData[mapIndex].userData;
    }

    public unsafe GameState PushStep(
        int index, 
        int mapIndex, 
        List<int> highLightGrids, 
        out bool isPushOK, 
        out bool isRefreshEnable)
    {
        isPushOK = false;
        isRefreshEnable = false;
        highLightGrids.Clear();

        //把方块放上去 检测消除 
        __pushHandler.lineCount = 0;
        __pushHandler.highLightGrids = highLightGrids;
        bool isPushEnable = __TryPushGridWithLineIndex(index, mapIndex, false, __pushHandler);

        //如果没有放成功，直接返回
        if(!isPushEnable)
            return GameState.RUNNING;

        isPushOK = true;

        currentRoungData.SetNodeEmpty(index);

        var gameMeta = GameDataMeta.s_Instance;

        //计算连击
        if(__pushHandler.lineCount == 0)
        {
            if(lastBlastStep > 0)
            {
                --lastBlastStep;
                if (lastBlastStep == 0)
                    currentComboIndex = -1;
            }
        }
        else
        {
            lastBlastStep = gameMeta.comboStepMax;
            if(currentComboIndex < gameMeta.comboScoreScale.Length-1)
                ++currentComboIndex;

            int scoreScale = currentComboIndex >= 0 ? gameMeta.comboScoreScale[currentComboIndex] : 1;

            //计算得分
            int scoreIndex = __pushHandler.lineCount >= gameMeta.blastScoreArray.Length ? gameMeta.blastScoreArray.Length-1 : __pushHandler.lineCount-1;
            score += gameMeta.blastScoreArray[scoreIndex] * scoreScale;

            scoreChangedEvent?.Invoke();
        }

        int emptyCount = 0;
        int* noneEmptyIndices = stackalloc int[3];
        int i;
        for(i=0; i< RoundState.MAX_COUNT; ++i)
        {
            if (!currentRoungData.IsNodeEmpty(i))
                noneEmptyIndices[emptyCount++] = i;
        }

        isRefreshEnable = emptyCount == 0;

        if (emptyCount > 0)
        {
            bool anySuccess = false;
            for(i=0; i<emptyCount; ++i)
            {
                if(__CheckCanFillBlockWithLineIndex(noneEmptyIndices[i]))
                {
                    anySuccess = true;
                    break;
                }
            }

            if (!anySuccess)
                return GameState.FAIL;
        }

        //如果没结束，检测是否为最后一道
        return GameState.RUNNING;
    }

    bool __CheckCanFillBlockWithLineIndex(int lineIndex)
    {
        return __CheckCanFillBlockWithConfigIndex(currentRoungData.lineNodes[lineIndex].index);
    }

    bool __CheckCanFillBlockWithConfigIndex(int configIndex)
    {
        //遍历所有的空格子，尝试放进去
        int i, j;
        for (i = 0; i < __width; ++i)
        {
            for(j=0; j<__height; ++j)
            {
                int mapIndex = j*__width + i;
                if (__mapData[mapIndex].value != GRID_EMPTY)
                    continue;

                if (__CheckPushGridByConfigIndex(configIndex, mapIndex))
                    return true;
            }
        }

        return false;
    }
}
