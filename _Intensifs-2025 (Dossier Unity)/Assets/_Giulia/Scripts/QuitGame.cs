using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public GameObject explosionPrefab;
    public float explosionTime = 1f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TriggerExplosions();
            Invoke("QuitGameMethod", explosionTime);
        }
    }

    void TriggerExplosions()
    {
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");
        foreach (GameObject tile in tiles)
        {
            GameObject explosion = Instantiate(explosionPrefab, tile.transform.position, Quaternion.identity, tile.transform);
        }
    }

    void QuitGameMethod()
    {
        Application.Quit();

#if UNITY_EDITOR
        Debug.Log("Quitter le jeu (non visible dans l'éditeur)");
#endif
    }
}