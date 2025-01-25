using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class TrainStation : MonoBehaviour
{
    private Tile parentTile;
    private HashSet<GameObject> connectedRails = new HashSet<GameObject>(); // Utilisation d'un HashSet pour �viter les doublons
    public List<GameObject> RailTrack;  
    
    private void Start()
    {
        parentTile = GetComponentInParent<Tile>();
        for (int i = 0; i < 6; i++)
        {
            RailTrack.Add(null);
        }
    }
}
