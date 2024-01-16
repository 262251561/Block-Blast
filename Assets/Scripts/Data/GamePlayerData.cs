using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GamePlayerData
{
    public static GamePlayerData s_Instance;

    public int bestScore
    {
        private set;
        get;
    }

    public GamePlayerData()
    {
        s_Instance = this;
    }

    public void Load()
    {
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
    }

    public void ApplyBestScore(int value)
    {
        bestScore = value;
        PlayerPrefs.SetInt("BestScore", value);
    }
}