using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using TMPro;
using Random = UnityEngine.Random;

//This is the class is responsible for creating level design for the real game. It is not the actual game. Desired elements are adjusted and saved into LevelSO s
public class LevelEditor : MonoBehaviour
{
    [SerializeField] private LevelSO levelSo;
    [SerializeField] private Transform pfCandyGridVisual;
    [SerializeField] private Transform pfGlassGridVisual;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private TextMeshProUGUI levelText;
    
    private Vector3 mousePosition;
    private Vector3 mousePositionWorld;

    private GridXY<GridPosition> grid;

    private void Awake()
    {
        grid = new GridXY<GridPosition>(levelSo.columns, levelSo.rows, 2.2f, Vector3.zero, (GridXY<GridPosition> g, int x, int y) => new GridPosition(levelSo, g, x, y));
        levelText.text = levelSo.name;

        if (levelSo.candyGridPositionsList == null || levelSo.candyGridPositionsList.Count != levelSo.columns*levelSo.rows) 
        {
            Debug.Log("Creating new level...");
            levelSo.candyGridPositionsList = new List<LevelSO.LevelGridPosition>();
            for (int x = 0; x < grid.GetColumnsCount(); x++)
            {
                for (int y = 0; y < grid.GetRowsCount(); y++)
                {
                    CandyBlockSO candyBlock = levelSo.candyBlocksList[Random.Range(0,levelSo.candyBlocksList.Count)];
                    LevelSO.LevelGridPosition levelGridPosition = new LevelSO.LevelGridPosition{candyBlockSO = candyBlock, x = x, y = y};
                    levelSo.candyGridPositionsList.Add(levelGridPosition);
                    
                    CreateVisual(grid.GetGridObject(x,y),levelGridPosition);
                }
            }
            
        }
        else
        {
            Debug.Log("loading level...");
            for (int x = 0; x < grid.GetColumnsCount(); x++)
            {
                for (int y = 0; y < grid.GetRowsCount(); y++)
                {
                    LevelSO.LevelGridPosition levelGridPosition = null;
                    foreach (LevelSO.LevelGridPosition tempLevelGridPosition in levelSo.candyGridPositionsList)
                    {
                        if (tempLevelGridPosition.x == x && tempLevelGridPosition.y == y)
                        {
                            levelGridPosition = tempLevelGridPosition;
                            break;
                        }
                    }
                    if (levelGridPosition == null)
                    {
                        Debug.LogError("LevelGridPosition not found");
                    }
                    CreateVisual(grid.GetGridObject(x,y), levelGridPosition);
                }
            }
        }
        cameraTransform.position = new Vector3(grid.GetColumnsCount() * .5f, grid.GetRowsCount() * .5f, cameraTransform.position.z);
    }

    private void Update()
    {
        mousePosition = Input.mousePosition;
        mousePositionWorld = Camera.main.ScreenToWorldPoint(mousePosition);
        mousePositionWorld.z = Camera.main.nearClipPlane -3f;
        grid.GetXY(mousePositionWorld, out int x, out int y);
        
        
        if (IsValidPosition(x,y))
        {
            
            if (Input.GetKeyDown(KeyCode.Alpha1)) { grid.GetGridObject(x, y).SetCandySO(levelSo.candyBlocksList[0]); }
            if (Input.GetKeyDown(KeyCode.Alpha2)) grid.GetGridObject(x, y).SetCandySO(levelSo.candyBlocksList[1]);
            if (Input.GetKeyDown(KeyCode.Alpha3)) grid.GetGridObject(x, y).SetCandySO(levelSo.candyBlocksList[2]);
            if (Input.GetKeyDown(KeyCode.Alpha4)) grid.GetGridObject(x, y).SetCandySO(levelSo.candyBlocksList[3]);
            if (Input.GetKeyDown(KeyCode.Alpha5)) grid.GetGridObject(x, y).SetCandySO(levelSo.candyBlocksList[4]);
            if (Input.GetKeyDown(KeyCode.Alpha5)) grid.GetGridObject(x, y).SetCandySO(levelSo.candyBlocksList[5]);

            if (Input.GetMouseButtonDown(1)) {
                grid.GetGridObject(x, y).SetHasGlass(!grid.GetGridObject(x, y).GetHasGlass());
            }
        }
    }

    private void CreateVisual(GridPosition gridPosition, LevelSO.LevelGridPosition levelGridPosition)
    {
        Transform candyGridVisualTransform = Instantiate(pfCandyGridVisual, gridPosition.GetWorldPosition(), Quaternion.identity);
        Transform glassGridVisualTransform = Instantiate(pfGlassGridVisual, gridPosition.GetWorldPosition(), Quaternion.identity);
        
        gridPosition.spriteRenderer = candyGridVisualTransform.Find("sprite").GetComponent<SpriteRenderer>();
        gridPosition.glassVisualGameObject = glassGridVisualTransform.gameObject;
        gridPosition.levelGridPosition = levelGridPosition;

        gridPosition.SetCandySO(levelGridPosition.candyBlockSO);
        gridPosition.SetHasGlass(levelGridPosition.hasGlass);

    }

    private bool IsValidPosition(int x, int y)
    {
        if (x < 0 || x >= grid.GetColumnsCount() || y < 0 || y >= grid.GetRowsCount())
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    #region GridPositionClass

    private class GridPosition //Dummy grid position class to create levels using level editor 
    {
        public SpriteRenderer spriteRenderer;
        public LevelSO.LevelGridPosition levelGridPosition;
        public GameObject glassVisualGameObject;

        private LevelSO levelSO;
        private GridXY<GridPosition> grid;
        private int x;
        private int y;
        
        public GridPosition(LevelSO levelSO, GridXY<GridPosition> grid, int x, int y)
        {
            this.levelSO = levelSO;
            this.grid = grid;
            this.x = x;
            this.y = y;
        }
        public Vector3 GetWorldPosition()
        {
            return grid.GetWorldPosition(x, y);
        }
        public void SetCandySO(CandyBlockSO candySO)
        {
            spriteRenderer.sprite = candySO.defaultCandySprite;
            levelGridPosition.candyBlockSO = candySO;
            
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(levelSO);
#endif
        }

        public void SetHasGlass(bool hasGlass)
        {
            levelGridPosition.hasGlass = hasGlass;
            glassVisualGameObject.SetActive(hasGlass);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(levelSO);
#endif
        }
        public bool GetHasGlass()
        {
            return levelGridPosition.hasGlass;
        }
    }

    #endregion
    
}


