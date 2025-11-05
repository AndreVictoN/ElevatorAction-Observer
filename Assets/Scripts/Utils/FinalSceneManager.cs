using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class FinalSceneManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textWinMessage;
    [SerializeField] private TextMeshProUGUI textScoreP1;
    [SerializeField] private TextMeshProUGUI textScoreP2;
    private int scoreP1;
    private int scoreP2;

    void Start()
    {
        scoreP1 = ScoreManager.scoreP1Save;
        scoreP2 = ScoreManager.scoreP2Save;
        textScoreP1.text = "P1 SCORE: " + scoreP1;
        textScoreP2.text = "P2 SCORE: " + scoreP2;

        if (scoreP1 > scoreP2) { textWinMessage.text = "Player 1 Win!"; }
        else if (scoreP1 < scoreP2) { textWinMessage.text = "Player 2 Win!"; }
        else { textWinMessage.text = "Tie!"; }

        List<PlayerController> allPlayers = new();
        allPlayers.AddRange(FindObjectsOfType<PlayerController>());

        if (NetworkManager.Singleton.IsServer)
        {
            allPlayers.ForEach(p => Destroy(p.gameObject));
        }
        else
        {
            List<NetworkObjectReference> allPlayersRef = new();

            foreach (PlayerController p in allPlayers)
            {
                allPlayersRef.Add(new NetworkObjectReference(p.GetComponent<NetworkObject>()));
            }

            RequestDespawnServerRpc(allPlayersRef);
        }

        StartCoroutine(TryShutdown());
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RequestDespawnServerRpc(List<NetworkObjectReference> netObjects)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        foreach(NetworkObjectReference nO in netObjects)
        {
            if(nO.TryGet(out NetworkObject netO)) if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(netO.NetworkObjectId) && netO.IsSpawned) netO.Despawn();
        }
    }
    
    private IEnumerator TryShutdown()
    {
        yield return new WaitForEndOfFrame();

        if(NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Destroy(GameObject.FindGameObjectWithTag("NetworkManager"));
        }
    }
}
