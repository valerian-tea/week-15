using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Yarn.Unity;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField]
    Image fadeImage;

    [SerializeField]
    float fadeDuration = 1f;

    [SerializeField]
    float fadeTimer = 0.1f;
    public bool isChangingScene { get; private set; } = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    [YarnCommand("change_scene")]
    public void LoadScene(string sceneName)
    {
        Debug.Log("Yarn command to change scene to: " + sceneName + "inside sceneloader");
        StartCoroutine(FadeAndLoadScene(sceneName));
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        isChangingScene = true;

        if (fadeImage == null)
        {
            Debug.Log("Fade image is null, loading scene directly");
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        fadeImage.gameObject.SetActive(true);
        Color c = fadeImage.color;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
        c.a = 1f;
        fadeImage.color = c;
        yield return new WaitForSeconds(fadeTimer);
        SceneManager.LoadScene(sceneName);
        isChangingScene = false;
    }

    [YarnCommand("fade_in")]
    public void CameraFadeIn()
    {
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        fadeImage.gameObject.SetActive(true);
        Color c = fadeImage.color;
        float t = fadeDuration;
        while (t > 0f)
        {
            t -= Time.deltaTime;
            c.a = Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
        c.a = 0f;
        fadeImage.color = c;
        yield return new WaitForSeconds(fadeTimer);
    }
}
