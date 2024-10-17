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
        [SerializeField] private LevelData currentLevelData;
        [SerializeField] private List<GameObject> _stackObj = new List<GameObject>();
        [SerializeField] private List<Slate> _newSlatesList;
        [SerializeField] private List<RopeHandler> _allRopes;
        [SerializeField] private List<SubStack> _stacksWithRope = new List<SubStack>();
        [SerializeField] private List<StackAddress> _stackAddresses = new List<StackAddress>();
        [SerializeField] private RopeHandler _ropeHandler;
        [SerializeField] private Material _ropeMaterial;
        [SerializeField] private GameObject _ropeEdge;
        [SerializeField] private Stack _coloredStack;
        [SerializeField] private Stack _emptyStack;
        [SerializeField] private GameObject _stackBase;
        [SerializeField] private GameObject _spark;
        [SerializeField] private Transform _trayParent;
        [SerializeField] private Transform _emptyParent;
        [SerializeField] private Transform _stacksParent;
        [SerializeField] private float _yOffset = 0.3f;
        [SerializeField] private float _baseOffset = 0.3f;
        private Slate _stack;
        private Stacks _stackDataList;
        private List<Stack> _stackList = new List<Stack>();
        private List<Transform> _gridPositions = new List<Transform>();
        private int _rows;
        private int _columns;
        private int _stackIndex;
        private int _slateIndex;
        public ILevelManager LevelManagerHandler { get; set; }
        
        public Material GetRopeMaterial => _ropeMaterial;

        public override void Initialize()
        {
            SetLevelData();
            SpawnData(transform, currentLevelData, _emptyStack, _coloredStack);
            SortStackByRope();
            SpawnMultipleRopes();
            SetParent();
            SpawnStack();
            SpawnStacks();
            GameLoop.Instance.OnContinueLevel(OnLevelResume);
        }

        void IGridGenerator.ReInitialize()
        {
            _newSlatesList.ForEach(x => x.gameObject.SetActive(false));
            _newSlatesList.Clear();
            SetLevelData();
            _stacksWithRope.Clear();
            _allRopes.Clear();
            _stackAddresses.Clear();
            SpawnData(transform, currentLevelData, _emptyStack, _coloredStack);
            SortStackByRope();
            SpawnMultipleRopes();
            SetParent();
            SpawnStack();
            SpawnStacks();
            GameLoop.Instance.UpdateSubLevelNo(LevelManagerHandler.GetTotalSubLevelCount - 1, LevelManagerHandler.GetSubLevel);
        }

        private void SetLevelData()
        {
            currentLevelData = LevelManagerHandler.GetCurrentLevel;
            _stackDataList = currentLevelData.allStacks;
        }

        public void OnLevelResume()
        {
            var minStack = FindPadWithLeastBrickCount();
            var isMinStackEmpty = minStack.chipsStack.Count == 0;
            var lastBrickIndex = minStack.chipsStack.Count - 1;
            var basePos = minStack.padBase.transform.position;
            var lastTwelve = TapController.Instance.TrayHandler.MoveBrickFromPocketToStack(
                minStack.chipsStack.Count > 0 ? minStack.chipsStack[lastBrickIndex].transform.position : basePos,
                0.32f);
            lastTwelve.ForEach(brick => _newSlatesList[_slateIndex]._padList[_stackIndex].chipsStack.Add(brick));
            if (!isMinStackEmpty)
            {
                _newSlatesList[_slateIndex].UpdateColliderLength(minStack);
            }
        }

        private void SpawnData(Transform parentObject, LevelData level, Stack emptyStack, Stack coloredStack)
        {
            _rows = level.Column;
            _columns = level.Row;
            var grid = level.Grid;
            var tileSpacing = 2.5f;
            var totalWidth = (_rows - 1) * tileSpacing;
            var totalHeight = (_columns - 1) * tileSpacing;
            var gridCenterOffset = new Vector3(totalWidth / 2, 0, totalHeight / 2);

            for (int row = 0; row < _rows; row++)
            {
                for (int col = 0; col < _columns; col++)
                {
                    // var cellData = grid[row, col];
                    var tilePosition = new Vector3(row * tileSpacing, 0, (level.Column - 1 - col) * tileSpacing) - gridCenterOffset + parentObject.transform.position;
                    switch (grid[row, col].tileType)
                    {
                        case TileType.Empty:
                            SpawnNewStack(emptyStack, _emptyParent, false, tilePosition, row, col);
                            break;
                        case TileType.Stack:
                            var stack = SpawnNewStack(coloredStack, _stacksParent, true, tilePosition, row, col);
                            _stackList.Add(stack);
                            _gridPositions.Add(stack.transform);
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

        private Stack SpawnNewStack(Stack chip, Transform parentObject, bool isTileOn, Vector3 tilePosition, int row, int col)
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
                    var indexOfStack = _stackDataList.stackData.IndexOf(currentStackData);
                    var chip = Instantiate(pile.myChip, _stackObj[indexOfStack].transform);
                    _stackList[indexOfStack].chipsStack.Add(chip);
                    position.y = _baseOffset;
                    chip.transform.position = position;
                    _baseOffset += _yOffset;
                }

                if (myStack != null)
                {
                    myStack.subStacks.Add(pileCopy);
                }
            }
        }

        private void SpawnStacks()
        {
            for (var index = 0; index < currentLevelData.allStacks.stackData.Count; index++)
            {
                var stack = currentLevelData.allStacks.stackData[index];
                if (index < _stackList.Count)
                {
                    LoadStack(stack, _stackList[index], _gridPositions[index].position); 
                    _stackList[index].UpdateColliderLength(_stackList[index]);
                }
                else
                {
                    LoadStack(stack, null, _gridPositions[index].position);
                }
                _baseOffset = 0.3f;
            }
        }

        private void SpawnStack()
        {
            for (int i = 0; i < _stackDataList.stackData.Count; i++)
            {
                var item = new GameObject("Stack" + i);
                item.transform.parent = _trayParent.transform;
                _stackObj.Add(item);
                _stackList[i].stackId = i;
                _stackList[i].isMultiProducer = _stackDataList.stackData[i].isMulti;
                _stackList[i].sparkParticle = _spark;
                _stackList[i].GridGeneratorHandler = this;
                _stackDataList.stackData[i].multiProducerData.ForEach(x => _stackList[i].multiProducerDataList.Add(GetCopyOfOriginalPile(x)));
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

            for (int i = 0; i < _newSlatesList.Count; i++)
            {
                for (int j = 0; j < _newSlatesList[i]._padList.Count; j++)
                {
                    var currentPad = _newSlatesList[i]._padList[j];
                    if (currentPad.chipsStack.Count == 0)
                    {
                        if (currentPad.chipsStack.Count < minBrickCount)
                        {
                            stackWithMinBrickCount = currentPad;
                            minBrickCount = currentPad.chipsStack.Count;
                            _stackIndex = j;
                            _slateIndex = i;
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
                            _stackIndex = j;
                            _slateIndex = i;
                        }
                    }
                }
            }

            return stackWithMinBrickCount;
        }

        private void SetParent()
        {
            foreach (var slate in _newSlatesList)
            {
                slate.transform.SetParent(transform);
            }
        }
        
        #region Rope Mechanic

        private void SpawnRope(Stack stack1, int brickIndex1, Stack stack2, int brickIndex2, int ropeId)
        {
            var newRopeHandler = Instantiate(_ropeHandler);
            _allRopes.Add(newRopeHandler);
            newRopeHandler.ropeId = ropeId;
            var edgePos1 = stack1.chipsStack[brickIndex1].transform.position;
            var edgePos2 = stack2.chipsStack[brickIndex2].transform.position;
            edgePos1.z -= 0.65f;
            edgePos2.z -= 0.65f;
            DOVirtual.DelayedCall(1f, () =>
            {
                newRopeHandler.SetRope(stack1.chipsStack[brickIndex1].ropeJoint,
                    stack2.chipsStack[brickIndex2].ropeJoint);
                DOVirtual.DelayedCall(1f, () =>
                {
                    SpawnRopeEdges(edgePos1, edgePos2, newRopeHandler.gameObject);
                    newRopeHandler.GetComponent<Rope>().material = _ropeMaterial;
                });
            });
        }

        private void SpawnRopeEdges(Vector3 edgePos1, Vector3 edgePos2, GameObject rope)
        {
            var edge1 = Instantiate(_ropeEdge, rope.transform, true);
            var edge2 = Instantiate(_ropeEdge, rope.transform, true);
            edge1.transform.position = edgePos1;
            edge2.transform.position = edgePos2;
        }

        public void AddRopePilesToList(SubStack subStack)
        {
            _stacksWithRope.Add(subStack);
        }

        void IGridGenerator.AddToStackAddress(int indexOfSlate, int indexOfStack, int indexOfSubStack, int ropeId)
        {
            var newAddress = new StackAddress
            {
                indexOfSlate = indexOfSlate, indexOfStack = indexOfStack, indexOfSubStack = indexOfSubStack,
                ropeId = ropeId
            };
            _stackAddresses.Add(newAddress);
        }

        private void SpawnMultipleRopes()
        {
            for (int i = 0; i < _stacksWithRope.Count; i += 2)
            {
                var nextIndex = i + 1;
                var pad1 = _newSlatesList[_stackAddresses[i].indexOfSlate]._padList[_stackAddresses[i].indexOfStack];
                var pad1BrickIndex = (_stacksWithRope[i].startIndex) + 1;
                var pad2 = _newSlatesList[_stackAddresses[nextIndex].indexOfSlate]
                    ._padList[_stackAddresses[nextIndex].indexOfStack];
                var pad2BrickIndex = (_stacksWithRope[nextIndex].startIndex) + 1;
                SpawnRope(pad1, pad1BrickIndex, pad2, pad2BrickIndex, _stackAddresses[i].ropeId);
            }
        }

        private void SortStackByRope()
        {
            _stacksWithRope.Sort((x, y) => x.ropeId.CompareTo(y.ropeId));
            _stackAddresses.Sort((x, y) => x.ropeId.CompareTo(y.ropeId));
        }

        SubStack IGridGenerator.GetSubStackByRopId(int existingRopId, SubStack existingSubStack)
        {
            var similarRopIdStack = _stacksWithRope.Find(x => x.ropeId == existingRopId && x != existingSubStack);
            return similarRopIdStack;
        }

        int IGridGenerator.GetIndexOfSubStack(SubStack subStackToGetIndexOf)
        {
            return _stacksWithRope.IndexOf(subStackToGetIndexOf);
        }

        StackAddress IGridGenerator.GetSubStackAddressByRopId(int existingRopId, StackAddress existingSubStackAddress)
        {
            var similarRopIdStackAddress =
                _stackAddresses.Find(x => x.ropeId == existingRopId && x != existingSubStackAddress);
            return similarRopIdStackAddress;
        }

        StackAddress IGridGenerator.GetSubStackAddressByIndex(int indexOfSubStackInList)
        {
            return _stackAddresses[indexOfSubStackInList];
        }

        Stack IGridGenerator.GetStackContainingSubStackWithEqualRopId(int indexOfSlate, int indexOfPad)
        {
            return _newSlatesList[indexOfSlate]._padList[indexOfPad];
        }

        RopeHandler IGridGenerator.GetRopeHandlerByRopeId(int subStackRopeId)
        {
            return _allRopes.Find(x => x.ropeId == subStackRopeId);
        }

        int IGridGenerator.GetTotalNumberOfSlates()
        {
            return currentLevelData.allStacks.stackData.Count; // need to fix later
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