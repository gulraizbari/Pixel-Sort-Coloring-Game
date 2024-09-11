using System;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "levelData", menuName = "LevelData/SpawnManagerLevel")]
public class LevelData : ScriptableObject
{
    public List<Transform> pocketsSpawnPos;
    public int noOfPocketsToSpawn;
    public Building curLevelBuilding;
    public int totalCarriers;
    public float xOffsetForColliders = 1.92f;
    public StackOffsets slateBuilderOffsets;
    public List<CarrierInfo> carrierToSpawn;
    public Stacks allStacks;
    public List<LevelData> subLevel;
    public bool isMultiTierLevel;
    
    // [Button]
    // public void ChangeAllBrickSprites()
    // {
    //     foreach (var stack in allStacks)
    //     {
    //         foreach (var data in stack.stackData)
    //         {
    //             foreach (var brick in data.chipData)
    //             {
    //                 brick.brickImage = brick.myBrick.brickSprite;
    //             }
    //         }
    //     }
    // }
    //
    // [Button]
    // public void ReplaceColorInData(BrickColor newColor, BrickColor colorToReplace)
    // {
    //     foreach (var stack in allStacks)
    //     {
    //         foreach (var data in stack.stackData)
    //         {
    //             var getBrick = data.chipData.Find(col => col.myBrick.brickColor == colorToReplace);
    //             if (getBrick is not null)
    //             {
    //                 getBrick.myBrick = LoadBrickByBrickType(newColor);
    //             }
    //         }
    //     }
    // }

    public static Chip LoadBrickByBrickType(BrickColor Type)
    {
        var enemies = Resources.LoadAll<Chip>("Bricks");
        var enemy = enemies.FirstOrDefault(c => c.brickColor == Type);
        return enemy;
    }
}


[Serializable]
public class Stacks
{
    public List<StackData> stackData;
}

[Serializable]
public class StackData
{
    [BoxGroup("Stack")] public bool isMulti;
    [ShowIf("@isMulti == true")] public List<SubStack> multiProducerData;
    [BoxGroup("Stack")] public List<SubStack> chipData;
}

[Serializable]
public class SubStack
{
    public int startIndex;
    public int endIndex;
    public Types.BrickType stackType;
    [ShowIf("@stackType == Types.BrickType.Rope")] public int ropeId;
    [OnValueChanged("UpdateBrickImage")] public Chip myChip;
    // [OnValueChanged("UpdateBrickImage")] public Brick myBrick;
    [PreviewField] public Sprite chipImage;
     private void UpdateBrickImage()
     {
         if (myChip != null)
         {
             chipImage = myChip.brickSprite;
         }
         else
         {
             chipImage = null;
         }
     }
}

[Serializable]
public class CarrierInfo
{
    public Carrier CarrierType;
}

[Serializable]
public class StackOffsets
{
    public float horizontalOffset = 2f;
    public float verticalOffset = 4f;
    public float firstStackPos = 5.7f;
}