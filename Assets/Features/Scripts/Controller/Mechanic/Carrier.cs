using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using PixelSort.Feature.GridGeneration;
using Sablo.Core;
using Sirenix.OdinInspector;
using UnityEngine;

public class Carrier : MonoBehaviour, ICarrier
{
    public List<Chip> carrierBrickList;
    public List<Transform> brickPositions;
    [HideInInspector] public List<Vector3> brickCellPositions;
    public int SpawnOrder;
    public BrickColor carrierColor;
    public Transform nextCarrierPos;
    [ShowInInspector] private const int MaxCapacity = 9;
    [SerializeField] private Transform lostPos;
    public bool isUpFront;
    public bool isFull;
    private Action _moveAllCarrier;
    public IRoller RollerInterface;
    public ITray TrayInterface;
    [SerializeField] private float brickDelay = 0.15f;
    public int bricksAddCount;
    public bool lastCarrier;
    public bool isMoving = false;
    // public ISlateBuilder SlateBuilderInterface;
    public IGridGenerator GridGeneratorHandler;
    public ILevelManager LevelManagerInterface;

    [Header("Haptic Values")] 
    [SerializeField] private float Amplitude = 0.5f;
    [SerializeField] private float Frquency = 1f; 
    [SerializeField] private float duration = 0.017f;

    private void SetCarrierFillAmount()
    {
        if (carrierBrickList.Count >= MaxCapacity)
        {
            isFull = true;
        }
    }
    
    public void Initialize()
    {
        isUpFront = true;
        TapController.Instance.curCarrierColor = carrierColor;
    }

    public void MoveToNextCarrierPos()
    {
        transform.DOMove(nextCarrierPos.position, 0.5f).SetEase(Ease.Linear);
    }

    private void ResetPositionOfALlBricks()
    {
        for (int i = 0; i < carrierBrickList.Count; i++)
        {
            var newPos = brickCellPositions[i];
            newPos.y += Configs.GameConfig.brickYOffset;
            carrierBrickList[i].transform.position = newPos;
        }
    }

    public IEnumerator ResetBrickPos(int from, List<Chip> selectedBricks)
    {
        var curCarrier = TapController.Instance.curCarrierHandler;
        var curCarrierPosList = TapController.Instance.curCarrierHandler.GetBrickPositionList();

        var bricksToMove = new List<Chip>(selectedBricks);
        var lastBrickIndex = bricksToMove.Count - 1;
        var lastBrick = bricksToMove[lastBrickIndex];

        foreach (var brick in bricksToMove)
        {
            var indexOfBrick = carrierBrickList.IndexOf(brick);

            brick.MoveToTargetCellPos(indexOfBrick, brick == lastBrick
                    ? () =>
                    {
                        ActionCallBack(curCarrier, brick);
                        TapController.Instance.curCarrierHandler.MoveAside();
                    }
                    : () =>
                    {
                        ActionCallBack(curCarrier, brick);
                    }
                , curCarrierPosList);

            yield return new WaitForSeconds(Configs.GameConfig.delayBetweenNextBrick);
        }
    }

    private void ActionCallBack(ICarrier curCarrier, Chip chip)
    {
        curCarrier.IncreaseBrickCount();
        chip.transform.SetParent(RollerInterface.GetRollerTransform());
        var brickMaterial = chip.brickRenderer.material;
        var brickTexture = RollerInterface.GetNewBrickTexture();
        brickMaterial.mainTexture = brickTexture;
        AudioManager.instance.ThrowSound();
    }


    private IEnumerator ThrowAllBricksToBuilding()
    {
        var allBricksReached = false;
        var carrier = TapController.Instance.curCarrierHandler;
        var brickCount = carrier.GetCarrierBricks().Count;
        var brickList = carrier.GetCarrierBricks();

        for (var i = 0; i < brickCount; i++)
        {
            var targetBrick = Building.Instance.GetCurrentBrick() ?? throw new ArgumentNullException("Building.Instance.GetCurrentBrick()");
            if (targetBrick != null)
            {
                var brick = brickList[i];
                brick.MoveToTarget(targetBrick, () =>
                {
                    AudioManager.instance.PlaceBrick();
                    var particlePos = brick.transform.position;
                    particlePos.y -= 0.1f;
                    SpawnParticleOnBrickDrop(RollerInterface.GetSpawnParticle, particlePos);
                    brick.gameObject.SetActive(false);
                    targetBrick.gameObject.SetActive(true);
                    var defaultScale = targetBrick.localScale;
                    targetBrick.DOPunchScale(defaultScale, 0.2f, 1, 0.2f).SetEase(Ease.OutBounce);
                    if (i == brickCount)
                    {
                        allBricksReached = true;
                    }
                });
            }

            yield return new WaitForSeconds(0.06f);
        }

        yield return new WaitUntil(() => allBricksReached);
        TapController.Instance.IncreaseNumberOfCarriersPass();
        ReInitializeSlateBuilder();
        carrier.GetCarrierTransform().DOMove(lostPos.position, 0.5f).SetEase(Ease.Linear).OnStart(() => isMoving = true)
            .OnComplete(MoveBrickFromPocketToCarrier);
        RollerInterface.MoveAllCarrierToNextOne();
    }

