using UnityEngine;

public class DebugPuntoReinicio : MonoBehaviour
{
    void OnDrawGizmos()
    {
        // Dibujar un gizmo en el editor para ver el punto de reinicio
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(1, 1, 0));
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.3f);
    }
}