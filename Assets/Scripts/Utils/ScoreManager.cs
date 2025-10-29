using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
{
    private ulong _playerOneId;
    public TextMeshProUGUI score;
    public TextMeshProUGUI playerTwoScore;
    private NetworkVariable<int> _score = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> _scoreP2 = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private bool _isPlayerTwo;

    public void UpdateScore(int sum, GameObject player)
    {
        /*if (!_isPlayerTwo)
        {
            _score += sum;
            score.text = "P1 Score: " + _score;
        }
        else
        {
            _scoreP2 += sum;
            playerTwoScore.text = "P2 Score: " + _scoreP2;
        }*/

        if(player.GetComponent<PlayerController>().GetNetworkId() == _playerOneId)
        {
            _isPlayerTwo = false;
        }
        else
        {
            _isPlayerTwo = true;
        }

        if (!_isPlayerTwo)
        {
            UpdateScoreP1ServerRpc(sum);
        }
        else
        {
            UpdateScoreP2ServerRpc(sum);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateScoreP1ServerRpc(int sum)
    {
        _score.Value += sum;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateScoreP2ServerRpc(int sum)
    {
        _scoreP2.Value += sum;
    }

    public override void OnNetworkSpawn()
    {
        _score.OnValueChanged += (oldVal, newVal) =>
        {
            score.text = "P1 Score: " + newVal;
        };

        _scoreP2.OnValueChanged += (oldVal, newVal) =>
        {
            playerTwoScore.text = "P2 Score: " + newVal;
        };

        CheckPlayerTwo();

        score.text = $"P1 Score: " + _score.Value;
        playerTwoScore.text = $"P1 Score: " + _scoreP2.Value;
    }

    public void CheckPlayerTwo()
    {
        if (GameObject.FindGameObjectsWithTag("Player").Length > 1) _isPlayerTwo = true;
        if (!_isPlayerTwo) { score.text = "P1 Score: 0"; _playerOneId = GameObject.FindGameObjectWithTag("Player").GetComponent<NetworkObject>().NetworkObjectId; }
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