    private void SpawnParticleOnBrickDrop(GameObject particle, Vector3 pos)
    {
        var newParticle = Instantiate(particle);
        newParticle.transform.position = pos;
        Destroy(newParticle, 0.34f);
    }

    private void MoveBrickFromPocketToCarrier()
    {
        var color = TapController.Instance.curCarrierColor;
        var brickToRem = new List<Chip>();
        TrayInterface.MoveBricksToCurCarrier(color, TapController.Instance.curCarrierHandler, brickToRem);
    }

    public void SetInitialPositions(Transform brickTransform)
    {
        brickCellPositions.Add(brickTransform.position);
    }

    private void ReInitializeSlateBuilder()
    {
        if (RollerInterface.IsLastCarrierOfSubLevel())
        {
            if (LevelManagerInterface.GetTotalSubLevelCount != 0)
            {
                ChangeColorBySubLevel();
            }
            if (LevelManagerInterface.GetSubLevel < LevelManagerInterface.GetTotalSubLevelCount - 1)
            {
                DOVirtual.DelayedCall(0.2f, () =>
                {
                    LevelManagerInterface.ReloadLevel();
                    GridGeneratorHandler.ReInitialize();
                });
            }
        }
    }

    private void ChangeColorBySubLevel()
    {
        switch (LevelManagerInterface.GetSubLevel)
        {
            case 0:
                var firstLevelBricks = 9 * (LevelManagerInterface.SubLevelList[LevelManagerInterface.GetSubLevel].carrierToSpawn.Count);
                Building.Instance.ChangeColorOfBrickAnimationByLevel(0, firstLevelBricks);
                break;
            case 1:
                var lastLevelBricks = 9 * (LevelManagerInterface.SubLevelList[(LevelManagerInterface.GetSubLevel) - 1].carrierToSpawn.Count);
                var secondLevelBricks = 9 * (LevelManagerInterface.SubLevelList[LevelManagerInterface.GetSubLevel].carrierToSpawn.Count);
                RollerInterface.MaterialSumCount = lastLevelBricks + secondLevelBricks;
                Building.Instance.ChangeColorOfBrickAnimationByLevel(lastLevelBricks, RollerInterface.MaterialSumCount);
                break;
            case 2:
                var max = Building.Instance.GetMaterialCount;
                Building.Instance.ChangeColorOfBrickAnimationByLevel(RollerInterface.MaterialSumCount, max);
                break;
        }
    }

    void ICarrier.SetOffMoving()
    {
        isMoving = false;
    }

    int ICarrier.CarrierCapacity()
    {
        var capacity = MaxCapacity - carrierBrickList.Count;
        return capacity;
    }

    #region Interface Methods

    void ICarrier.MoveAside()
    {
        if (isFull && TapController.Instance.curCarrierHandler.GetBrickCount() >= MaxCapacity)
        {
            var curCarrier = TapController.Instance.curCarrierHandler.GetCarrierTransform();
            StartCoroutine(ThrowAllBricksToBuilding());
        }
    }

    Transform ICarrier.GetCarrierTransform()
    {
        return transform;
    }

    public void OnMoveAside(Action fnToPerform)
    {
        _moveAllCarrier = fnToPerform;
    }

    int ICarrier.GetCarrierCount()
    {
        return carrierBrickList.Count;
    }

    int ICarrier.GetMaxSize()
    {
        return MaxCapacity;
    }

    bool ICarrier.IsCarrierMoving()
    {
        return isMoving;
    }

    void ICarrier.IncreaseBrickCount()
    {
        bricksAddCount++;
    }

    int ICarrier.GetBrickCount()
    {
        return bricksAddCount;
    }

    List<Chip> ICarrier.GetCarrierBricks()
    {
        return carrierBrickList;
    }

    List<Vector3> ICarrier.GetBrickPositionList()
    {
        return brickCellPositions;
    }


    void ICarrier.AddBricksToCarrier(List<Chip> selectedBricks, Action callBack)
    {
        for (var i = 0; i < selectedBricks.Count; i++)
        {
            if (carrierBrickList.Count < MaxCapacity)
            {
                if (!carrierBrickList.Contains(selectedBricks[i]))
                {
                    carrierBrickList.Add(selectedBricks[i]);
                }
            }
        }

        callBack?.Invoke();
        IsLastCarrierFull();
        SetCarrierFillAmount();
    }

    private void IsLastCarrierFull()
    {
        var carrierList = RollerInterface.GetCarrierList();
        var indexOfLastCarrier = carrierList.Count - 1;
        if (this == carrierList[indexOfLastCarrier] && carrierBrickList.Count == MaxCapacity)
        {
            DOVirtual.DelayedCall(2.56f, () =>
            {
                GameLoop.Instance.GameWin();
            });
        }
    }

    #endregion
}