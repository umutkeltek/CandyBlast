using System;
using System.Collections;
using System.Collections.Generic;
using CodeMonkey.Utils;
using UnityEngine;

public class GridXY<TGridObject> // Generic class to create grid in XY axis
{   public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged; // Event to notify when a TGridObject is changed
    public class OnGridObjectChangedEventArgs : EventArgs { // When a TGridObject is changed, this event is called to pass x and y.
        public int x;
        public int y;
    }
    private int columns; // Number of columns in grid
    private int rows; // Number of rows in grid
    private float cellSize; // Size of each cell
    private Vector3 originPosition; // Origin position of grid
    private TGridObject[,] gridArray; // Array of TGridObject which stores the values of the grid cell
    
    public GridXY(int columns, int rows, float cellSize, Vector3 originPosition, Func<GridXY<TGridObject>, int, int, TGridObject> createGridObject) 
    {   
        this.columns = columns;
        this.rows = rows;
        this.cellSize = cellSize;
        this.originPosition = originPosition;
        
        gridArray = new TGridObject[columns, rows]; // Create a new array of TGridObject with the size of columns and rows
        
        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                gridArray[x, y] = createGridObject(this, x, y); // Create a new TGridObject in each cell of the grid
                
            }
        }
        
        bool showDebug = true;
        if (showDebug)
        {
            TextMesh[,] debugTextArray = new TextMesh[columns, rows];
            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < gridArray.GetLength(1); y++)
                {
                    debugTextArray[x, y] = UtilsClass.CreateWorldText(gridArray[x, y]?.ToString(), null, GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * .5f, 20, Color.white, TextAnchor.MiddleCenter);
                    debugTextArray[x, y].transform.localScale = new Vector3(0.2f,0.2f);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
                }
            }
            Debug.DrawLine(GetWorldPosition(0, rows), GetWorldPosition(columns, rows), Color.white, 100f);
            Debug.DrawLine(GetWorldPosition(columns, 0), GetWorldPosition(columns, rows), Color.white, 100f);
            
            OnGridObjectChanged  += (object sender, OnGridObjectChangedEventArgs  eventArgs) =>
            {
                debugTextArray[eventArgs.x, eventArgs.y].text = gridArray[eventArgs.x, eventArgs.y]?.ToString();
            };
        }
    }
    public static List<TGridObject> GridObjects {get; set;}
    
    public int GetColumnsCount() // Returns the number of columns in the grid
    {
        return columns;
    }
    public int GetRowsCount() // Returns the number of rows in the grid
    {
        return rows;
    }
    public float GetCellSize() // Returns the size of each cell in the grid
    {
        return cellSize;
    }
    
    public Vector3 GetWorldPosition(int x, int y) // Returns the world position of a cell in the grid
    {
        return new Vector3(x, y) * cellSize + originPosition;
    }
    public Vector3 GetWorldPositionCenterOfGrid(int x, int y) // Returns the world position of the center of a cell in the grid
    {
        return new Vector3(x, y) * cellSize + originPosition + new Vector3(cellSize, cellSize) * .5f;
    }
    
    public void GetXY(Vector3 worldPosition, out int x, out int y) // GetXY is a function that returns the grid position of a world position
    {
        /*if (Mathf.FloorToInt((worldPosition - originPosition).x / cellSize)<0 || Mathf.FloorToInt((worldPosition - originPosition).y / cellSize)<0 || Mathf.FloorToInt((worldPosition - originPosition).x / cellSize)>=columns || Mathf.FloorToInt((worldPosition - originPosition).y / cellSize)>=rows)
        {
            x = -1;
            y = -1;
            
        }*/
        /*else*/
        {
            x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
            y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);
        }
        
    }
    
    
    public void SetGridObject(int x, int y, TGridObject value) { //setting grid object based on grid coordinates
        if (x >= 0 && y >= 0 && x < columns && y < rows) { // if the x and y coordinates are within the grid. It checks wrong values
            gridArray[x, y] = value;
            TriggerGridObjectChanged(x, y);
        }
    }
    public void SetGridObject(Vector3 worldPosition, TGridObject value) {
        GetXY(worldPosition, out int x, out int y); //get the grid coodinates of the world position and store them in x and y
        SetGridObject(x, y, value); // set the desired object at the grid coordinates
    }
    
    public TGridObject GetGridObject(int x, int y) { //return the grid object at the grid coordinates
        if (x >= 0 && y >= 0 && x < columns && y < rows) {
            return gridArray[x, y];
        } else {
            return default(TGridObject);
        }
    }
    public TGridObject GetGridObject(Vector3 worldPosition) {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetGridObject(x, y);
    }
    
    /*public void TriggerGridObjectChanged(int x, int y) {
        if (OnGridObjectChanged != null) OnGridObjectChanged(this, new OnGridObjectChangedEventArgs { x = x, y = y });
    }*/
    
    
    public void TriggerGridObjectChanged(int x, int y) { // trigger the event when the grid object is changed 
        OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, y = y });
    }
    
    public Vector2Int ValidateGridPosition(Vector2Int gridPosition) { 
        return new Vector2Int(
            Mathf.Clamp(gridPosition.x, 0, columns - 1),
            Mathf.Clamp(gridPosition.y, 0, rows - 1)
        );
    }
}