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
        GameOver,
        Victory
    }
    private float _busyTimer;
    public Action OnBusyTimerElapsedAction;
    public GameState gameState;
    private bool _isSetup;
    [SerializeField] private GridLogicSystem gridLogicSystem;
    [SerializeField] private GridVisualSystem gridVisualSystem;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClips;
    [SerializeField] private Transform _camera;

    private void Awake()
    {   
        gameState = GameState.Busy;
        _isSetup = false;
        LevelManager.Instance.OnWin += LevelManager_OnWin;
        LevelManager.Instance.OnLose += LevelManager_OnLose;
        gridLogicSystem.OnLevelSet += GridLogicSystem_OnLevelSet;
        gridVisualSystem.OnVisualSetupComplete += GridVisualSystem_OnVisualSetupComplete;
    }
    private void LevelManager_OnWin(object sender, EventArgs e)
    {   
        SetBusyState(0.1f,() => SetState(GameState.Victory));
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }
    private void LevelManager_OnLose(object sender, EventArgs e)
    {
        SetBusyState(0.1f,() => SetState(GameState.GameOver));
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }
    
    //Whenever grid logic system reports that the level is set, we set the state to busy and wait for the visual system to finish setting up the level
    private void GridLogicSystem_OnLevelSet(object sender, GridLogicSystem.OnLevelSetEventArgs e)
    {   CameraManager.Instance.SetCameraOrthoSize(gridLogicSystem.Grid, _camera);
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
                    List<CandyGridCellPosition> dummyConnectedSameColorCandyBlocks = gridLogicSystem.GetConnectedSameColorCandyBlocks(x, y);
                    if (gridLogicSystem.HasAnyConnectedSameColorCandyBlocks(x,y))
                    {   int count = dummyConnectedSameColorCandyBlocks.Count;
                        switch (count)
                        {
                            case 1:
                                audioSource.PlayOneShot(audioClips[0]);
                                break;
                            case 2:
                                audioSource.PlayOneShot(audioClips[1]);
                                break;
                            case 3:
                                audioSource.PlayOneShot(audioClips[2]);
                                break;
                            case 4:
                                audioSource.PlayOneShot(audioClips[3]);
                                break;
                            case 5:
                                audioSource.PlayOneShot(audioClips[4]);
                                break;
                            case 6:
                                audioSource.PlayOneShot(audioClips[5]);
                                break;
                            case 7:
                                audioSource.PlayOneShot(audioClips[5]);
                                break;
                            case 8:
                                audioSource.PlayOneShot(audioClips[5]);
                                break;
                            case 9:
                                audioSource.PlayOneShot(audioClips[5]);
                                break;
                            case 10:
                                audioSource.PlayOneShot(audioClips[5]);
                                break;
                            
                            default:
                                audioSource.PlayOneShot(audioClips[0]);
                                break;
                        }
                        
                        gridLogicSystem.DestroyConnectedSameColorCandyBlocks(dummyConnectedSameColorCandyBlocks);
                        SetBusyState(.1f, () => SetState(GameState.AfterPlayerTurn));
                    }
                }
                break;
            case GameState.AfterPlayerTurn:
                SetBusyState(.1f, () =>
                {
                    gridLogicSystem.FallGemsIntoEmptyPosition();
                    
                    SetBusyState(.1f, () =>
                    {
                        gridLogicSystem.SpawnNewMissingGridPositions();
                        SetBusyState(.1f, () =>
                        {   
                            gridLogicSystem.ChangeAllCandyBlocksState();
                            
                            SetBusyState(.1f, () =>
                            {
                                if (LevelManager.Instance.WinConditionCheck())
                                {
                                    LevelManager.Instance.TryIsGameOver();
                                    
                                }
                                else if (!LevelManager.Instance.HasMoveAvailable())
                                {
                                    LevelManager.Instance.TryIsGameOver();
                                }
                                else
                                {
                                    SetBusyState(.1f, () => SetState(GameState.Checking));
                                }
                                
                            });
                        });
                    });
                });
                break;
            case GameState.GameOver:
                Debug.Log("Game Over");
                break;
            case GameState.Victory:
                Debug.Log("Victory");
                break;


        }
    }
    
}

