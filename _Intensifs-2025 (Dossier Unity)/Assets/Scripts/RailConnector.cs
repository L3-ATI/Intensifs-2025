using UnityEngine;

public class RailConnector : MonoBehaviour
{
    private Tile parentTile;

    private void Start()
    {
        parentTile = GetComponentInParent<Tile>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Rail"))
        {
            Tile otherTile = other.GetComponentInParent<Tile>();

            if (otherTile != null)
            {
                Debug.Log($"Rail connected to: {otherTile.name}");
                if (!parentTile.connectedRails.Contains(otherTile.gameObject))
                {
                    parentTile.connectedRails.Add(otherTile.gameObject);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Rail"))
        {
            Tile otherTile = other.GetComponentInParent<Tile>();

            if (otherTile != null)
            {
                Debug.Log($"Rail disconnected from: {otherTile.name}");
                if (parentTile.connectedRails.Contains(otherTile.gameObject))
                {
                    parentTile.connectedRails.Remove(otherTile.gameObject);
                }
            }
        }
    }
}