using UnityEngine;

public class QuitGame : MonoBehaviour
{
    void Update()
    {
        // Vérifie si la touche Échap est pressée
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Quitte le jeu dans une build
            Application.Quit();

            // Affiche un message dans l'éditeur pour confirmer que le script fonctionne
#if UNITY_EDITOR
            Debug.Log("Quitter le jeu (non visible dans l'éditeur)");
#endif
        }
    }
}