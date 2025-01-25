using System.Collections.Generic;
using UnityEngine;

public class CityTile : MonoBehaviour
{
    [System.Serializable]
    public class PrefabEntry
    {
        public GameObject prefab; // Le prefab à placer
        [Range(0f, 1f)] public float probability; // Probabilité associée
    }

    [Header("Prefab Settings")]
    public List<PrefabEntry> prefabEntries; // Liste des prefabs avec probabilités
    public Transform parentTransform; // Parent pour organiser l'objet

    [Header("Tile Settings")]
    public bool isCityTile = true; // Détermine si cette tuile est de type "City"

    private void Start()
    {
        if (isCityTile)
        {
            GeneratePrefab();
        }
    }

    private void GeneratePrefab()
    {
        if (prefabEntries == null || prefabEntries.Count == 0 || parentTransform == null)
        {
            Debug.LogWarning("Aucun prefab ou parent non défini !");
            return;
        }

        // Calculer la probabilité cumulée
        float cumulativeProbability = 0f;
        foreach (var entry in prefabEntries)
        {
            cumulativeProbability += entry.probability;
        }

        if (cumulativeProbability <= 0f)
        {
            Debug.LogWarning("Les probabilités cumulées doivent être supérieures à 0 !");
            return;
        }

        // Générer un nombre aléatoire pour déterminer quel prefab placer
        float randomValue = Random.value * cumulativeProbability;
        float currentSum = 0f;

        foreach (var entry in prefabEntries)
        {
            currentSum += entry.probability;

            if (randomValue <= currentSum)
            {
                // Placer le prefab sélectionné
                GameObject instance = Instantiate(entry.prefab, parentTransform);

                // Positionner au centre de la tuile
                instance.transform.localPosition = Vector3.zero;

                // Appliquer une rotation aléatoire (optionnelle)
                float randomRotationY = Random.Range(0f, 360f);
                instance.transform.localRotation = Quaternion.Euler(0, randomRotationY, 0);

                return;
            }
        }
    }
}
