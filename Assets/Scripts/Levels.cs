using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Levels")]
public class Levels : ScriptableObject
{
    public List<GridData> levels;

    public static Levels instance;

    public static Levels Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load("Level") as Levels;
            }

            return instance;
        }
    }
}
