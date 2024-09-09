using System.Collections.Generic;
using UnityEngine;

public interface ITray
{
    List<Transform> GetPosListOfPocketWithEmptySpace();

    void AddBricksToPocket(List<Brick> selectedBricks,BrickColor selectedStackColor);

    int GetMaxSize();
    void MoveBricksToCurCarrier(BrickColor carrierColor, ICarrier curCarrierHandler,List<Brick> bricksToMov);

    int GetPocketCount();

    bool CheckIfGameFails(BrickColor nextCarrierColor);

    List<Brick> MoveBrickFromPocketToStack(Vector3 topBrick, float offset);

    List<Brick> GetPocketBrickList();

    public void RemoveBrickFromPocketByTray(List<Brick> bricksToRemove, int startPoint);

    List<Brick> FindBricksOfCarrierColor(ICarrier curCarrierHandler, BrickColor carrierColor);

}