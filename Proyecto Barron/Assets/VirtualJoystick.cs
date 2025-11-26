using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Joystick Settings")]
    public RectTransform joystickBackground;
    public RectTransform joystickHandle;
    public float handleRange = 50f;
    public float deadZone = 0.1f;

    [Header("Joystick Type")]
    public bool isMovementJoystick = true;

    private Vector2 inputVector;
    private Canvas canvas;
    private Camera mainCamera;

    // ✅ Eventos para notificar cuando hay input
    public System.Action<Vector2> OnJoystickInput;
    public System.Action OnJoystickReleased;

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ VirtualJoystick debe estar dentro de un Canvas!");
        }

        mainCamera = Camera.main;

        if (joystickBackground == null)
        {
            joystickBackground = GetComponent<RectTransform>();
        }

        // Buscar el Handle de forma más robusta
        FindHandle();

        Debug.Log($"✅ VirtualJoystick inicializado: {gameObject.name}");

        // ✅ Reconectar después de inicializar
        StartCoroutine(ReconnectAfterDelay());
    }

    void OnEnable()
    {
        // Reconectar Handle cada vez que se reactiva
        if (joystickHandle == null)
        {
            FindHandle();
        }
    }

    private IEnumerator ReconnectAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        ReconnectToPlayer();
    }

    private void FindHandle()
    {
        if (joystickHandle == null)
        {
            Transform handleTransform = transform.Find("Handle");
            if (handleTransform != null)
            {
                joystickHandle = handleTransform.GetComponent<RectTransform>();
                Debug.Log($"✅ Handle encontrado: {handleTransform.name}");
            }
            else
            {
                // Buscar en todos los hijos
                RectTransform[] children = GetComponentsInChildren<RectTransform>();
                foreach (RectTransform child in children)
                {
                    if (child != joystickBackground && child.name.Contains("Handle"))
                    {
                        joystickHandle = child;
                        Debug.Log($"✅ Handle encontrado en búsqueda profunda: {child.name}");
                        break;
                    }
                }

                if (joystickHandle == null)
                {
                    Debug.LogWarning($"⚠️ No se encontró 'Handle' en {gameObject.name}. Créalo en el Inspector.");
                }
            }
        }
    }

    // ✅ CORREGIDO: Método para reconectar con el jugador
    public void ReconnectToPlayer()
    {
        Debug.Log($"🔄 Reconectando joystick: {gameObject.name}");

        // ✅ BUSCAR POR MULTIPLES MÉTODOS
        GameObject player = FindPlayerByMultipleMethods();

        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.SetVirtualJoystick(this);
                Debug.Log($"✅ Joystick {gameObject.name} conectado a: {player.name}");
            }
            else
            {
                Debug.LogWarning($"⚠️ No se encontró PlayerController en: {player.name}");
                // ✅ REINTENTAR después de un delay
                StartCoroutine(RetryPlayerConnection());
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ No se encontró jugador para conectar joystick");
            // ✅ REINTENTAR después de un delay
            StartCoroutine(RetryPlayerConnection());
        }
    }

    // ✅ NUEVO: Buscar jugador por múltiples métodos
    private GameObject FindPlayerByMultipleMethods()
    {
        // Método 1: Buscar por tag "Player"
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Debug.Log($"🎯 Jugador encontrado por TAG: {player.name}");
            return player;
        }

        // Método 2: Buscar cualquier PlayerController
        PlayerController[] allPlayers = FindObjectsOfType<PlayerController>();
        if (allPlayers.Length > 0)
        {
            Debug.Log($"🎯 Jugador encontrado por COMPONENTE: {allPlayers[0].gameObject.name}");
            return allPlayers[0].gameObject;
        }

        // Método 3: Buscar por nombre que contenga "Player" o "Dog"
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.activeInHierarchy &&
                (obj.name.Contains("Player") || obj.name.Contains("Dog") || obj.name.Contains("Character")))
            {
                PlayerController pc = obj.GetComponent<PlayerController>();
                if (pc != null)
                {
                    Debug.Log($"🎯 Jugador encontrado por NOMBRE: {obj.name}");
                    return obj;
                }
            }
        }

        return null;
    }

    // ✅ NUEVO: Reintentar conexión
    private IEnumerator RetryPlayerConnection()
    {
        yield return new WaitForSeconds(1f);

        GameObject player = FindPlayerByMultipleMethods();
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.SetVirtualJoystick(this);
                Debug.Log($"✅ Joystick RECONECTADO después de retry: {player.name}");
            }
        }
        else
        {
            Debug.LogWarning("🔄 Reintentando conexión en 2 segundos...");
            StartCoroutine(RetryPlayerConnection());
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }
    // ✅ ACTUALIZADO: Método OnDrag para mejor debug
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 position;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackground,
            eventData.position,
            eventData.pressEventCamera,
            out position))
        {
            position.x = (position.x / joystickBackground.sizeDelta.x);
            position.y = (position.y / joystickBackground.sizeDelta.y);

            inputVector = new Vector2(position.x * 2, position.y * 2);

            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

            if (inputVector.magnitude < deadZone)
            {
                inputVector = Vector2.zero;
            }

            if (joystickHandle != null)
            {
                joystickHandle.anchoredPosition = new Vector2(
                    inputVector.x * handleRange,
                    inputVector.y * handleRange
                );
            }

            // ✅ DEBUG MEJORADO: Mostrar input
            if (inputVector.magnitude > 0.1f)
            {
                Debug.Log($"🎮 Joystick {gameObject.name} input: ({inputVector.x:F2}, {inputVector.y:F2})");
            }

            // ✅ Notificar del input
            OnJoystickInput?.Invoke(inputVector);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;

        if (joystickHandle != null)
        {
            joystickHandle.anchoredPosition = Vector2.zero;
        }

        // ✅ Notificar que se soltó el joystick
        OnJoystickReleased?.Invoke();
    }

    // ✅ MÉTODOS PÚBLICOS NECESARIOS
    public float Horizontal()
    {
        return inputVector.x;
    }

    public float Vertical()
    {
        return inputVector.y;
    }

    public Vector2 GetInputVector()
    {
        return inputVector;
    }

    public bool IsJumpPressed()
    {
        return inputVector.y > 0.5f;
    }

    // ✅ Limpiar eventos al destruir
    private void OnDestroy()
    {
        OnJoystickInput = null;
        OnJoystickReleased = null;
    }
}