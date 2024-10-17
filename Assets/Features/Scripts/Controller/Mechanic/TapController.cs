using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sablo.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class TapController : MonoBehaviour
{
    [SerializeField] private float brickYOffset = 0.09f;
    [SerializeField] private Stack lastSelectedStack;
    [SerializeField] private List<Carrier> carriersList;
    [SerializeField] private int noOfCarriersPass;
    public static TapController Instance;
    public List<Chip> _selectedStack = new List<Chip>();
    public BrickColor curCarrierColor = BrickColor.None;
    [ShowInInspector] public ICarrier curCarrierHandler;
    public Carrier theCurCarrier;
    public float brickMoveSpeed = 0.37f;
    public float brickDelay = 0.2f;
    public bool isMoving;
    public BrickColor selectedStackColor;
    
    public ITray TrayHandler { get; set; }
    public IRoller RollerHandler { get; set; }

    private void Awake()
    {
        Instance = this;
        Invoke(nameof(Initialize), 0.1f);
    }

    private void Initialize()
    {
        carriersList = RollerHandler.GetCarrierList();
    }

    public void AddBricksToSelectedStack(Chip chip, Stack selectedStack)
    {
        _selectedStack.Add(chip);
        lastSelectedStack = selectedStack;
    }

    private IEnumerator currentCoroutine;
    
    public void MoveSelectedBricksToTargetContainer()
    {
        isMoving = true;
        if (curCarrierHandler.GetCarrierCount() < 9 || TrayHandler.GetPocketCount() < TrayHandler.GetMaxSize())
        {
            if (_selectedStack.Count != 0)
            {
                selectedStackColor = _selectedStack[0].brickColor;

                if (_selectedStack[0].brickColor == curCarrierColor)
                {
                    var currentCarrierCount = curCarrierHandler.GetCarrierCount();
                    var maxCarrierCapacity = curCarrierHandler.GetMaxSize();
                    var availableCapacity = maxCarrierCapacity - currentCarrierCount;
                    if (availableCapacity > 0)
                    {
                        // Determine how many bricks can be added to the carrier
                        var bricksToAddToCarrier = Math.Min(_selectedStack.Count, availableCapacity);
                        // Bricks that will be moved to the carrier
                        var bricksForCarrier = new List<Chip>();
                        // Bricks that will remain and be moved to the pocket
                        var bricksForPocket = new List<Chip>();

                        for (var i = 0; i < _selectedStack.Count; i++)
                        {
                            if (i < bricksToAddToCarrier)
                            {
                                bricksForCarrier.Add(_selectedStack[i]);
                            }
                            else
                            {
                                bricksForPocket.Add(_selectedStack[i]);
                            }
                        }

                        // Add bricks to the carrier
                        curCarrierHandler.AddBricksToCarrier(bricksForCarrier, () =>
                        {
                            var startMovPos = curCarrierHandler.GetCarrierCount() - bricksToAddToCarrier;
                            StartCoroutine(curCarrierHandler.ResetBrickPos(startMovPos, bricksForCarrier));
                        });

                        // Add remaining bricks to the pocket
                        StartCoroutine(MoveRemainingBricksToPocketWithDelay(bricksForPocket));

                        // Clear the selected stack
                        _selectedStack.Clear();
                    }
                    else
                    {
                        MoveRemainingBricksToPocket(_selectedStack);
                    }
                }
                else
                {
                    MoveRemainingBricksToPocket(_selectedStack);
                }
            }
        }
        else
        {
            _selectedStack.Reverse(); // stack is reversed so that it added to stack in seq
            lastSelectedStack.AddBrickBackToStack(_selectedStack);
            _selectedStack.Clear();
        }
    }

    private void MoveRemainingBricksToPocket(List<Chip> bricksForPocket)
    {
        var currentPocketCount = TrayHandler.GetPocketCount();
        var maxPocketCapacity = TrayHandler.GetMaxSize();
        var pocketAvailableCapacity = maxPocketCapacity - currentPocketCount;

        if (pocketAvailableCapacity > 0)
        {
            if (bricksForPocket.Count > pocketAvailableCapacity)
            {
                var bricksToAddToPocket = Math.Min(bricksForPocket.Count, pocketAvailableCapacity);

                var bricksToAddIntoPocket = bricksForPocket.GetRange(0, bricksToAddToPocket);
                // Bricks that will remain and be moved to the pocket
                var bricksToAddBackToPad = bricksForPocket.GetRange(bricksToAddToPocket, bricksForPocket.Count - bricksToAddToPocket);
                TrayHandler.AddBricksToPocket(bricksToAddIntoPocket, selectedStackColor);
                bricksToAddBackToPad.Reverse(); // stack is reversed so that it added to stack in seq
                lastSelectedStack.AddBrickBackToStack(bricksToAddBackToPad);
                bricksForPocket.Clear();
            }
            else
            {
                // Move all bricks to the pocket if colors don't match
                var brickMovPos = TrayHandler.GetPosListOfPocketWithEmptySpace();
                TrayHandler.AddBricksToPocket(bricksForPocket, selectedStackColor);
                bricksForPocket.Clear();
            }
        }
        else
        {
            bricksForPocket.Reverse();
            lastSelectedStack.AddBrickBackToStack(bricksForPocket);
            bricksForPocket.Clear();
        }

        // add brick logic
        if (bricksForPocket.Count > 0)
        {
            var brickMovPos = TrayHandler.GetPosListOfPocketWithEmptySpace();
            TrayHandler.AddBricksToPocket(bricksForPocket, selectedStackColor);
        }
    }

    private IEnumerator MoveRemainingBricksToPocketWithDelay(List<Chip> bricksForPocket)
    {
        yield return new WaitForSeconds(0.15f);
        MoveRemainingBricksToPocket(bricksForPocket);
    }

    public Carrier GetPreviousCarrier(Carrier currentCarrier)
    {
        var indexOfPrevious = carriersList.IndexOf(currentCarrier) - 1;
        if (indexOfPrevious < 0)
        {
            indexOfPrevious = 0;
        }
        return carriersList[indexOfPrevious];
    }

    public void IncreaseNumberOfCarriersPass()
    {
        noOfCarriersPass++;
    }

    public int SetNoOfCarriersPass(int value)
    {
        return noOfCarriersPass = value;
    }

    public int GetNoOfCarriersPass()
    {
        return noOfCarriersPass;
    }

    public void GameFail()
    {
        carriersList = RollerHandler.GetCarrierList();
        var indexOfNextCarrier = carriersList.IndexOf(theCurCarrier) + 1;
        if (TrayHandler.CheckIfGameFails(carriersList[indexOfNextCarrier].carrierColor))
        {
            GameLoop.Instance.GameFail();
        }
    }

    #region MoveBrickMethods

    // Move To Carrier
    public void MoveBricksToCarrier(int indexToMoveTowards, int startMovPos, List<Chip> brickListToRemBricks, List<Vector3> targetPosList, Action callBack)
    {
        var targetPos = targetPosList[startMovPos + indexToMoveTowards];
        targetPos.y += Configs.GameConfig.brickYOffset;
        brickListToRemBricks[indexToMoveTowards].transform
            .DORotate(new Vector3(90, 90, 0), Configs.GameConfig.timeToMoveBrick);
        brickListToRemBricks[indexToMoveTowards].transform.SetParent(curCarrierHandler.GetCarrierTransform());
        brickListToRemBricks[indexToMoveTowards].transform
            .DOJump(targetPos, Configs.GameConfig.brickJumpPower, 1, Configs.GameConfig.timeToMovBrPocToCar)
            .SetEase(Configs.GameConfig.brickJumpEaseType2).OnComplete(
                () => { callBack?.Invoke(); });
    }

    // Move To Pocket
    /*
    private void MoveBricksToPocket(int indexToMoveToward,List<Transform> targetPosList,Action callBack)
    {
        var targetPos = targetPosList[indexToMoveToward].position;
        targetPos.y += brickYOffset;
        _selectedStack[indexToMoveToward].transform.DOJump(targetPos,0.25f,1, brickMoveSpeed).SetEase(Ease.OutQuad).OnComplete(
            () =>
            {
                callBack?.Invoke();
            });
    }
    */

    #endregion
}