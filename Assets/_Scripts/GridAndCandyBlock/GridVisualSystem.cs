using System;
using System.Collections.Generic;
using CodeMonkey.Utils;
using UnityEngine;

//Visual Representation of the grid logic
public class GridVisualSystem : MonoBehaviour
{   public event EventHandler OnStateChanged;
    public enum State
    {
        Busy,
        BeforePlayerTurn,
        AfterPlayerTurn,
        GameOver
    }
    [SerializeField] private Transform pfCandyGridVisual;
    [SerializeField] private Transform pfGlassGridVisual;
    [SerializeField] private Transform pfBackgroundGridVisual; //it will be used if it is wanted to add a background to the grid
    [SerializeField] private Transform cameraTransform;
    [SerializeField] public GridLogicSystem gridLogicSystem;

    private GridXY<CandyGridCellPosition> _grid;
    private Dictionary<CandyOnGridCell, CandyGridVisual> _candyGridDictionary;
    private Dictionary<CandyGridCellPosition, GlassGridVisual> _glassGridDictionary;
    
    private State _state;
    private bool _isSetup;
    private float _busyTimer;
    private Action _onBusyTimerElapsedAction;

    private void Awake()
    {
        _state = State.Busy;
        _isSetup = false;
        
        gridLogicSystem.OnLevelSet += GridLogicSystem_OnLevelSet; // we subscribe to the event which notify when level set.
        
    }
    //after getting notified from On levelSet event, we set up the grid after certain time.
    private void GridLogicSystem_OnLevelSet(object sender, GridLogicSystem.OnLevelSetEventArgs e)
    {
        FunctionTimer.Create(() => Setup(sender as GridLogicSystem,e.Grid), .1f); //we set up the grid after 0.1 seconds
    }
    private void Setup(GridLogicSystem gridLogicSystem, GridXY<CandyGridCellPosition> grid)
    {
        this.gridLogicSystem = gridLogicSystem;
        this._grid = grid;
        
        //float cameraYOffset = 1f;
        //cameraTransform.position = new Vector3(grid.GetColumnsCount() * .5f, grid.GetRowsCount() * .5f + cameraYOffset, cameraTransform.position.z);
        
        gridLogicSystem.OnCandyGridPositionDestroyed += GridLogicSystem_OnCandyGridPositionDestroyed;
        gridLogicSystem.OnNewCandyGridSpawned += GridLogicSystem_OnNewCandyGridSpawned;
        
        
        _candyGridDictionary = new Dictionary<CandyOnGridCell, CandyGridVisual>();
        _glassGridDictionary = new Dictionary<CandyGridCellPosition, GlassGridVisual>();

        for (int x = 0; x < grid.GetColumnsCount(); x++)
        {
            for (int y = 0; y < grid.GetRowsCount(); y++)
            {
                CandyGridCellPosition candyGridCellPosition = grid.GetGridObject(x, y);
                CandyOnGridCell candyOnGridCell = candyGridCellPosition.GetCandyBlock();

                Vector3 position = grid.GetWorldPosition(x, y);
                position = new Vector3(position.x,position.y);
                
                //VisualTransform
                Transform candyGridVisualTransform = Instantiate(pfCandyGridVisual, position, Quaternion.identity);
                candyGridVisualTransform.GetChild(0).GetComponent<SpriteRenderer>().sprite = candyOnGridCell.GetCandyBlockSo().defaultCandySprite;
                candyGridVisualTransform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = y;
                CandyGridVisual candyGridVisual = new CandyGridVisual(candyGridVisualTransform, candyOnGridCell,gridLogicSystem);
                
                _candyGridDictionary[candyOnGridCell] = candyGridVisual;
                
                Transform glassGridVisualTransform = Instantiate(pfGlassGridVisual, grid.GetWorldPosition(x,y), Quaternion.identity);
                GlassGridVisual glassGridVisual = new GlassGridVisual(glassGridVisualTransform, candyGridCellPosition);
                
                _glassGridDictionary[candyGridCellPosition] = glassGridVisual;
                //Instantiate(pfBackgroundGridVisual, grid.GetWorldPosition(x, y), Quaternion.identity);
                
            }
        }
        this.gridLogicSystem.ChangeAllCandyBlocksState();
        //Debug.Log(gridLogicSystem.GetAllPossibleMoves().Count.ToString());
        SetBusyState(0.1f, () => SetState(State.BeforePlayerTurn));
        _isSetup= true;

    }
    private void GridLogicSystem_OnCandyGridPositionDestroyed(object sender, EventArgs e)
    {
        if (sender is CandyGridCellPosition candyGridCellPosition && candyGridCellPosition.GetCandyBlock() != null)
        {
            _candyGridDictionary.Remove(candyGridCellPosition.GetCandyBlock());
        }
    }
    private void GridLogicSystem_OnNewCandyGridSpawned(object sender, GridLogicSystem.OnNewCandyGridSpawnedEventArgs e)
    {
        Vector3 position = e.CandyGridCellPosition.GetWorldPosition();
        position = new Vector3(position.x,20);
        
        Transform candyGridVisualTransform = Instantiate(pfCandyGridVisual, position, Quaternion.identity);
        candyGridVisualTransform.GetChild(0).GetComponent<SpriteRenderer>().sprite = e.CandyOnGridCell.GetCandyBlockSo().defaultCandySprite;
        candyGridVisualTransform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = e.CandyGridCellPosition.GetY();
        CandyGridVisual candyGridVisual = new CandyGridVisual(candyGridVisualTransform, e.CandyOnGridCell,gridLogicSystem);
        _candyGridDictionary[e.CandyOnGridCell] = candyGridVisual;
    }

