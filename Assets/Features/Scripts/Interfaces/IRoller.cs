using System.Collections.Generic;
using UnityEngine;

public interface IRoller
{
    void MoveAllCarrierToNextOne();
    void RemoveCarrierFromRoller(Carrier carrierToRemove);

    List<Carrier> GetCarrierList();

    bool IsLastCarrierOfSubLevel();
    
    GameObject GetSpawnParticle { get; }

    Texture GetNewBrickTexture();

    Transform GetRollerTransform();
    
     int MaterialSumCount { get ; set; }
}