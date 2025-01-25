using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class RailCollisions : MonoBehaviour
{
    public List<string> objectTag = new List<string>(){"RailPath", "Station"}; // Tag auquel cet objet doit r�agir (par d�faut "Tile")
    
    private RailPath Rail;
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
        Rail = GetComponentInParent<RailPath>();
        if (Rail == null)
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
        if (objectTag.Contains(other.tag))
        {
            Connection = other.gameObject;
            Debug.Log(this.name + " connected to " + Connection.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (objectTag.Contains(other.tag))
        {
            Debug.Log(this.name + " disconnected from " + Connection.name);
            StartCoroutine(RemoveSpline());
            Connection = null;
        }   
    }
    
    IEnumerator RemoveSpline()
    {
        yield return new WaitForSeconds(0.1f);
        
        SplineController.TryRemoveSpline(this);
    }
}
