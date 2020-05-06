using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeTransition : MonoBehaviour
{
    public static FadeTransition fade;

    public Image fadeTransition;

    private void Start()
    {
        fade = this;
        fadeTransitionOut();
    }

    public void fadeTo(int scene)
    {
        StartCoroutine(FadeIn(scene));
    }

    public void fadeTransitionOut()
    {
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeIn(int scene)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime;
            fadeTransition.color = new Color(0, 0, 0, t);
            yield return null;
        }

        SceneManager.LoadScene(scene);

    }

    IEnumerator FadeOut()
    {

        float b = 1f;

        while (b > 0f)
        {
            b -= Time.deltaTime;
            try
            {
                fadeTransition.color = new Color(0, 0, 0, b);
            }
            catch (MissingReferenceException) { }
            yield return null;
        }
    }
}
