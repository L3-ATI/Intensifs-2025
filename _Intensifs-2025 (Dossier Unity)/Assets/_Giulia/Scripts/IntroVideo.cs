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
        videoPlayer.loopPointReached += OnVideoFinished;

        logoImage.gameObject.SetActive(false);
    }

    void Update()
    {
        // Vérifie si la souris est cliquée ou si un bouton est pressé
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            SkipIntro();
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        StartCoroutine(ShowLogoAndFade());
    }

    private IEnumerator ShowLogoAndFade()
    {
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

    private void SkipIntro()
    {
        // Si la vidéo est en cours, on la saute immédiatement
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }

        // Si le logo est visible, on commence immédiatement le fade out
        if (logoImage.gameObject.activeSelf)
        {
            StopCoroutine("ShowLogoAndFade");
            logoImage.CrossFadeAlpha(0.0f, fadeDuration, false);
            StartCoroutine(LoadNextSceneAfterFade());
        }
        else
        {
            // Si le logo n'est pas encore affiché, on passe directement à la scène suivante
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    private IEnumerator LoadNextSceneAfterFade()
    {
        // Attendre la fin du fade out du logo
        yield return new WaitForSeconds(fadeDuration);

        // Charger la scène suivante
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
