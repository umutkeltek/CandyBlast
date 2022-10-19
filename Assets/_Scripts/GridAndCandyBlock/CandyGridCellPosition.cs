
using UnityEngine;
using System;

//Represents a single tile in the grid. It just stores the position and the type of tile it is.
public class CandyGridCellPosition 
{   
    public event EventHandler OnGlassDestroyed;
    
    private CandyOnGridCell _candyBlock;
    private GridXY<CandyGridCellPosition> _grid;
    private int _x;
    private int _y;
    private bool _hasGlass;
    
    
    public CandyGridCellPosition(GridXY<CandyGridCellPosition> grid, int x, int y)
    {
        this._grid = grid;
        this._x = x;
        this._y = y;
    }
    
    public int GetX()
    {
        return _x;
    }
    public int GetY()
    {
        return _y;
    }
    
    public void SetCandyBlock(CandyOnGridCell candyBlock)
    {
        this._candyBlock = candyBlock;
        _grid.TriggerGridObjectChanged(_x,_y);
    }
    
    public CandyOnGridCell GetCandyBlock()
    {
        return _candyBlock;
    }
    public void ClearCandyBlock()
    {
        _candyBlock = null;
    }
    public void DestroyCandyBlock()
    {
        _candyBlock?.Destroy();
        _grid.TriggerGridObjectChanged(_x,_y);
    }
    public bool HasCandyBlock()
    {
        return _candyBlock != null;
    }
    public bool IsEmpty()
    {
        return _candyBlock == null;
    }
    public bool HasGlass()
    {
        return _hasGlass;
    }
    public void SetHasGlass(bool hasGlass)
    {
        this._hasGlass = hasGlass;
    }
    public void DestroyGlass()
    {
        SetHasGlass(false);
        OnGlassDestroyed?.Invoke(this, EventArgs.Empty);
    }

    public Vector3 GetWorldPosition()
    {
        return _grid.GetWorldPosition(_x, _y);
    }
    
    public override string ToString()
    {
        return _candyBlock?.ToString() ?? "Empty";
    } 
    
    
    
    
}
