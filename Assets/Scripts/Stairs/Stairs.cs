using UnityEngine;

public class Stairs : MonoBehaviour
{
    public enum StairDirection
    {
        UpLeft,
        UpRight,
        DownLeft,
        DownRight
    }

    public StairDirection direction;
    public Transform targetPosition;
}