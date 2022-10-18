using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


//Represents the candy block itself in the gridCell.
public class CandyOnGridCell
{
    public event EventHandler OnDestroyed;
    public event EventHandler<OnIconLevelChangedEventArgs> OnIconLevelChanged;
    private int iconLevel;
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
    
    public class OnIconLevelChangedEventArgs : EventArgs
    {   public CandyOnGridCell candyOnGridCell { get; set; }
        public CandyBlockSO candyBlock;
        public int iconLevel;
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
    public Sprite GetSprite()
    {   
        switch (iconLevel)
        {
            case 0:
                return candyBlock.defaultCandySprite;
                break;
            case 1:
                return candyBlock.level1CandySprite;
                break;
            case 2:
                return candyBlock.level2CandySprite;
                break;
            case 3:
                return candyBlock.level3CandySprite;
                break;
            default:
                return candyBlock.defaultCandySprite;
        }
        
    }
    public void SetIconLevel(int IconLevel)
    {   
        iconLevel = IconLevel;
        OnIconLevelChanged?.Invoke(this, new OnIconLevelChangedEventArgs()
        {   candyOnGridCell = this,
            candyBlock = candyBlock,
            iconLevel = iconLevel
        });
    }

    public void Destroy()
    {
        isDestroyed = true;
        OnDestroyed?.Invoke(this, EventArgs.Empty);
        
    }

    public override string ToString()
    {
        return GetX().ToString() + " " + GetY().ToString();
    }
}
