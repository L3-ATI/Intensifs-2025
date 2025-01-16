using UnityEngine;

public class Script_BetterTrainCollisions : MonoBehaviour
{
    public GameObject WagonMesh; // R�f�rence au GameObject contenant le mesh
    private Vector3 originalScale; // Stocke l'�chelle originale de WagonMesh
    private bool isColliding = false; // V�rifie si le wagon est en collision avec un autre train

    void Start()
    {
        if (WagonMesh != null)
        {
            originalScale = WagonMesh.transform.localScale; // Sauvegarde l'�chelle de base de WagonMesh
        }
        else
        {
            Debug.LogError("WagonMesh n'est pas assign� dans l'inspecteur !");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Train") && !isColliding && WagonMesh != null)
        {
            isColliding = true; // Indique qu'une collision est active
            WagonMesh.transform.localScale = originalScale * 0.02f; // R�duit l'�chelle de WagonMesh
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Train") && WagonMesh != null)
        {
            isColliding = false; // Indique que la collision est termin�e
            WagonMesh.transform.localScale = originalScale; // R�tablit imm�diatement l��chelle normale
        }
    }
}
