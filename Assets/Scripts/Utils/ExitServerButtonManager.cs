using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitServerButtonManager : MonoBehaviour
{
    public void GoToMenu()
    {
        if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) { DOTween.KillAll(false); NetworkManager.Singleton.Shutdown(); }
        else if (NetworkManager.Singleton.IsClient && NetworkManager.Singleton.IsServer) { DOTween.KillAll(false); FindObjectOfType<GameManager>().GameOver(); }
        SceneManager.LoadScene("MainMenu");
    }
}
