using UnityEngine;

public class UndoData
{
    public TileNode node;
    public GameManager.DIRECTIONS direction;
    public Quaternion previousRotation;
    public Vector3 previousPosition;
}
