using System.Collections.Generic;
using DG.Tweening;
using RopeToolkit;
using Sablo.Gameplay;
using Sirenix.OdinInspector;
using UnityEngine;

public class Slate : MonoBehaviour, ISlate
{
    [SerializeField] private List<Brick> brickList;
    //public LevelData curLevelData;
    public float yOffset = .2f;
    public float baseOffset=.3f;
    public int slateIndex;
    [SerializeField] private List<GameObject> _padObj = new List<GameObject>();
    public List<Pad> _padList=new List<Pad>();
    [SerializeField] private float internalPaddingBwStack=.8f;
    [SerializeField] private float internalPaddingBwStack2=1;
    public List<StackData> myStackData;
    public float padYPos = 1.0f;
    public float padXOffset;
    [SerializeField] private GameObject stackBase;
      [SerializeField] private GameObject stackBaseMultiProducer;
    [SerializeField] private GameObject spark;
    [SerializeField] private GameObject hiddenPrefab;
    public ISlateBuilder SlateBuilderInterface;


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
            var item = new GameObject("Pad"+i);
            _padObj.Add(item);
            _padList.Add(_padObj[i].AddComponent<Pad>());
            _padList[i].padBase = stackBase;
            _padList[i].stackId = i;
            _padList[i].isMultiProducer = myStackData[i].isMulti;
            _padList[i].SlateBuilderInterface = SlateBuilderInterface;
            _padList[i].sparkParticle = spark;
            _padList[i].SlateHandler = this;
            myStackData[i].multiProducerData.ForEach(x =>  _padList[i].multiProducerDataList.Add(GetCopyOfOriginalPile(x)));

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
        float intPadVal = SlateBuilderInterface.GetTotalNumberOfSlates() == 4 ?  internalPaddingBwStack : internalPaddingBwStack2;
        for (int i = 0; i < _padObj.Count; i++)
        {
            if (_padList[i].bricksStack[0].brickColor != BrickColor.EmptyBrick)
            {
                var padCol = _padObj[i].AddComponent<BoxCollider>();
                float totalSize = (yOffset * _padList[i].bricksStack.Count)/*+0.3f*/;
                
                padCol.center = new Vector3((padXOffset+(i*intPadVal))-intPadVal, (totalSize/2)+0.13f, _padList[i].bricksStack[0].transform.position.z);
                padCol.size = new Vector3(0.89f, totalSize, 1);
            }
          
        }

    }

    public void UpdateColliderLength(Pad updatedStack)
    {   
        float intPadVal = SlateBuilderInterface.GetTotalNumberOfSlates() == 4 ?  internalPaddingBwStack : internalPaddingBwStack2;
        var padCol = updatedStack.gameObject.GetComponent<BoxCollider>();
        float totalSize = (yOffset * updatedStack.bricksStack.Count);
        var padBrickPos = updatedStack.bricksStack[0].transform.position;
        padBrickPos.x += 0.5f;
        padCol.center = new Vector3(padBrickPos.x, (totalSize/2)+0.13f, padBrickPos.z);
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
    
    private void LoadBrick(StackData curStackData,Pad myStack)
    {    
        float intPadVal = SlateBuilderInterface.GetTotalNumberOfSlates() == 4 ?  internalPaddingBwStack : internalPaddingBwStack2;
        foreach (var pile in curStackData.brickData)
        {
            var pileCopy = GetCopyOfOriginalPile(pile);
            for (int i = pile.startIndex; i < pile.endIndex; i++)
            {
                int indexOfStack = myStackData.IndexOf(curStackData);
                var brick = Instantiate(pile.myBrick,_padObj[indexOfStack].transform);
                var pos = transform.position;
                pos.y = baseOffset;
                pos.x -= intPadVal;
                pos.x += indexOfStack*(intPadVal);
                brick.transform.position = pos;
                baseOffset += yOffset;
                _padList[indexOfStack].bricksStack.Add(brick);
            }
            switch (pile.stackType)
            {
                case Types.BrickType.Hidden:
                {   
                    int indexOfStack = myStackData.IndexOf(curStackData);
                    var newHiddenObj = Instantiate(hiddenPrefab,_padObj[indexOfStack].transform);
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
                    var midBrickPos = _padList[indexOfStack].bricksStack[pile.startIndex+1].transform.position;
                    midBrickPos.y -= 0.05f;
                    midBrickPos.z -= 0.55f;
                    question.position = midBrickPos;
                   question.SetParent(newHiddenObj.transform);
                    break;
                }
                case Types.BrickType.Rope:
                {
                    int indexOfSlate = slateIndex;
                    int indexOfStack = myStackData.IndexOf(curStackData);
                    int indexOfSubStackInStack = myStackData[indexOfStack].brickData.IndexOf(pileCopy);
                    SlateBuilderInterface.AddRopePilesToList(pileCopy);
                    SlateBuilderInterface.AddToStackAddress(indexOfSlate,indexOfStack,indexOfSubStackInStack,pileCopy.ropeId);
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
       pileCopy.brickImage = pile.brickImage;
       pileCopy.myBrick = pile.myBrick;
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
            LoadBrick(stack, index < _padList.Count? _padList[index]:null);     // This code is changed to for loop may cause trouble
        }
    }
    
    [Button]
    public void ProduceMultiProducerData(int stackId, SubStack topSubStack, Pad stack)
    {   
            baseOffset = .3f;
          float intPadVal = SlateBuilderInterface.GetTotalNumberOfSlates() == 4 ?  internalPaddingBwStack : internalPaddingBwStack2;
          int indexOfStack = stackId;
          Debug.Log(indexOfStack);
          var pile = topSubStack;
          
          for (int i = pile.startIndex; i < pile.endIndex; i++)
          {
              var brick = Instantiate(pile.myBrick,_padObj[indexOfStack].transform);
              var pos = transform.position;
              pos.y = baseOffset;
              pos.x -= intPadVal+(0.5f);
              pos.x += indexOfStack*(intPadVal);
              brick.transform.position = pos;
              baseOffset += yOffset;
              _padList[indexOfStack].bricksStack.Add(brick);
              stack.InitialAnimationBrick();
          }
        
    }
}
