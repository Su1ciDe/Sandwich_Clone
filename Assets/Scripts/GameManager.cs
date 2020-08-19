using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum DIRECTIONS { DEFAULT, LEFT, RIGHT, UP, DOWN };

    public static GameManager Instance;

    public GameObject plate;
    public Button btnUndo;

    private const float TILE_HEIGHT = 0.05f;

    public float rotationSpeed = 1000;

    public Touch touch;
    public Vector3 startSwipe, endSwipe;

    private List<UndoData> undoHistory;

    private InputReader inputReader;
    private GameObject selectedTile;
    private Vector3 selectedTileRot;

    public int curToastElement;

    private bool canMove = true;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        undoHistory = new List<UndoData>();

        btnUndo.enabled = true;
        plate.SetActive(false);
    }

    void Update()
    {
        Swipe();
    }

    void Swipe()
    {
        if (Input.touchCount == 1)
        {
            touch = Input.GetTouch(0);
        }

        if (touch.phase == TouchPhase.Began)
        {
            SelectObject(touch.position);
            startSwipe = (touch.position);
        }
        if (touch.phase == TouchPhase.Ended)
        {
            endSwipe = (touch.position);

            Vector3 swipe = (endSwipe - startSwipe).normalized;

            if (inputReader != null)
            {
                TileNode selectedTileNode = inputReader.tileNode;
                if (selectedTileNode != null)
                {
                    if (Mathf.RoundToInt(swipe.y) == 1)
                    {
                        selectedTileNode = selectedTileNode.ParentOfAll;

                        if (selectedTileNode.up != null && selectedTileNode.up.isAvailable)
                        {
                            MoveTiles(DIRECTIONS.UP, selectedTileNode, selectedTileNode.up);
                        }

                        Debug.Log("Up");
                    }
                    else if (Mathf.RoundToInt(swipe.y) == -1)
                    {
                        selectedTileNode = selectedTileNode.ParentOfAll;

                        if (selectedTileNode.down != null && selectedTileNode.down.isAvailable)
                        {
                            MoveTiles(DIRECTIONS.DOWN, selectedTileNode, selectedTileNode.down);
                        }

                        Debug.Log("down");
                    }
                    else if (Mathf.RoundToInt(swipe.x) == 1)
                    {
                        selectedTileNode = selectedTileNode.ParentOfAll;

                        if (selectedTileNode.right != null && selectedTileNode.right.isAvailable)
                        {
                            MoveTiles(DIRECTIONS.RIGHT, selectedTileNode, selectedTileNode.right);
                        }

                        Debug.Log("right");
                    }
                    else if (Mathf.RoundToInt(swipe.x) == -1)
                    {
                        selectedTileNode = selectedTileNode.ParentOfAll;

                        if (selectedTileNode.left != null && selectedTileNode.left.isAvailable)
                        {
                            MoveTiles(DIRECTIONS.LEFT, selectedTileNode, selectedTileNode.left);
                        }

                        Debug.Log("left");
                    }
                }
            }

            inputReader = null;
        }
    }

    public void MoveTiles(DIRECTIONS dir, TileNode _selectedTileNode, TileNode targetTileNode)
    {
        if (!canMove)
            return;

        curToastElement--;

        canMove = false;
        float targetHeight = CalculateHeight(targetTileNode);
        float selectedHeight = CalculateHeight(_selectedTileNode);

        _selectedTileNode.isAvailable = false;

        targetTileNode.children.Add(_selectedTileNode);
        _selectedTileNode.parent = targetTileNode;

        // Undo
        undoHistory.Insert(0, new UndoData
        {
            node = targetTileNode,
            direction = dir,
            previousRotation = _selectedTileNode.sceneObject.transform.localRotation,
            previousPosition = _selectedTileNode.sceneObject.transform.position
        });

        // move
        Vector3 newPos = new Vector3(targetTileNode.sceneObject.transform.position.x, _selectedTileNode.sceneObject.transform.position.y + targetHeight + selectedHeight, targetTileNode.sceneObject.transform.position.z);
        StartCoroutine(MoveFunc(newPos, _selectedTileNode.sceneObject.gameObject));

        // rotation
        Vector3 newRot = _selectedTileNode.sceneObject.transform.localRotation.eulerAngles + new Vector3(-GetRotationAngle(dir).x, 0, -GetRotationAngle(dir).z);
        StartCoroutine(RotateFunc(newRot, _selectedTileNode.sceneObject.gameObject));

        _selectedTileNode.sceneObject.transform.SetParent(targetTileNode.sceneObject.transform);
        _selectedTileNode.sceneObject.gameObject.layer = 0;

        if (curToastElement == 1)
        {
            _selectedTileNode.isOnTheTop = true;

            if (isGameCompleted(_selectedTileNode))
            {
                btnUndo.enabled = false;
                plate.SetActive(true);

                StartCoroutine(LevelManager.Instance.LoadNextLevel());


            }
        }

        Invoke("EnableMove", 0.4f);
    }

    IEnumerator MoveFunc(Vector3 newPos, GameObject obj)
    {
        float timeSinceStarted = 0f;
        while (true)
        {
            timeSinceStarted += Time.deltaTime;
            obj.transform.position = Vector3.Lerp(obj.transform.position, newPos, timeSinceStarted);

            if (obj.transform.position == newPos)
            {
                yield break;
            }

            yield return null;
        }
    }

    IEnumerator RotateFunc(Vector3 newRot, GameObject obj)
    {
        while (true)
        {
            obj.transform.localRotation = Quaternion.RotateTowards(obj.transform.localRotation, Quaternion.Euler(newRot), Time.deltaTime * rotationSpeed);

            if (obj.transform.localRotation == Quaternion.Euler(newRot))
            {
                yield break;
            }

            yield return null;
        }
    }

    void EnableMove() => canMove = true;

    private float CalculateHeight(TileNode tileNode)
    {
        float height = TILE_HEIGHT / 2;
        int childCount = tileNode.ChildCount;
        height += (childCount * TILE_HEIGHT);

        return height;
    }

    private bool isGameCompleted(TileNode tileOnTop)
    {
        var tileNodes = GridManager.Instance.grid;

        int breadCount = 0;
        foreach (TileNode tileNode in tileNodes)
        {
            if (tileNode.tile.tileState != TileData.TileState.NONE && tileNode.isAvailable)
            {
                if (tileNode.tile.tileState != TileData.TileState.BREAD)
                {
                    return false;
                }

                breadCount++;
            }
        }

        if (tileOnTop.tile.tileState != TileData.TileState.BREAD)
        {
            return false;
        }

        if (breadCount == 0 || breadCount > 1)
        {
            return false;
        }

        return true;
    }

    private void SelectObject(Vector3 touchPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(touchPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~LayerMask.NameToLayer("Tiles")))
        {
            inputReader = hit.collider.GetComponent<InputReader>();
            selectedTile = inputReader.gameObject;
        }
    }

    public Vector3 GetRotationAngle(DIRECTIONS direction)
    {
        switch (direction)
        {
            case DIRECTIONS.UP:
                return new Vector3(180.0f, 0, 0);
            case DIRECTIONS.RIGHT:
                return new Vector3(0, 0, -180.0f);
            case DIRECTIONS.LEFT:
                return new Vector3(0, 0, 180.0f);
            case DIRECTIONS.DOWN:
                return new Vector3(-180.0f, 0, 0);
        }

        return Vector3.zero;
    }

    public void Undo()
    {
        canMove = false;
        ExecuteUndo();
        canMove = true;
    }

    private void ExecuteUndo()
    {
        if (undoHistory.Count == 0)
            return;

        var moveData = undoHistory[0];
        undoHistory.RemoveAt(0);

        curToastElement++;
        TileNode tileNode = moveData.node.children[moveData.node.children.Count - 1];

        DIRECTIONS dir = moveData.direction;
        moveData.node.children.Remove(tileNode);
        tileNode.parent = null;
        tileNode.isAvailable = true;

        //rotate
        Vector3 oldRot = new Vector3(Mathf.Sign(GetRotationAngle(dir).x) * moveData.previousRotation.x, 0, Mathf.Sign(GetRotationAngle(dir).z) * moveData.previousRotation.z);
        StartCoroutine(RotateFunc(oldRot, tileNode.sceneObject));

        tileNode.sceneObject.transform.SetParent(null);

        //move
        Vector3 oldPos = new Vector3(moveData.previousPosition.x, 0.1f, moveData.previousPosition.z);
        StartCoroutine(MoveFunc(oldPos, tileNode.sceneObject));

    }
}