using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class MoveAlongSpline : MonoBehaviour
{
    private SplineContainer Thespline;
    public float speed = 1f;
    private float distancePercentage = 0f;
    private float splineLength;
    private bool movingForward = true; // Pour savoir si on avance ou recule

    private void Start()
    {
        Thespline = TrainsSplineController.Path;
        splineLength = Thespline.CalculateLength();
    }

    // Update is called once per frame
    void Update()
    {
        if (movingForward)
        {
            distancePercentage += speed * Time.deltaTime / splineLength;
            if (distancePercentage >= 1f) // On atteint la fin de la spline
            {
                distancePercentage = 1f;  // On s'assure de ne pas dépasser 1
                movingForward = false;    // Inverser la direction
            }
        }
        else
        {
            distancePercentage -= speed * Time.deltaTime / splineLength;
            if (distancePercentage <= 0f) // On atteint le début de la spline
            {
                distancePercentage = 0f;  // On s'assure de ne pas dépasser 0
                movingForward = true;     // Inverser la direction
            }
        }

        // Mise à jour de la position de l'objet
        Vector3 currentPosition = Thespline.EvaluatePosition(distancePercentage);
        transform.position = currentPosition;

        // Calcul de la direction pour que l'objet regarde dans la direction de déplacement
        Vector3 nextPosition = Thespline.EvaluatePosition(distancePercentage + 0.05f);
        Vector3 direction = nextPosition - currentPosition;
        transform.rotation = Quaternion.LookRotation(direction, transform.up);
    }
}
