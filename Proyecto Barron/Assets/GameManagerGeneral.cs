using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerGeneral : MonoBehaviour
{
    public static GameManagerGeneral Instance;

    [System.Serializable]
    public class DatosPersonaje
    {
        public string nombre;
        public GameObject prefabJugador;
        public Sprite imagen;
    }

    public List<DatosPersonaje> listaPersonajes = new List<DatosPersonaje>();

    [Header("Prefabs de jugadores (asignar manualmente)")]
    public GameObject[] prefabsJugadores;

    private void Awake()
    {
        Debug.Log("🔄 GameManagerGeneral Awake iniciado");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ GameManagerGeneral creado como Singleton");

            // INICIALIZAR INMEDIATAMENTE
            InicializarPersonajesForzado();
        }
        else
        {
            Debug.Log("⚠️ GameManagerGeneral duplicado - destruyendo");
            Destroy(gameObject);
        }
    }

    void InicializarPersonajesForzado()
    {
        Debug.Log("🎯 INICIALIZANDO PERSONAJES FORZADAMENTE");

        // LIMPIAR LISTA
        listaPersonajes.Clear();

        // MÉTODO 1: Usar prefabs asignados manualmente
        if (prefabsJugadores != null && prefabsJugadores.Length > 0)
        {
            Debug.Log($"📦 Usando {prefabsJugadores.Length} prefabs asignados manualmente");
            foreach (GameObject prefab in prefabsJugadores)
            {
                if (prefab != null)
                {
                    AgregarPersonajeALista(prefab);
                }
            }
        }

        // MÉTODO 2: Buscar en Resources como respaldo
        if (listaPersonajes.Count == 0)
        {
            Debug.Log("🔍 Buscando en Resources...");
            BuscarEnResources();
        }

        // MÉTODO 3: Crear de emergencia si no hay nada
        if (listaPersonajes.Count == 0)
        {
            Debug.Log("🛠️ Creando jugador de emergencia");
            CrearJugadorEmergencia();
        }

        Debug.Log($"🎉 INICIALIZACIÓN COMPLETADA: {listaPersonajes.Count} personajes cargados");

        // Inicializar PlayerPrefs
        if (!PlayerPrefs.HasKey("JugadorIndex"))
        {
            PlayerPrefs.SetInt("JugadorIndex", 0);
            Debug.Log("📝 PlayerPrefs inicializado con índice 0");
        }
    }

    void BuscarEnResources()
    {
        // Buscar TODOS los objetos en Resources
        object[] loadedObjects = Resources.LoadAll("");

        foreach (object obj in loadedObjects)
        {
            if (obj is GameObject)
            {
                GameObject gameObj = (GameObject)obj;

                // Verificar si es un jugador por tag o nombre
                if (gameObj.CompareTag("Player") ||
                    gameObj.name.ToLower().Contains("player") ||
                    gameObj.name.ToLower().Contains("jugador") ||
                    gameObj.name.ToLower().Contains("personaje"))
                {
                    AgregarPersonajeALista(gameObj);
                }
            }
        }
    }

    void AgregarPersonajeALista(GameObject prefab)
    {
        if (prefab == null) return;

        DatosPersonaje nuevoPersonaje = new DatosPersonaje();
        nuevoPersonaje.nombre = prefab.name;
        nuevoPersonaje.prefabJugador = prefab;

        // Obtener sprite del prefab
        SpriteRenderer spriteRenderer = prefab.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            nuevoPersonaje.imagen = spriteRenderer.sprite;
            Debug.Log($"✅ Personaje agregado: {prefab.name} (con sprite)");
        }
        else
        {
            nuevoPersonaje.imagen = null;
            Debug.Log($"✅ Personaje agregado: {prefab.name} (sin sprite)");
        }

        listaPersonajes.Add(nuevoPersonaje);
    }

    void CrearJugadorEmergencia()
    {
        Debug.Log("🚨 CREANDO JUGADOR DE EMERGENCIA");

        GameObject jugadorEmergencia = new GameObject("JugadorEmergencia");
        jugadorEmergencia.tag = "Player";

        // Añadir SpriteRenderer con color
        SpriteRenderer sr = jugadorEmergencia.AddComponent<SpriteRenderer>();
        sr.color = Color.cyan;

        // Crear sprite cuadrado básico
        Texture2D texture = new Texture2D(64, 64);
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                texture.SetPixel(x, y, Color.cyan);
            }
        }
        texture.Apply();
        sr.sprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));

        // Añadir componentes básicos
        Rigidbody2D rb = jugadorEmergencia.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        jugadorEmergencia.AddComponent<BoxCollider2D>();

        // Agregar a la lista
        DatosPersonaje datos = new DatosPersonaje();
        datos.nombre = "JugadorEmergencia";
        datos.prefabJugador = jugadorEmergencia;
        datos.imagen = sr.sprite;

        listaPersonajes.Add(datos);

        Debug.Log("✅ Jugador de emergencia creado");
    }

    public GameObject CrearJugador(int indice, Vector3 posicion)
    {
        Debug.Log($"🎮 Intentando crear jugador índice {indice} en {posicion}");

        if (listaPersonajes == null || listaPersonajes.Count == 0)
        {
            Debug.LogError("❌ No hay personajes en la lista");
            return CrearJugadorInstantaneo(posicion);
        }

        // Asegurar que el índice sea válido
        if (indice < 0 || indice >= listaPersonajes.Count)
        {
            Debug.LogWarning($"⚠️ Índice {indice} inválido. Usando 0");
            indice = 0;
        }

        DatosPersonaje personaje = listaPersonajes[indice];

        if (personaje.prefabJugador != null)
        {
            GameObject jugadorInstanciado = Instantiate(personaje.prefabJugador, posicion, Quaternion.identity);
            jugadorInstanciado.name = "Jugador_" + personaje.nombre;
            jugadorInstanciado.tag = "Player";

            Debug.Log($"✅ Jugador creado exitosamente: {personaje.nombre} en posición {posicion}");
            return jugadorInstanciado;
        }
        else
        {
            Debug.LogError($"❌ Prefab del personaje {personaje.nombre} es nulo");
            return CrearJugadorInstantaneo(posicion);
        }
    }

    private GameObject CrearJugadorInstantaneo(Vector3 posicion)
    {
        Debug.Log("🔄 Creando jugador instantáneo");

        GameObject jugador = new GameObject("JugadorInstantaneo");
        jugador.transform.position = posicion;
        jugador.tag = "Player";

        // Componentes mínimos
        SpriteRenderer sr = jugador.AddComponent<SpriteRenderer>();
        sr.color = Color.magenta;

        Rigidbody2D rb = jugador.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        jugador.AddComponent<BoxCollider2D>();

        return jugador;
    }

    public void SeleccionarPersonaje(int indice)
    {
        if (indice >= 0 && indice < listaPersonajes.Count)
        {
            PlayerPrefs.SetInt("JugadorIndex", indice);
            PlayerPrefs.Save();
            Debug.Log($"✅ Personaje seleccionado: {listaPersonajes[indice].nombre} (índice: {indice})");
        }
        else
        {
            Debug.LogError($"❌ Índice de personaje inválido: {indice}");
        }
    }

    // Método para debug
    public void DebugInfo()
    {
        Debug.Log($"🔍 DEBUG GameManagerGeneral:");
        Debug.Log($"- Personajes en lista: {listaPersonajes.Count}");
        for (int i = 0; i < listaPersonajes.Count; i++)
        {
            Debug.Log($"  {i}: {listaPersonajes[i].nombre} (prefab: {listaPersonajes[i].prefabJugador != null})");
        }
        Debug.Log($"- PlayerPrefs JugadorIndex: {PlayerPrefs.GetInt("JugadorIndex", -1)}");
    }
}