using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MenuOpciones : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    // Método para activar o desactivar la pantalla completa
    public void PantallaCompleta(bool pantallaCompleta)
    {
        Screen.fullScreen = pantallaCompleta;
        Debug.Log("Pantalla completa: " + pantallaCompleta);
    }

    // Método para cambiar el volumen
    public void CambiarVolumen(float volumen)
    {
        audioMixer.SetFloat("Volumen", volumen);
        Debug.Log("Volumen ajustado a: " + volumen);
    }
    public void CambiarCalidad(int index)
    {
        QualitySettings.SetQualityLevel(index);
    }
}
