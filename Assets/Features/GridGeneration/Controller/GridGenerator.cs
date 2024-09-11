using System;
using System.Collections.Generic;
using DG.Tweening;
using RopeToolkit;
using Sablo.Gameplay;
using UnityEngine;

namespace PixelSort.Feature.GridGeneration
{
    public class GridGenerator : BaseGameplayModule, IGridGenerator
    {
        [SerializeField] private Slate _stack;
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
        [SerializeField] private Stacks stackDataList;
        [SerializeField] private Stack coloredStack;
        [SerializeField] private Stack emptyStack;
        [SerializeField] private List<GameObject> _stackObj = new List<GameObject>();
        [SerializeField] private GameObject stackBase;
        [SerializeField] private GameObject spark;
        [SerializeField] private Transform emptyParent;
        [SerializeField] private Transform stacksParent;
        public List<Stack> _stackList = new List<Stack>();
        private List<Transform> gridPositions = new List<Transform>();

        private int rows = 0;
        private int columns = 0;
        private int slateIndex;
        private int padIndex;
        public float yOffset = 0.1f;
        public float baseOffset = 0.3f;
        public ILevelManager LevelManagerHandler { get; set; }
        public Material GetRopeMaterial => ropeMaterial;

        public override void Initialize()
        {
            SetLevelPositionData();
            SetLevelData();
            SpawnData(transform, curLevelPositionData, emptyStack, coloredStack);
            SortStackByRope();
            SpawnMultipleRopes();
            SetParent();
            SpawnStack();
            SpawnStacks();
            GameLoop.Instance.OnContinueLevel(OnLevelResume);
        }

        void IGridGenerator.ReInitialize()
        {
            newSlatesList.ForEach(x => x.gameObject.SetActive(false));
            newSlatesList.Clear();
            SetLevelPositionData();
            SetLevelData();
            stacksWithRope.Clear();
            allRopes.Clear();
            stackAddresses.Clear();
            SpawnData(transform, curLevelPositionData, emptyStack, coloredStack);
            SortStackByRope();
            SpawnMultipleRopes();
            SetParent();
            SpawnStack();
            SpawnStacks();
            GameLoop.Instance.UpdateSubLevelNo(LevelManagerHandler.GetTotalSubLevelCount - 1, LevelManagerHandler.GetSubLevel);
        }

        private void SetLevelData()
        {
            curLevelData = LevelManagerHandler.GetCurrentLevel;
            stackDataList = curLevelData.allStacks;
        }

        private void SetLevelPositionData()
        {
            curLevelPositionData = LevelManagerHandler.GetCurrentPositionLevel;
        }

        public void OnLevelResume()
        {
            var minStack = FindPadWithLeastBrickCount();
            var isMinStackEmpty = minStack.bricksStack.Count == 0;
            var lastBrickIndex = minStack.bricksStack.Count - 1;
            var basePos = minStack.padBase.transform.position;
            var lastTwelve = TapController.Instance.TrayHandler.MoveBrickFromPocketToStack(
                minStack.bricksStack.Count > 0 ? minStack.bricksStack[lastBrickIndex].transform.position : basePos,
                0.32f);
            lastTwelve.ForEach(brick => newSlatesList[slateIndex]._padList[padIndex].bricksStack.Add(brick));
            if (!isMinStackEmpty)
            {
                newSlatesList[slateIndex].UpdateColliderLength(minStack);
            }
        }

