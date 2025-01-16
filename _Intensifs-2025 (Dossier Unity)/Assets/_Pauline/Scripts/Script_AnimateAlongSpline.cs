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
    private bool movingForward = true;
    private bool isRotated = false;
    private void Start()
    {
        Thespline = TrainsSplineController.Path;
        splineLength = Thespline.CalculateLength();
    }

    void Update()
    {
        if (movingForward)
        {
            distancePercentage += speed * Time.deltaTime / splineLength;
            if (distancePercentage >= 1f)
            {
                distancePercentage = 1f;
                movingForward = false;
                RotateObject(); // Appliquer la rotation à 180°
            }
        }
        else
        {
            distancePercentage -= speed * Time.deltaTime / splineLength;
            if (distancePercentage <= 0f)
            {
                distancePercentage = 0f;
                movingForward = true;
                RotateObject(); // Remettre l’orientation initiale
            }
        }

        // Mise à jour de la position
        Vector3 currentPosition = Thespline.EvaluatePosition(distancePercentage);
        transform.position = currentPosition;

        // Calcul de la direction
        Vector3 nextPosition = Thespline.EvaluatePosition(distancePercentage + 0.05f);
        Vector3 direction = nextPosition - currentPosition;
        transform.rotation = Quaternion.LookRotation(direction) * (isRotated ? Quaternion.Euler(0, 180, 0) : Quaternion.identity);
    }

    // Fonction pour faire tourner l'objet
    private void RotateObject()
    {
        isRotated = !isRotated;
    }
}
