using System.Collections;
using UnityEngine;

public class TileCollision : MonoBehaviour
{
    public bool isTrigger = false;
    public GameObject isTriggeringWith;
    public string objectTag = "TileTrigger"; // Tag auquel cet objet doit réagir (par défaut "Tile")
    
    private Tile tile;

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
        tile = GetComponentInParent<Tile>();
        if (tile == null) 
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
        Tile neighborTile = other.GetComponentInParent<Tile>();

        if (neighborTile != null && neighborTile != tile)
        {
            if (other.CompareTag(objectTag))
            {
                // Ajout du voisin dans la liste du parent `Tile`
                tile.AddNeighbor(neighborTile); // Méthode ajoutée dans Tile pour gérer les voisins
            }

            isTrigger = true;
            isTriggeringWith = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Tile neighborTile = other.GetComponentInParent<Tile>();

        if (neighborTile != null && neighborTile != tile)
        {
            if (other.CompareTag(objectTag))
            {
                // Retirer le voisin de la liste du parent `Tile`
                tile.RemoveNeighbor(neighborTile); // Méthode ajoutée dans Tile pour supprimer les voisins
            }

            isTrigger = false;
            isTriggeringWith = null;
        }
    }
}