    private void Update()
    {
        if (!_isSetup) { return; }
        UpdateVisual();

        switch (_state)
        {
            case State.Busy:
                _busyTimer -= Time.deltaTime;
                if (_busyTimer <= 0f)
                {
                    _onBusyTimerElapsedAction();
                }
                break;
            case State.BeforePlayerTurn:
                
                if (Input.GetMouseButtonDown(0))
                {   //List<List<CandyGridCellPosition>> allSameColorConnectedGroups = gridLogicSystem.GetAllConnectedGroups();
                    List<GridLogicSystem.PossibleMove> allPossibleMoves = gridLogicSystem.GetAllPossibleMoves();
                    if (!gridLogicSystem.IsAnyPossibleMoveLeft(allPossibleMoves))
                    {
                        gridLogicSystem.DestroyAllCandyBlocks();
                        SetBusyState(.1f, () => SetState(State.AfterPlayerTurn));
                    }
                    Vector3 mousePosition = Input.mousePosition;
                    mousePosition.z = 60f;
                    Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
                    _grid.GetXY(worldPosition, out int x, out int y);
                    if (gridLogicSystem.HasAnyConnectedSameColorCandyBlocks(x,y))
                    {   gridLogicSystem.DestroyConnectedSameColorCandyBlocks(x,y);
                        SetState(State.AfterPlayerTurn);
                    }

                }
                break;
            case State.AfterPlayerTurn:
                SetBusyState(.2f, () =>
                {
                    gridLogicSystem.FallGemsIntoEmptyPosition();
                    
                    SetBusyState(.2f, () =>
                    {
                        gridLogicSystem.SpawnNewMissingGridPositions();
                        SetBusyState(.4f, () =>
                        {   
                            gridLogicSystem.ChangeAllCandyBlocksState();
                            SetBusyState(.1f, ()=>SetState(State.BeforePlayerTurn));
                        });
                    });
                });
                
                
                break;
            
            case State.GameOver:
                break;
        }
        
    }

    private void UpdateVisual()
    {
        foreach (CandyOnGridCell candyGrid in _candyGridDictionary.Keys)
        {
            _candyGridDictionary[candyGrid].Update();
        }
        
    }

    private void SetBusyState(float busyTimer, Action onBusyTimerElapsedAction) {
        SetState(State.Busy);
        this._busyTimer = busyTimer;
        this._onBusyTimerElapsedAction = onBusyTimerElapsedAction;
        
    }
    private void SetState(State state) {
        this._state = state;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }
    public State GetState() {
        return _state;
    }

    

    private class CandyGridVisual
    {
        private Transform _transform;
        private CandyOnGridCell _candyOnGridCell;
        private GridLogicSystem _gridLogicSystem;
        
        

        public CandyGridVisual(Transform transform, CandyOnGridCell candyOnGridCell, GridLogicSystem gridLogicSystem)
        {
            this._transform = transform;
            this._candyOnGridCell = candyOnGridCell;
            this._gridLogicSystem = gridLogicSystem;
            
            candyOnGridCell.OnDestroyed += CandyOnGridCell_OnDestroyed;
            candyOnGridCell.OnIconLevelChanged += CandyOnGridCell_OnIconLevelChanged;
        }
        private void CandyOnGridCell_OnDestroyed(object sender, EventArgs e)
        {   _transform.GetComponent<Animation>().Play();
            Destroy(_transform.gameObject,0.2f); //,1f);
        }
        
        private void CandyOnGridCell_OnIconLevelChanged(object sender, CandyOnGridCell.OnIconLevelChangedEventArgs e)
        {  
            _transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = e.CandyOnGridCell.GetSprite();
            _transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = e.CandyOnGridCell.GetY();
            
        }
        public void Update()
        {
            Vector3 targetPosition = _candyOnGridCell.GetWorldPosition()* _gridLogicSystem.Grid.GetCellSize();
            _transform.GetComponentInChildren<SpriteRenderer>().sprite = _candyOnGridCell.GetSprite();
            _transform.GetComponentInChildren<SpriteRenderer>().sortingOrder = _candyOnGridCell.GetY();
            var position = _transform.position;
            Vector3 moveDir = (targetPosition - position);
            float moveSpeed = 10f;
            position += moveDir * (moveSpeed * Time.deltaTime);
            _transform.position = position;
        }
        
    }
    private class GlassGridVisual
    {
        private Transform _transform;
        private CandyGridCellPosition _candyGridCellPosition;
        
        public GlassGridVisual(Transform transform, CandyGridCellPosition candyGridCellPosition)
        {
            this._transform = transform;
            this._candyGridCellPosition = candyGridCellPosition;
            
            transform.gameObject.SetActive(candyGridCellPosition.HasGlass());
            
            
            candyGridCellPosition.OnGlassDestroyed += CandyGridPosition_OnGlassDestroyed;
        }
        private void CandyGridPosition_OnGlassDestroyed(object sender, EventArgs e)
        {   
            _transform.gameObject.SetActive(_candyGridCellPosition.HasGlass());
        }
    }
    
}
