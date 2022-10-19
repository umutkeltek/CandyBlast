using UnityEngine;
using System;


//Represents the candy block itself in the gridCell.
public class CandyOnGridCell
{
    public event EventHandler OnDestroyed;
    public event EventHandler<OnIconLevelChangedEventArgs> OnIconLevelChanged;
    private int _iconLevel;
    private CandyBlockSO _candyBlock;
    private int _x;
    private int _y;
    private bool _isDestroyed;
    
    public CandyOnGridCell(CandyBlockSO candyBlock, int x, int y)
    {
        this._candyBlock = candyBlock;
        this._x = x;
        this._y = y;
        _isDestroyed = false;
        
    }
    
    public class OnIconLevelChangedEventArgs : EventArgs
    {   public CandyOnGridCell CandyOnGridCell { get; set; }
        public CandyBlockSO CandyBlock;
        public int IconLevel;
    }
    
    
    public CandyBlockSO GetCandyBlockSo()
    {
        return _candyBlock;
    }
    
    public Vector3 GetWorldPosition()
    {
        return new Vector3(_x, _y );
    }
    public int GetX()
    {
        return _x;
    }
    public int GetY()
    {
        return _y;
    }
    public void SetCandyXY(int x, int y)
    {
        this._x = x;
        this._y = y;
    }
    public Sprite GetSprite()
    {
        return _iconLevel switch
        {
            0 => _candyBlock.defaultCandySprite,
            1 => _candyBlock.level1CandySprite,
            2 => _candyBlock.level2CandySprite,
            3 => _candyBlock.level3CandySprite,
            _ => _candyBlock.defaultCandySprite
        };
    }
    public void SetIconLevel(int iconLevel)
    {   
        _iconLevel = iconLevel;
        OnIconLevelChanged?.Invoke(this, new OnIconLevelChangedEventArgs()
        {   CandyOnGridCell = this,
            CandyBlock = _candyBlock,
            IconLevel = _iconLevel
        });
    }

    public void Destroy()
    {
        _isDestroyed = true;
        OnDestroyed?.Invoke(this, EventArgs.Empty);
        
    }

    public override string ToString()
    {
        return GetX().ToString() + " " + GetY().ToString();
    }
}
