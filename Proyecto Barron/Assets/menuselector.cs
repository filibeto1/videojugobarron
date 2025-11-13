using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // Asegúrate de incluir esto para usar SceneManager

public class MenuSelector : MonoBehaviour
{
    private int index;

    [SerializeField] private Image imagen;
    [SerializeField] private TextMeshProUGUI nombre;

    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.Instance;

        // Corregido: PlayerPrefs.GetInt (no "GetInit") y agregado un valor por defecto.
        index = PlayerPrefs.GetInt("JugadorIndex", 0);

        if (index > gameManager.personajes.Count - 1)
        {
            index = 0; // Asegura que el índice sea válido
        }

        // Inicializa la pantalla al comenzar.
        CambiarPantalla();
    }

    private void CambiarPantalla()
    {
        // Asegúrate de que el índice esté dentro del rango.
        if (index >= 0 && index < gameManager.personajes.Count)
        {
            PlayerPrefs.SetInt("JugadorIndex", index);
            imagen.sprite = gameManager.personajes[index].imagen;
            nombre.text = gameManager.personajes[index].nombre;
        }
        else
        {
            Debug.LogWarning("Índice fuera de rango: " + index);
        }
    }

    public void SiguientePersonaje()
    {
        if (index == gameManager.personajes.Count - 1)
        {
            index = 0; // Volver al primer personaje
        }
        else
        {
            index += 1; // Avanzar al siguiente personaje
        }
        CambiarPantalla();
    }

    public void AnteriorPersonaje()
    {
        if (index == 0)
        {
            index = gameManager.personajes.Count - 1; // Volver al último personaje
        }
        else
        {
            index -= 1; // Retroceder al personaje anterior
        }
        CambiarPantalla();
    }

    public void IniciarJuego()
    {
        // Corregido: Cambié "SceneMananger" por "SceneManager" y corregí el método para cargar la siguiente escena.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}

