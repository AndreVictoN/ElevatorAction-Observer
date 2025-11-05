using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneManager : MonoBehaviour
{
    public void Host(AudioSource source)
    {
        PlayerPrefs.SetInt("isHosting", 1);
        ChangeScene(source);
    }

    public void Join(AudioSource source)
    {
        PlayerPrefs.SetInt("isHosting", 0);
        ChangeScene(source);
    }

    private void ChangeScene(AudioSource source)
    {
        StartCoroutine(Change("Level01", source));
    }

    public void Exit(AudioSource source)
    {
        StartCoroutine(ExitGame(source));
    }

    private IEnumerator Change(String sceneToLoad, AudioSource source)
    {
        if (source != null) source.Play();
        yield return new WaitForSeconds(source.clip.length);
        SceneManager.LoadScene(sceneToLoad);
    }

    private IEnumerator ExitGame(AudioSource source)
    {
        if (source != null) source.Play();
        yield return new WaitForSeconds(source.clip.length);
        Application.Quit();
    }
}
