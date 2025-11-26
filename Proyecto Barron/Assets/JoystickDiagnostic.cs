using UnityEngine;

public class JoystickDiagnostic : MonoBehaviour
{
    void Start()
    {
        Debug.Log("🔍 DIAGNÓSTICO DE JOYSTICK PACK");

        // Buscar todos los componentes en el Floating Joystick
        GameObject joystickObj = GameObject.Find("Floating Joystick");
        if (joystickObj != null)
        {
            Debug.Log($"✅ Encontrado GameObject: {joystickObj.name}");

            Component[] allComponents = joystickObj.GetComponents<Component>();
            foreach (Component comp in allComponents)
            {
                Debug.Log($"📋 Componente: {comp.GetType().FullName}");
            }
        }
        else
        {
            Debug.LogError("❌ No se encontró 'Floating Joystick' en la escena");
        }
    }
}