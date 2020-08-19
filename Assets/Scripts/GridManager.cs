using TMPro;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    private const float STARTX = -0.453f;
    private const float STARTY = 0.1f;
    private const float STARTZ = -0.9f;
    private const int DEFAULTROWCOUNT = 4;
    private const int DEFAULTCOLUMNCOUNT = 3;
    private const float TILESIZE_X = .45f;
    private const float TILESIZE_Z = .45f;

    private int rows, columns;
    public TileNode[,] grid;

    public GameObject bread, cheese, lettuce, tomato, empty;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        rows = DEFAULTROWCOUNT;
        columns = DEFAULTCOLUMNCOUNT;

        grid = new TileNode[rows, columns];

        GridData gridData = LevelManager.Instance.GetCurrentLevelData();

        if (gridData != null)
        {
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    TileData tileData = GetTileData(gridData, j, i);

                    CreateTile(tileData, j, i);
                }
            }

            CompleteTileNodeInfos();
        }
    }

    private void CreateTile(TileData tileData, int j, int i)
    {
        GameObject tileType = ChooseObjectByTileState(tileData.tileState);

        GameObject tileObj = Instantiate(tileType, new Vector3(STARTX + TILESIZE_X * i, STARTY, STARTZ + TILESIZE_Z * j), Quaternion.identity) as GameObject;

        TileNode tileNode = new TileNode();
        tileNode.tile = tileData;
        tileNode.sceneObject = tileObj;

        if (tileData.tileState == TileData.TileState.NONE)
        {
            tileNode.isAvailable = false;
        }
        else
        {
            tileNode.isAvailable = true;
        }

        tileObj.GetComponent<InputReader>().tileNode = tileNode;

        grid[tileData.row, tileData.column] = tileNode;
    }

    private TileData GetTileData(GridData gridData, int row, int column)
    {
        foreach (var tile in gridData.tiles)
        {
            if (tile.row == row && tile.column == column)
            {
                return tile;
            }
        }

        return null;
    }

    void CompleteTileNodeInfos()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                TileNode tileNode = grid[i, j];

                tileNode.left = GetLeft(grid, i, j);
                tileNode.right = GetRight(grid, i, j);
                tileNode.up = GetTop(grid, i, j);
                tileNode.down = GetBottom(grid, i, j);
            }
        }
    }

    private TileNode GetLeft(TileNode[,] grid, int row, int column)
    {
        int newColumn = column - 1;
        if (newColumn < 0)
        {
            return null;
        }
        return grid[row, newColumn];
    }

    private TileNode GetRight(TileNode[,] grid, int row, int column)
    {
        int newColumn = column + 1;
        if (newColumn >= DEFAULTCOLUMNCOUNT)
        {
            return null;
        }
        return grid[row, newColumn];
    }

    private TileNode GetTop(TileNode[,] grid, int row, int column)
    {
        int newRow = row + 1;
        if (newRow >= DEFAULTROWCOUNT)
        {
            return null;
        }

        return grid[newRow, column];
    }

    private TileNode GetBottom(TileNode[,] grid, int row, int column)
    {
        int newRow = row - 1;
        if (newRow < 0)
        {
            return null;
        }
        return grid[newRow, column];
    }

    private GameObject ChooseObjectByTileState(TileData.TileState tileState)
    {
        switch (tileState)
        {
            case TileData.TileState.NONE:
                return empty;
            case TileData.TileState.BREAD:
                return bread;
            case TileData.TileState.TOMATO:
                return tomato;
            case TileData.TileState.LETTUCE:
                return lettuce;
            case TileData.TileState.CHEESE:
                return cheese;
        }
        return null;
    }
}