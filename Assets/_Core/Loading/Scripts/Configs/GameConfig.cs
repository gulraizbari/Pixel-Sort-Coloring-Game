using DG.Tweening;
using UnityEngine;

public partial class GameConfig : ScriptableObject
{
    public float timeToMoveBrick;
    public float timeToMovBrPocToCar;
    public float delayBetweenNextBrick;
    public float delayToMoveNextBrickToCar;
    public float brickJumpPower;
    public Ease brickJumpEaseType;
    public Ease brickJumpEaseType2;
    public float delayBeforeCarrierToMoveAside;
    public float brickYOffset;
    public float buildingBrickUpScaleValue;
    
    [Header("Dancing Bricks")] 
    public float timeToMoveUp;
    public float timeToMoveBack;
    public float offsetYDancing;
    public float nextDanceDelay;
    public float danceDelay;
  
}
