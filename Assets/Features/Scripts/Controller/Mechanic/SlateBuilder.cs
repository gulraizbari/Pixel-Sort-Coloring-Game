using System;
using System.Collections.Generic;
using DG.Tweening;
using PixelSort.Feature.GridGeneration;
using RopeToolkit;
using Sablo.Gameplay;
using UnityEngine;

public class SlateBuilder : BaseGameplayModule, ISlateBuilder
{
    [SerializeField] private Slate _slate;
    [SerializeField] private LevelData curLevelData;
    [SerializeField] private LevelPositionData curLevelPositionData;
    [SerializeField] private List<Slate> newSlatesList;
    [SerializeField] float horizontalOffset = 1.27f;
    [SerializeField] float verticalOffset = 3.5f;
    [SerializeField] private float firstRowZPos = 0;
    [SerializeField] private RopeHandler theRopeHandler;
    [SerializeField] private List<SubStack> stacksWithRope = new List<SubStack>();
    [SerializeField] private List<StackAddress> stackAddresses = new List<StackAddress>();
    [SerializeField] private Material ropeMaterial;
    [SerializeField] private List<RopeHandler> allRopes;
    [SerializeField] private GameObject ropeEdge;
    private int slateIndex, padIndex;
    public ILevelManager LevelManagerHandler { get; set; }
    public Material GetRopeMaterial => ropeMaterial;

    public override void Initialize()
    {
        SetLevelPositionData();
        SetLevelData();
        SpawnSlates();
        SortStackByRope();
        SpawnMultipleRopes();
        SetParent();
        GameLoop.Instance.OnContinueLevel(OnLevelResume);
    }

    void ISlateBuilder.ReInitialize()
    {
        newSlatesList.ForEach(x => x.gameObject.SetActive(false));
        newSlatesList.Clear();
        SetLevelPositionData();
        SetLevelData();
        stacksWithRope.Clear();
        allRopes.Clear();
        stackAddresses.Clear();
        SpawnSlates();
        SortStackByRope();
        SpawnMultipleRopes();
        SetParent();
        GameLoop.Instance.UpdateSubLevelNo(LevelManagerHandler.GetTotalSubLevelCount - 1, LevelManagerHandler.GetSubLevel);
    }

    private void SetLevelData()
    {
        curLevelData = LevelManagerHandler.GetCurrentLevel;
    }

    private void SetLevelPositionData()
    {
        curLevelPositionData = LevelManagerHandler.GetCurrentPositionLevel;
    }
    
    public void OnLevelResume()
    {
        var minStack = FindPadWithLeastBrickCount();
        var isMinStackEmpty = minStack.chipsStack.Count == 0;
        var lastBrickIndex = minStack.chipsStack.Count - 1;
        var basePos = minStack.padBase.transform.position;
        var lastTwelve = TapController.Instance.TrayHandler.MoveBrickFromPocketToStack(minStack.chipsStack.Count > 0 ? minStack.chipsStack[lastBrickIndex].transform.position : basePos, 0.32f);
        lastTwelve.ForEach(brick => newSlatesList[slateIndex]._padList[padIndex].chipsStack.Add(brick));
        if (!isMinStackEmpty)
        {
            newSlatesList[slateIndex].UpdateColliderLength(minStack);
        }
    }
    
