using System.Collections.Generic;
using UnityEngine;

public interface ITray
{
    List<Transform> GetPosListOfPocketWithEmptySpace();

    void AddBricksToPocket(List<Chip> selectedBricks,BrickColor selectedStackColor);

    int GetMaxSize();
    void MoveBricksToCurrentCarrier(BrickColor carrierColor, ICarrier currentCarrierHandler,List<Chip> bricksToMov);

    int GetPocketCount();

    bool CheckIfGameFails(BrickColor nextCarrierColor);

    List<Chip> MoveBrickFromPocketToStack(Vector3 topBrick, float offset);

    List<Chip> GetPocketBrickList();

    public void RemoveBrickFromPocketByTray(List<Chip> bricksToRemove, int startPoint);

    List<Chip> FindBricksOfCarrierColor(ICarrier curCarrierHandler, BrickColor carrierColor);

}