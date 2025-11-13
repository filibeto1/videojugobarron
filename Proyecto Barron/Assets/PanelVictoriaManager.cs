using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelVictoriaManager : MonoBehaviour
{
    public void ReiniciarNivel()
    {
        Time.timeScale = 1f; // Restaura el tiempo
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Reinicia el nivel actual
    }

    public void SiguienteNivel()
    {
        Time.timeScale = 1f; // Restaura el tiempo
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); // Carga el siguiente nivel
    }
}
