using UnityEngine;
using UnityEngine.Splines;
using System.Collections; // Pour utiliser les coroutines
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public class TrainsSplineController : MonoBehaviour
{

    public GameObject Locomotive;
    public GameObject Wagon;

    public static SplineContainer Path;

    private List<MoveAlongSpline> trains;
    
    void Start()
    {
        if (!Application.isPlaying) return;
        
        // Assure que le SplineContainer est pr�sent
        Path = GetComponent<SplineContainer>() ?? gameObject.AddComponent<SplineContainer>();
        Path.Spline.Clear();
        
        trains = new List<MoveAlongSpline>();

        // D�marrer la coroutine pour ajouter un autre wagon apr�s 0.1 seconde
        //StartCoroutine(InstantiateWagonAfterDelay(1f));
    }

    void AddAutoSmoothSplinePoint(Spline spline, Vector3 position)
    {
        var knot = new BezierKnot(position);
        spline.Add(knot);

        // Set les points de la spline en "Auto" cela les rend plus smooth
        spline.SetTangentMode(spline.Count - 1, TangentMode.AutoSmooth);
    }

    public void TryCreateSpline(StationCollision station)
    {
        Spline currentSpline = new Spline();
        
        bool isPathComplete = false;
        GameObject path = station.Connection;
        while (path)
        {
            Debug.Log("PATH: " + path);
            RailPath rail = path.GetComponentInParent<RailPath>();

            if (!rail)
            {
                isPathComplete = true;
                break;
            }
            
            foreach (var pathConnector in rail.RailPathPoints)
            {
                if (pathConnector.points[0] == path)
                {
                    foreach (var point in pathConnector.points)
                        AddAutoSmoothSplinePoint(currentSpline, point.transform.position);
                    path = pathConnector.points[pathConnector.points.Count - 1].GetComponent<RailCollisions>().Connection;
                    break;
                }
                
                if (pathConnector.points[pathConnector.points.Count - 1] == path)
                {
                    for (int i = pathConnector.points.Count - 1; i >= 0; i--)
                        AddAutoSmoothSplinePoint(currentSpline, pathConnector.points[i].transform.position);
                    path = pathConnector.points[0].GetComponent<RailCollisions>().Connection;
                    break;
                }
            }
        }

        if (!isPathComplete) return;
        
        Debug.Log("Path complete: " + currentSpline.Count);

        foreach (var spline in Path.Splines)
            if (DoesSplineContains(spline, station.Connection.GetComponent<RailCollisions>()))
                return;
        
        Path.AddSpline(currentSpline);

        if (Path.Spline.Count-1 == trains.Count) return;
        
        MoveAlongSpline train = Instantiate(Locomotive).GetComponent<MoveAlongSpline>();
        train.SetSpline(currentSpline);
        trains.Add(train);
    }
    
    public void TryRemoveSpline(RailCollisions rail)
    {
        List<Spline> splineToRemove = new List<Spline>();
        List<MoveAlongSpline> trainToRemove = new List<MoveAlongSpline>();

        for (int i = 1; i < Path.Splines.Count; i++)
        {
            if (DoesSplineContains(Path.Splines[i], rail))
            {
                splineToRemove.Add(Path.Splines[i]);
                trainToRemove.Add(trains[i-1]);
            }
        }

        for (int i = 0; i < splineToRemove.Count; i++)
        {
            Path.RemoveSpline(splineToRemove[i]);
            trains.Remove(trainToRemove[i]);
        }
        
        Debug.Log("Spline to remove count: " + splineToRemove.Count);
    }

    private bool DoesSplineContains(Spline spline, RailCollisions rail)
    {
        foreach (var knot in spline.Knots)
        {
            Vector3 knotPos = new Vector3(knot.Position.x, knot.Position.y, knot.Position.z);
            if ((rail.transform.position - knotPos).magnitude < 0.1f)
                return true;
        }

        return false;
    }

    // Coroutine pour instancier un wagon apr�s un d�lai
    IEnumerator InstantiateWagonAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // Attendre pendant le d�lai

        // Instancier un autre wagon
        Instantiate(Wagon);
    }
}
