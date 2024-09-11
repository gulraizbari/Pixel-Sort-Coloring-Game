using System.Collections.Generic;
using PixelSort.Feature.GridGeneration;
using Sirenix.OdinInspector;
using UnityEngine;

public class Slate : MonoBehaviour, ISlate
{
    [SerializeField] private GameObject stackBase;
    [SerializeField] private GameObject stackBaseMultiProducer;
    [SerializeField] private GameObject spark;
    [SerializeField] private GameObject hiddenPrefab;
    [SerializeField] private List<Chip> brickList;
    [SerializeField] private List<GameObject> _padObj = new List<GameObject>();
    [SerializeField] private float internalPaddingBwStack = 0.8f;
    [SerializeField] private float internalPaddingBwStack2 = 1;
    public float yOffset = 0.2f;
    public float baseOffset = 0.3f;
    public int slateIndex;
    public List<Stack> _padList = new List<Stack>();
    public List<StackData> myStackData;
    public float padYPos = 1.0f;
    public float padXOffset;

    public IGridGenerator GridGeneratorHandler;
    // public ISlateBuilder SlateBuilderInterface;
    
    public void Initialize()
    {
        SpawnPad();
        SetParent();
        SpawnStacks();
        SetPadCollider();
        SpawnAllBases();
        ReSetPadPos();
        ReverseSubStacksOfAllStacks();
        _padList.ForEach(pad => pad.InitialAnimationBricks());
    }

    private void SetParent()
    {
        foreach (var pad in _padList)
        {
            pad.transform.SetParent(transform);
        }
    }

    public void SetData(List<StackData> stackData)
    {
        myStackData = stackData;
    }

    private void SpawnPad()
    {
        for (int i = 0; i < myStackData.Count; i++)
        {
            var item = new GameObject("Pad" + i);
            _padObj.Add(item);
            _padList.Add(_padObj[i].AddComponent<Stack>());
            _padList[i].padBase = stackBase;
            _padList[i].stackId = i;
            _padList[i].isMultiProducer = myStackData[i].isMulti;
            _padList[i].GridGeneratorHandler = GridGeneratorHandler;
            // _padList[i].SlateBuilderInterface = SlateBuilderInterface;
            _padList[i].sparkParticle = spark;
            _padList[i].SlateHandler = this;
            myStackData[i].multiProducerData.ForEach(x => _padList[i].multiProducerDataList.Add(GetCopyOfOriginalPile(x)));
        }
    }

    private void SpawnAllBases()
    {
        foreach (var pad in _padList)
        {
            if (pad.bricksStack[0].brickColor != BrickColor.EmptyBrick)
            {
                pad.SpawnBase(pad.isMultiProducer ? stackBaseMultiProducer : stackBase);
            }
        }
    }

    private void SetPadCollider()
    {
        var intPadVal = GridGeneratorHandler.GetTotalNumberOfSlates() == 4 ? internalPaddingBwStack : internalPaddingBwStack2;
        for (int i = 0; i < _padObj.Count; i++)
        {
            if (_padList[i].bricksStack[0].brickColor != BrickColor.EmptyBrick)
            {
                var padCol = _padObj[i].AddComponent<BoxCollider>();
                var totalSize = (yOffset * _padList[i].bricksStack.Count);
                padCol.center = new Vector3((padXOffset + (i * intPadVal)) - intPadVal, (totalSize / 2) + 0.13f, _padList[i].bricksStack[0].transform.position.z);
                padCol.size = new Vector3(0.89f, totalSize, 1);
            }
        }
    }

    public void UpdateColliderLength(Stack updatedStack)
    {
        var intPadVal = GridGeneratorHandler.GetTotalNumberOfSlates() == 4 ? internalPaddingBwStack : internalPaddingBwStack2;
        var padCol = updatedStack.gameObject.GetComponent<BoxCollider>();
        var totalSize = (yOffset * updatedStack.bricksStack.Count);
        var padBrickPos = updatedStack.bricksStack[0].transform.position;
        padBrickPos.x += 0.5f;
        padCol.center = new Vector3(padBrickPos.x, (totalSize / 2) + 0.13f, padBrickPos.z);
        padCol.size = new Vector3(0.89f, totalSize, 1);
    }

