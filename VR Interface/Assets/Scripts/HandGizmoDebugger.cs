using UnityEngine;

public class HandGizmoDebugger : MonoBehaviour
{
    public Color gizmoColor = Color.green;
    public float sphereRadius = 0.01f;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, sphereRadius);
    }
}
