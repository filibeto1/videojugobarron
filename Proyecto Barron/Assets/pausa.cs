using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Para manejar las escenas

public class Pausa : MonoBehaviour
{
    public GameObject menuPausa; // Referencia al menú de pausa en la escena
    public bool juegoPausado = false; // Estado del juego (pausado o no)

    private void Update()
    {
        // Detecta si se presiona la tecla Escape o P
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (juegoPausado)
            {
                Reanudar();
            }
            else
            {
                Pausar();
            }
        }
    }

    // Método para pausar el juego
    public void Pausar()
    {
        juegoPausado = true;
        menuPausa.SetActive(true); // Muestra el menú de pausa
        Time.timeScale = 0f; // Detiene el tiempo del juego
    }

    // Método para reanudar el juego
    public void Reanudar()
    {
        juegoPausado = false;
        menuPausa.SetActive(false); // Oculta el menú de pausa
        Time.timeScale = 1f; // Restaura el tiempo del juego
    }

    // MÉTODO NUEVO: Reiniciar el nivel actual
    public void ReiniciarNivel()
    {
        // Primero reanudamos el juego para que todo funcione correctamente
        Time.timeScale = 1f;
        juegoPausado = false;

        // Obtenemos el nombre de la escena actual y la recargamos
        string nombreEscenaActual = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(nombreEscenaActual);

        // Alternativa: Si prefieres usar el índice de la escena
        // int indiceEscenaActual = SceneManager.GetActiveScene().buildIndex;
        // SceneManager.LoadScene(indiceEscenaActual);
    }

    // Método para salir al menú principal
    public void SalirAlMenu()
    {
        Time.timeScale = 1f; // Restaura el tiempo del juego
        SceneManager.LoadScene("MenuInicio"); // Carga la escena del menú principal
    }
}