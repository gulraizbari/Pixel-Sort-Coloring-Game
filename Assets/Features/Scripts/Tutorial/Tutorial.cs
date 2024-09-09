using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    [SerializeField] private List<Transform> targets;
    public Vector3 scaleDown = new Vector3(1f, 2f, 1f);
    public float duration = 0.35f;
    public int touchCount;
    public static Tutorial Instance;
    private bool _isActive;

    [Button]
    public void MoveToTarget(Transform target)
    {
        transform.position = target.position;
    }
    
    private void Start()
    {
        Invoke(nameof(Initialize),0.8f);
    }

    private void Initialize()
    {
        if (PlayerPrefs.GetInt("Tutorial", 1) == 1)
        {
            if(Instance==null)
                Instance = this;
            _isActive=true;
            MoveToTarget(targets[0]);
            KeepPopping();
        }
        else
        {
            transform.gameObject.SetActive(false);
        }
    }

   

    private void KeepPopping()
    {
        Sequence scaleSequence = DOTween.Sequence();
        
        // Add the scale up tween
        scaleSequence.Append(transform.DOScale(scaleDown*1.1f, duration).SetEase(Ease.InOutSine));

        // Add the scale down tween
        scaleSequence.Append(transform.DOScale(scaleDown, duration).SetEase(Ease.InOutSine));

        // Set the sequence to loop infinitely
        scaleSequence.SetLoops(-1, LoopType.Yoyo);
    }

    public void IncreaseTouchCount()
    {
        if (_isActive)
        {
            touchCount++;
            if(touchCount<3)
                MoveToTarget(targets[touchCount]);
            if(touchCount>=3)
                DisableTutorial();
        }
        
    }

    private void DisableTutorial()
    {
        PlayerPrefs.SetInt("Tutorial",0);
        transform.gameObject.SetActive(false);
    }
    
    
}
