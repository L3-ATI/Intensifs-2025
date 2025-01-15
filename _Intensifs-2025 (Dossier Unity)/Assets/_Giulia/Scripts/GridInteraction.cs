using UnityEngine;

public class GridInteraction : MonoBehaviour
{
    public GridManager gridManager;
    public GameObject stationPrefab; // Drag & Drop du prefab dans l'inspecteur

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Tile tile = hit.collider.GetComponent<Tile>();

                if (tile != null && !tile.isOccupied)
                {
                    // Vérifier les règles de placement ici si nécessaire
                    if (tile.CanPlaceObject("Station"))
                    {
                        PlaceObject(tile, stationPrefab);
                    }
                    else
                    {
                        Debug.LogWarning("Placement non valide sur cette tuile !");
                    }
                }
            }
        }
    }

    void PlaceObject(Tile tile, GameObject prefabToPlace)
    {
        // Instancier l'objet
        GameObject structure = Instantiate(prefabToPlace, tile.transform.position, Quaternion.identity);

        // Marquer la tuile comme occupée
        tile.SetOccupied(true);

        Debug.Log($"Objet placé sur la tuile : {tile.name}");
    }
}