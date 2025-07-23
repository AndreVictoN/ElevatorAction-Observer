using DG.Tweening;
using UnityEngine;

public abstract class Teleporters : MonoBehaviour
{
    public abstract Tween MovePlayer(GameObject player, float playerPositionY);
}