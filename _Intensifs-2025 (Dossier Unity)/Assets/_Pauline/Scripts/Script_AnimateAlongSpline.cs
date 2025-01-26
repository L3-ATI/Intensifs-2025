using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class MoveAlongSpline : MonoBehaviour
{
    private Spline TheSpline;
    public float speed = 1f;
    private float distancePercentage = 0f;
    private float splineLength;
    private bool movingForward = true;
    private bool isRotated = false;

    void Update()
    {
        if (TheSpline == null) return;
        
        if (movingForward)
        {
            distancePercentage += speed * Time.deltaTime / splineLength;
            if (distancePercentage >= 1f)
            {
                distancePercentage = 1f;
                movingForward = false;
                RotateObject(); // Appliquer la rotation � 180�
            }
        }
        else
        {
            distancePercentage -= speed * Time.deltaTime / splineLength;
            if (distancePercentage <= 0f)
            {
                distancePercentage = 0f;
                movingForward = true;
                RotateObject(); // Remettre l�orientation initiale
            }
        }

        // Mise � jour de la position
        Vector3 currentPosition = TheSpline.EvaluatePosition(distancePercentage);
        transform.position = currentPosition;

        // Calcul de la direction
        Vector3 nextPosition = TheSpline.EvaluatePosition(distancePercentage + 0.05f);
        Vector3 direction = nextPosition - currentPosition;
        transform.rotation = (direction.Equals(Vector3.zero) ? Quaternion.identity : Quaternion.LookRotation(direction)) * (isRotated ? Quaternion.Euler(0, 180, 0) : Quaternion.identity);
    }

    // Fonction pour faire tourner l'objet
    private void RotateObject()
    {
        isRotated = !isRotated;
    }

    public void SetSpline(Spline spline)
    {
        TheSpline = spline;
        splineLength = spline.GetLength();
    }

    public Spline GetSpline()
    {
        return TheSpline;
    }
}
