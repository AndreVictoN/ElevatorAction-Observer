using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class UpTeleporter : Teleporters
{
    public override Tween MovePlayer(PlayerController player, float playerPositionY)
    {
        return player.gameObject.transform.DOLocalMoveY(playerPositionY + 2, 1f);
    }
}
