using System;
using System.Collections.Generic;
using DCL.Components;
using UnityEngine;

namespace DCL.Models
{
    [Serializable]
    public class DecentralandEntity
    {
        public Dictionary<CLASS_ID_COMPONENT, BaseComponent> components = new Dictionary<CLASS_ID_COMPONENT, BaseComponent>();
        public GameObject gameObject;
        public string entityId;

        public System.Action<MonoBehaviour> OnComponentUpdated;
        public System.Action OnShapeUpdated;

        public GameObject meshGameObject;
        public BaseShape currentShape;
    }
}
