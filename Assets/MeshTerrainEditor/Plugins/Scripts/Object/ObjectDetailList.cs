using System.Collections.Generic;
using UnityEngine;

namespace MTE
{
#if UNITY_EDITOR
    [CreateAssetMenu(fileName = "NewObjectDefine.asset", menuName = "Mesh Terrain Editor/Object Detail List")]
#endif
    public class ObjectDetailList : ScriptableObject
    {
        public List<ObjectDetail> list = new List<ObjectDetail>();
    }
}