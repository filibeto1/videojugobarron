using UnityEngine;

public class Numeros : MonoBehaviour
{
    public int valorNumero;

    void Start()
    {
        // ✅ Asegurar tag
        if (!gameObject.CompareTag("Numero"))
        {
            gameObject.tag = "Numero";
        }

        // ✅ SIEMPRE re-verificar el valor
        VerificarYAsignarValor();
    }

    void VerificarYAsignarValor()
    {
        // Si el valor es 0, intentar asignar desde el nombre
        if (valorNumero == 0)
        {
            string nombre = gameObject.name;

            if (nombre.Contains("(1)")) valorNumero = 1;
            else if (nombre.Contains("(2)")) valorNumero = 2;
            else if (nombre.Contains("(3)")) valorNumero = 3;
            else if (nombre.Contains("(4)")) valorNumero = 4;
            else if (nombre.Contains("(5)")) valorNumero = 5;
            else if (nombre.Contains("(6)")) valorNumero = 6;
            else if (nombre.Contains("(7)")) valorNumero = 7;
            else if (nombre.Contains("(8)")) valorNumero = 8;
            else if (nombre.Contains("(9)")) valorNumero = 9;
            else
            {
                // Valor por defecto basado en el primer carácter
                if (nombre.StartsWith("1")) valorNumero = 1;
                else if (nombre.StartsWith("2")) valorNumero = 2;
                else if (nombre.StartsWith("3")) valorNumero = 3;
                else if (nombre.StartsWith("4")) valorNumero = 4;
                else if (nombre.StartsWith("5")) valorNumero = 5;
                else if (nombre.StartsWith("6")) valorNumero = 6;
                else if (nombre.StartsWith("7")) valorNumero = 7;
                else if (nombre.StartsWith("8")) valorNumero = 8;
                else if (nombre.StartsWith("9")) valorNumero = 9;
            }
        }

        Debug.Log($"🔢 Número {gameObject.name} → Valor: {valorNumero}");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"🎯 Número {valorNumero} recolectado");

            if (GameController.Instance != null)
            {
                GameController.Instance.ProcesarNumero(valorNumero, gameObject);
            }
            else
            {
                Debug.LogError("❌ GameController.Instance es null");
                Destroy(gameObject);
            }
        }
    }
}