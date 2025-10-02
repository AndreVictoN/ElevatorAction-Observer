using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Door : NetworkBehaviour
{
    private bool _isActive = true;

    void Start()
    {
        _isActive = true;
    }

    void Update()
    {
        if (!_isActive) ChangeColor();
    }

    public void ChangeColor()
    {
        this.gameObject.GetComponent<SpriteRenderer>().color = new Color(0.22082f, 0.5754717f, 0.122152f, 1f);
    }

    public void SetIsActive(bool isDoorActive)
    {
        _isActive = isDoorActive;
    }

    public bool GetIsActive() { return _isActive; }
    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
