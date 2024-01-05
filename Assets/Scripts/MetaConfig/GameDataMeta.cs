using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

public class GameDataMeta : MonoBehaviour
{
    [Serializable]
    public struct BlockData
    {
        public int width;
        public int height;
        public int[] dataArray;
    }
    public enum LevelTarget
    {
        SCORE,
        ITEM_1,
        ITEM_2,
        ITEM_3,
        ITEM_4,
        ITEM_5,
    }

    public struct LevelTargetPair
    {
        public LevelTarget targetType;
        public int socre;
    }

    public static GameDataMeta s_Instance = null;

    public int mapWidth;
    public int mapHeight;

    public BlockData[] blockConfigArray;

    public int[] blastScoreArray;

    public int comboStepMax;
    public int[] comboScoreScale;


    private void Awake()
    {
        s_Instance = this;
    }
}