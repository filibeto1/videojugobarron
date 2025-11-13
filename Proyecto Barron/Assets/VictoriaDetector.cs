using UnityEngine;

public class VictoriaDetector : MonoBehaviour
{
    public GameObject panelVictoria; // Referencia al panel de victoria

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // Verifica si el jugador toca la imagen
        {
            ActivarPanelVictoria();
        }
    }

    void ActivarPanelVictoria()
    {
        Time.timeScale = 0f; // Pausa el juego
        panelVictoria.SetActive(true); // Muestra el panel de victoria
    }
}
