using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RailPath : MonoBehaviour
{
    public List<CurvePoint> RailPathPoints;
}

[System.Serializable]
public class CurvePoint
{
    public List<GameObject> points;

    public CurvePoint()
    {
        points = new List<GameObject>();
    }
}

