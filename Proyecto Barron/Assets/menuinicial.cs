using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void Jugar()
    {
        // Carga la siguiente escena en la lista de escenas
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Salir()
    {
        // Mensaje de salida para pruebas en el editor
        Debug.Log("Saliendo del juego...");
        // Cierra la aplicación (no funciona en el editor)
        Application.Quit();
    }
}
