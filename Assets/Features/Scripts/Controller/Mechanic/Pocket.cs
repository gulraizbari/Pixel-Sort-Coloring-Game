using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sablo.Core;
using Sirenix.OdinInspector;
using UnityEngine;

public class Pocket : MonoBehaviour
{
    public List<Transform> pocketCell;
    public List<Chip> pocBrickList;
    [ShowInInspector] public const int MaxCapacity = 21;
    public bool isFull;
    [SerializeField] private float brickDelay = 0.15f;
    [Header("SpawnCellFields")] 
    [SerializeField] private GameObject objectPrefab;
    [SerializeField] private GameObject edgeObj;
    [SerializeField] private GameObject edgeObj2;
    [SerializeField] private int numberOfObjects = 10;
    [SerializeField] private float spacing = 2.0f;
    [SerializeField] private int noOfBricksToMoveOnContinue = 9;
    [Header("Haptic Values")] 
    [SerializeField] private float Amplitude = 0.5f;
    [SerializeField] private float Frquency = 1f;
    [SerializeField] private float duration = 0.017f;

    public void Initialize()
    {
        ArrangeObjects();
    }

    public void CheckIfPocketIsFull()
    {
        if (pocBrickList.Count >= MaxCapacity)
        {
            isFull = true;
        }
    }

    private void ArrangeObjects()
    {
        for (var index = 0; index < numberOfObjects; index++)
        {
            GameObject obj;
            if (index == 0)
            {
                obj = Instantiate(edgeObj2, transform);
            }
            else if (index == numberOfObjects - 1)
            {
                obj = Instantiate(edgeObj, transform);
            }
            else
            {
                obj = Instantiate(objectPrefab, transform);
            }
            
            var xPosition = (index - (numberOfObjects - 1) / 2.0f) * spacing;
            if (index == 0)
            {
                obj.transform.localPosition = new Vector3(-2.5f, 0, 0);
            } 
            else if (index == numberOfObjects - 1)
            {
                obj.transform.localPosition = new Vector3(2.2f, 0, 0);
            }
            else
            {
                obj.transform.localPosition = new Vector3(xPosition, 0, 0);
            }
            pocketCell.Add(obj.transform);
        }
    }

    public int FillBricksToPocket(List<Chip> selectedBricks)
    {
        var startPoint = pocBrickList.Count;
        var listOfBrickToRemove = new List<Chip>();
        for (var i = 0; i < selectedBricks.Count; i++)
        {
            if (pocBrickList.Count < MaxCapacity)
            {
                if (!pocBrickList.Contains(selectedBricks[i]))
                {
                    pocBrickList.Add(selectedBricks[i]);
                    listOfBrickToRemove.Add(selectedBricks[i]);
                }
            }
        }

        var endPoint = pocBrickList.Count;
        StartCoroutine(ResetBrickPos(startPoint, endPoint));
        ClearList(listOfBrickToRemove, selectedBricks);
        CheckIfPocketIsFull();
        if (pocBrickList.Count >= 14)
        {
            GameLoop.Instance.RunPocketFillTutorial();
        }

        return selectedBricks.Count;
    }

    public void InsertBricksToPocket(int similarItemIndex, List<Chip> selectedBricks)
    {
        var startPoint = similarItemIndex + 1;
        var indexOfBrickInsertion = similarItemIndex + 1;
        var listOfBrickToRemove = new List<Chip>();
        for (var i = 0; i < selectedBricks.Count; i++)
        {
            if (pocBrickList.Count < MaxCapacity)
            {
                pocBrickList.Insert(indexOfBrickInsertion, selectedBricks[i]);
                indexOfBrickInsertion++;
                listOfBrickToRemove.Add(selectedBricks[i]);
            }
        }

        ShiftBrickToNextIndex(similarItemIndex + selectedBricks.Count);
        var endPoint = startPoint + selectedBricks.Count;
        endPoint = Mathf.Min(endPoint, pocBrickList.Count);
        if (selectedBricks.Count > 0)
        {
            StartCoroutine(ResetBrickPos(startPoint, endPoint));
        }

        CheckIfPocketIsFull();
        if (pocBrickList.Count >= 14)
        {
            GameLoop.Instance.RunPocketFillTutorial();
        }
    }

