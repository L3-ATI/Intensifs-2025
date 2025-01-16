using UnityEngine;
using UnityEngine.Splines;
using System.Collections; // Pour utiliser les coroutines

public class TrainsSplineController : MonoBehaviour
{
    public GameObject PointA;
    public GameObject PointB;
    public GameObject PointC;
    public GameObject Locomotive;
    public GameObject Wagon;

    public static SplineContainer Path;

    void Start()
    {
        if (!Application.isPlaying) return;

        // Assure que le SplineContainer est présent
        Path = GetComponent<SplineContainer>() ?? gameObject.AddComponent<SplineContainer>();
        Path.Spline.Clear();

        // Ajoute les points avec tangentes automatiques pour une transition fluide
        AddAutoSmoothSplinePoint(PointA.transform.position);
        AddAutoSmoothSplinePoint(PointB.transform.position);
        AddAutoSmoothSplinePoint(PointC.transform.position);

        // Instancier le premier wagon
        Instantiate(Locomotive);

        // Démarrer la coroutine pour ajouter un autre wagon après 0.1 seconde
        StartCoroutine(InstantiateWagonAfterDelay(1f));
    }

    void AddAutoSmoothSplinePoint(Vector3 position)
    {
        var knot = new BezierKnot(position);
        Path.Spline.Add(knot);

        // Set les points de la spline en "Auto" cela les rend plus smooth
        Path.Spline.SetTangentMode(Path.Spline.Count - 1, TangentMode.AutoSmooth);
    }

    // Coroutine pour instancier un wagon après un délai
    IEnumerator InstantiateWagonAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // Attendre pendant le délai

        // Instancier un autre wagon
        Instantiate(Wagon);
    }
}
