using UnityEngine;

[ExecuteInEditMode]
public class ColliderVisualizer : MonoBehaviour
{
    void OnDrawGizmos()
    {
        Collider[] colliders = FindObjectsOfType<Collider>();

        foreach (Collider col in colliders)
        {
            Gizmos.color = Color.green;

            if (col is BoxCollider)
            {
                BoxCollider box = col as BoxCollider;
                Gizmos.matrix = box.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (col is SphereCollider)
            {
                SphereCollider sphere = col as SphereCollider;
                Gizmos.matrix = sphere.transform.localToWorldMatrix;
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
            else if (col is CapsuleCollider)
            {
                CapsuleCollider capsule = col as CapsuleCollider;
                Gizmos.matrix = capsule.transform.localToWorldMatrix;
                Gizmos.DrawWireSphere(capsule.center, capsule.radius);
            }
            else if (col is MeshCollider)
            {
                MeshCollider mesh = col as MeshCollider;
                Gizmos.matrix = mesh.transform.localToWorldMatrix;
                Gizmos.DrawWireMesh(mesh.sharedMesh);
            }
        }
    }
}