using System;
using System.Collections;
using System.Collections.Generic;
using CodeMonkey.Utils;
using UnityEditor.SceneManagement;
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
    public class OnNewCandyGridSpawnedEventArgs : EventArgs {
        public CandyOnGridCell candyOnGridCell;
        public CandyGridCellPosition candyGridCellPosition;
    }
    public class OnLevelSetEventArgs : EventArgs // when a level is set this event is called to pass desired Leve
    {   public LevelSO levelSO;
        public GridXY<CandyGridCellPosition> grid;
    }
    [Header("Grid's Size and Position")]
    [SerializeField] private float cellSize;
    [SerializeField] private Vector3 originPosition;
    
    [Header("Initializing Level Settings")]
    [SerializeField] private LevelSO levelSO;
    [SerializeField] private bool autoLoadLevel;
    
    public GridXY<CandyGridCellPosition> grid;
    private int score;
    private int columns;
    private int rows;
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
    } //returns the score
    public bool HasMoveAvailable() {
        return moveCount > 0;
    } //returns true if there are moves available
    public int GetMoveCount() {
        return moveCount;
    } //returns the move count
    public int GetUsedMoveCount() {
        return levelSO.moveAmount - moveCount;
    } //returns the used move count
    public void UseMove() {
        moveCount--;
        OnMoveUsed?.Invoke(this, EventArgs.Empty);
    } //decreses the move count by 1
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
    } //returns the amount of glass blocks in the grid
    public void SpawnNewMissingGridPositions()
    {
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                CandyGridCellPosition candyGridCellPosition = grid.GetGridObject(x, y);
                if (candyGridCellPosition.IsEmpty())
                {
                    CandyBlockSO candyBlock = levelSO.candyBlocksList[UnityEngine.Random.Range(0, levelSO.candyBlocksList.Count)];
                    CandyOnGridCell candyOnGridCell = new CandyOnGridCell(candyBlock,x,y);
                    candyGridCellPosition.SetCandyBlock(candyOnGridCell);
                    
                    OnNewCandyGridSpawned?.Invoke(candyOnGridCell, new OnNewCandyGridSpawnedEventArgs
                    {
                        candyOnGridCell= candyOnGridCell, 
                        candyGridCellPosition = candyGridCellPosition
                    });
                }
            }
        }
    }
    public void FallGemsIntoEmptyPosition()
    { for (int x = 0; x < columns; x++)
        { for (int y = 0; y < rows; y++)
            { 
                CandyGridCellPosition candyGridCellPosition = grid.GetGridObject(x, y);
                if (candyGridCellPosition.HasCandyBlock())
                { for (int i = y-1; i >=0; i--)
                    { CandyGridCellPosition candyGridCellPositionBelow = grid.GetGridObject(x, i);
                        if (candyGridCellPositionBelow.IsEmpty())
                        {   //if the grid cell below is empty then move the candy block to that cell
                            candyGridCellPosition.GetCandyBlock().SetCandyXY(x,i);
                            candyGridCellPositionBelow.SetCandyBlock(candyGridCellPosition.GetCandyBlock());
                            candyGridCellPosition.ClearCandyBlock();

                            candyGridCellPosition = candyGridCellPositionBelow;
                            
                        }
                        else
                        {
                            //if the grid cell below is not empty then break the loop
                            break;
                        }
                    }
                }
            }
        }
    }
    private bool IsValidPosition(int x, int y)
    {
        if (x<0 || x>=columns || y<0 || y>=rows)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    private CandyBlockSO GetCandyBlockSO(int x, int y) 
    {
        if (!IsValidPosition(x, y))
        {
            return null;
        }
        CandyGridCellPosition candyGridCellPosition = grid.GetGridObject(x, y);

        if (candyGridCellPosition.GetCandyBlock() == null)
        {
            return null;
        }
        return candyGridCellPosition.GetCandyBlock().GetCandyBlockSO();
    }
    public List<List<CandyGridCellPosition>> GetAllConnectedGroups()
    {
        List<List<CandyGridCellPosition>> connectedAllSameColorGroups = new List<List<CandyGridCellPosition>>();
        bool [,] visited = new bool[columns,rows];
        for (int x = 0; x < columns; x++)
        { for (int y = 0; y < rows; y++)
            { if (HasAnyConnectedSameColorCandyBlocks(x, y)&& !visited[x,y])
                {
                    List<CandyGridCellPosition> connectedSameColorGroup = new List<CandyGridCellPosition>();
                    connectedSameColorGroup = GetConnectedSameColorCandyBlocks(x, y);
                    foreach (var gridCellPosition in connectedSameColorGroup)
                    {
                        visited[gridCellPosition.GetX(),gridCellPosition.GetY()] = true;
                    }
                    
                    connectedAllSameColorGroups.Add(connectedSameColorGroup);
                }
                
            }
        }

        return connectedAllSameColorGroups;
        
    }
    public List<CandyGridCellPosition> GetConnectedSameColorCandyBlocks(int x, int y)
    {   
        CandyBlockSO candyBlockSO = GetCandyBlockSO(x, y);
        if (candyBlockSO == null) { return null; }
        if (!HasAnyConnectedSameColorCandyBlocks(x,y)) { return null;}
        else
        {
            List<CandyGridCellPosition> connectedSameColorGroup = new List<CandyGridCellPosition>();
            bool[,] visited = new bool[columns, rows];
            visited[x, y] = true;
            connectedSameColorGroup = AdjacentCandyGridCellPositionsSameColor(x, y);
            bool conditionX = false;
            while (!conditionX) 
            { foreach (var gridCellPosition in connectedSameColorGroup)
                { if (!visited[gridCellPosition.GetX(), gridCellPosition.GetY()])
                    {   
                        visited[gridCellPosition.GetX(), gridCellPosition.GetY()] = true;
                        connectedSameColorGroup.AddRange(GetConnectedSameColorCandyBlocks(gridCellPosition.GetX(), gridCellPosition.GetY()));
                    }
                    
                }
            }
            
        }

        return null;

    }
    public List<List<CandyGridCellPosition>> findSameColorBlocksInRow(int y)
    {
        List<List<CandyGridCellPosition>> matches = new List<List<CandyGridCellPosition>>();

        for (int i = 0; y < columns; y++)
        {
            List<CandyGridCellPosition> match = new List<CandyGridCellPosition>();
            CandyBlockSO candyBlockSO = GetCandyBlockSO(i, y);
            if (candyBlockSO == null)
            {
                continue;
            }
            match.Add(grid.GetGridObject(i, y));
            for (int j = i + 1; j < columns; j++)
            {
                if (candyBlockSO == GetCandyBlockSO(j, y))
                {
                    match.Add(grid.GetGridObject(j, y));
                }
                else
                {
                    break;
                }
            }
            
        }

        return matches;
    }

    
    public List<CandyGridCellPosition> AdjacentCandyGridCellPositionsSameColor(int x, int  y )//, //List<CandyGridCellPosition> candyGridCellPositions) 
    {   //this method checks if the candy block is adjacent to another candy block of the same type
        CandyBlockSO candyBlockSO = GetCandyBlockSO(x, y);
        List<CandyGridCellPosition> adjacentCandyBlockList = new List<CandyGridCellPosition>();
        
        if (IsValidPosition(x,y) == false)
        {
            return adjacentCandyBlockList;
        }
        if (IsValidPosition(x, y))
        {   
            adjacentCandyBlockList.Add(grid.GetGridObject(x, y));
            if (IsValidPosition(x, y+1) &&  GetCandyBlockSO(x, y+1) == candyBlockSO)
            {   
                adjacentCandyBlockList.Add(grid.GetGridObject(x, y+1));
            }
            if (IsValidPosition(x, y-1) &&  GetCandyBlockSO(x, y-1) == candyBlockSO)
            {   
                adjacentCandyBlockList.Add(grid.GetGridObject(x, y-1));
            }
            if (IsValidPosition(x+1, y) &&  GetCandyBlockSO(x+1, y) == candyBlockSO)
            {   
                adjacentCandyBlockList.Add(grid.GetGridObject(x+1, y));
            }
            if (IsValidPosition(x-1, y) && GetCandyBlockSO(x-1, y) == candyBlockSO)
            {  
                adjacentCandyBlockList.Add(grid.GetGridObject(x-1, y));
            }
        }
        return adjacentCandyBlockList;
    }
    /*public List<CandyGridCellPosition> GetConnectedSameColorCandyBlocks(int x, int y)
    {   //this method is responsible for finding connected same color candy blocks
        CandyBlockSO candyBlockSO = GetCandyBlockSO(x,y);
        
        List<CandyGridCellPosition> connectedSameColorCandyBlocks = new List<CandyGridCellPosition>();
        connectedSameColorCandyBlocks = AdjacentCandyGridCellPositionsSameColor(x,y,candyBlockSO);
        
        if (candyBlockSO == null)
        {
            
            return null;
        }

        if(AdjacentCandyGridCellPositionsSameColor(x,y,candyBlockSO).Count == 0)
        {
            
            return null;
        }
        else
        {
            return connectedSameColorCandyBlocks;
        }
    }*/
    public void ClearConnectedSameColorCandyBlocks(int x, int y)
    {   //this method is responsible for destroying connected same color candy blocks
        List<CandyGridCellPosition> connectedSameColorCandyBlocks = new List<CandyGridCellPosition>();
        connectedSameColorCandyBlocks = this.AdjacentCandyGridCellPositionsSameColor(x,y);
        
        int connectedSameColorCandyBlockAmount = connectedSameColorCandyBlocks.Count;
        if (connectedSameColorCandyBlockAmount >= 2)
        { foreach (var VARIABLE in connectedSameColorCandyBlocks)
            {   
                VARIABLE.DestroyCandyBlock();
                OnCandyGridPositionDestroyed?.Invoke(VARIABLE, EventArgs.Empty);
                VARIABLE.ClearCandyBlock();
                
            }
            
            /*{   Debug.Log("for loop");
                connectedSameColorCandyBlocks[i].DestroyCandyBlock();
                break;  
            }*/
        }
    }
    public bool HasAnyConnectedSameColorCandyBlocks(int x, int y)
    {   //this method checks if there are any connected same color candy blocks
        int connectedSameColorCandyBlockAmount = 0;
        connectedSameColorCandyBlockAmount = AdjacentCandyGridCellPositionsSameColor(x,y).Count;
        if (connectedSameColorCandyBlockAmount >= 2)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    
    
    
    


       private void Update()
    {
        /*if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 60f;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            grid.GetXY(worldPosition, out int x, out int y);
            Debug.Log($"Mouse Position: {worldPosition} | Grid Position: [{x}, {y}]");
        }*/
    }
}