    private void ShiftBrickToNextIndex(int from)
    {
        var index = 0;
        foreach (var brick in pocBrickList)
        {
            if (index > from)
            {
                var movePos = pocketCell[index].transform.position;
                movePos.y += Configs.GameConfig.brickYOffset;
                brick.transform
                    .DOMove(movePos, Configs.GameConfig.timeToMoveBrick)
                    .SetEase(Configs.GameConfig.brickJumpEaseType);
                brick.transform.DORotate(new Vector3(90, 90, 0), Configs.GameConfig.timeToMoveBrick);
            }

            index++;
        }
    }

    private void ClearList(List<Chip> brickListToRem, List<Chip> selectedList)
    {
        foreach (var brick in brickListToRem)
        {
            selectedList.Remove(brick);
        }
    }

    public void RemoveBrickFromPocket(List<Chip> bricksToRemove, int startPoint)
    {
        foreach (var brick in bricksToRemove)
        {
            pocBrickList.Remove(brick);
        }

        Invoke(nameof(Reposition), 0.01f);
    }

    private void Reposition()
    {
        for (var i = 0; i < pocBrickList.Count; i++)
        {
            var movePos = pocketCell[i].transform.position;
            movePos.y += Configs.GameConfig.brickYOffset;
            pocBrickList[i].transform.DOMove(movePos, Configs.GameConfig.timeToMoveBrick).SetEase(Configs.GameConfig.brickJumpEaseType);
            pocBrickList[i].transform.DORotate(new Vector3(90, 0, -90), Configs.GameConfig.timeToMoveBrick / 1.5f);
        }
    }

    private IEnumerator ResetBrickPos(int from, int to)
    {
        var posList = TapController.Instance.TrayHandler.GetPosListOfPocketWithEmptySpace();
        for (var i = from; i < to; i++)
        {
            if (i < pocBrickList.Count)
            {
                var brick = pocBrickList[i];
                pocBrickList[i].MoveToTargetCellPosWR(i, posList, () => ActionCallBack(brick));
                yield return new WaitForSeconds(Configs.GameConfig.delayBetweenNextBrick);
            }
        }
    }

    private void ActionCallBack(Chip chip)
    {
        TapController.Instance.GameFail();
        AudioManager.instance.ThrowSound();
        chip.transform.SetParent(transform);
    }

    public bool IsGameFailed(BrickColor nextCarrierColor)
    {
        var tapInstance = TapController.Instance;
        var carrier = tapInstance.curCarrierHandler;

        if (tapInstance.TrayHandler.GetPocketCount() >= MaxCapacity && carrier.GetCarrierCount() >= carrier.GetMaxSize())
        {
            // Check if there's a brick with the specified color in the list
            var foundBrick = pocBrickList.Find(brick => brick.brickColor == nextCarrierColor);
            if (foundBrick != null)
            {
                return false; // Game is not failed if a brick with the same color is found
            }
            else
            {
                return true; // Game is failed if no brick with the same color is found
            }
        }
        else if (pocBrickList.Count >= MaxCapacity && carrier.GetCarrierCount() < carrier.GetMaxSize())
        {
            return true; // fail
        }
        else
        {
            return false; // Game is not failed if the list is not at maximum capacity
        }
    }

    public List<Chip> MoveBricksBackToStack(Vector3 topBrick, float offset)
    {
        var posList = new List<Vector3>();
        topBrick.y += 0.11f;
        var count = 0;
        for (int i = 0; i < 50; i++)
        {
            posList.Add(topBrick);
            topBrick.y += (offset);
        }

        var endPoint = (pocBrickList.Count - 1);
        var startPoint = endPoint - noOfBricksToMoveOnContinue;
        if (startPoint < 0)
        {
            startPoint = 0;
        }

        var lastTwelve = new List<Chip>();

        for (int i = startPoint; i <= endPoint; i++)
        {
            lastTwelve.Add(pocBrickList[i]);
        }

        for (int i = pocBrickList.Count - 1; i >= startPoint; i--)
        {
            var brick = pocBrickList[i];
            var brickMaterial = brick.brickRenderer.material;
            var brickTexture = brick.oldBrickTexture;
            brickMaterial.mainTexture = brickTexture;
            brick.MoveBackToPocket(count, () => { }, posList);
            count++;
        }

        lastTwelve.ForEach(brick => pocBrickList.Remove(brick));
        lastTwelve.Reverse();
        return lastTwelve;
    }
}