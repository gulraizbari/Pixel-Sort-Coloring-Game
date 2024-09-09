using System;
using System.Collections.Generic;
using DG.Tweening;
using Sablo.Core;
using Sirenix.OdinInspector;
using UnityEngine;

public class Brick : MonoBehaviour
{
    public BrickColor brickColor;
    [PreviewField] public Sprite brickSprite;
    [SerializeField] float brickMoveSpeed = 0.37f;
    [SerializeField] private float brickYOffset = 0.09f;
    [SerializeField] private Material duplicateMaterial;
    public Renderer brickRenderer;
    public Texture oldBrickTexture;
    public Texture newBrickTexture;
    public Transform ropeJoint;

    [Button]
    public void ChangeMeshToBig(Mesh BigMesh)
    {
        transform.GetChild(0).GetComponent<MeshFilter>().mesh = BigMesh;
    }

    public void MoveToTargetCellPos(int targetIndex, Action action, List<Vector3> posList)
    {
        var targetPos = posList[targetIndex];
        targetPos.y += Configs.GameConfig.brickYOffset;
        transform
            .DOJump(targetPos, Configs.GameConfig.brickJumpPower, 1, Configs.GameConfig.timeToMoveBrick)
            .SetEase(Configs.GameConfig.brickJumpEaseType).OnComplete(() =>
            {
                DOVirtual.DelayedCall(Configs.GameConfig.delayBeforeCarrierToMoveAside, () => action?.Invoke());
            });
        transform.DORotate(new Vector3(90, -90, 0), Configs.GameConfig.timeToMoveBrick / 1.5f);
    }

    public void MoveToTargetCellPosByTray(int targetIndex, Action action, List<Vector3> posList)
    {
        var targetPos = posList[targetIndex];
        targetPos.y += Configs.GameConfig.brickYOffset;
        transform.DORotate(new Vector3(90, 90, 0), Configs.GameConfig.timeToMoveBrick);
        transform
            .DOJump(targetPos, Configs.GameConfig.brickJumpPower, 1, Configs.GameConfig.timeToMovBrPocToCar)
            .SetEase(Configs.GameConfig.brickJumpEaseType2).OnComplete(() =>
            {
                DOVirtual.DelayedCall(Configs.GameConfig.delayBeforeCarrierToMoveAside, () => action?.Invoke());
            });
    }

    public void MoveToTargetCellPosWR(int targetIndex, List<Transform> posList, Action action)
    {
        var targetPos = posList[targetIndex].position;
        targetPos.y += Configs.GameConfig.brickYOffset;
        transform
            .DOJump(targetPos, Configs.GameConfig.brickJumpPower, 1, Configs.GameConfig.timeToMoveBrick)
            .SetEase(Configs.GameConfig.brickJumpEaseType)
            .OnComplete(() => { action?.Invoke(); });
        transform.DORotate(new Vector3(90, 90, 0), Configs.GameConfig.timeToMoveBrick / 1.5f);
    }

    public void MoveToTarget(Transform target, Action OnTargetReached)
    {
        transform.DORotateQuaternion(target.transform.rotation, Configs.GameConfig.timeToMoveBrick);
        transform
            .DOJump(target.transform.position, Configs.GameConfig.brickJumpPower, 1, Configs.GameConfig.timeToMoveBrick)
            .SetEase(Configs.GameConfig.brickJumpEaseType).OnComplete(() =>
            {
                OnTargetReached?.Invoke();
                OnTargetReached = null;
            });
    }

    public void MoveBackToPocket(int targetIndex, Action action, List<Vector3> posList)
    {
        var targetPos = posList[targetIndex];
        targetPos.y += Configs.GameConfig.brickYOffset;
        transform
            .DOJump(targetPos, Configs.GameConfig.brickJumpPower, 1, Configs.GameConfig.timeToMoveBrick)
            .SetEase(Configs.GameConfig.brickJumpEaseType).OnComplete((() =>
            {
                DOVirtual.DelayedCall(Configs.GameConfig.delayBeforeCarrierToMoveAside, () => action?.Invoke());
            }));
        transform.DORotate(new Vector3(0, 0, 0), Configs.GameConfig.timeToMoveBrick / 1.5f);
    }
}