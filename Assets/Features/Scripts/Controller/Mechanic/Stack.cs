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
    public int stackId;
    public List<Chip> chipsStack = new List<Chip>();
    public List<SubStack> subStacks = new List<SubStack>();
    public bool isMultiProducer;
    public List<SubStack> multiProducerDataList = new List<SubStack>();
    private List<int> _indicesToRemove = new List<int>();
    public GameObject sparkParticle;
    public bool isSparked;
    public int questionEndIndex;
    public bool isSelected;
    private bool isPulled;
    public GameObject padBase;
    public GameObject hiddenBox;
    private BrickColor _selectedObjType;
    [SerializeField] private SubStack chainedSubStack;
    [SerializeField] private StackAddress chainedStackAddress;
    [SerializeField] [Range(0, 5)] private float duration2 = 0.45f;
    [Header("Haptic Values")] 
    [SerializeField] private float Amplitude = 0.5f;
    [SerializeField] private float Frquency = 1f; 
    [SerializeField] private float duration = 0.02f;
    public ISlate SlateHandler;
    public IGridGenerator GridGeneratorHandler;

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
            foreach (var brick in chipsStack)
            {
                brick.transform.DOScale(new Vector3(1, 1, 1), 0.02f);
                yield return new WaitForSeconds(0.02f);
            }

            var originalPositions = chipsStack.Select(brick => brick.transform.position).ToList();
            for (var index = 0; index < chipsStack.Count; index++)
            {
                var brick = chipsStack[index];
                var offsetPos = brick.transform.position;
                offsetPos.y += 2;
                brick.transform.DOMove(offsetPos, 0.25f).SetEase(Ease.InQuad);
            }

            yield return new WaitForSeconds(0.25f);
            for (int i = 0; i < chipsStack.Count; i++)
            {
                chipsStack[i].transform.DOMove(originalPositions[i], 0.25f);
                yield return new WaitForSeconds(0.02f);
            }
        }
    }

    private void OnClickPad()
    {
        var tapInstance = TapController.Instance;
        if (tapInstance.curCarrierHandler.GetCarrierCount() < 9 || tapInstance.TrayHandler.GetPocketCount() < tapInstance.TrayHandler.GetMaxSize())
        {
            if (chipsStack.Count > 0)
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
        if (chipsStack.Count == questionEndIndex)
        {
            if (hiddenBox != null)
            {
                DOVirtual.DelayedCall(0.3f, () => { hiddenBox.SetActive(false); });
            }
        }
    }

    private void PickStack(float offset, bool isOn)
    {
        var shouldBreak = false;
        if (chipsStack.Count > 0)
        {
            shouldBreak = IfPadContainSubsStackWithRope();
            if (shouldBreak)
            {
                return;
            }
            var lastCoin = (chipsStack.Count) - 1;
            _selectedObjType = chipsStack[lastCoin].brickColor;
            var isDone = false;
            for (int i = chipsStack.Count - 1; i >= 0; i--)
            {
                if (chipsStack[i].brickColor.Equals(_selectedObjType))
                {
                    var brick = chipsStack[i];
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
            subStacks.Reverse();
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
            chainedSubStack = GridGeneratorHandler.GetSubStackByRopId(subStacks[0].ropeId, subStacks[0]);
            var indexOfChainedSubStack = GridGeneratorHandler.GetIndexOfSubStack(chainedSubStack);
            chainedStackAddress = GridGeneratorHandler.GetSubStackAddressByIndex(indexOfChainedSubStack);

            // logic to Check if both chained SubStack is at top ? call the move fn : do nothing
            var stackWithChainedRope = GridGeneratorHandler.GetStackContainingSubStackWithEqualRopId(chainedStackAddress.indexOfSlate, chainedStackAddress.indexOfStack);
            var rope = GridGeneratorHandler.GetRopeHandlerByRopeId(subStacks[0].ropeId);
            Rope theRope = null;
            if (rope.TryGetComponent(out Rope myRope))
            {
                theRope = myRope;
            }
            var ropeOriginalMaterial = GridGeneratorHandler.GetRopeMaterial;
            if (stackWithChainedRope)
            {
                if (stackWithChainedRope.subStacks.IndexOf(chainedSubStack) == 0) // chained subStack is at top
                {
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
        }

        return false;
    }

    private void TurnOffBase()
    {
        if (!isSparked)
        {
            if (chipsStack.Count <= 0)
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
        var pos = chipsStack[0].transform.position;
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
            chipsStack.RemoveAt(index);
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
            chipsStack.Add(brick);
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
        if (chipsStack.Count > 0)
        {
            StartCoroutine(JumpWithDelay());
        }

        IEnumerator JumpWithDelay()
        {
            yield return new WaitForSeconds(0.2f);
            foreach (var brick in chipsStack)
            {
                brick.transform.DOScale(new Vector3(1, 1, 1), 0.02f);
                yield return new WaitForSeconds(0.02f);
            }

            var originalPositions = chipsStack.Select(brick => brick.transform.position).ToList();
            for (var index = 0; index < chipsStack.Count; index++)
            {
                var brick = chipsStack[index];
                var offsetPos = brick.transform.position;
                offsetPos.y += 2;
                brick.transform.DOMove(offsetPos, 0.25f).SetEase(Ease.InQuad);
            }

            yield return new WaitForSeconds(0.25f);
            for (int i = 0; i < chipsStack.Count; i++)
            {
                chipsStack[i].transform.DOMove(originalPositions[i], 0.25f);
                yield return new WaitForSeconds(0.02f);
            }
        }
    }
}