        private void SpawnData(Transform parentObject, LevelPositionData level, Stack emptyStack, Stack coloredStack)
        {
            rows = level.Column;
            columns = level.Row;
            var grid = level.Grid;
            var tileSpacing = 2f;
            var totalWidth = rows * tileSpacing;
            var totalHeight = columns * tileSpacing;
            var gridCenterOffset = new Vector3(totalWidth / 2, 0, totalHeight / 2);

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    // var cellData = grid[row, col];
                    var tilePosition = new Vector3(row * tileSpacing, 0, (level.Column - 1 - col) * tileSpacing) - gridCenterOffset + parentObject.transform.position;
                    switch (grid[row, col].tileType)
                    {
                        case TileType.Empty:
                            Debug.LogError("No Stack here");
                            SpawnNewStack(emptyStack, emptyParent, false, tilePosition, row, col);
                            break;
                        case TileType.Stack:
                            var stack = SpawnNewStack(coloredStack, stacksParent, true, tilePosition, row, col);
                            _stackList.Add(stack);
                            gridPositions.Add(stack.transform);
                            break;
                    }

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

        private Stack SpawnNewStack(Stack chip, Transform parentObject, bool isTileOn, Vector3 tilePosition, int row,
            int col)
        {
            var stack = Instantiate(chip, parentObject.transform);
            stack.transform.position = tilePosition;
            stack.gameObject.name = "Stack_" + row.ToString() + "*" + col.ToString();
            stack.gameObject.SetActive(isTileOn);
            return stack;
        }

        private void LoadStack(StackData currentStackData, Stack myStack, Vector3 position)
        {
            foreach (var pile in currentStackData.chipData)
            {
                var pileCopy = GetCopyOfOriginalPile(pile);
                for (int index = pile.startIndex; index <= pile.endIndex; index++)
                {
                    var indexOfStack = stackDataList.stackData.IndexOf(currentStackData);
                    var chip = Instantiate(pile.myChip, _stackObj[indexOfStack].transform);
                    _stackList[indexOfStack].bricksStack.Add(chip);
                    position.y = baseOffset;
                    chip.transform.position = position;
                    baseOffset += yOffset;
                }

                if (myStack != null)
                {
                    myStack.subStacks.Add(pileCopy);
                }
            }
        }

        private void SpawnStacks()
        {
            for (var index = 0; index < curLevelData.allStacks.stackData.Count; index++)
            {
                var stack = curLevelData.allStacks.stackData[index];
                LoadStack(stack, index < _stackList.Count ? _stackList[index] : null, gridPositions[index].position);
                baseOffset = 0.3f;
            }
        }

        private void SpawnStack()
        {
            for (int i = 0; i < stackDataList.stackData.Count; i++)
            {
                var item = new GameObject("Stack" + i);
                _stackObj.Add(item);
                _stackList[i].padBase = stackBase;
                _stackList[i].stackId = i;
                _stackList[i].isMultiProducer = stackDataList.stackData[i].isMulti;
                _stackList[i].sparkParticle = spark;
                _stackList[i].GridGeneratorHandler = this;
                stackDataList.stackData[i].multiProducerData.ForEach(x => _stackList[i].multiProducerDataList.Add(GetCopyOfOriginalPile(x)));
            }
        }

        private SubStack GetCopyOfOriginalPile(SubStack pile)
        {
            var pileCopy = new SubStack();
            pileCopy.startIndex = pile.startIndex;
            pileCopy.endIndex = pile.endIndex;
            pileCopy.ropeId = pile.ropeId;
            pileCopy.chipImage = pile.chipImage;
            pileCopy.myChip = pile.myChip;
            pileCopy.stackType = pile.stackType;
            return pileCopy;
        }

        private Stack FindPadWithLeastBrickCount()
        {
            Stack stackWithMinBrickCount = null;
            var minBrickCount = int.MaxValue;

            for (int i = 0; i < newSlatesList.Count; i++)
            {
                for (int j = 0; j < newSlatesList[i]._padList.Count; j++)
                {
                    var currentPad = newSlatesList[i]._padList[j];
                    if (currentPad.bricksStack.Count == 0)
                    {
                        if (currentPad.bricksStack.Count < minBrickCount)
                        {
                            stackWithMinBrickCount = currentPad;
                            minBrickCount = currentPad.bricksStack.Count;
                            padIndex = j;
                            slateIndex = i;
                            stackWithMinBrickCount.isSparked = false;
                            stackWithMinBrickCount.padBase.SetActive(true); // activating base again
                        }
                    }
                    else if (currentPad.bricksStack.Count > 0 &&
                             currentPad.bricksStack[0].brickColor != BrickColor.EmptyBrick)
                    {
                        if (currentPad.bricksStack.Count < minBrickCount)
                        {
                            stackWithMinBrickCount = currentPad;
                            minBrickCount = currentPad.bricksStack.Count;
                            padIndex = j;
                            slateIndex = i;
                        }
                    }
                }
            }

            return stackWithMinBrickCount;
        }

        private void SetParent()
        {
            foreach (var slate in newSlatesList)
            {
                slate.transform.SetParent(transform);
            }
        }

        #region Rope Mechanic

        private void SpawnRope(Stack stack1, int brickIndex1, Stack stack2, int brickIndex2, int ropeId)
        {
            var newRopeHandler = Instantiate(theRopeHandler);
            allRopes.Add(newRopeHandler);
            newRopeHandler.ropeId = ropeId;
            var edgePos1 = stack1.bricksStack[brickIndex1].transform.position;
            var edgePos2 = stack2.bricksStack[brickIndex2].transform.position;
            edgePos1.z -= 0.65f;
            edgePos2.z -= 0.65f;
            DOVirtual.DelayedCall(1f, () =>
            {
                newRopeHandler.SetRope(stack1.bricksStack[brickIndex1].ropeJoint,
                    stack2.bricksStack[brickIndex2].ropeJoint);
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

        void IGridGenerator.AddToStackAddress(int indexOfSlate, int indexOfStack, int indexOfSubStack, int ropeId)
        {
            var newAddress = new StackAddress
            {
                indexOfSlate = indexOfSlate, indexOfStack = indexOfStack, indexOfSubStack = indexOfSubStack,
                ropeId = ropeId
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
                var pad2 = newSlatesList[stackAddresses[nextIndex].indexOfSlate]
                    ._padList[stackAddresses[nextIndex].indexOfStack];
                var pad2BrickIndex = (stacksWithRope[nextIndex].startIndex) + 1;
                SpawnRope(pad1, pad1BrickIndex, pad2, pad2BrickIndex, stackAddresses[i].ropeId);
            }
        }

        private void SortStackByRope()
        {
            stacksWithRope.Sort((x, y) => x.ropeId.CompareTo(y.ropeId));
            stackAddresses.Sort((x, y) => x.ropeId.CompareTo(y.ropeId));
        }

        SubStack IGridGenerator.GetSubStackByRopId(int existingRopId, SubStack existingSubStack)
        {
            var similarRopIdStack = stacksWithRope.Find(x => x.ropeId == existingRopId && x != existingSubStack);
            return similarRopIdStack;
        }

        int IGridGenerator.GetIndexOfSubStack(SubStack subStackToGetIndexOf)
        {
            return stacksWithRope.IndexOf(subStackToGetIndexOf);
        }

        StackAddress IGridGenerator.GetSubStackAddressByRopId(int existingRopId, StackAddress existingSubStackAddress)
        {
            var similarRopIdStackAddress =
                stackAddresses.Find(x => x.ropeId == existingRopId && x != existingSubStackAddress);
            return similarRopIdStackAddress;
        }

        StackAddress IGridGenerator.GetSubStackAddressByIndex(int indexOfSubStackInList)
        {
            return stackAddresses[indexOfSubStackInList];
        }

        Stack IGridGenerator.GetStackContainingSubStackWithEqualRopId(int indexOfSlate, int indexOfPad)
        {
            return newSlatesList[indexOfSlate]._padList[indexOfPad];
        }

        RopeHandler IGridGenerator.GetRopeHandlerByRopeId(int subStackRopeId)
        {
            return allRopes.Find(x => x.ropeId == subStackRopeId);
        }

        int IGridGenerator.GetTotalNumberOfSlates()
        {
            return curLevelData.allStacks.stackData.Count; // need to fix later
        }

        #endregion
        
    }

    [Serializable]
    public class StackAddress
    {
        public int indexOfSlate;
        public int indexOfStack;
        public int indexOfSubStack;
        [HideInInspector] public int ropeId;
    }

}