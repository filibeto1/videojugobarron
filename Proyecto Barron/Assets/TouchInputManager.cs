using UnityEngine;

public class TouchInputManager : MonoBehaviour
{
    void Update()
    {
        // Manejar múltiples toques
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            // Prevenir toques accidentales en UI
            if (touch.phase == TouchPhase.Began)
            {
                // Aquí puedes agregar lógica para otros controles táctiles
            }
        }
    }
}