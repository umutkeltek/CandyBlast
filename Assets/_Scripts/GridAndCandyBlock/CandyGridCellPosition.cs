using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//Represents a single tile in the grid. It just stores the position and the type of tile it is.
public class CandyGridCellPosition 
{   
    public event EventHandler OnGlassDestroyed;
    
    private CandyOnGridCell candyBlock;
    private GridXY<CandyGridCellPosition> grid;
    private int x;
    private int y;
    private bool hasGlass;
    
    
    public CandyGridCellPosition(GridXY<CandyGridCellPosition> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
    }
    
    public int GetX()
    {
        return x;
    }
    public int GetY()
    {
        return y;
    }
    
    public void SetCandyBlock(CandyOnGridCell candyBlock)
    {
        this.candyBlock = candyBlock;
        grid.TriggerGridObjectChanged(x,y);
    }
    
    public CandyOnGridCell GetCandyBlock()
    {
        return candyBlock;
    }
    public void ClearCandyBlock()
    {
        candyBlock = null;
    }
    public void DestroyCandyBlock()
    {
        candyBlock?.Destroy();
        grid.TriggerGridObjectChanged(x,y);
    }
    public bool HasCandyBlock()
    {
        return candyBlock != null;
    }
    public bool IsEmpty()
    {
        return candyBlock == null;
    }
    public bool HasGlass()
    {
        return hasGlass;
    }
    public void SetHasGlass(bool hasGlass)
    {
        this.hasGlass = hasGlass;
    }
    public void DestroyGlass()
    {
        SetHasGlass(false);
        OnGlassDestroyed?.Invoke(this, EventArgs.Empty);
    }

    public Vector3 GetWorldPosition()
    {
        return grid.GetWorldPosition(x, y);
    }
    
    public override string ToString()
    {
        return candyBlock?.ToString();
    } 
    
    
    
    
}
