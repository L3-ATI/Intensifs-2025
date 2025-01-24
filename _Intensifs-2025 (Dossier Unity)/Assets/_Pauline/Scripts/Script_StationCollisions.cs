using System.Collections;
using UnityEngine;

public class StationCollision : MonoBehaviour
{
    public bool isTrigger = false;
    public GameObject isTriggeringWith;
    public string objectTag = "Rail"; // Tag auquel cet objet doit r�agir (par d�faut "Tile")
    public enum Direction
    {
        Left,
        TopLeft,
        TopRight,
        Right,
        BottomRight,
        BottomLeft
    }

    public Direction CurrentSide;

    private TrainStation Station;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.1f); // Attendez un court instant avant d'ex�cuter le code de d�tection des collisions

        // Assurez-vous que le collider est activ� et que les objets sont pr�ts
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
        }
    }

    private void Awake()
    {
        // R�cup�re la r�f�rence � la tuile (le parent du collider)
        Station = GetComponentInParent<TrainStation>();
        if (Station == null)
        {
            Debug.LogError("Aucune r�f�rence � la tuile trouv�e ! Le collider n'est peut-�tre pas attach� au bon objet.");
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
            Station.RailTrack[(int)CurrentSide] = other.gameObject;
            RailPath path = other.gameObject.GetComponent<RailPath>();
            path.RailPathPrevious[((int)CurrentSide + 3) % 6] = this.gameObject;
            Debug.Log("Added " + CurrentSide.ToString() + " Side, Name: " + Station.RailTrack[(int)CurrentSide].name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(objectTag))
        {
            Debug.Log("Removed " + CurrentSide.ToString() + " Side, Name: " + Station.RailTrack[(int)CurrentSide].name);
            Station.RailTrack[(int)CurrentSide] = null;
            RailPath path = other.gameObject.GetComponent<RailPath>();
            path.RailPathPrevious[((int)CurrentSide + 3) % 6] = null;
        }   
    }
}
