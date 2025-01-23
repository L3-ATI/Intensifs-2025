using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrainStation : MonoBehaviour
{
    private Tile parentTile;
    private HashSet<GameObject> connectedRails = new HashSet<GameObject>(); // Utilisation d'un HashSet pour éviter les doublons
    public List<List<GameObject>> RailPath;  
    private void Start()
    {
        parentTile = GetComponentInParent<Tile>();
    }


}
