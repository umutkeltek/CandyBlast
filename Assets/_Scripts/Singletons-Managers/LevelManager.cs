using System;
using UnityEngine;

//This script is responsible for scoring and ending the game
public class LevelManager : MonoSingleton<LevelManager>
{

    [SerializeField] GridLogicSystem gridLogicSystem;


    private int _score;
    private int _moveCount;
    private int _targetScore;
    private int _glassCount;
    private LevelSO.WinCondition _winCondition;
    public event EventHandler OnWin;
    public event EventHandler OnLose;

    private void Awake()
    {   gridLogicSystem.OnLevelSet += OnLevelSetup;
    }
    private void OnLevelSetup(object sender, GridLogicSystem.OnLevelSetEventArgs e)
    {
        _winCondition = gridLogicSystem.GetLevelSo().winCondition;
        _moveCount = gridLogicSystem.GetLevelSo().moveAmount;
        _targetScore = gridLogicSystem.GetLevelSo().targetScore;
        _glassCount = GetGlassAmount(gridLogicSystem.Grid);
        
        if (_winCondition == LevelSO.WinCondition.ReachSpecificScore)
        {   
            gridLogicSystem.OnScoreChanged += OnScoreChanged;
        }

        else if (_winCondition == LevelSO.WinCondition.RemoveAllGlassBlocks)
        {
            gridLogicSystem.OnGlassDestroyed += OnGlassDestroyed;
        }
        
        gridLogicSystem.OnMoveUsed += OnMoveUsed;
    }
    private void OnGlassDestroyed(object sender, EventArgs e)
    {   
        _glassCount--;
    }
    private void OnScoreChanged(object sender, GridLogicSystem.OnScoreChangedEventArgs e)
    {
        _score += e.score;
    }

    private void OnMoveUsed(object sender, EventArgs e)
    {   Debug.Log("Move Used");
        _moveCount--;
    }
    
    public bool WinConditionCheck()
    {   if (_winCondition == LevelSO.WinCondition.ReachSpecificScore)
        {
            if (_score >= _targetScore)
            {
                return true;
            }
        }
        else if (_winCondition == LevelSO.WinCondition.RemoveAllGlassBlocks)
        {
            if (_glassCount <= 0)
            {
                return true;
            }
        }
        return false;
    }

    public void TryIsGameOver()
    {
        if (WinConditionCheck())
        {   Debug.Log("win");
            OnWin?.Invoke(this, EventArgs.Empty);
        }
        else if (!HasMoveAvailable())
        {   Debug.Log("Lose");
            OnLose?.Invoke(this, EventArgs.Empty);
        }
        /*if (!HasMoveAvailable()) 
        {   
            switch (_winCondition) 
            {
                case LevelSO.WinCondition.ReachSpecificScore:
                    if (_score >= _targetScore) {
                        // Reached Target Score!
                        OnWin?.Invoke(this, EventArgs.Empty);
                        return true;
                    }
                    break;
                break;
                case LevelSO.WinCondition.RemoveAllGlassBlocks:
                    if (_glassCount <= 0) {
                        // All glass destroyed!
                        OnWin?.Invoke(this, EventArgs.Empty);
                        return true;
                    }
                    break;
                break;
            }
            OnLose?.Invoke(this, EventArgs.Empty);
            // No more moves, game over!
            return true;
        }
        // Not game over
        return false;
    }*/
    }

    //returns the amount of glass blocks in the grid
    public int GetGlassAmount(GridXY<CandyGridCellPosition> candyGrid) {
        int glassAmount = 0;
        for (int x = 0; x < candyGrid.GetColumnsCount(); x++) {
            for (int y = 0; y < candyGrid.GetRowsCount(); y++) {
                CandyGridCellPosition candyGridCellPosition = candyGrid.GetGridObject(x, y);
                if (candyGridCellPosition.HasGlass()) {
                    glassAmount++;
                }
            }
        }
        return glassAmount;
    } //returns the amount of glass blocks in the grid
    public bool HasMoveAvailable() {
        return _moveCount > 0;
    } //returns true if there are moves available
    public int GetScore() {
        return _score;
    } //returns the score
    public int GetMoveCount() {
        return _moveCount;
    } //returns the move count
    public int GetUsedMoveCount() {
        return gridLogicSystem.GetLevelSo().moveAmount - _moveCount;
    } //returns the used move count
    public void UseMove() 
    { _moveCount--;
    } //decreases the move count by 1

}
