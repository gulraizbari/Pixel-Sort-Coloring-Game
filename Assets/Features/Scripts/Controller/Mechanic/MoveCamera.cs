using System;
using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] private Transform targetBuilding;
    [SerializeField] private float rotationSpeed = 10f; // Speed of the rotation
    [SerializeField] private Vector3 distance;
    [SerializeField] private GameObject[] ObjectToHide;

    public static MoveCamera Instance;

    private bool isRotating = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

   
    public void TriggerFunctionUsingButton()
    {   
        targetBuilding = Building.Instance.transform;
        var newRotation = new Vector3(35f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        HideObjects();
        transform.DOLocalRotate(newRotation, 1f);
        MoveCameraCloseToBuilding(RotateAroundBuildingInLoop);
    }

    private void MoveCameraCloseToBuilding(Action onCompleteMovement)
    {
        transform.DOMove(targetBuilding.position + distance, 1f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                onCompleteMovement?.Invoke();
            });
    }

    private void RotateAroundBuildingInLoop()
    {
        isRotating = true;
    }

    private void HideObjects()
    {
        foreach (var obj in ObjectToHide)
        {
            obj.SetActive(false);
        }
    }

    private void Update()
    {
        if (isRotating)
        {
            // Rotate around the target building at the specified speed
            transform.RotateAround(targetBuilding.position, Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}