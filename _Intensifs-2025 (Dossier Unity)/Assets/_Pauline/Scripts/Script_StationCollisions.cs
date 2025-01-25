using System.Collections;
using UnityEngine;

public class StationCollision : MonoBehaviour
{
    public string objectTag = "RailPath"; // Tag auquel cet objet doit r�agir (par d�faut "Tile")
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
    private TrainsSplineController SplineController;
    public GameObject Connection;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.1f); // Attendez un court instant avant d'ex�cuter le code de d�tection des collisions

        // Assurez-vous que le collider est activ� et que les objets sont pr�ts
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
        }
        
        SplineController = FindFirstObjectByType<TrainsSplineController>();
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
            Connection = other.gameObject;
            Debug.Log(this.name + " connected to " + Connection.name);
            StartCoroutine(CreateSpline());
            StopCoroutine(RemoveSpline(other.gameObject.GetComponent<RailCollisions>()));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(objectTag))
        {
            Debug.Log(this.name + " disconnected from " + Connection.name);
            StartCoroutine(RemoveSpline(other.gameObject.GetComponent<RailCollisions>()));
            StopCoroutine(CreateSpline());
            Connection = null;
        }   
    }

    IEnumerator CreateSpline()
    {
        yield return new WaitForSeconds(0.5f);
        
        SplineController.TryCreateSpline(this);
    }
    
    IEnumerator RemoveSpline(RailCollisions rail)
    {
        yield return new WaitForSeconds(0.5f);
        
        SplineController.TryRemoveSpline(rail);
    }
}
