using UnityEngine;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour
{
    [Header("Timer References")]
    public Text timerText;
    public Text objectiveText;

    [Header("Timer Settings")]
    public float cambioTiempoMin = 20f;
    public float cambioTiempoMax = 30f;

    private float tiempoParaCambio;
    private bool objetivoEsPar = true;

    void Start()
    {
        IniciarTemporizadorCambio();
        ActualizarObjetivoUI();
    }

    void Update()
    {
        ActualizarTemporizadorCambio();
    }

    void IniciarTemporizadorCambio()
    {
        tiempoParaCambio = Random.Range(cambioTiempoMin, cambioTiempoMax);
    }

    void ActualizarTemporizadorCambio()
    {
        if (tiempoParaCambio > 0)
        {
            tiempoParaCambio -= Time.deltaTime;

            if (timerText != null)
            {
                timerText.text = "Cambio en: " + Mathf.Ceil(tiempoParaCambio).ToString() + "s";
            }
        }
        else
        {
            CambiarObjetivo();
            IniciarTemporizadorCambio();
        }
    }

    void CambiarObjetivo()
    {
        objetivoEsPar = !objetivoEsPar;
        ActualizarObjetivoUI();
    }

    void ActualizarObjetivoUI()
    {
        if (objectiveText != null)
        {
            if (objetivoEsPar)
            {
                objectiveText.text = "OBJETIVO: Recoger NÚMEROS PARES";
                objectiveText.color = Color.blue;
            }
            else
            {
                objectiveText.text = "OBJETIVO: Recoger NÚMEROS IMPARES";
                objectiveText.color = Color.red;
            }
        }
    }

    public bool EsNumeroCorrecto(int numero)
    {
        if (objetivoEsPar)
        {
            return numero % 2 == 0;
        }
        else
        {
            return numero % 2 != 0;
        }
    }
}