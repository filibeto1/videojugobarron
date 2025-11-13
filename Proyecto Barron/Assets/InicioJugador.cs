using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InicioJugador : MonoBehaviour
{
    private void Start()
    {
        int indexJugador = PlayerPrefs.GetInt("JugadorIndex");
        GameObject jugador = Instantiate(GameManager.Instance.personajes[indexJugador].personajeJugable, transform.position, Quaternion.identity);

        // Asegúrate de que la cámara siga al personaje instanciado.
        Camera.main.GetComponent<SeguirJugador>().SetObjetivo(jugador.transform);
    }
}
