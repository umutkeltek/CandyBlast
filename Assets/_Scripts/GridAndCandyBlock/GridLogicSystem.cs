using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CodeMonkey.Utils;
using UnityEditor.SceneManagement;
using UnityEngine;
using Random = UnityEngine.Random;


public class GridLogicSystem : MonoBehaviour
{   public event EventHandler OnCandyGridPositionDestroyed; //event for when a candy is destroyed
    public event EventHandler<OnNewCandyGridSpawnedEventArgs> OnNewCandyGridSpawned; //event for when a new candy is spawned
    public event EventHandler<OnLevelSetEventArgs> OnLevelSet; //event for when a level is set
    
    public event EventHandler<OnChangeIconLevelEventArgs> OnChangeIconLevel; //event for when the icon level is changed === TODO: make this work
    public event EventHandler OnGlassDestroyed; 
    public event EventHandler OnMoveUsed; 
    public event EventHandler OnOutOfMoves;
    public event EventHandler OnScoreChanged;
    public event EventHandler OnWin;
    public class OnChangeIconLevelEventArgs : EventArgs
    {   
        public int iconLevel;
    }
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
    [Header("Conditions for changing icons")]
    [SerializeField] private int level1Icons;
    [SerializeField] private int level2Icons;
    [SerializeField] private int level3Icons;
    
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
    public List<List<CandyGridCellPosition>> GetAllConnectedGroups()
    {
        List<List<CandyGridCellPosition>> connectedAllSameColorGroups = new List<List<CandyGridCellPosition>>();
        bool[,] visited = new bool[columns, rows];
        bool[,] isGroup = new bool[columns, rows];
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {   
                if (isGroup[x,y] == false)
                {
                    List<CandyGridCellPosition> candyGridCellPositions = GetConnectedSameColorCandyBlocks(x, y,ref visited);
                    
                    if (candyGridCellPositions != null )//&& candyGridCellPositions.Count >= 2)
                    {   
                        connectedAllSameColorGroups.Add(candyGridCellPositions);
                        foreach (var candyGridCellPosition in candyGridCellPositions)
                        {
                            isGroup[candyGridCellPosition.GetX(),candyGridCellPosition.GetY()] = true;
                        }
                        
                    }
                }
                
            }
            
        }
    
