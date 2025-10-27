using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
{
    public TextMeshProUGUI score;
    public TextMeshProUGUI playerTwoScore;
    private int _score = 0;
    private int _scoreP2 = 0;
    private bool _isPlayerTwo;

    public void UpdateScore(int sum)
    {
        if (!_isPlayerTwo)
        {
            _score += sum;
            score.text = "P1 Score: " + _score;
        }
        else
        {
            _scoreP2 += sum;
            playerTwoScore.text = "P2 Score: " + _scoreP2;
        }

        UpdateScoreServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateScoreServerRpc()
    {
        UpdateScoreClientRpc();
    }

    [ClientRpc]
    private void UpdateScoreClientRpc()
    {
        score.text = "P1 Score: " + _score;
        if (playerTwoScore.gameObject.activeSelf) playerTwoScore.text = "P2 Score: " + _scoreP2;
    }

    public override void OnNetworkSpawn()
    {
        CheckPlayerTwo();
    }

    public void CheckPlayerTwo()
    {
        if (GameObject.FindGameObjectsWithTag("Player").Length > 1) _isPlayerTwo = true;
        if (!_isPlayerTwo) score.text = "P1 Score: 0";
        else { SpawnPlayerTwoScoreServerRpc(); playerTwoScore.text = "P2 Score: 0"; }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerTwoScoreServerRpc()
    {
        SpawnPlayerTwoScoreClientRpc();
    }

    [ClientRpc]
    private void SpawnPlayerTwoScoreClientRpc()
    {
        playerTwoScore.gameObject.SetActive(true);
    }
}
