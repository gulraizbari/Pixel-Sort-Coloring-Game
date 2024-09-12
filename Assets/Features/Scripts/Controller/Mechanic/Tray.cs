using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sablo.Core;
using Sablo.Gameplay;
using UnityEngine;

public class Tray : BaseGameplayModule, ITray
{
    [SerializeField] private List<Pocket> pocketList;
    [SerializeField] private Pocket pocketPrefab;
    [SerializeField] private LevelData curLevelData;
    [SerializeField] private float moveBricksSpeed = 0.5f;
    
    public ILevelManager LevelManagerHandler { get; set; }

    public override void Initialize()
    {
        curLevelData = LevelManagerHandler.GetCurrentLevel;
        SpawnPockets();
    }

    private void SpawnPockets()
    {
        for (int i = 0; i < curLevelData.noOfPocketsToSpawn; i++)
        {
            var newPocket = Instantiate(pocketPrefab, transform);
            newPocket.transform.position = curLevelData.pocketsSpawnPos[i].transform.position;
            pocketList.Add(newPocket);
            newPocket.Initialize();
        }
    }

    List<Transform> ITray.GetPosListOfPocketWithEmptySpace()
    {
        return pocketList[0].pocketCell;
    }

    public void AddBricksToPocket(List<Chip> selectedBricks, BrickColor selectedStackColor)
    {
        var similarBrick = FindBrickByType(pocketList[0].pocBrickList, selectedStackColor);
        if (similarBrick != null)
        {
            var similarItemIndex = pocketList[0].pocBrickList.IndexOf(similarBrick);
            pocketList[0].InsertBricksToPocket(similarItemIndex, selectedBricks);
        }
        else
        {
            pocketList[0].FillBricksToPocket(selectedBricks);
        }
    }

    Chip FindBrickByType(List<Chip> brickList, BrickColor color)
    {
        return brickList.LastOrDefault(brick => brick.brickColor == color);
    }

    List<Chip> ITray.GetPocketBrickList()
    {
        return pocketList[0].pocBrickList;
    }
    
    public void MoveBricksToCurrentCarrier(BrickColor carrierColor, ICarrier currentCarrierHandler, List<Chip> bricksToMov)
    {
        var tapInstance = TapController.Instance;
        var posList = currentCarrierHandler.GetBrickPositionList();
        var myCarrierCapacity = currentCarrierHandler.CarrierCapacity();

        var pocket = pocketList[0];
        if (pocket.pocBrickList.Count > 0)
        {
            var carrierCount = currentCarrierHandler.GetCarrierCount();
            pocket.isFull = false;
            var aLlBrickOfCarrierColor = pocket.pocBrickList.FindAll(brick => brick.brickColor == carrierColor);
            var bricksToMove = new List<Chip>();

            if (myCarrierCapacity > 0)
            {
                bricksToMove = FindBricksOfCarrierColor(currentCarrierHandler, carrierColor);
            }


            if (bricksToMove.Count > 0)
            {
                currentCarrierHandler.AddBricksToCarrier(bricksToMove, null); // adding bricks to car
                MoveBrickOneByOne(bricksToMove, myCarrierCapacity, carrierCount, posList, pocket);
                pocket.RemoveBrickFromPocket(bricksToMove, bricksToMove.Count);
            }
        }
        else
        {
            var carrier = tapInstance.GetPreviousCarrier(tapInstance.theCurCarrier) as ICarrier;
            carrier.SetOffMoving();
        }
    }

    public List<Chip> FindBricksOfCarrierColor(ICarrier curCarrierHandler, BrickColor carrierColor)
    {
        var myCarrierCapacity = curCarrierHandler.CarrierCapacity();
        var pocket = pocketList[0];
        var aLlBrickOfCarrierColor = pocket.pocBrickList.FindAll(brick => brick.brickColor == carrierColor);
        var bricksToMove = new List<Chip>();

        if (myCarrierCapacity > 0)
        {
            if (aLlBrickOfCarrierColor.Count >= myCarrierCapacity)
            {
                for (int i = 0; i < myCarrierCapacity; i++)
                {
                    bricksToMove.Add(aLlBrickOfCarrierColor[i]);
                }
            }
            else
            {
                var brickCount = aLlBrickOfCarrierColor.Count;
                for (int i = 0; i < brickCount; i++)
                {
                    if (i < myCarrierCapacity)
                        bricksToMove.Add(aLlBrickOfCarrierColor[i]);
                }
            }
        }

        return bricksToMove;
    }

    int ITray.GetMaxSize()
    {
        return Pocket.MaxCapacity;
    }

    int ITray.GetPocketCount()
    {
        return pocketList[0].pocBrickList.Count;
    }

    private void MoveBrickOneByOne(List<Chip> bricksToMove, int carrierCapacity, int carrierCount, List<Vector3> posList, Pocket pocket)
    {
        var selBricksToMove = new List<Chip>(bricksToMove);
        var tapInstance = TapController.Instance;
        var carrier = tapInstance.GetPreviousCarrier(tapInstance.theCurCarrier) as ICarrier;
        carrier.SetOffMoving();
        var lastBrickIndex = selBricksToMove.Count - 1;
        var lastBrick = selBricksToMove[lastBrickIndex];
        StartCoroutine(MoveAllBricks(selBricksToMove, lastBrick, carrierCapacity, tapInstance.curCarrierHandler, pocket));
    }

    private IEnumerator MoveAllBricks(List<Chip> bricksToMove, Chip lastChip, int carrierCapacity, ICarrier curCarrier, Pocket pocket)
    {
        for (var index = 0; index < bricksToMove.Count; index++)
        {
            if (index <= carrierCapacity)
            {
                var brick = bricksToMove[index];
                var indexOfBrick = curCarrier.GetCarrierBricks().IndexOf(brick);
                brick.MoveToTargetCellPosByTray(indexOfBrick, brick == lastChip
                        ? () =>
                        {
                            curCarrier.IncreaseBrickCount();
                            brick.transform.SetParent(pocketList[0].transform);
                            TapController.Instance.curCarrierHandler.MoveAside();
                        }
                        : () =>
                        {
                            curCarrier.IncreaseBrickCount();
                            brick.transform.SetParent(pocketList[0].transform);
                        }
                    , curCarrier.GetBrickPositionList());
                yield return new WaitForSeconds(Configs.GameConfig.delayToMoveNextBrickToCar);
            }
        }
    }

    bool ITray.CheckIfGameFails(BrickColor nextCarrierColor)
    {
        return pocketList[0].IsGameFailed(nextCarrierColor);
    }

    public List<Chip> MoveBrickFromPocketToStack(Vector3 topBrick, float offset)
    {
        return pocketList[0].MoveBricksBackToStack(topBrick, offset);
    }

    public void RemoveBrickFromPocketByTray(List<Chip> bricksToRemove, int startPoint)
    {
        pocketList[0].RemoveBrickFromPocket(bricksToRemove, startPoint);
    }
}