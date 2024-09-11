using RopeToolkit;
using UnityEngine;

namespace PixelSort.Feature.GridGeneration
{
    public interface IGridGenerator
    {
        void ReInitialize();
        void AddRopePilesToList(SubStack subStack);
        void AddToStackAddress(int indexOfSlate, int indexOfStack, int indexOfSubStack,int ropeId);
        SubStack GetSubStackByRopId(int existingRopId, SubStack existingSubStack);
        int GetIndexOfSubStack(SubStack subStackToGetIndexOf);
        StackAddress GetSubStackAddressByRopId(int existingRopId, StackAddress existingSubStackAddress);
        StackAddress GetSubStackAddressByIndex(int indexOfSubStackInList);
        Stack GetStackContainingSubStackWithEqualRopId(int indexOfSlate, int indexOfPad);
        RopeHandler GetRopeHandlerByRopeId(int subStackRopeId);
        int GetTotalNumberOfSlates();
        Material GetRopeMaterial { get; }
    }

}
