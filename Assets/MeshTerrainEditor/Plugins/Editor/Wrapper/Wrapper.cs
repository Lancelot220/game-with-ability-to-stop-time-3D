using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MTE
{
internal class Wrapper : IEditor
{
    public int Id { get; } = 4;
    public bool Enabled { get; set; } = true;
    public string Name { get; } = nameof(Wrapper);
    public Texture Icon { get; } = MTEStyles.WrapperToolIcon;
    public string Header { get { return StringTable.Get(C.Wrapper_Header); } }
    public string Description { get { return StringTable.Get(C.Wrapper_Description); } }

    private Collider shape;
    private float offset = 0.1f;

    public void DoArgsGUI()
    {
        shape = (Collider)EditorGUILayout.ObjectField(StringTable.Get(C.Shape), shape, typeof(Collider), true);
        offset = EditorGUILayout.Slider(offset, -0.5f, 0.5f);
        GUI.enabled = (shape != null);
        if (GUILayout.Button(StringTable.Get(C.Wrap), GUILayout.Height(80)))
        {
            Wrap(shape);
        }
        GUI.enabled = true;

        //TODO Notify user if the mesh quads are too sparse, in which case wrapping has no effect.
    }
    
    private readonly List<MeshWrapModifyGroup> modifyGroups = new List<MeshWrapModifyGroup>(4);
    private void Wrap(Collider collider)
    {
        var bounds = collider.bounds;
        var center = bounds.center;
        var radius = bounds.extents.magnitude;

        modifyGroups.Clear();
        foreach (var target in MTEContext.Targets)
        {
            MTE.VertexMap.GetAffectedVertex(target, center, radius, out var indexList);

            if (indexList.Count == 0)
            {
                continue;
            }

            var meshFilter = target.GetComponent<MeshFilter>();
            var meshCollider = target.GetComponent<MeshCollider>();
            var mesh = meshFilter.sharedMesh;
            modifyGroups.Add(new MeshWrapModifyGroup(shape, target, mesh, meshCollider, indexList, vDistance: null));
        }

        if (modifyGroups.Count == 0)
        {
            return;
        }

        Utility.Record("Wrap Mesh");

        foreach (var modifyGroup in modifyGroups)
        {
            WrapMesh(modifyGroup.shapeCollider, modifyGroup.mesh, modifyGroup.meshTransform,
                modifyGroup.meshCollider, modifyGroup.vIndex);
        }

        MeshSaving.UpdateDirtyMeshCollidersImmediately();
        MeshSaving.HandleMeshSave();
    }

    private void WrapMesh(Collider shapeCollider, Mesh mesh, Transform meshTransofrm, MeshCollider meshCollider, List<int> indexList)
    {
        var vertexes = mesh.vertices;
        bool modified = false;
        var maxY = meshCollider.bounds.max.y + 100;
        for (var i = 0; i < indexList.Count; ++i)
        {
            var index = indexList[i];
            var localO = vertexes[index];
            var o = meshTransofrm.TransformPoint(localO);
            
            var ray = new Ray(new Vector3(o.x, maxY, o.z), new Vector3(0, -1, 0));
            if (!shapeCollider.Raycast(ray, out var hitInfo, float.PositiveInfinity))
            {
                continue;
            }

            modified = true;
            var worldHitPoint = hitInfo.point;
            var meshPoint = meshTransofrm.InverseTransformPoint(worldHitPoint);
            vertexes[index].y = meshPoint.y + offset;
        }

        if (!modified)
        {
            return;
        }

        mesh.vertices = vertexes;

        MeshSaving.SetMeshDirty(meshCollider.gameObject);
        MeshSaving.SetMeshColliderDirty(meshCollider, mesh.vertexCount);
    }

    public void OnSceneGUI()
    {
        if (Settings.DebugMode)
        {
            foreach (var modifyGroup in modifyGroups)
            {
                Utility.ShowAffectedVertices(modifyGroup.gameObject, modifyGroup.mesh, modifyGroup.vIndex);
            }
        }
    }

    public HashSet<Hotkey> DefineHotkeys()
    {
        return new HashSet<Hotkey>();
    }

    public bool WantMouseMove { get; } = false;

    public bool WillEditMesh { get; } = true;
}
}