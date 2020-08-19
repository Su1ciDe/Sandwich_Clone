[System.Serializable]
public class TileData
{
    public enum TileState { NONE, BREAD, TOMATO, LETTUCE, CHEESE };
    public int row, column;
    public TileState tileState;
}
