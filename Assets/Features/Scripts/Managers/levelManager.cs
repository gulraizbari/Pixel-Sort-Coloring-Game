using UnityEngine;
using System.Collections.Generic;
using System;
using Sablo.Analytics;
using Sablo.Gameplay;
using Sirenix.OdinInspector;

public class levelManager : BaseGameplayModule, IlevelManager
{
    public bool Is_debug,IsSetCoins;
    [SerializeField] private int _level = 0, CoinToSet=0;
    [SerializeField] private List<LevelData> levelData = new List<LevelData>();
    [SerializeField] private LevelData _currentLevel;
    [SerializeField] private int subLevelNum = 0;
    private int totalSubLevels;
    public int currentLevel;
    private bool isMultiTierLevel;
    private List<LevelData> subLevels;
    public bool isSaveSystemActive;

    public LevelData GetCurrentLevel => _currentLevel;

    public int GetSubLevel => subLevelNum;

    public List<LevelData> SubLevelList{ get => subLevels; set => subLevels = value; }
    

    public int GetTotalSubLevelCount => totalSubLevels;
    
     bool IlevelManager.isOfMultiTierLevel => isMultiTierLevel;

    public override void Initialize()
    {

// #if UNITY_EDITOR
//         if (Is_debug)
//         {
//             PlayerPrefs.SetInt("level", _level);
//             currentLevel = PlayerPrefs.GetInt("level");
//         }
//         if (IsSetCoins)
//         {
//            // PlayerPrefs.SetInt(Constant.Coin, CoinToSet);
//         }
// #endif
       
            currentLevel = PlayerPrefs.GetInt("level");
            if (currentLevel > levelData.Count-1)
            {
                currentLevel=0;
            }
            if(isSaveSystemActive)
                subLevelNum = SubLevelPref;       // for save system usage 
            GameLoop.Instance.UpdateLevelNo(currentLevel+1);
            GameLoop.Instance.ActivateTutorialPanel(currentLevel);

        GameLoop.Instance.RefreshCoins();
        LionAnalyticEvents.MissionStarted(false,"GamePlay",Level+1 , subLevelNum + 1 , LevelAttempt);
        LoadLevel();
    }

    public void LoadLevel()
    {
        int level = PlayerPrefs.GetInt("level");
        if (level > levelData.Count-1)
        {
            level=0;
            PlayerPrefs.SetInt("level",0);
        }
        _currentLevel = levelData[level];
        SubLevelList = _currentLevel.subLevel;
        isMultiTierLevel = _currentLevel.isMultiTierLevel;
        totalSubLevels = _currentLevel.subLevel.Count;
        GameLoop.Instance.UpdateSubLevelNo(_currentLevel.subLevel.Count-1, subLevelNum);
        if (_currentLevel.subLevel.Count != 0)
        {
            _currentLevel = _currentLevel.subLevel[subLevelNum];
        }
    }
    
    [Button]
    void IlevelManager.ReloadLevel()
    {   
        subLevelNum++;
        TapController.Instance.SetNoOfCarriersPass(0);
        if(isSaveSystemActive) 
            SubLevelPref =   subLevelNum;  // for save system usage 
        LionAnalyticEvents.MissionStepCompleted(false,"GamePlay",Level+1 , subLevelNum + 1 , LevelAttempt); //missionStep
        int level = PlayerPrefs.GetInt("level");
        if (level > levelData.Count-1)
        {
            level=0;
            currentLevel = 0;
            PlayerPrefs.SetInt("level",0);
        }
        _currentLevel = levelData[level];
        if (_currentLevel.subLevel.Count != 0)
        {
            _currentLevel = _currentLevel.subLevel[subLevelNum];
        }
        Debug.LogError("ReloadLevel");
    }

    void IlevelManager.OnLevelWin()
    {
        int level = currentLevel;

        if (level < levelData.Count-1/*Utils.MaxLevel*/)
        {
            IncrementLevel();
        }
        else if (level == levelData.Count-1/*Utils.MaxLevel*/)
        {
            LevelToStart();
        }
    }

    private void IncrementLevel()
    {
        int level = PlayerPrefs.GetInt("level", 0);
        if (_level >= levelData.Count-1)
        {
            level = 0;
            PlayerPrefs.SetInt("level", 0);
        }
        else
        {
            level++;
            PlayerPrefs.SetInt("level", level);
        }
    }

    private void LevelToStart()
    {
        PlayerPrefs.SetInt("level", 0);
    }

    int IlevelManager.GetCurrentLevelIndex()
    {
        return currentLevel;
    }

    void IlevelManager.IncrementLevel()
    {
        IncrementLevel();
    }

    public int CurrentRunningLevel { get=>PlayerPrefs.GetInt("CurrentRunningLevel"); set=>PlayerPrefs.SetInt("CurrentRunningLevel",value); }
    public int Level { get=>PlayerPrefs.GetInt("level"); set=>PlayerPrefs.SetInt("level",value); }
    public int LevelAttempt { get=>PlayerPrefs.GetInt("LevelAttempt",1); set=>PlayerPrefs.SetInt("LevelAttempt",value); }
    
    public int Coins { get=>PlayerPrefs.GetInt("Coins",0); set=>PlayerPrefs.SetInt("Coins",value); }
    
    public int SubLevelPref
    {
        get => PlayerPrefs.GetInt("SubLevel"+currentLevel, 0);
        set => PlayerPrefs.SetInt("SubLevel"+currentLevel, value);
    }

}
