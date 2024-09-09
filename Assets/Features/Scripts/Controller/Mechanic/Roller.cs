using System.Collections;
using System.Collections.Generic;
using Sablo.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Roller : BaseGameplayModule, IRoller
{
    [SerializeField] private LevelData curLevelData;
    [SerializeField] private Transform firstCarSpawnPos;
    [SerializeField] private List<Carrier> spawnCarriers;
    [SerializeField] private float CarrierXOffset=2.5f;
    [SerializeField] private GameObject dustParticle;
    [SerializeField] private List<CarrierInfo> totalCarriersList;
    public Texture newBrickTexture;
    private int materialSumOfFirstTwoLevel;
    int preCarrierOrder=0;
    
    public GameObject GetSpawnParticle => dustParticle;
    
     int IRoller.MaterialSumCount { get => materialSumOfFirstTwoLevel; set => materialSumOfFirstTwoLevel =value; }

    public ISlateBuilder SlateBuilderHandler { get; set; }
    public IlevelManager LevelManagerHandler { get; set; }
    public ITray TrayHandler { get; set; }
    public override void Initialize()
    {   
        Application.targetFrameRate = 120;
        curLevelData = LevelManagerHandler.GetCurrentLevel;
        SumCarriersListForAllSubLevels();
        SpawnCarrier();
        SetParentOfCarriers();
        TapController.Instance.curCarrierHandler = spawnCarriers[0];
        TapController.Instance.theCurCarrier = spawnCarriers[0];
        spawnCarriers[0].Initialize();
        SetNextNextCarrierPos();
        spawnCarriers[0].OnMoveAside(MoveAllCarrierToNextOne);
        SetAllCarrierCellPosList();
        int lastIndex = spawnCarriers.Count - 1;
        spawnCarriers[lastIndex].lastCarrier = true;
    }

    private void SumCarriersListForAllSubLevels()
    {
        int totalSubLevelCount=0;
        totalSubLevelCount = LevelManagerHandler.GetTotalSubLevelCount;

        if (totalSubLevelCount > 0)
        {
            for (int i = LevelManagerHandler.SubLevelPref; i < totalSubLevelCount; i++)
            {
                AddToList(LevelManagerHandler.SubLevelList[i].carrierToSpawn, totalCarriersList);
            }
        }
        else
        {
            AddToList(curLevelData.carrierToSpawn, totalCarriersList);
        }
        
    }

    private void AddToList(List<CarrierInfo> carrierListOfSubLevel,List<CarrierInfo> totalCarrierList)
    {
        foreach (var c in carrierListOfSubLevel)
        {
            totalCarrierList.Add(c);
        }
    }
    

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Application.targetFrameRate = 120;
    }
 
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void SpawnCarrier()
    {
        var spawnPos = firstCarSpawnPos.position;
        int count=0;
        foreach (var carrier in totalCarriersList)
        {
            var newCarrier = Instantiate(carrier.CarrierType);
            newCarrier.SpawnOrder += count;
            newCarrier.transform.position = spawnPos;
            spawnPos.x -= CarrierXOffset;
            spawnCarriers.Add(newCarrier);
            newCarrier.TrayInterface = TrayHandler;
            newCarrier.RollerInterface = this;
            newCarrier.LevelManagerInterface = LevelManagerHandler;
            newCarrier.SlateBuilderInterface = SlateBuilderHandler;
            count++;
        }
    }

    private void SetParentOfCarriers()
    {
        foreach (var carrier in spawnCarriers)
        {
            carrier.transform.SetParent(transform);
        }
    }

    private void SetNextNextCarrierPos()
    {
        for (int i = spawnCarriers.Count-1; i >= 1 ; i--)
        {
            spawnCarriers[i].nextCarrierPos = spawnCarriers[i-1].transform;
        }
    }

    public void MoveAllCarrierToNextOne()
    {
        Debug.Log("<color=yellow>{MoveAllCarrierToNextOne}</color>");

        foreach (var car in spawnCarriers)
        {
            if (car.isUpFront)
                preCarrierOrder = car.SpawnOrder + 1;
            else
            {   
                car.MoveToNextCarrierPos();
            }
        }
        
        // Debug.Log($"<color=yellow>{spawnCarriers[preCarrierOrder].carrierColor}</color>");
        if (preCarrierOrder<spawnCarriers.Count)
        {
            spawnCarriers[preCarrierOrder].Initialize();
            TapController.Instance.theCurCarrier = spawnCarriers[preCarrierOrder];
            TapController.Instance.curCarrierHandler = spawnCarriers[preCarrierOrder];
        }
    }

    private void SetAllCarrierCellPosList()
    {
        foreach (var carrier in spawnCarriers)
        {
            for (int i = 0; i < spawnCarriers[0].brickPositions.Count; i++)
            {
                carrier.SetInitialPositions(spawnCarriers[0].brickPositions[i]);
            }
        }
    }
    
    List<Carrier> IRoller.GetCarrierList()
    {
        return spawnCarriers;
    }

     void IRoller.RemoveCarrierFromRoller(Carrier carrierToRemove)
    {
        //Debug.LogError("Removed !");
        spawnCarriers.Remove(carrierToRemove);
    }

     Texture IRoller.GetNewBrickTexture()
     {
         return newBrickTexture;
     }
     
     Transform IRoller.GetRollerTransform()
     {
         return transform;
     }

     bool IRoller.IsLastCarrierOfSubLevel()
     {
         if (TapController.Instance.GetNoOfCarriersPass() == LevelManagerHandler.GetCurrentLevel.carrierToSpawn.Count/*totalCarriers && LevelManagerHandler.isOfMultiTierLevel*/)
         {
             Debug.Log("true");
             return true;
         }
         else
         {
             Debug.Log("false");
             return false;
         }
           
     }
}
