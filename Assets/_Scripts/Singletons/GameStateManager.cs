using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
public class GameStateManager : MonoSingleton<GameStateManager>
{   public event EventHandler OnStateChanged;
    
    public enum GameState
    {
        Busy,
        Checking,
        BeforePlayerTurn,
        AfterPlayerTurn,
        GameOver
    }
    private float _busyTimer;
    public Action OnBusyTimerElapsedAction;
    public GameState gameState;
    private bool _isSetup;
    [SerializeField] private GridLogicSystem gridLogicSystem;
    [SerializeField] private GridVisualSystem gridVisualSystem;

    private void Awake()
    {
        gameState = GameState.Busy;
        _isSetup = false;
        
        gridLogicSystem.OnLevelSet += GridLogicSystem_OnLevelSet;
        gridVisualSystem.OnVisualSetupComplete += GridVisualSystem_OnVisualSetupComplete;
    }
    
    //Whenever grid logic system reports that the level is set, we set the state to busy and wait for the visual system to finish setting up the level
    private void GridLogicSystem_OnLevelSet(object sender, GridLogicSystem.OnLevelSetEventArgs e)
    {
        FunctionTimer.Create(() => gridVisualSystem.Setup(sender as GridLogicSystem,e.Grid), .1f); //we set up the grid after 0.1 seconds
    }
    //After the visual system says setup is complete, we can start the game
    private void GridVisualSystem_OnVisualSetupComplete(object sender, EventArgs e)
    {
        _isSetup = true;
        SetBusyState(.1f, () => SetState(GameState.Checking));
    }
    
    //Setting the state of the game after a certain amount of time. This is used to make sure that the player can't click on the grid before the game is ready
    public void SetBusyState(float busyTimer, Action onBusyTimerElapsedAction)
    {
        SetState(GameState.Busy);
        //Debug.Log(onBusyTimerElapsedAction.Method.Name);
        this._busyTimer = busyTimer;
        this.OnBusyTimerElapsedAction = onBusyTimerElapsedAction;
    }
    
    //Setting the state of the game
    public void SetState(GameState state) {
        this.gameState = state;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }
    
    //Responsible for applying the logic of the game
    private void Update()
    {
        if (!_isSetup) return;
        {
            gridVisualSystem.UpdateVisual();
        }
        switch (gameState)
        {
            case GameState.Busy:
                _busyTimer -= Time.deltaTime;
                if (_busyTimer <= 0f)
                {
                    OnBusyTimerElapsedAction();
                }
                break;
            case GameState.Checking:
                List<GridLogicSystem.PossibleMove> allPossibleMoves = gridLogicSystem.GetAllPossibleMoves();
                if (!gridLogicSystem.IsAnyPossibleMoveLeft(allPossibleMoves))
                {
                    gridLogicSystem.DestroyAllCandyBlocks();
                    SetState(GameState.AfterPlayerTurn);
                }
                else
                {
                    SetState(GameState.BeforePlayerTurn);;
                }
                break;
            case GameState.BeforePlayerTurn:
                
                if (Input.GetMouseButtonDown(0))
                {   
                    Vector3 mousePosition = Input.mousePosition;
                    mousePosition.z = 60f;
                    Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
                    gridLogicSystem.Grid.GetXY(worldPosition, out int x, out int y);
                    
                    if (gridLogicSystem.HasAnyConnectedSameColorCandyBlocks(x,y))
                    {   gridLogicSystem.DestroyConnectedSameColorCandyBlocks(x,y);
                        SetBusyState(.1f, () => SetState(GameState.AfterPlayerTurn));
                    }
                }
                break;
            case GameState.AfterPlayerTurn:
                SetBusyState(.2f, () =>
                {
                    gridLogicSystem.FallGemsIntoEmptyPosition();
                    
                    SetBusyState(.2f, () =>
                    {
                        gridLogicSystem.SpawnNewMissingGridPositions();
                        SetBusyState(.2f, () =>
                        {   
                            gridLogicSystem.ChangeAllCandyBlocksState();
                            
                            SetBusyState(.1f, ()=>SetState(GameState.Checking));
                        });
                    });
                });
                
                
                break;
            case GameState.GameOver:
                break;


        }
    }
    
}

