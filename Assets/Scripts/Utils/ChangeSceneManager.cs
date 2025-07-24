using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneManager : MonoBehaviour
{
    public void ChangeScene(AudioSource source)
    {
        StartCoroutine(Change("Level01", source));
    }

    private IEnumerator Change(String sceneToLoad, AudioSource source)
    {
        if (source != null) source.Play();
        yield return new WaitForSeconds(source.clip.length);
        SceneManager.LoadScene(sceneToLoad);
    }
}
