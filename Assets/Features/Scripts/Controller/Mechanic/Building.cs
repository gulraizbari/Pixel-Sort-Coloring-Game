using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Building : MonoBehaviour
{
    [SerializeField] private List<Transform> buildingParents;
    public List<Transform> buildingBricks;
    public List<Brick> brickContainer;
    public int currentBrickIndex;
    public static Building Instance;
    [SerializeField] private List<GameObject> Ornaments;
    [SerializeField] private Building coloredBuilding;
    [SerializeField] private List<Material> buildingColoredMaterials;

    [SerializeField] private Material whiteMaterial;
    public bool isColorizeAble;
    public Slider slider;
    public TMP_Text percentageText;

    public int GetMaterialCount
    {
        get => buildingColoredMaterials.Count;
    }

    public void UpdateBuildingPercentage()
    {
        if (slider == null)
            return;
        var targetValue = 1.0f * currentBrickIndex / buildingBricks.Count;
    
        // Animate the slider value
        slider.DOValue(targetValue, 0.01f).SetEase(Ease.Linear);

        // Animate the text percentage
        var initialPercentage = slider.value * 100f;
        var targetPercentage = targetValue * 100f;

        DOTween.To(() => initialPercentage, x => 
        {
            initialPercentage = x;
            percentageText.text = $"{x:F0} %";
        }, targetPercentage, 0.01f).SetEase(Ease.Linear);

        slider.gameObject.SetActive(true);
    }

#if UNITY_EDITOR
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            currentBrickIndex = buildingBricks.Count - 9;
        }
    }
    
#endif
    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        SetScaleOfOrnamentsItemsToZero();
        if(slider)
            slider.gameObject.SetActive(false);
    }

    public Transform GetCurrentBrick()
    {
        int maxCount = buildingBricks.Count;
        Transform brick = null;
        /*var brick = buildingBricks[maxCount - 1];*/
        if (currentBrickIndex < maxCount)
        {
             brick = buildingBricks[currentBrickIndex];
            currentBrickIndex++;
            UpdateBuildingPercentage();
            TurnOnOrnamentsOnEnd();
        }
        else
        {
            brick = null;
        }
        return brick;
    }

    [Button]
    public void AssignColoredMaterials()
    {
        buildingColoredMaterials.Clear();
        for (int i = 0; i < coloredBuilding.buildingBricks.Count; i++)
        {
            buildingColoredMaterials.Add(coloredBuilding.buildingBricks[i].GetComponent<MeshRenderer>().sharedMaterial);
        }
    }
    
    [Button]
    public void AssignWhiteMaterial()
    {
        for (int i = 0; i < buildingBricks.Count; i++)
        {
            buildingBricks[i].GetComponent<MeshRenderer>().sharedMaterial = whiteMaterial;
        }
    }
    
    [Button]
    public void AssignBuildingBricks()
    {
        buildingBricks.Clear();
        buildingParents.ForEach(building =>
        {
            for (int i = 0; i < building.childCount; i++)
            {
                buildingBricks.Add(building.GetChild(i));
                building.GetChild(i).gameObject.SetActive(false);
            }   
        });
    }
    [Button]
    public void ActivateFullBuilding()
    {
        buildingBricks.ForEach(transform1 =>
        {
            transform1.gameObject.SetActive(true);
        });
    }
    [Button]
    public void DeActivateFullBuilding()
    {
        buildingBricks.ForEach(transform1 =>
        {
            transform1.gameObject.SetActive(false);
        });
    }

    public void AddBrickToBuilding(Brick brickToAdd)
    {
        brickContainer.Add(brickToAdd);
    }

    private void TurnOnOrnamentsOnEnd()
    {
        
        if (currentBrickIndex >= (buildingBricks.Count-1))
        {   
            if(slider) 
                slider.gameObject.SetActive(false);
            // Debug.LogError("boom");
            DOVirtual.DelayedCall(0.5f, (() =>
            {
                for (var index = 0; index < Ornaments.Count; index++)
                {
                    var item = Ornaments[index];
                    item.gameObject.SetActive(true);
                    item.transform.DOScale(new Vector3(1.2f,1.2f,1.2f), 0.35f).OnComplete(() =>
                    {
                        item.transform.DOScale(new Vector3(1f,1f,1f), 0.21f);
                    });
                }
            }));
        }
        
    }

    private void SetScaleOfOrnamentsItemsToZero()
    {
        foreach (var item in Ornaments)
        {
            item.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        }
    }

    public void ChangeColorOfBrickAnimation()
    {
        StartCoroutine(ChangeColorWithDelay());

         IEnumerator ChangeColorWithDelay()
        {
            for (var index = 0; index < buildingBricks.Count; index++)
            {
                var brick = buildingBricks[index];
                var defaultScale = brick.localScale;
                brick.DOPunchScale(defaultScale*(0.8f), 0.2f, 1, 0.2f).SetEase(Ease.OutBounce);
                brick.GetComponent<MeshRenderer>().material.DOColor(buildingColoredMaterials[index].color , 0.05f);
                yield return new WaitForSeconds(0.01f);
            }
        }
    }
    
    public void ChangeColorOfBrickAnimationByLevel(int startPoint, int totalBrickPerLevel)
    {   
        Debug.LogError("ChangeColorOfBrickAnimationByLevel");
        StartCoroutine(ChangeColorWithDelay());

        IEnumerator ChangeColorWithDelay()
        {
            for (var index = startPoint; index < totalBrickPerLevel; index++)
            {
                var brick = buildingBricks[index];
                var defaultScale = brick.localScale;
                brick.DOPunchScale(defaultScale*(0.8f), 0.2f, 1, 0.2f).SetEase(Ease.OutBounce);
                brick.GetComponent<MeshRenderer>().material.DOColor(buildingColoredMaterials[index].color , 0.05f);
                yield return new WaitForSeconds(0.01f);
            }
        }
    }
    
    

}
