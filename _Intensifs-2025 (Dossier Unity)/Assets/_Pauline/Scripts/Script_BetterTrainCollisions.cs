using UnityEngine;

public class Script_BetterTrainCollisions : MonoBehaviour
{
    public GameObject WagonMesh; // Référence au GameObject contenant le mesh
    private Vector3 originalScale; // Stocke l'échelle originale de WagonMesh
    private bool isColliding = false; // Vérifie si le wagon est en collision avec un autre train

    void Start()
    {
        if (WagonMesh != null)
        {
            originalScale = WagonMesh.transform.localScale; // Sauvegarde l'échelle de base de WagonMesh
        }
        else
        {
            Debug.LogError("WagonMesh n'est pas assigné dans l'inspecteur !");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Train") && !isColliding && WagonMesh != null)
        {
            isColliding = true; // Indique qu'une collision est active
            WagonMesh.transform.localScale = originalScale * 0.02f; // Réduit l'échelle de WagonMesh
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Train") && WagonMesh != null)
        {
            isColliding = false; // Indique que la collision est terminée
            WagonMesh.transform.localScale = originalScale; // Rétablit immédiatement l’échelle normale
        }
    }
}
