using System.Collections;
using UnityEngine;

public class LoadSceneOnStart : MonoBehaviour
{
    [SerializeField] float delay;
    [SerializeField] int sceneToLoad;
    private void Start()
    {
        StartCoroutine(delayNLoad());
    }

    IEnumerator delayNLoad()
    {
        yield return new WaitForSecondsRealtime(delay);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
    }
}