    private void ReSetPadPos()
    {
        foreach (var pad in _padObj)
        {
            var padNewPos = pad.transform.position;
            padNewPos.y -= padYPos;
            padNewPos.x -= 0.5f;
            pad.transform.position = padNewPos;
        }
    }

    private void LoadBrick(StackData curStackData, Stack myStack)
    {
        var intPadVal = GridGeneratorHandler.GetTotalNumberOfSlates() == 4 ? internalPaddingBwStack : internalPaddingBwStack2;
        foreach (var pile in curStackData.chipData)
        {
            var pileCopy = GetCopyOfOriginalPile(pile);
            for (int i = pile.startIndex; i < pile.endIndex; i++)
            {
                var indexOfStack = myStackData.IndexOf(curStackData);
                var brick = Instantiate(pile.myChip, _padObj[indexOfStack].transform);
                var pos = transform.position;
                pos.y = baseOffset;
                pos.x -= intPadVal;
                pos.x += indexOfStack * (intPadVal);
                brick.transform.position = pos;
                baseOffset += yOffset;
                _padList[indexOfStack].bricksStack.Add(brick);
            }

            switch (pile.stackType)
            {
                case Types.BrickType.Hidden:
                {
                    var indexOfStack = myStackData.IndexOf(curStackData);
                    var newHiddenObj = Instantiate(hiddenPrefab, _padObj[indexOfStack].transform);
                    _padList[indexOfStack].hiddenBox = newHiddenObj;
                    _padList[indexOfStack].questionEndIndex = pile.endIndex;
                    var question = newHiddenObj.transform.GetChild(0).transform;
                    question.SetParent(_padObj[indexOfStack].transform);
                    var hidObjScale = newHiddenObj.transform.localScale;
                    hidObjScale.y *= (pile.endIndex - pile.startIndex);
                    newHiddenObj.transform.localScale = hidObjScale;
                    var firstBrickPos = _padList[indexOfStack].bricksStack[pile.startIndex].transform.position;
                    firstBrickPos.y -= 0.17f;
                    newHiddenObj.transform.position = firstBrickPos;
                    var midBrickPos = _padList[indexOfStack].bricksStack[pile.startIndex + 1].transform.position;
                    midBrickPos.y -= 0.05f;
                    midBrickPos.z -= 0.55f;
                    question.position = midBrickPos;
                    question.SetParent(newHiddenObj.transform);
                    break;
                }
                case Types.BrickType.Rope:
                {
                    var indexOfSlate = slateIndex;
                    var indexOfStack = myStackData.IndexOf(curStackData);
                    var indexOfSubStackInStack = myStackData[indexOfStack].chipData.IndexOf(pileCopy);
                    GridGeneratorHandler.AddRopePilesToList(pileCopy);
                    GridGeneratorHandler.AddToStackAddress(indexOfSlate, indexOfStack, indexOfSubStackInStack, pileCopy.ropeId);
                    break;
                }
            }

            if (myStack != null)
            {
                myStack.subStacks.Add(pileCopy);
            }
        }

        baseOffset = .3f;
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

    private void ReverseSubStacksOfAllStacks()
    {
        foreach (var stack in _padList)
        {
            stack.ReverseSubStacksList();
        }
    }

    private void SpawnStacks()
    {
        for (var index = 0; index < myStackData.Count; index++)
        {
            var stack = myStackData[index];
            LoadBrick(stack, index < _padList.Count ? _padList[index] : null); // This code is changed to for loop may cause trouble
        }
    }

    [Button]
    public void ProduceMultiProducerData(int stackId, SubStack topSubStack, Stack stack)
    {
        baseOffset = 0.3f;
        var intPadVal = GridGeneratorHandler.GetTotalNumberOfSlates() == 4 ? internalPaddingBwStack : internalPaddingBwStack2;
        var indexOfStack = stackId;
        var pile = topSubStack;
        for (int i = pile.startIndex; i < pile.endIndex; i++)
        {
            var brick = Instantiate(pile.myChip, _padObj[indexOfStack].transform);
            var pos = transform.position;
            pos.y = baseOffset;
            pos.x -= intPadVal + (0.5f);
            pos.x += indexOfStack * (intPadVal);
            brick.transform.position = pos;
            baseOffset += yOffset;
            _padList[indexOfStack].bricksStack.Add(brick);
            stack.InitialAnimationBrick();
        }
    }
}