using UnityEngine;

public class MovimientoTemp : MonoBehaviour
{
    public float velocidad = 5f;

    void Update()
    {
        float moverHorizontal = Input.GetAxis("Horizontal");
        float moverVertical = Input.GetAxis("Vertical");

        Vector3 movimiento = new Vector3(moverHorizontal, moverVertical, 0) * velocidad * Time.deltaTime;
        transform.Translate(movimiento);
    }
}