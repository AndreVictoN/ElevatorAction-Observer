using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : Core.Singleton.Singleton<GameManager>
{
    public GameObject pausedScreen;
    public List<AudioClip> audioClips = new();
    private List<AudioSource> _audioSources = new();
    private bool _pausedGame = false;

    void Start()
    {
        _audioSources.AddRange(GameObject.FindObjectsOfType<AudioSource>());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!_pausedGame)
            {
                _pausedGame = true;

                foreach(AudioSource audioSource in _audioSources)
                {
                    audioSource.Pause();
                }

                foreach(AudioSource audioSource in _audioSources){if(audioSource.name == "SFX") {audioSource.clip = audioClips[0]; audioSource.Play();}}
                pausedScreen.SetActive(true);
            }else {
                _pausedGame = false;

                foreach(AudioSource audioSource in _audioSources)
                {
                    if(audioSource.name == "BGSong") audioSource.Play();
                }

                pausedScreen.SetActive(false);
            }
        }
    }

    public bool IsGamePaused(){return _pausedGame;}
}
