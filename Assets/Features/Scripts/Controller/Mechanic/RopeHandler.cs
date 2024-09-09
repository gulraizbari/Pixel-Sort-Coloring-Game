using System.Collections;
using System.Collections.Generic;
using RopeToolkit;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RopeToolkit
{
    public class RopeHandler : MonoBehaviour
    {
        [SerializeField] private Rope rope;
        [SerializeField] public RopeConnection ropeConnection1 , ropeConnection2;
        public int ropeId;

        [Button]
        public void SetRope(Transform t1, Transform t2)
        {
            ropeConnection1.transformSettings.transform = t1;
            ropeConnection2.transformSettings.transform = t2;
            ropeConnection1.EnforceConnection();
            ropeConnection2.EnforceConnection();
        }
    }
}

