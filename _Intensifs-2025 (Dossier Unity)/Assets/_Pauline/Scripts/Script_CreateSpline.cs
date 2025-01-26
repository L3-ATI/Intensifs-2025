using UnityEngine;
using UnityEngine.Splines;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public class TrainsSplineController : MonoBehaviour
{
    public GameObject Locomotive;
    public GameObject Wpassager;
    public GameObject Wressource;

    public static SplineContainer Path;

    private List<MoveAlongSpline> trains;
    private bool isInstantiatingPassager = false;
    private bool isInstantiatingRessource = false;
    
    void Start()
    {
        if (!Application.isPlaying) return;
        
        Path = GetComponent<SplineContainer>() ?? gameObject.AddComponent<SplineContainer>();
        Path.Spline.Clear();
        
        trains = new List<MoveAlongSpline>();
    }

    void AddAutoSmoothSplinePoint(Spline spline, Vector3 position)
    {
        var knot = new BezierKnot(position);
        spline.Add(knot);
        spline.SetTangentMode(spline.Count - 1, TangentMode.AutoSmooth);
    }

    public void TryCreateSpline(StationCollision station)
    {
        Spline currentSpline = new Spline();
        bool isPathComplete = false;
        GameObject path = station.Connection;

        while (path)
        {
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
                    path = pathConnector.points[^1].GetComponent<RailCollisions>().Connection;
                    break;
                }
                
                if (pathConnector.points[^1] == path)
                {
                    for (int i = pathConnector.points.Count - 1; i >= 0; i--)
                        AddAutoSmoothSplinePoint(currentSpline, pathConnector.points[i].transform.position);
                    path = pathConnector.points[0].GetComponent<RailCollisions>().Connection;
                    break;
                }
            }
        }

        if (!isPathComplete) return;
        
        foreach (var spline in Path.Splines)
            if (DoesSplineContains(spline, station.Connection.GetComponent<RailCollisions>()))
                return;
        
        Path.AddSpline(currentSpline);
        
        if (Path.Splines.Count - 1 == trains.Count) return;

        MoveAlongSpline train = Instantiate(Locomotive).GetComponent<MoveAlongSpline>();
        train.SetSpline(currentSpline);
        trains.Add(train);

        if (currentSpline.GetLength() > 20)
            StartCoroutine(InstantiateWagonAfterDelay(0.78f, true, currentSpline));
        
        if (currentSpline.GetLength() > 40)
            StartCoroutine(InstantiateWagonAfterDelay(1.3f, false, currentSpline));
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
                trainToRemove.Add(trains[i - 1]);
            }
        }

        for (int i = 0; i < splineToRemove.Count; i++)
        {
            foreach (var wagon in FindObjectsByType<MoveAlongSpline>(FindObjectsSortMode.None))
            {
                if (wagon.GetSpline().Equals(splineToRemove[i]))
                    Destroy(wagon.gameObject);
            }
            Path.RemoveSpline(splineToRemove[i]);
            Destroy(trainToRemove[i].gameObject);
            trains.Remove(trainToRemove[i]);
        }
    }

    private bool DoesSplineContains(Spline spline, RailCollisions rail)
    {
        foreach (var knot in spline.Knots)
        {
            if ((rail.transform.position - (Vector3)knot.Position).magnitude < 0.1f)
                return true;
        }
        return false;
    }

    IEnumerator InstantiateWagonAfterDelay(float delay, bool passager, Spline spline)
    {
        if ((passager && isInstantiatingPassager) || (!passager && isInstantiatingRessource))
            yield break;

        if (passager) isInstantiatingPassager = true;
        else isInstantiatingRessource = true;

        yield return new WaitForSeconds(delay);

        MoveAlongSpline wagon = Instantiate(passager ? Wpassager : Wressource).GetComponent<MoveAlongSpline>();
        wagon.SetSpline(spline);

        if (passager) isInstantiatingPassager = false;
        else isInstantiatingRessource = false;
    }
}