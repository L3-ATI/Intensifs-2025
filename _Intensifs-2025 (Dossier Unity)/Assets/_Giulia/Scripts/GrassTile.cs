using System.Collections.Generic;
using UnityEngine;

public class GrassTile : MonoBehaviour
{
    [Header("Vegetation Settings")]
    public GameObject[] grassPrefabs;
    public GameObject[] bushPrefabs;
    public GameObject[] flowerPrefabs;
    public GameObject[] treePrefabs;
    public GameObject[] cactusPrefabs, rocksPrefabs;

    public Vector2 vegetationScaleRange = new Vector2(0.5f, 1.5f);
    public Transform vegetationParent;

    [Header("Tile Type Settings")]
    public bool isGrassTile = true;
    public bool isDesertTile = false;

    [Range(0f, 1f)] public float grassProbability = 0.7f;
    [Range(0f, 1f)] public float bushProbability = 0.3f;
    [Range(0f, 1f)] public float flowerProbability = 0.4f;
    [Range(0f, 1f)] public float treeProbability = 0.1f;
    [Range(0f, 1f)] public float cactusProbability = 0.8f, rocksProbability = 0.7f;

    private void Start()
    {
        if (isDesertTile)
        {
            // Augmenter la probabilité et la plage pour les cactus et les pierres
            GenerateVegetation(cactusPrefabs, cactusProbability, 0, 3);
            GenerateVegetation(rocksPrefabs, rocksProbability, 0, 5);
        }
        else if (isGrassTile)
        {
            GenerateVegetation(grassPrefabs, grassProbability, 0, 8);
            GenerateVegetation(bushPrefabs, bushProbability, 0, 4);
            GenerateVegetation(flowerPrefabs, flowerProbability, 0, 6);
            GenerateVegetation(treePrefabs, treeProbability, 0, 2);
        }
    }

    private void GenerateVegetation(GameObject[] prefabs, float probability, int minCount, int maxCount)
    {
        if (prefabs.Length == 0 || vegetationParent == null)
        {
            Debug.LogWarning("Aucun prefab ou parent de végétation non défini !");
            return;
        }

        int vegetationCount = Mathf.FloorToInt(Random.Range(minCount, maxCount + 1) * probability);
        Debug.Log($"Tentative de génération de végétation : {vegetationCount} éléments.");

        if (vegetationCount <= 0) return;

        List<Vector3> usedPositions = new List<Vector3>();

        for (int i = 0; i < vegetationCount; i++)
        {
            GameObject selectedPrefab = prefabs[Random.Range(0, prefabs.Length)];
            GameObject instance = Instantiate(selectedPrefab, vegetationParent);

            Vector3 randomPosition;
            int attempts = 0;

            do
            {
                randomPosition = new Vector3(
                    Random.Range(-3.5f, 3.5f),
                    0,
                    Random.Range(-3.5f, 3.5f)
                );
                attempts++;
            } while (!IsPositionValid(randomPosition, usedPositions, 0.5f) && attempts < 10);

            if (attempts < 10)
            {
                usedPositions.Add(randomPosition);
                instance.transform.localPosition = randomPosition;

                float randomScale = Random.Range(vegetationScaleRange.x, vegetationScaleRange.y);
                instance.transform.localScale = Vector3.one * randomScale;

                float randomRotationY = Random.Range(0f, 360f);
                instance.transform.localRotation = Quaternion.Euler(0, randomRotationY, 0);
            }
            else
            {
                Destroy(instance);
            }
        }
    }

    private bool IsPositionValid(Vector3 position, List<Vector3> existingPositions, float minDistance)
    {
        foreach (var existingPosition in existingPositions)
        {
            if (Vector3.Distance(existingPosition, position) < minDistance)
                return false;
        }
        return true;
    }
}