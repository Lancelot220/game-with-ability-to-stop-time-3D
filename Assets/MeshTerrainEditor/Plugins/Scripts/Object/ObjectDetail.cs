using System;
using UnityEngine;

namespace MTE
{
    [Serializable]
    public class ObjectDetail : Detail
    {
        [SerializeField]
        public string name;

        [SerializeField]
        public bool autoName;

        [SerializeField]
        public GameObject Object;

        [SerializeField]
        public Vector3 MinScale = Vector3.one;

        [SerializeField]
        public Vector3 MaxScale = Vector3.one;

        [SerializeField]
        public bool UseUnifiedScale = true;

        public ObjectDetail ShallowCopy()
        {
            var copy = new ObjectDetail();
            copy.Object = Object;
            copy.MinScale = MinScale;
            copy.MaxScale = MaxScale;
            copy.UseUnifiedScale = UseUnifiedScale;
            return copy;
        }
    }
}