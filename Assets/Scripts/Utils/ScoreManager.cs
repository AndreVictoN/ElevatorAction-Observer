using Core.Singleton;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreManager : NetworkSingleton<ScoreManager>
{
    private ulong _playerOneId;
    public TextMeshProUGUI score;
    public TextMeshProUGUI playerTwoScore;
    private NetworkVariable<int> _score = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> _scoreP2 = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    static public int scoreP1Save;
    static public int scoreP2Save;
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

        if (player != null && player.GetComponent<PlayerController>() != null)
        {
            if (player.GetComponent<PlayerController>().GetNetworkId() == _playerOneId)
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
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateScoreP1ServerRpc(int sum)
    {
        _score.Value += sum;
        scoreP1Save = _score.Value;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateScoreP2ServerRpc(int sum)
    {
        _scoreP2.Value += sum;
        scoreP2Save = _scoreP2.Value;
    }

    public override void OnNetworkSpawn()
    {
        if (SceneManager.GetActiveScene().name.Equals("FinalScene")) return;
        
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
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 1) _isPlayerTwo = true;
        if (!_isPlayerTwo)
        {
            if(!SceneManager.GetActiveScene().name.Equals("Level02")) score.text = "P1 Score: 0";
            foreach (GameObject gO in players)
            {
                if (gO.GetComponent<PlayerController>().IsServer) _playerOneId = gO.GetComponent<PlayerController>().GetNetworkId();
            }
        }
        else { SpawnPlayerTwoScoreServerRpc(); if(!SceneManager.GetActiveScene().name.Equals("Level02"))playerTwoScore.text = "P2 Score: 0"; }

        if (_playerOneId == 0) _playerOneId = 1;
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

    public int GetPlayerOneScore() { return _score.Value; }
    public int GetPlayerTwoScore() { return _scoreP2.Value; }
}
