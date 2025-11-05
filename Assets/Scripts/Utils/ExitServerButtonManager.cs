using Unity.Netcode;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitServerButtonManager : MonoBehaviour
{
    public void GoToMenu()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) { DOTween.KillAll(false); NetworkManager.Singleton.Shutdown(); }
            else if (NetworkManager.Singleton.IsClient && NetworkManager.Singleton.IsServer) { DOTween.KillAll(false); FindObjectOfType<GameManager>().GameOver(); }
        }

        SceneManager.LoadScene("MainMenu");
    }

    public void GoToMenuLocally()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
