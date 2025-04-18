using System;
using System.Collections.Generic;
using UnityEngine;

namespace MTE
{
#if UNITY_EDITOR
    [CreateAssetMenu(fileName = "NewGrassDefine.asset", menuName = "Mesh Terrain Editor/Grass Detail List")]
#endif
    [Serializable]
    public class GrassDetailList : ScriptableObject
    {
        public List<GrassDetail> grassDetailList = new List<GrassDetail>();
    }
}
