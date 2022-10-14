using System;
using System.Collections;
using System.Collections.Generic;
using CodeMonkey.Utils;
using UnityEngine;


public class GridLogicSystem : MonoBehaviour
{   public event EventHandler OnCandyGridPositionDestroyed; //event for when a candy is destroyed
    public event EventHandler<OnNewCandyGridSpawnedEventArgs> OnNewCandyGridSpawned; //event for when a new candy is spawned
    public event EventHandler<OnLevelSetEventArgs> OnLevelSet; //event for when a level is set
    public event EventHandler OnGlassDestroyed; 
    public event EventHandler OnMoveUsed; 
    public event EventHandler OnOutOfMoves;
    public event EventHandler OnScoreChanged;
    public event EventHandler OnWin;
    public class OnLevelSetEventArgs : EventArgs // when a level is set this event is called to pass desired Leve
    {
        public LevelSO levelSO;
        public GridXY<CandyGridCellPosition> grid;
    }
    public class OnNewCandyGridSpawnedEventArgs : EventArgs {
        public CandyOnGridCell gemGrid;
        public CandyGridCellPosition candyGridCellPosition;
    }
    public GridXY<CandyGridCellPosition> grid;
    private int columns;
    private int rows;
    
    [Header("Grid's Size and Position")]
    [SerializeField] private float cellSize;
    [SerializeField] private Vector3 originPosition;
    
    [Header("Initializing Level Settings")]
    [SerializeField] private LevelSO levelSO;
    [SerializeField] private bool autoLoadLevel;

    private int score;
    private int moveCount;
    

    private void Start()
    {
        if (autoLoadLevel)
        {
            SetLevelSO(levelSO);
        }
    }
    public LevelSO GetLevelSO() //returns the scriptable object that stores the level elements.
    {
        return levelSO;
    }
    
    public void SetLevelSO(LevelSO levelSO) //this method is responsible for setting levelSO created by level editor script/scene
    {
        this.levelSO = levelSO;
        columns = levelSO.columns;
        rows = levelSO.rows;
        grid = new GridXY<CandyGridCellPosition>(levelSO.columns, levelSO.rows, cellSize, originPosition,
            (GridXY<CandyGridCellPosition> g, int x, int y) => new CandyGridCellPosition(g, x, y));
        //Initialize Grid in desired candy blocks in every grid cell
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                LevelSO.LevelGridPosition levelGridPosition = null; // creating a new empty levelGridPosition for each grid cell which stores candy block SO,x,y and glass block type.
                foreach (LevelSO.LevelGridPosition tempLevelGridPosition in levelSO.candyGridPositionsList)
                {
                    if (tempLevelGridPosition.x == x && tempLevelGridPosition.y == y)
                    {
                        levelGridPosition = tempLevelGridPosition;
                        break;
                    }
                }

                if (levelGridPosition == null)
                {
                    //No candy block in this grid cell
                    Debug.LogError("No candy block in this grid cell");
                }
                
                CandyBlockSO candyBlock = levelGridPosition.candyBlockSO;
                CandyOnGridCell candyOnGridCell = new CandyOnGridCell(candyBlock,x,y);
                grid.GetGridObject(x, y).SetCandyBlock(candyOnGridCell);
                grid.GetGridObject(x, y).SetHasGlass(levelGridPosition.hasGlass);
                
            }
            
        }

        score = 0;
        moveCount = levelSO.moveAmount;
        OnLevelSet?.Invoke(this, new OnLevelSetEventArgs {levelSO = levelSO, grid = grid});
        
    }
    
    public int GetScore() {
        return score;
    }
    
    public bool HasMoveAvailable() {
        return moveCount > 0;
    }
    public int GetMoveCount() {
        return moveCount;
    }
    public int GetUsedMoveCount() {
        return levelSO.moveAmount - moveCount;
    }
    public void UseMove() {
        moveCount--;
        OnMoveUsed?.Invoke(this, EventArgs.Empty);
    }
    public int GetGlassAmount() {
        int glassAmount = 0;
        for (int x = 0; x < columns; x++) {
            for (int y = 0; y < rows; y++) {
                CandyGridCellPosition candyGridCellPosition = grid.GetGridObject(x, y);
                if (candyGridCellPosition.HasGlass()) {
                    glassAmount++;
                }
            }
        }
        return glassAmount;
    }
    
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 60f;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            grid.GetXY(worldPosition, out int x, out int y);
            Debug.Log($"Mouse Position: {worldPosition} | Grid Position: [{x}, {y}]");
        }
    }
}
