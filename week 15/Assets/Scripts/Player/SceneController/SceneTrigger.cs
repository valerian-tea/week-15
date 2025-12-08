using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Yarn.Unity;

public class SceneTrigger : MonoBehaviour
{
    [SerializeField]
    string sceneToLoad;

    public GameObject sceneChangeText;

    [SerializeField]
    Image fadeImage;

    [SerializeField]
    float fadeDuration = 1f;

    [SerializeField]
    float fadeTimer = 0.1f;

    private bool isChangingScene = false;

    void Start()
    {
        if (sceneChangeText != null)
        {
            sceneChangeText.SetActive(false);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isChangingScene)
            return;

        if (other.CompareTag("Player"))
        {
            if (sceneChangeText != null)
                sceneChangeText.SetActive(true);

            if (Input.GetKey(KeyCode.Space))
            {
                Debug.Log("Player reached destination, loading scene: " + sceneToLoad);
                StartCoroutine(FadeAndLoadScene());
                if (sceneChangeText != null)
                    sceneChangeText.SetActive(false);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            sceneChangeText.SetActive(false);
        }
    }

    [YarnCommand("change_scene")]
    public void YarnSceneChange(string sceneName)
    {
        sceneToLoad = sceneName;
        StartCoroutine(FadeAndLoadScene());
    }

    private IEnumerator FadeAndLoadScene()
    {
        isChangingScene = true;

        if (fadeImage == null)
        {
            Debug.Log("Fade image is null, loading scene directly");
            SceneManager.LoadScene(sceneToLoad);
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
        DontDestroyOnLoad(fadeImage.gameObject);
        SceneManager.LoadScene(sceneToLoad);
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
        DontDestroyOnLoad(fadeImage.gameObject);
    }
}
