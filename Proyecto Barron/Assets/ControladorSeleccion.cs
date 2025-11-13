using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ControladorSeleccion : MonoBehaviour
{
    public Image personajeImagen;
    public TextMeshProUGUI textoNombre;

    private int indiceActual = 0;

    void Start()
    {
        ActualizarPersonaje();
    }

    public void SiguientePersonaje()
    {
        indiceActual++;
        if (indiceActual >= GameManager.Instance.personajes.Count)
        {
            indiceActual = 0;
        }
        ActualizarPersonaje();
    }

    public void PersonajeAnterior()
    {
        indiceActual--;
        if (indiceActual < 0)
        {
            indiceActual = GameManager.Instance.personajes.Count - 1;
        }
        ActualizarPersonaje();
    }

    public void Jugar()
    {
        GameManager.Instance.SeleccionarPersonaje(indiceActual);
        Debug.Log($"🎮 Iniciando juego con personaje: {GameManager.Instance.personajes[indiceActual].nombre}");

        // Cambiar a la escena del juego
        SceneManager.LoadScene("Juego"); // Cambia "Juego" por el nombre de tu escena
    }

    private void ActualizarPersonaje()
    {
        if (GameManager.Instance.personajes.Count == 0) return;

        var personaje = GameManager.Instance.personajes[indiceActual];

        // Actualizar imagen
        if (personajeImagen != null && personaje.imagen != null)
        {
            personajeImagen.sprite = personaje.imagen;
        }

        // Actualizar texto
        if (textoNombre != null)
        {
            textoNombre.text = personaje.nombre;
        }

        Debug.Log($"🔄 Personaje actual: {personaje.nombre} (Índice: {indiceActual})");
    }
}