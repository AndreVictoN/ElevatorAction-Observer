using Unity.Netcode;
using UnityEngine;

public class Door : NetworkBehaviour
{
    private bool _isActive = true;
    private NetworkVariable<bool> _isNetworkActive = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField]private SpriteRenderer _renderer;
    private NetworkVariable<Color> _deactivateColor = new NetworkVariable<Color>(new Color(0.22082f, 0.5754717f, 0.122152f, 1f), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    /*void Start()
    {
        _isActive = true;
    }*/

    public override void OnNetworkSpawn()
    {
        SetIsActive(true);
        //_isNetworkActive.Value = true;
    }

    public void ChangeColor()
    {
        if (!IsServer && !IsHost && !IsClient) { this.gameObject.GetComponent<SpriteRenderer>().color = new Color(0.22082f, 0.5754717f, 0.122152f, 1f); }
        else
        {
            ChangeColorServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeColorServerRpc()
    {
        ChangeColorClientRpc();
    }

    [ClientRpc]
    private void ChangeColorClientRpc()
    {
        _renderer.color = _deactivateColor.Value;
    }

    public void SetIsActive(bool isDoorActive)
    {
        if (!IsServer && !IsHost && !IsClient)
        {
            _isActive = isDoorActive;
        }else
        {
            ChangeIsActiveServerRpc(isDoorActive);
        }
    }

    public bool GetIsActive() { return _isNetworkActive.Value; }
    public Vector3 GetPosition()
    {
        return transform.position;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeIsActiveServerRpc(bool value)
    {
        _isNetworkActive.Value = value;
        _isActive = _isNetworkActive.Value;

        ChangeIsActiveClientRpc();
    }

    [ClientRpc]
    private void ChangeIsActiveClientRpc()
    {
        _isActive = _isNetworkActive.Value;
    }
}
