using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public int levelNo = 1;

    private List<GridData> levels;

    public int toastElementCount;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        levels = Levels.Instance.levels;

        levelNo = PlayerPrefs.GetInt("LevelNo");
    }

    void Start()
    {
    }

    public GridData GetCurrentLevelData()
    {
        CheckLevelNo();

        foreach (var tile in levels[levelNo - 1].tiles)
        {
            if (tile.tileState != TileData.TileState.NONE)
            {
                toastElementCount++;
            }
        }

        GameManager.Instance.curToastElement = toastElementCount;
        return levels[levelNo - 1];
    }

    private void CheckLevelNo()
    {
        if (levelNo <= 0 || levelNo > levels.Count)
            levelNo = 1;
    }

    public IEnumerator LoadNextLevel()
    {
        levelNo += 1;
        CheckLevelNo();
        PlayerPrefs.SetInt("LevelNo", levelNo);

        yield return new WaitForSeconds(2);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);


    }
}