    private void SpawnSlates()
    {
        var grid = curLevelPositionData.Grid;
        var rows = curLevelPositionData.Row;
        var columns = curLevelPositionData.Column;

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                var cellData = grid[x, y];
                if (cellData.tileType == TileType.Empty)
                {
                    Debug.LogError("No Stack here");
                    continue;
                }
                var newSlate = Instantiate(_slate);
                newSlate.gameObject.name = $"Slate{x}-{y}";
                newSlatesList.Add(newSlate);
                // newSlate.SlateBuilderInterface = this;
                var slatePosition = new Vector3(x * horizontalOffset, 0, y * verticalOffset);
                var data = curLevelData.slateBuilderOffsets;
                newSlate.transform.position = slatePosition;
                SetPadOffset();
                newSlate.Initialize();
                
                // if (cellData.isMirror)
                // {
                //     
                // }

                // if (cellData.isMultiProducer)
                // {
                //     
                // }
            }
        }
    }

    // private void SpawnSlates()
    // {
    //     var slateCount = curLevelData.allStacks.Count;
    //
    //     for (int i = 0; i < slateCount; i++)
    //     {
    //         var newSlate = Instantiate(_slate);
    //         newSlate.gameObject.name = "Slate" + i;
    //         newSlatesList.Add(newSlate);
    //         newSlate.SlateBuilderInterface = this;
    //         newSlate.slateIndex = i;
    //         var slatePos = Vector3.zero;
    //         var data = curLevelData.slateBuilderOffsets;
    //         
    //         if (slateCount == 1)
    //         {
    //             // Center position
    //             slatePos = new Vector3(0, 0, 0);
    //         }
    //         else if (slateCount == 2)
    //         {
    //             // Left and right positions
    //             GameLoop.Instance.ChangeCameraPosAccToRowCount(2);
    //             if (i == 0)
    //             {
    //                 slatePos = new Vector3(-data.horizontalOffset, 0, data.firstStackPos); // Top-left position
    //             }
    //             else if (i == 1)
    //             {
    //                 slatePos = new Vector3(data.horizontalOffset, 0, data.firstStackPos); // Top-right position
    //             }
    //         }
    //         else if (slateCount == 3)
    //         {
    //             // Vertical positions
    //             slatePos = new Vector3(0, -i * verticalOffset, 0);
    //         }
    //         else if (slateCount == 4)
    //         {
    //             GameLoop.Instance.ChangeCameraPosAccToRowCount(4);
    //             // Left and right positions with vertical offset
    //             if (i == 0)
    //             {
    //                 slatePos = new Vector3(-horizontalOffset, 0, firstRowZPos); // Top-left position
    //             }
    //             else if (i == 1)
    //             {
    //                 slatePos = new Vector3(horizontalOffset, 0, firstRowZPos); // Top-right position
    //             }
    //             else if (i == 2)
    //             {
    //                 slatePos = new Vector3(-horizontalOffset, 0, firstRowZPos - verticalOffset); // Bottom-left position
    //             }
    //             else if (i == 3)
    //             {
    //                 slatePos = new Vector3(horizontalOffset, 0, firstRowZPos - verticalOffset); // Bottom-right position
    //             }
    //         }
    //         else if (slateCount == 6)
    //         {
    //             // Left and right positions with vertical offset
    //             if (i == 0)
    //             {
    //                 slatePos = new Vector3(-data.horizontalOffset, 0, data.firstStackPos); // Top-left position
    //             }
    //             else if (i == 1)
    //             {
    //                 slatePos = new Vector3(data.horizontalOffset, 0, data.firstStackPos); // Top-right position
    //             }
    //             else if (i == 2)
    //             {
    //                 slatePos = new Vector3(-data.horizontalOffset, 0,
    //                     data.firstStackPos - data.verticalOffset); // Bottom-left position
    //             }
    //             else if (i == 3)
    //             {
    //                 slatePos = new Vector3(data.horizontalOffset, 0,
    //                     data.firstStackPos - data.verticalOffset); // Bottom-right position
    //             }
    //             else if (i == 4)
    //             {
    //                 slatePos = new Vector3(-data.horizontalOffset, 0,
    //                     data.firstStackPos - (data.verticalOffset * 1.9f)); // Bottom-left position
    //             }
    //             else if (i == 5)
    //             {
    //                 slatePos = new Vector3(data.horizontalOffset, 0,
    //                     data.firstStackPos - (data.verticalOffset * 1.9f)); // Bottom-right position
    //             }
    //         }
    //         else
    //         {
    //             // Default vertical arrangement for more than 4 slates
    //             slatePos = new Vector3(0, -i * data.verticalOffset, 0);
    //         }
    //
    //         newSlate.transform.position = slatePos;
    //         newSlate.SetData(curLevelData.allStacks[i].stackData);
    //         SetPadOffset();
    //         newSlate.Initialize();
    //     }
    // }

    private Stack FindPadWithLeastBrickCount()
    {
        Stack stackWithMinBrickCount = null;
        var minBrickCount = int.MaxValue;

        for (int i = 0; i < newSlatesList.Count; i++)
        {
            for (int j = 0; j < newSlatesList[i]._padList.Count; j++)
            {
                var currentPad = newSlatesList[i]._padList[j];
                if (currentPad.chipsStack.Count == 0)
                {
                    if (currentPad.chipsStack.Count < minBrickCount)
                    {
                        stackWithMinBrickCount = currentPad;
                        minBrickCount = currentPad.chipsStack.Count;
                        padIndex = j;
                        slateIndex = i;
                        stackWithMinBrickCount.isSparked = false;
                        stackWithMinBrickCount.padBase.SetActive(true); // activating base again
                    }
                }
                else if (currentPad.chipsStack.Count > 0 && currentPad.chipsStack[0].brickColor != BrickColor.EmptyBrick)
                {
                    if (currentPad.chipsStack.Count < minBrickCount)
                    {
                        stackWithMinBrickCount = currentPad;
                        minBrickCount = currentPad.chipsStack.Count;
                        padIndex = j;
                        slateIndex = i;
                    }
                }
            }
        }

        return stackWithMinBrickCount;
    }
    
    private void SetPadOffset()
    {
        for (int i = 0; i < newSlatesList.Count; i++)
        {
            if (i == 0)
            {
                newSlatesList[i].padXOffset = -(curLevelData.xOffsetForColliders);
                newSlatesList[i].padYPos += (0);
            }
            else if (i == 1)
            {
                newSlatesList[i].padXOffset = (curLevelData.xOffsetForColliders);
                newSlatesList[i].padYPos += (0);
            }
            else if (i == 2)
            {
                newSlatesList[i].padXOffset = -((curLevelData.xOffsetForColliders));
                newSlatesList[i].padYPos += (0);
            }
            else if (i == 3)
            {
                newSlatesList[i].padXOffset = (curLevelData.xOffsetForColliders);
                newSlatesList[i].padYPos += (0);
            }
            else if (i == 4)
            {
                newSlatesList[i].padXOffset = -((curLevelData.xOffsetForColliders));
                newSlatesList[i].padYPos += (0);
            }
            else if (i == 5)
            {
                newSlatesList[i].padXOffset = (curLevelData.xOffsetForColliders);
                newSlatesList[i].padYPos += (0);
            }
        }
    }

    private void SetParent()
    {
        foreach (var slate in newSlatesList)
        {
            slate.transform.SetParent(transform);
        }
    }

    private void SpawnRope(Stack stack1, int brickIndex1, Stack stack2, int brickIndex2, int ropeId)
    {
        var newRopeHandler = Instantiate(theRopeHandler);
        allRopes.Add(newRopeHandler);
        newRopeHandler.ropeId = ropeId;
        var edgePos1 = stack1.chipsStack[brickIndex1].transform.position;
        var edgePos2 = stack2.chipsStack[brickIndex2].transform.position;
        edgePos1.z -= 0.65f;
        edgePos2.z -= 0.65f;
        DOVirtual.DelayedCall(1f, () =>
        {
            newRopeHandler.SetRope(stack1.chipsStack[brickIndex1].ropeJoint, stack2.chipsStack[brickIndex2].ropeJoint);
            DOVirtual.DelayedCall(1f, () =>
            {
                SpawnRopeEdges(edgePos1, edgePos2, newRopeHandler.gameObject);
                newRopeHandler.GetComponent<Rope>().material = ropeMaterial;
            });
        });
    }

    private void SpawnRopeEdges(Vector3 edgePos1, Vector3 edgePos2, GameObject rope)
    {
        var edge1 = Instantiate(ropeEdge, rope.transform, true);
        var edge2 = Instantiate(ropeEdge, rope.transform, true);
        edge1.transform.position = edgePos1;
        edge2.transform.position = edgePos2;
    }

    public void AddRopePilesToList(SubStack subStack)
    {
        stacksWithRope.Add(subStack);
    }

    void ISlateBuilder.AddToStackAddress(int indexOfSlate, int indexOfStack, int indexOfSubStack, int ropeId)
    {
        var newAddress = new StackAddress
        {
            indexOfSlate = indexOfSlate, indexOfStack = indexOfStack, indexOfSubStack = indexOfSubStack, ropeId = ropeId
        };
        stackAddresses.Add(newAddress);
    }

    private void SpawnMultipleRopes()
    {
        for (int i = 0; i < stacksWithRope.Count; i += 2)
        {
            var nextIndex = i + 1;
            var pad1 = newSlatesList[stackAddresses[i].indexOfSlate]._padList[stackAddresses[i].indexOfStack];
            var pad1BrickIndex = (stacksWithRope[i].startIndex) + 1;
            var pad2 = newSlatesList[stackAddresses[nextIndex].indexOfSlate]._padList[stackAddresses[nextIndex].indexOfStack];
            var pad2BrickIndex = (stacksWithRope[nextIndex].startIndex) + 1;
            SpawnRope(pad1, pad1BrickIndex, pad2, pad2BrickIndex, stackAddresses[i].ropeId);
        }
    }

    void SortStackByRope()
    {
        stacksWithRope.Sort((x, y) => x.ropeId.CompareTo(y.ropeId));
        stackAddresses.Sort((x, y) => x.ropeId.CompareTo(y.ropeId));
    }

    SubStack ISlateBuilder.GetSubStackByRopId(int existingRopId, SubStack existingSubStack)
    {
        var similarRopIdStack = stacksWithRope.Find(x => x.ropeId == existingRopId && x != existingSubStack);
        return similarRopIdStack;
    }

    int ISlateBuilder.GetIndexOfSubStack(SubStack subStackToGetIndexOf)
    {
        return stacksWithRope.IndexOf(subStackToGetIndexOf);
    }

    // StackAddress ISlateBuilder.GetSubStackAddressByRopId(int existingRopId, StackAddress existingSubStackAddress)
    // {
    //     var similarRopIdStackAddress = stackAddresses.Find(x => x.ropeId == existingRopId && x != existingSubStackAddress);
    //     return similarRopIdStackAddress;
    // }
    //
    // StackAddress ISlateBuilder.GetSubStackAddressByIndex(int indexOfSubStackInList)
    // {
    //     return stackAddresses[indexOfSubStackInList];
    // }

    Stack ISlateBuilder.GetStackContainingSubStackWithEqualRopId(int indexOfSlate, int indexOfPad)
    {
        return newSlatesList[indexOfSlate]._padList[indexOfPad];
    }

    RopeHandler ISlateBuilder.GetRopeHandlerByRopeId(int subStackRopeId)
    {
        return allRopes.Find(x => x.ropeId == subStackRopeId);
    }

    int ISlateBuilder.GetTotalNumberOfSlates()
    {
        return curLevelData.allStacks.stackData.Count;
    }
}

//
// [Serializable]
// public class StackAddress
// {
//     public int indexOfSlate;
//     public int indexOfStack;
//     public int indexOfSubStack;
//     [HideInInspector] public int ropeId;
// }