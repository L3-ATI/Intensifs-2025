using UnityEngine;

public class GridInteraction : MonoBehaviour
{
    public static GridInteraction Instance;  // Singleton pour accéder à cette classe partout
    public string objectTypeToPlace;  // Détermine le type d'objet à placer (par exemple, "Station")
    public GameObject stationPrefab;  // Prefab à placer pour la station
    public GameObject railStraightPrefab;  // Prefab à placer pour le rail droit
    public GameObject railCurvedPrefab;  // Prefab à placer pour le rail courbé

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);  // Assure qu'il n'y a qu'une seule instance
    }

    void Update()
    {
        // Choisir un type d'objet à placer (par exemple "Station")
        if (Input.GetKeyDown(KeyCode.S))  // Exemple de changement d'objet à placer
        {
            objectTypeToPlace = "Station";  // Change ici pour un autre type (comme "Tunnel", "Bridge", etc.)
        }

        // Si l'utilisateur clique sur une tuile valide, on place l'objet
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Tile tile = hit.collider.GetComponent<Tile>();
                if (tile != null && !tile.isOccupied)
                {
                    // Vérifier les règles de placement ici si nécessaire
                    if (tile.CanPlaceObject(objectTypeToPlace))
                    {
                        if (objectTypeToPlace == "Station")
                        {
                            PlaceObject(tile, stationPrefab);
                        }
                        else if (objectTypeToPlace == "RailStraight" || objectTypeToPlace == "RailCurved")
                        {
                            GameObject prefab = objectTypeToPlace == "RailStraight" ? railStraightPrefab : railCurvedPrefab;
                            PlaceObject(tile, prefab);
                        }
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
        if (objectTypeToPlace == "RailStraight" || objectTypeToPlace == "RailCurved")
        {
            // Vérifier si le rail peut être placé
            if (!tile.CanPlaceRail(tile, GridManager.Instance))
            {
                Debug.LogWarning("Rail non connecté à une station !");
                return;
            }
        }

        // Instancier l'objet (la station ou le rail) à la position de la tuile
        GameObject structure = Instantiate(prefabToPlace, tile.transform.position, Quaternion.identity);
        structure.transform.SetParent(tile.transform);
        tile.SetOccupied(true);
        Debug.Log($"Objet placé sur la tuile : {tile.name}");
    }
}
