using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using PixelSort.Feature.GridGeneration;
using RopeToolkit;
using TMPro;
using UnityEngine;

public class Stack : MonoBehaviour
{
    public List<Chip> bricksStack = new List<Chip>();
    private BrickColor _selectedObjType;
    public bool isSelected;
    public GameObject padBase;
    public GameObject hiddenBox;
    List<int> _indicesToRemove = new List<int>();
    public GameObject sparkParticle;
    public int stackId;
    public bool isSparked;
    public int questionEndIndex;
    public List<SubStack> subStacks = new List<SubStack>();

    public IGridGenerator GridGeneratorHandler;
    // public ISlateBuilder SlateBuilderInterface;

    [Header("Haptic Values")] 
    [SerializeField] private float Amplitude = 0.5f;
    [SerializeField] private float Frquency = 1f; 
    [SerializeField] private float duration = 0.02f;
    public bool isMultiProducer;
    [SerializeField] private SubStack chainedSubStack;
    [SerializeField] private StackAddress chainedStackAddress;
    public ISlate SlateHandler;
    public List<SubStack> multiProducerDataList = new List<SubStack>();
    private bool isPulled;
    [SerializeField] [Range(0, 5)] private float duration2 = 0.45f;

    private void OnMouseDown()
    {
        if (!GameLoop.Instance.isUiActive)
        {
            OnClickPad();
            if (Tutorial.Instance != null)
            {
                Tutorial.Instance.IncreaseTouchCount();
            }
        }
    }

    public void InitialAnimationBricks()
    {
        StartCoroutine(JumpWithDelay());

        IEnumerator JumpWithDelay()
        {
            foreach (var brick in bricksStack)
            {
                brick.transform.DOScale(new Vector3(1, 1, 1), 0.02f);
                yield return new WaitForSeconds(0.02f);
            }

            var originalPositions = bricksStack.Select(brick => brick.transform.position).ToList();
            for (var index = 0; index < bricksStack.Count; index++)
            {
                var brick = bricksStack[index];
                var offsetPos = brick.transform.position;
                offsetPos.y += 2;
                brick.transform.DOMove(offsetPos, 0.25f).SetEase(Ease.InQuad);
            }

            yield return new WaitForSeconds(0.25f);
            for (int i = 0; i < bricksStack.Count; i++)
            {
                bricksStack[i].transform.DOMove(originalPositions[i], 0.25f);
                yield return new WaitForSeconds(0.02f);
            }
        }
    }

    public void OnClickPad()
    {
        var tapInstance = TapController.Instance;
        if (tapInstance.curCarrierHandler.GetCarrierCount() < 9 || tapInstance.TrayHandler.GetPocketCount() < tapInstance.TrayHandler.GetMaxSize())
        {
            if (bricksStack.Count > 0)
            {
                AudioManager.instance.AddSound();
            }
            PickStack(1f, true);
            ShowHiddenBricks();
            tapInstance.MoveSelectedBricksToTargetContainer();
        }
    }

    private void ShowHiddenBricks()
    {
        if (bricksStack.Count == questionEndIndex)
        {
            if (hiddenBox != null)
            {
                DOVirtual.DelayedCall(0.3f, () => { hiddenBox.SetActive(false); });
            }
        }
    }

    public void PickStack(float offset, bool isOn)
    {
        var shouldBreak = false;
        if (bricksStack.Count > 0)
        {
            shouldBreak = IfPadContainSubsStackWithRope();
            if (shouldBreak)
            {
                Debug.Log("Execution halted");
                return;
            }
            var lastCoin = (bricksStack.Count) - 1;
            _selectedObjType = bricksStack[lastCoin].brickColor;
            var isDone = false;
            for (int i = bricksStack.Count - 1; i >= 0; i--)
            {
                if (bricksStack[i].brickColor.Equals(_selectedObjType))
                {
                    var brick = bricksStack[i];
                    TapController.Instance.AddBricksToSelectedStack(brick, this);
                    _indicesToRemove.Add(i);
                    isDone = isOn;
                }
                else
                {
                    break;
                }
            }
            
            if (_indicesToRemove.Count != 0 && isDone)
            {
                ClearBricksFromLastSelected(offset);
            }
            
            _indicesToRemove.Clear();
            isSelected = false;
            TurnOffBase();
            
            if (subStacks.Count > 0)
            {
                subStacks.RemoveAt(0);
            }
        }
    }
    
