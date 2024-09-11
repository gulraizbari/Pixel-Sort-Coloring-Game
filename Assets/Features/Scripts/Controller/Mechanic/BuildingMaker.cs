using System.Collections;
using System.Collections.Generic;
using Sablo.Gameplay;
using UnityEngine;

public class BuildingMaker : BaseGameplayModule
{
  [SerializeField] private LevelData curLevelData;
  [SerializeField] private Building buildingToSpawn;
  public ILevelManager LevelManagerHandler { get; set; }

  public override void Initialize()
  {
    SetData();
    SpawnBuilding();
    SetParent();
  }

  private void SetData()
  {
    curLevelData = LevelManagerHandler.GetCurrentLevel;
    buildingToSpawn = curLevelData.curLevelBuilding;
 
  }

  private void SpawnBuilding()
  {
    buildingToSpawn = Instantiate(buildingToSpawn);
  }

  private void SetParent()
  {
    buildingToSpawn.transform.SetParent(transform);
  }
}
