using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0f, 0f, -10f); // Offset para 2D
    public float smoothSpeed = 0.125f;

    [Header("Camera Bounds (Optional)")]
    public bool useBounds = false;
    public float minX = -50f;
    public float maxX = 50f;
    public float minY = -50f;
    public float maxY = 50f;

    [Header("Look Settings")]
    public bool lookAtTarget = false; // Para juegos 3D, en 2D debe estar en false

    void LateUpdate()
    {
        if (target == null)
        {
            // Debug.LogWarning("⚠️ CameraFollow: No hay objetivo asignado");
            return;
        }

        // Calcular la posición deseada
        Vector3 desiredPosition = target.position + offset;

        // Aplicar límites si están habilitados
        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }

        // Suavizar el movimiento
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // ✅ IMPORTANTE: Mantener la Z fija para juegos 2D
        smoothedPosition.z = offset.z;

        // Aplicar la nueva posición
        transform.position = smoothedPosition;

        // Opcional: Mirar hacia el objetivo (solo para 3D)
        if (lookAtTarget)
        {
            transform.LookAt(target);
        }
    }

    // Método para cambiar el objetivo dinámicamente
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (newTarget != null)
        {
            Debug.Log($"🎯 CameraFollow: Nuevo objetivo establecido - {newTarget.name}");
        }
    }

    // Método para cambiar el offset
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
        Debug.Log($"📐 CameraFollow: Nuevo offset establecido - {newOffset}");
    }

    // Gizmos para visualizar los límites en el editor
    void OnDrawGizmosSelected()
    {
        if (useBounds)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, 0f);
            Vector3 size = new Vector3(maxX - minX, maxY - minY, 0f);
            Gizmos.DrawWireCube(center, size);
        }

        // Dibujar línea hacia el target
        if (target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}