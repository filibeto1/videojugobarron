using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiguelController : MonoBehaviour
{
    public float velocidad = 7f; // Velocidad de movimiento horizontal
    public float saltoVelocidad = 7f; // Velocidad de salto
    public Animator animator; // Controlador de animaciones

    private Rigidbody2D rb;
    private Vector2 movimiento;
    private bool enElSuelo = true; // Indica si el personaje está en el suelo
    private bool saltar = false;

    [SerializeField] private Transform comprobadorSuelo; // Punto para verificar si está en el suelo
    [SerializeField] private LayerMask capaSuelo; // Capas consideradas como suelo

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (animator == null)
        {
            Debug.LogWarning("Animator no asignado. Asegúrate de arrastrar el componente Animator al script en el Inspector.");
        }
    }

    void Update()
    {
        // Entrada horizontal para el movimiento
        movimiento.x = Input.GetAxis("Horizontal");

        // Actualizar animaciones
        if (animator != null)
        {
            animator.SetFloat("Velocidad", Mathf.Abs(movimiento.x));
            animator.SetBool("EnElSuelo", enElSuelo);
        }

        // Detectar si el personaje debe saltar
        if (enElSuelo && Input.GetButtonDown("Jump"))
        {
            saltar = true;
        }

        // Voltear al personaje según la dirección
        if (movimiento.x > 0.01f)
            transform.localScale = new Vector3(1, 1, 1); // Hacia la derecha
        else if (movimiento.x < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1); // Hacia la izquierda
    }

    void FixedUpdate()
    {
        // Verificar si está en el suelo
        enElSuelo = Physics2D.OverlapCircle(comprobadorSuelo.position, 0.1f, capaSuelo);

        // Aplicar movimiento horizontal
        rb.velocity = new Vector2(movimiento.x * velocidad, rb.velocity.y);

        // Aplicar salto
        if (saltar)
        {
            rb.velocity = new Vector2(rb.velocity.x, saltoVelocidad);
            saltar = false;
        }
    }
}
