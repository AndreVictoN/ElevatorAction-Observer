using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DownTeleporter : Teleporters
{
    public override Tween MovePlayer(GameObject player, float playerPositionY)
    {
        return player.transform.DOLocalMoveY(playerPositionY - 2, 1f);
    }
}
