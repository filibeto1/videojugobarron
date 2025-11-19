using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void OnSinglePlayerClicked()
    {
        Debug.Log("✅ Modo seleccionado: 1 JUGADOR (vs Bot)");
        GameSettings.IsSinglePlayer = true;

        // Cargar la escena de juego
        SceneManager.LoadScene("Nivel2 1");
    }

    public void OnTwoPlayersClicked()
    {
        Debug.Log("✅ Modo seleccionado: 2 JUGADORES");
        GameSettings.IsSinglePlayer = false;

        // Cargar la escena de juego
        SceneManager.LoadScene("Nivel2 1");
    }
}