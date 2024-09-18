using System.Collections.Generic;
using PixelSort.Feature.GridGeneration;

public interface ILevelManager
{
    void OnLevelWin();
    int GetCurrentLevelIndex();
    LevelData GetCurrentLevel { get; }
    bool isOfMultiTierLevel { get; }
    void IncrementLevel();
    void ReloadLevel();
    int LevelAttempt { get; set; }
    int GetSubLevel { get; }
    int GetTotalSubLevelCount { get; }
    int CurrentRunningLevel { get; set; }
    int SubLevelPref { get; set; }
    public List<LevelData> SubLevelList{ get; set; }
    public int Coins { get; set; }
}