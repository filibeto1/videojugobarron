using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Para usar textos en UI

public class VictoriaPanelController : MonoBehaviour
{
    public GameObject panelVictoria; // Panel de victoria
    public GameObject panelNiveles;  // Panel de niveles
    public Text puntuacionTexto;     // Texto para mostrar la puntuación

    private int puntosPorNivel = 100; // Puntos base por pasar el nivel
    private int puntosPorVida = 50;   // Puntos adicionales por vida restante

    void Start()
    {
        CalcularPuntuacion();
    }

    // Calcula la puntuación al completar el nivel
    void CalcularPuntuacion()
{
    int vidasRestantes = GameController.Instance.vidas; // Obtén las vidas restantes del GameController
    Debug.Log("Vidas restantes: " + vidasRestantes); // Imprime las vidas restantes en la consola

    int puntuacionNivel = puntosPorNivel + (vidasRestantes * puntosPorVida);
    PuntuacionTotal.AgregarPuntos(puntuacionNivel);

    if (puntuacionTexto != null)
    {
        puntuacionTexto.text = "Puntuación: " + PuntuacionTotal.ObtenerPuntos();
    }
}


    // Método para reiniciar el nivel actual
    public void ReiniciarNivel()
    {
        Time.timeScale = 1f; // Restaura el tiempo del juego
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Reinicia el nivel actual
    }

    // Método para ir al menú principal
    public void SalirAlMenu()
    {
        Time.timeScale = 1f; // Restaura el tiempo del juego
        SceneManager.LoadScene("MenuInicio"); // Carga la escena del menú principal
    }

    // Método para mostrar el panel de niveles
    public void VerNiveles()
    {
        panelVictoria.SetActive(false); // Oculta el panel de victoria
        panelNiveles.SetActive(true);   // Muestra el panel de niveles
    }

    // Método para cargar un nivel específico desde el panel de niveles
    public void CargarNivel(int nivelIndex)
    {
        Time.timeScale = 1f; // Restaura el tiempo del juego
        SceneManager.LoadScene(nivelIndex); // Carga el nivel seleccionado según su índice
    }

    // Método para regresar al panel de victoria
    public void RegresarAlPanelVictoria()
    {
        panelVictoria.SetActive(true);  // Muestra el panel de victoria
        panelNiveles.SetActive(false);  // Oculta el panel de niveles
    }
}

// Clase estática para manejar la puntuación total
public static class PuntuacionTotal
{
    private static int puntosTotales = 0;

    public static void AgregarPuntos(int puntos)
{
    Debug.Log("Puntos agregados: " + puntos); // Imprime los puntos que se agregan
    puntosTotales += puntos;
}

    public static int ObtenerPuntos()
    {
        return puntosTotales;
    }
}
