using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
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
        this.gameObject.GetComponent<SpriteRenderer>().color = new Color(0.0504628f, 0.1849264f, 0.3962264f, 1f);
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
