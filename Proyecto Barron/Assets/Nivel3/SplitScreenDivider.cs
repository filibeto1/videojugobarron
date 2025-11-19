using UnityEngine;
using UnityEngine.UI;

public class SplitScreenDivider : MonoBehaviour
{
    [Header("Divider Settings")]
    public Color dividerColor = Color.white;
    public float dividerThickness = 3f;
    public bool alwaysVisible = true;

    private GameObject dividerLine;
    private Camera player1Camera;
    private Camera player2Camera;

    void Start()
    {
        CreateDividerLine();
        FindCameras();
        UpdateDividerPosition();
    }

    void Update()
    {
        if (alwaysVisible)
        {
            UpdateDividerPosition();
        }
    }

    void CreateDividerLine()
    {
        // Crear objeto para la línea divisoria
        dividerLine = new GameObject("ScreenDivider");
        dividerLine.transform.SetParent(transform);

        // Agregar Image component
        Image image = dividerLine.AddComponent<Image>();
        image.color = dividerColor;

        // Agregar RectTransform
        RectTransform rect = dividerLine.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0.5f);
        rect.anchorMax = new Vector2(1, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(0, dividerThickness);
        rect.anchoredPosition = Vector2.zero;

        // Asegurar que esté en la capa superior
        dividerLine.transform.SetAsLastSibling();

        Debug.Log("📱 Línea divisoria creada");
    }

    void FindCameras()
    {
        Camera[] cameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in cameras)
        {
            if (cam.CompareTag("MainCamera"))
            {
                player1Camera = cam;
            }
            else if (cam.CompareTag("CameraP2"))
            {
                player2Camera = cam;
            }
        }
    }

    void UpdateDividerPosition()
    {
        if (dividerLine == null) return;

        // Mostrar/ocultar basado en si hay dos cámaras activas
        bool shouldShow = player2Camera != null && player2Camera.enabled;

        if (dividerLine.activeSelf != shouldShow)
        {
            dividerLine.SetActive(shouldShow);
        }

        if (!shouldShow) return;

        // Posicionar en el centro exacto
        RectTransform rect = dividerLine.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
    }

    [ContextMenu("Cambiar Color a Rojo")]
    public void SetRedColor()
    {
        SetDividerColor(Color.red);
    }

    [ContextMenu("Cambiar Color a Negro")]
    public void SetBlackColor()
    {
        SetDividerColor(Color.black);
    }

    [ContextMenu("Cambiar Color a Blanco")]
    public void SetWhiteColor()
    {
        SetDividerColor(Color.white);
    }

    public void SetDividerColor(Color newColor)
    {
        if (dividerLine != null)
        {
            Image image = dividerLine.GetComponent<Image>();
            image.color = newColor;
            dividerColor = newColor;
            Debug.Log($"🎨 Color de divisoria cambiado a: {newColor}");
        }
    }

    public void SetDividerThickness(float thickness)
    {
        dividerThickness = thickness;
        if (dividerLine != null)
        {
            RectTransform rect = dividerLine.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, thickness);
            Debug.Log($"📏 Grosor de divisoria cambiado a: {thickness}");
        }
    }

    // Método para cambiar visibilidad
    public void SetVisibility(bool visible)
    {
        alwaysVisible = visible;
        if (dividerLine != null)
        {
            dividerLine.SetActive(visible);
        }
    }

    void OnDestroy()
    {
        Debug.Log("📱 Divider destruido");
    }
}