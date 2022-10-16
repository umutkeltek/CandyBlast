using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


//Represents the candy block itself in the gridCell.
public class CandyOnGridCell
{
    public event EventHandler OnDestroyed;
    private CandyBlockSO candyBlock;
    private int x;
    private int y;
    private bool isDestroyed;
    
    public CandyOnGridCell(CandyBlockSO candyBlock, int x, int y)
    {
        this.candyBlock = candyBlock;
        this.x = x;
        this.y = y;
        isDestroyed = false;
        
    }
    
    public CandyBlockSO GetCandyBlockSO()
    {
        return candyBlock;
    }
    
    public Vector3 GetWorldPosition()
    {
        return new Vector3(x, y );
    }
    public int GetX()
    {
        return x;
    }
    public int GetY()
    {
        return y;
    }
    public void SetCandyXY(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public void Destroy()
    {
        isDestroyed = true;
        OnDestroyed?.Invoke(this, EventArgs.Empty);
        
    }

    public override string ToString()
    {
        return isDestroyed.ToString();
    }
}
