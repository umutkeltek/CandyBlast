using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using TMPro;
using UnityEngine.UI;
using Random = UnityEngine.Random;

//This is the class is responsible for creating level design for the real game. It is not the actual game. Desired elements are adjusted and saved into LevelSO s
public class LevelEditor : MonoBehaviour
{
    [SerializeField] private LevelSO levelSo;
    [SerializeField] private Transform pfCandyGridVisual;
    [SerializeField] private Transform pfGlassGridVisual;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI selectedCell;
    

    
    
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
                    grid.GetGridObject(x,y).spriteRenderer.sortingOrder = y;
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
                    grid.GetGridObject(x,y).spriteRenderer.sortingOrder = y;
                }
            }
        }
        
        SetCameraOrthoSize();
    }

    

    private (Vector3 center, float size) CalculateOrthoSize(Vector3 positionA, Vector3 positionB)
    {
        Vector3 center = (positionA + positionB) / 2f;
        float size = Mathf.Max(Mathf.Abs(positionA.x - positionB.x), Mathf.Abs(positionA.y - positionB.y));
        return (center, size);
    }
    
    private void SetCameraOrthoSize()
    {
        var desiredCameraWidth = grid.GetColumnsCount() * 256f / 48f;
        float screenRatio = (float)Screen.width / (float)Screen.height;
        var desiredCameraHeight = desiredCameraWidth / screenRatio;
        Camera.main.orthographicSize = desiredCameraHeight / 2f;
        cameraTransform.position = CalculateOrthoSize(
            grid.GetWorldPosition(0, 0) - new Vector3(grid.GetCellSize(), grid.GetCellSize()),
            grid.GetWorldPosition(grid.GetColumnsCount() - 1, grid.GetRowsCount() - 1) +
            new Vector3(grid.GetCellSize(), grid.GetCellSize())).center + Vector3.right * 1.22f + new Vector3(0,grid.GetCellSize()/2.56f,-10);
        
        
    }
    
    
    

    private void Update()
    {
        mousePosition = Input.mousePosition;
        mousePositionWorld = Camera.main.ScreenToWorldPoint(mousePosition);
        mousePositionWorld.z = Camera.main.nearClipPlane -3f;
        grid.GetXY(mousePositionWorld, out int x, out int y);
        if (x<0 || y<0 || x>=grid.GetColumnsCount() || y>=grid.GetRowsCount())
        {
            selectedCell.text = "Out of bounds";
        }
        else
        {
            selectedCell.text = "Selected Grid Cell: \n" + x + " : " + y;
        }
        
        if (Screen.height != 1080 || Screen.width != 1920)
        {
            
            //Screen.SetResolution(1920,1080,true); 
            
            
            Debug.Log("Please set resolution to 1920 1080 in order to use level editor!");
            
        }
        
        
        
        
        
        if (IsValidPosition(x,y))
        {

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                grid.GetGridObject(x, y).SetCandySO(levelSo.candyBlocksList[0]);
                grid.GetGridObject(x, y).spriteRenderer.sortingOrder = y;
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                grid.GetGridObject(x, y).SetCandySO(levelSo.candyBlocksList[1]);
                grid.GetGridObject(x, y).spriteRenderer.sortingOrder = y;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                grid.GetGridObject(x, y).SetCandySO(levelSo.candyBlocksList[2]);
                grid.GetGridObject(x, y).spriteRenderer.sortingOrder = y;
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                grid.GetGridObject(x, y).SetCandySO(levelSo.candyBlocksList[3]);
                grid.GetGridObject(x, y).spriteRenderer.sortingOrder = y;
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                grid.GetGridObject(x, y).SetCandySO(levelSo.candyBlocksList[4]);
                grid.GetGridObject(x, y).spriteRenderer.sortingOrder = y;
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                grid.GetGridObject(x, y).SetCandySO(levelSo.candyBlocksList[5]);
                grid.GetGridObject(x, y).spriteRenderer.sortingOrder = y;
            }
            

            if (Input.GetMouseButtonDown(1)) {
                grid.GetGridObject(x, y).SetHasGlass(!grid.GetGridObject(x, y).GetHasGlass());
            }
        }
    }
    public LevelSO GetLevelSO()
    {
        return levelSo;
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


