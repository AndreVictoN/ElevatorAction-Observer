using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    void Start()
    {
        NetworkManager nm = FindObjectOfType<NetworkManager>();
        if (nm != null) Destroy(nm.gameObject);
    }
}
