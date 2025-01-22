using System.Collections;
using UnityEngine;

public class StationCollision : MonoBehaviour
{
    public bool isTrigger = false;
    public GameObject isTriggeringWith;
    public string objectTag = "Rail"; // Tag auquel cet objet doit réagir (par défaut "Tile")
    public enum Direction
    {
        Left,
        TopLeft,
        TopRight,
        Right,
        BottomRight,
        BottomLeft
    }
    private List<GameObject> Path;

    public Direction CurrentSide;

    private TrainStation Station;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.1f); // Attendez un court instant avant d'exécuter le code de détection des collisions

        // Assurez-vous que le collider est activé et que les objets sont prêts
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
        }
    }

    private void Awake()
    {
        // Récupère la référence à la tuile (le parent du collider)
        Station = GetComponentInParent<TrainStation>();
        if (Station == null)
        {
            Debug.LogError("Aucune référence à la tuile trouvée ! Le collider n'est peut-être pas attaché au bon objet.");
        }

        // Assurez-vous que le collider est un trigger
        Collider collider = GetComponent<Collider>();
        if (collider != null && !collider.isTrigger)
        {
            collider.isTrigger = true; // S'assure que le collider est un trigger
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(objectTag))
        {
            Path.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {

    }
}
