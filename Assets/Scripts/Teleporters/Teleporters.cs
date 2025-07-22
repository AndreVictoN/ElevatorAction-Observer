using DG.Tweening;
using UnityEngine;

public abstract class Teleporters : MonoBehaviour
{
    public abstract Tween MovePlayer(PlayerController player, float playerPositionY);
}