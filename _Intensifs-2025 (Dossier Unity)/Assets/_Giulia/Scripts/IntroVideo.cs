using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;

public class IntroVideo : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    public RawImage logoImage; // Référence à l'image/logo
    public float fadeDuration = 1f; // Durée du fade

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();

        // S'abonner à l'événement de fin de la vidéo
        videoPlayer.loopPointReached += OnVideoFinished;

        // Masquer le logo au début
        logoImage.gameObject.SetActive(false);
    }

    // Méthode appelée quand la vidéo se termine
    void OnVideoFinished(VideoPlayer vp)
    {
        StartCoroutine(ShowLogoAndFade());
    }

    private IEnumerator ShowLogoAndFade()
    {
        // Activer et afficher le logo
        logoImage.gameObject.SetActive(true);
        logoImage.canvasRenderer.SetAlpha(0.0f); // Initialiser la transparence à 0
        logoImage.CrossFadeAlpha(1.0f, 0.5f, false); // Fade in sur 0.5 seconde

        // Attendre 1 seconde avant de commencer le fade out
        yield return new WaitForSeconds(1f);

        // Fade out du logo
        logoImage.CrossFadeAlpha(0.0f, fadeDuration, false);

        // Attendre que le fade out soit terminé
        yield return new WaitForSeconds(fadeDuration);

        // Charger la scène suivante
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}