        //List<List<CandyGridCellPosition>> finalList = connectedAllSameColorGroups.Distinct().ToList();
        Debug.Log(connectedAllSameColorGroups.Count);
        return connectedAllSameColorGroups;
        
    }
    public bool IsAnyConnectedGroupAvailable()
    {
        return GetAllConnectedGroups().Count > 0;
    }
    public bool IsAnyConnectedGroupAvailable(List<List<CandyGridCellPosition>> connectedAllSameColorGroups)
    {
        return connectedAllSameColorGroups.Count > 0;
    }
    public void ChangeIconBasedOnSameColorConnectedGroups()
    {   List<CandyGridCellPosition> notConnectedCandyGridCellPositions = findNotConnectedCandyBlocks();
        List<List<CandyGridCellPosition>> connectedAllSameColorGroups = GetAllConnectedGroups();
        

        foreach (var candyGridCellPosition in notConnectedCandyGridCellPositions)
        {   
            candyGridCellPosition.GetCandyBlock().SetIconLevel(0);
        }
        foreach (List<CandyGridCellPosition> connectedSameColorGroup in connectedAllSameColorGroups)
        {   
            if (connectedSameColorGroup.Count<level1Icons)
            {
                foreach (CandyGridCellPosition candyGridCellPosition in connectedSameColorGroup)
                {   
                    //Debug.Log(candyGridCellPosition.GetX() + candyGridCellPosition.GetY() + candyGridCellPosition.GetCandyBlock().GetSprite().name + connectedSameColorGroup.Count);
                    candyGridCellPosition.GetCandyBlock().SetIconLevel(0);
                    //Debug.Log("bosluk "+ candyGridCellPosition.GetCandyBlock().GetX() + candyGridCellPosition.GetCandyBlock().GetY() + candyGridCellPosition.GetCandyBlock().GetSprite().name + connectedSameColorGroup.Count);
                }
                
            }
            
            if (connectedSameColorGroup.Count>=level1Icons && connectedSameColorGroup.Count<level2Icons)
            {
                foreach (CandyGridCellPosition candyGridCellPosition in connectedSameColorGroup)
                {   
                    //Debug.Log(candyGridCellPosition.GetX() + candyGridCellPosition.GetY() + candyGridCellPosition.GetCandyBlock().GetSprite().name + connectedSameColorGroup.Count);
                    candyGridCellPosition.GetCandyBlock().SetIconLevel(1);
                    //Debug.Log(candyGridCellPosition.GetX() + candyGridCellPosition.GetY() + candyGridCellPosition.GetCandyBlock().GetSprite().name);
                }
            }
            if (connectedSameColorGroup.Count >= level2Icons && connectedSameColorGroup.Count < level3Icons)
            {
                foreach (CandyGridCellPosition candyGridCellPosition in connectedSameColorGroup)
                {   //Debug.Log(candyGridCellPosition.GetX() + candyGridCellPosition.GetY() + candyGridCellPosition.GetCandyBlock().GetSprite().name+ connectedSameColorGroup.Count);
                    candyGridCellPosition.GetCandyBlock().SetIconLevel(2);
                    //Debug.Log(candyGridCellPosition.GetX() + candyGridCellPosition.GetY() + candyGridCellPosition.GetCandyBlock().GetSprite().name);
                }
            }
           if (connectedSameColorGroup.Count >= level3Icons)
            {
                foreach (CandyGridCellPosition candyGridCellPosition in connectedSameColorGroup)
                {   //Debug.Log(candyGridCellPosition.GetX() + candyGridCellPosition.GetY() + candyGridCellPosition.GetCandyBlock().GetSprite().name+ connectedSameColorGroup.Count);
                    candyGridCellPosition.GetCandyBlock().SetIconLevel(3);
                    //Debug.Log(candyGridCellPosition.GetX() + candyGridCellPosition.GetY() + candyGridCellPosition.GetCandyBlock().GetSprite().name);
                }
            }
        }
    }
    public List<CandyGridCellPosition> findNotConnectedCandyBlocks()
    {
        List<CandyGridCellPosition> notConnectedCandyBlocks = new List<CandyGridCellPosition>();
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                CandyGridCellPosition candyGridCellPosition = grid.GetGridObject(x, y);
                if (candyGridCellPosition.HasCandyBlock())
                {
                    if (AdjacentGridCellsWithSameColorIncludingItself(x,y).Count==0)
                    {
                        notConnectedCandyBlocks.Add(candyGridCellPosition);
                    }
                }
            }
        }

        List<CandyGridCellPosition> finalList = notConnectedCandyBlocks.Distinct().ToList();
        return finalList;
    }
    public void Shuffle()
    {
        if (!IsAnyConnectedGroupAvailable())
        {
            DestroyAllCandyBlocks();
        }
    }
    public List<CandyGridCellPosition> GetConnectedSameColorCandyBlocks(int x, int y,ref bool[,] visited)
    {   List<CandyGridCellPosition> connectedSameColorGroup = new List<CandyGridCellPosition>();
        CandyBlockSO candyBlockSO = GetCandyBlockSO(x, y);
        if (candyBlockSO == null) { return null; }
        if (!HasAnyConnectedSameColorCandyBlocks(x,y)) { return null;}
        else
        {
            
            AdjacentCandyGridCellPositionsSameColor(x, y, ref connectedSameColorGroup, ref visited, candyBlockSO);
            
            
        }
        List<CandyGridCellPosition> finalList = connectedSameColorGroup.Distinct().ToList();
        return finalList;

    }
    public List<CandyGridCellPosition> GetConnectedSameColorCandyBlocks(int x, int y)
    {   List<CandyGridCellPosition> connectedSameColorGroup = new List<CandyGridCellPosition>();
        CandyBlockSO candyBlockSO = GetCandyBlockSO(x, y);
        if (candyBlockSO == null) { return null; }
        if (!HasAnyConnectedSameColorCandyBlocks(x,y)) { return null;}
        else
        {
            bool[,] visited = new bool[columns, rows];
            AdjacentCandyGridCellPositionsSameColor(x, y, ref connectedSameColorGroup, ref visited,candyBlockSO);
            
        }

        List<CandyGridCellPosition> finalList = connectedSameColorGroup.Distinct().ToList();
        return finalList;

    }
    public List<CandyGridCellPosition> AdjacentCandyGridCellPositionsSameColor(int x, int  y, ref List<CandyGridCellPosition> candyGridCellPositions,ref bool[,] visited) 
    {   //this method checks if the candy block is adjacent to another candy block of the same type
        CandyBlockSO candyBlockSO = GetCandyBlockSO(x, y);
        //List<CandyGridCellPosition> adjacentCandyBlockList = new List<CandyGridCellPosition>();
        
        if (IsValidPosition(x,y) == false)
        {
            return null;
        }
        if (IsValidPosition(x, y))//&& visited[x,y] ==false )
        {   visited[x, y] = true;
            //candyGridCellPositions.Add(grid.GetGridObject(x, y));
            if (IsValidPosition(x,y+1) && visited[x,y+1]==false)
            {
                visited[x, y + 1] = true;
                if (GetCandyBlockSO(x,y+1) == candyBlockSO)
                {
                    candyGridCellPositions.Add(grid.GetGridObject(x, y+1));
                    //Debug.Log(grid.GetGridObject(x, y+1).GetX() +" bosluk "+ grid.GetGridObject(x, y+1).GetY() +" bosluk "+ grid.GetGridObject(x, y+1).GetCandyBlock().GetCandyBlockSO().candyName);
                    AdjacentCandyGridCellPositionsSameColor(x, y+1, ref candyGridCellPositions, ref visited);
                }
            }
            if (IsValidPosition(x+1,y) && visited[x+1,y]==false)
            {
                visited[x+1, y] = true;
                if (GetCandyBlockSO(x+1,y) == candyBlockSO)
                {
                    candyGridCellPositions.Add(grid.GetGridObject(x+1, y));
                    AdjacentCandyGridCellPositionsSameColor(x+1, y, ref candyGridCellPositions, ref visited);
                }
            }
            if (IsValidPosition(x,y-1) && visited[x,y-1]==false)
            {
                visited[x, y - 1] = true;
                if (GetCandyBlockSO(x,y-1) == candyBlockSO)
                {
                    candyGridCellPositions.Add(grid.GetGridObject(x, y-1));
                    AdjacentCandyGridCellPositionsSameColor(x, y-1, ref candyGridCellPositions, ref visited);
                }
            }
            
            if (IsValidPosition(x-1,y) && visited[x-1,y]==false)
            {
                visited[x-1, y] = true;
                if (GetCandyBlockSO(x-1,y) == candyBlockSO)
                {
                    candyGridCellPositions.Add(grid.GetGridObject(x-1, y));
                    AdjacentCandyGridCellPositionsSameColor(x-1, y, ref candyGridCellPositions, ref visited);
                }
            }
            
        }
        List<CandyGridCellPosition> distinctCandyGridCellPositions = candyGridCellPositions.Distinct().ToList();
        //Debug.Log(candyGridCellPositions.Count + " candyGridCellPositions.Count");
        return distinctCandyGridCellPositions;
    }
    public List<CandyGridCellPosition> AdjacentCandyGridCellPositionsSameColor(int x, int  y, ref List<CandyGridCellPosition> candyGridCellPositions,ref bool[,] visited,CandyBlockSO candyBlockSO) 
    {   //this method checks if the candy block is adjacent to another candy block of the same type
        
        //List<CandyGridCellPosition> adjacentCandyBlockList = new List<CandyGridCellPosition>();
        
        if (IsValidPosition(x,y) == false)
        {
            return null;
        }
        if (IsValidPosition(x, y))//&& visited[x,y] ==false)
        {   visited[x, y] = true;
            candyGridCellPositions.Add(grid.GetGridObject(x, y));
            if (IsValidPosition(x,y+1) && visited[x,y+1]==false)
            {
                visited[x, y + 1] = true;
                if (GetCandyBlockSO(x,y+1) == candyBlockSO)
                {   //Debug.Log(grid.GetGridObject(x,y-1));
                    candyGridCellPositions.Add(grid.GetGridObject(x, y+1));
                    //Debug.Log(grid.GetGridObject(x, y+1).GetX() + grid.GetGridObject(x, y+1).GetY() + grid.GetGridObject(x, y+1).GetCandyBlock().GetCandyBlockSO().candyName);
                    AdjacentCandyGridCellPositionsSameColor(x, y+1, ref candyGridCellPositions, ref visited,candyBlockSO);
                }
            }
            if (IsValidPosition(x+1,y) && visited[x+1,y]==false)
            {
                visited[x+1, y] = true;
                if (GetCandyBlockSO(x+1,y) == candyBlockSO)
                {   //Debug.Log(grid.GetGridObject(x,y-1));
                    candyGridCellPositions.Add(grid.GetGridObject(x+1, y));
                    AdjacentCandyGridCellPositionsSameColor(x+1, y, ref candyGridCellPositions, ref visited,candyBlockSO);
                }
            }
            if (IsValidPosition(x,y-1) && visited[x,y-1]==false)
            {
                visited[x, y - 1] = true;
                if (GetCandyBlockSO(x,y-1) == candyBlockSO)
                {
                    candyGridCellPositions.Add(grid.GetGridObject(x, y-1));
                    //Debug.Log(grid.GetGridObject(x,y-1));
                    AdjacentCandyGridCellPositionsSameColor(x, y-1, ref candyGridCellPositions, ref visited,candyBlockSO);
                }
            }
            
            if (IsValidPosition(x-1,y) && visited[x-1,y]==false)
            {
                visited[x-1, y] = true;
                if (GetCandyBlockSO(x-1,y) == candyBlockSO)
                {   //Debug.Log(grid.GetGridObject(x,y-1));
                    candyGridCellPositions.Add(grid.GetGridObject(x-1, y));
                    AdjacentCandyGridCellPositionsSameColor(x-1, y, ref candyGridCellPositions, ref visited,candyBlockSO);
                }
            }
            
        }

        List<CandyGridCellPosition> finalList = candyGridCellPositions.Distinct().ToList();
        //Debug.Log(a.Count + " candyGridCellPositions.Count");
        
        return finalList;
    }
    public void DestroyConnectedSameColorCandyBlocks(int x, int y) //this method is responsible for destroying connected same color candy blocks
    {   
        List<CandyGridCellPosition> connectedSameColorCandyBlocks = new List<CandyGridCellPosition>();
        connectedSameColorCandyBlocks = GetConnectedSameColorCandyBlocks(x,y);
        int connectedSameColorCandyBlockAmount = connectedSameColorCandyBlocks.Count;
        if (connectedSameColorCandyBlockAmount >= 2)
        {   //Debug.Log("connectedSameColorCandyBlockAmount: " + connectedSameColorCandyBlockAmount);
            foreach (var candyGridCellPosition in connectedSameColorCandyBlocks)
            {   //Debug.Log("candyGridCellPosition: " + candyGridCellPosition.GetX() + candyGridCellPosition.GetY());
                candyGridCellPosition.DestroyCandyBlock();
                candyGridCellPosition.DestroyGlass();
                OnGlassDestroyed?.Invoke(this, EventArgs.Empty);
                OnCandyGridPositionDestroyed?.Invoke(candyGridCellPosition, EventArgs.Empty);
                candyGridCellPosition.ClearCandyBlock();
                
            }
        }
    }
    public void DestroyAllCandyBlocks() //this method is responsible for destroying all candy blocks
    {   
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                CandyGridCellPosition candyGridCellPosition = grid.GetGridObject(x, y);
                candyGridCellPosition.DestroyCandyBlock();
                OnCandyGridPositionDestroyed?.Invoke(candyGridCellPosition, EventArgs.Empty);
                candyGridCellPosition.ClearCandyBlock();
                
            }
        }
    }
    
    //This method returns a list of CandyGridCellPositions that are connected to each other and have the same color and it include the CandyGridCellPosition that is passed as a parameter
    public List<CandyGridCellPosition> AdjacentGridCellsWithSameColorIncludingItself(int x, int  y )
    {   
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
    
    //This method returns a list of CandyGridCellPositions that are connected to each other and have the same color but it does not include the CandyGridCellPosition that is passed as a parameter
    public List<CandyGridCellPosition> AdjacentGridCellsWithSameColorExcludingItself(int x, int  y )
    {   
        CandyBlockSO candyBlockSO = GetCandyBlockSO(x, y);
        List<CandyGridCellPosition> adjacentCandyBlockList = new List<CandyGridCellPosition>();
        
        if (IsValidPosition(x,y) == false)
        {
            return adjacentCandyBlockList;
        }
        if (IsValidPosition(x, y))
        {   
            
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
    
    //This method returns true value if the CandyGridCellPosition x,y that is passed as a parameter is connected to another CandyGridCellPosition with the same color
    public bool HasAnyConnectedSameColorCandyBlocks(int x, int y)
    {   
        int connectedSameColorCandyBlockAmount = 0;
        connectedSameColorCandyBlockAmount = AdjacentGridCellsWithSameColorExcludingItself(x,y).Count;
        if (connectedSameColorCandyBlockAmount >= 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    } 
    
    //This method returns CandyBlockSO of the CandyGridCellPosition x,y that is passed as a parameter
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
}
