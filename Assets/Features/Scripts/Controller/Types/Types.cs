using System;
using UnityEngine;

[Serializable]
public class Types : MonoBehaviour
{
   public enum BrickType
   {  
      None,
      Hidden,
      Rope
   }
   
   public enum ChipType
   {
      None,
      Hidden,
      Mirror
   }
}