    private bool IfPadContainSubsStackWithRope()
    {
        if (subStacks.Count > 0 && subStacks[0].stackType == Types.BrickType.Rope)
        {
            Debug.Log($"<color=green>{subStacks[0].ropeId} : RopeId! </color>");
            chainedSubStack = GridGeneratorHandler.GetSubStackByRopId(subStacks[0].ropeId, subStacks[0]);
            
            var indexOfChainedSubStack = GridGeneratorHandler.GetIndexOfSubStack(chainedSubStack);
            chainedStackAddress = GridGeneratorHandler.GetSubStackAddressByIndex(indexOfChainedSubStack);

            // logic to Check if both chained SubStack is at top ? call the move fn : do nothing
            Stack stackWithChainedRope = GridGeneratorHandler.GetStackContainingSubStackWithEqualRopId(chainedStackAddress.indexOfSlate, chainedStackAddress.indexOfStack);
            var rope = GridGeneratorHandler.GetRopeHandlerByRopeId(subStacks[0].ropeId);
            Rope theRope = null;
            if (rope.TryGetComponent(out Rope myRope))
                theRope = myRope;
            var ropeOriginalMaterial = GridGeneratorHandler.GetRopeMaterial;
            
            if (stackWithChainedRope)
            {
                Debug.Log($"<color=red>{stackWithChainedRope.subStacks[0].myChip.brickColor} : Is not null </color>");
                if (stackWithChainedRope.subStacks.IndexOf(chainedSubStack) == 0) // chained subStack is at top
                {
                    Debug.Log($"<color=red>{stackWithChainedRope.subStacks.IndexOf(chainedSubStack)} : chained Is at top </color>");
                    stackWithChainedRope.subStacks[0].stackType = Types.BrickType.None;
                    Destroy(rope.gameObject, 0.01f);
                    stackWithChainedRope.OnClickPad();
                    return false;
                }
                else
                {
                    // play can't move animation
                    var mySequence = DOTween.Sequence();
                    var shakeStrengthX = 0f;
                    var shakeStrengthY = 2f;
                    var shakeStrengthZ = 0f;
                    var shakeVibrato = 12;
                    var shakeRandomness = 0;
                    mySequence.Append(transform.DOShakePosition(duration2,
                        new Vector3(shakeStrengthX, shakeStrengthY, shakeStrengthZ), shakeVibrato,
                        shakeRandomness, false,
                        false).SetEase(Ease.Linear));

                    mySequence.Join(stackWithChainedRope.transform.DOShakePosition(duration2,
                        new Vector3(shakeStrengthX, shakeStrengthY, shakeStrengthZ), shakeVibrato,
                        shakeRandomness, false,
                        false).SetEase(Ease.Linear));

                    mySequence.OnStart(() => theRope.material = theRope.redMaterial);
                    mySequence.OnComplete(() => theRope.material = ropeOriginalMaterial);
                    return true;
                }
            }
            else
            {
                Debug.Log("<color=red>Can not find pad </color>");
            }
        }

        return false;
    }

    private void TurnOffBase()
    {
        if (!isSparked)
        {
            if (bricksStack.Count <= 0)
            {
                DOVirtual.DelayedCall(0.3f, () =>
                {
                    var spark = Instantiate(sparkParticle, padBase.transform.position, Quaternion.identity);
                    spark.transform.SetParent(transform);
                    if (!isMultiProducer)
                    {
                        padBase.SetActive(false);
                        isSparked = true;
                    }
                    else
                        ProduceMultiProducerDataOnEmptyStack();
                });
            }
        }
    }

    public void SpawnBase(GameObject baseObj)
    {
        padBase = Instantiate(baseObj, transform);
        var pos = bricksStack[0].transform.position;
        pos.y -= 0.25f;
        padBase.transform.position = pos;
        if (isMultiProducer)
        {
            padBase.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = multiProducerDataList.Count.ToString();
        }
    }

    void ClearBricksFromLastSelected(float offset)
    {
        foreach (var index in _indicesToRemove)
        {
            bricksStack.RemoveAt(index);
        }
    }

    public void ReverseSubStacksList()
    {
        subStacks.Reverse();
    }

    public void AddBrickBackToStack(List<Chip> bricks)
    {
        foreach (var brick in bricks)
        {
            bricksStack.Add(brick);
        }
    }

    private void ProduceMultiProducerDataOnEmptyStack()
    {
        var totalCnt = multiProducerDataList.Count;
        if (totalCnt > 0)
        {
            SlateHandler.ProduceMultiProducerData(stackId, multiProducerDataList[0], this);
            multiProducerDataList.RemoveAt(0);
            totalCnt = multiProducerDataList.Count;
            if (totalCnt > 0)
            {
                padBase.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = totalCnt.ToString();
            }
            else
            {
                padBase.transform.GetChild(0).gameObject.SetActive(false);
            }
            isPulled = true;
        }
        else
        {
            padBase.SetActive(false);
        }
    }

    public void InitialAnimationBrick()
    {
        if (bricksStack.Count > 0)
        {
            StartCoroutine(JumpWithDelay());
        }

        IEnumerator JumpWithDelay()
        {
            yield return new WaitForSeconds(0.2f);
            foreach (var brick in bricksStack)
            {
                brick.transform.DOScale(new Vector3(1, 1, 1), 0.02f);
                yield return new WaitForSeconds(0.02f);
            }

            var originalPositions = bricksStack.Select(brick => brick.transform.position).ToList();
            for (var index = 0; index < bricksStack.Count; index++)
            {
                var brick = bricksStack[index];
                var offsetPos = brick.transform.position;
                offsetPos.y += 2;
                brick.transform.DOMove(offsetPos, 0.25f).SetEase(Ease.InQuad);
            }

            yield return new WaitForSeconds(0.25f);
            for (int i = 0; i < bricksStack.Count; i++)
            {
                bricksStack[i].transform.DOMove(originalPositions[i], 0.25f);
                yield return new WaitForSeconds(0.02f);
            }
        }
    }
}