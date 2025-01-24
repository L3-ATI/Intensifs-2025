using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RailPath : MonoBehaviour
{
    public List<GameObject> RailPathNext;  
    public List<GameObject> RailPathPrevious;  
    
    private void Start()
    {
        for (int i = 0; i < 6; i++)
        {
            RailPathNext.Add(null);
            RailPathPrevious.Add(null);
        }
    }
}

