using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/*
 * Script is used to add a black fade out/in on scene change
 */
public class FadeTransition : MonoBehaviour
{
    public static FadeTransition fade;  // Static reference for easy access
    public Image fadeTransition;        // Default black image used for fade

    private void Start()
    {
        fade = this;
        fadeTransitionOut();
    }

    /*
     * @param scene describes which scene to fade to
     */
    public void fadeTo(int scene)
    {
        StartCoroutine(FadeIn(scene));
    }

    /*
     * Method starts coroutine FadeOut
     */
    public void fadeTransitionOut()
    {
        StartCoroutine(FadeOut());
    }

    /*
     * IEnumerator gradually fades the scene to black and then changes the scene
     */
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

    /*
     * IEnumerator gradually fades to the scene
     */
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
