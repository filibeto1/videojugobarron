using UnityEngine;

public class QuestionPowerUp : MonoBehaviour
{
    public GameObject questionManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("¡Comodín encontrado!");

            if (questionManager != null)
            {
                questionManager.GetComponent<QuestionManager>().ShowQuestion();
            }

            gameObject.SetActive(false);
            Invoke("ReactivatePowerUp", 15f);
        }
    }

    void ReactivatePowerUp()
    {
        gameObject.SetActive(true);
    }
}