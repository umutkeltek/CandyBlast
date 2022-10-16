using System;
using System.Collections;
using System.Collections.Generic;
using CodeMonkey.Utils;
using Unity.VisualScripting;
using UnityEngine;

//Visual Representation of the grid logic
public class GridVisualSystem : MonoBehaviour
{   public event EventHandler OnStateChanged;
    public enum State
    {
        Busy,
        WaitingForPlayer,
        TryFindMatches,
        GameOver
    }
    [SerializeField] private Transform pfCandyGridVisual;
    [SerializeField] private Transform pfGlassGridVisual;
    [SerializeField] private Transform pfBackgroundGridVisual; //it will be used if it is wanted to add a background to the grid
    [SerializeField] private Transform cameraTransform;
    [SerializeField] public GridLogicSystem gridLogicSystem;

    private GridXY<CandyGridCellPosition> grid;
    private Dictionary<CandyOnGridCell, CandyGridVisual> candyGridDictionary;
    private Dictionary<CandyGridCellPosition, GlassGridVisual> glassGridDictionary;
    
    private State state;
    private bool isSetup;
    private float busyTimer;
    private Action onBusyTimerElapsedAction;

    private void Awake()
    {
        state = State.Busy;
        isSetup = false;
        
        gridLogicSystem.OnLevelSet += GridLogicSystem_OnLevelSet; // we subscribe to the event which notify when level set.
        
    }
    //after getting notified from OnlevelSet event, we set up the grid after certain time.
    private void GridLogicSystem_OnLevelSet(object sender, GridLogicSystem.OnLevelSetEventArgs e)
    {
        FunctionTimer.Create(() => Setup(sender as GridLogicSystem,e.grid), .1f); //we set up the grid after 0.1 seconds
    }

    public void Setup(GridLogicSystem gridLogicSystem, GridXY<CandyGridCellPosition> grid)
    {
        this.gridLogicSystem = gridLogicSystem;
        this.grid = grid;
        
        float cameraYOffset = 1f;
        //cameraTransform.position = new Vector3(grid.GetColumnsCount() * .5f, grid.GetRowsCount() * .5f + cameraYOffset, cameraTransform.position.z);
        
        gridLogicSystem.OnCandyGridPositionDestroyed += GridLogicSystem_OnCandyGridPositionDestroyed;
        gridLogicSystem.OnNewCandyGridSpawned += GridLogicSystem_OnNewCandyGridSpawned;
        
        
        candyGridDictionary = new Dictionary<CandyOnGridCell, CandyGridVisual>();
        glassGridDictionary = new Dictionary<CandyGridCellPosition, GlassGridVisual>();

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
                candyGridVisualTransform.Find("sprite").GetComponent<SpriteRenderer>().sprite = candyOnGridCell.GetCandyBlockSO().defaultCandySprite;
                candyGridVisualTransform.Find("sprite").GetComponent<SpriteRenderer>().sortingOrder = y;
                CandyGridVisual candyGridVisual = new CandyGridVisual(candyGridVisualTransform, candyOnGridCell,gridLogicSystem);
                
                candyGridDictionary[candyOnGridCell] = candyGridVisual;
                
                Transform glassGridVisualTransform = Instantiate(pfGlassGridVisual, grid.GetWorldPosition(x,y), Quaternion.identity);
                GlassGridVisual glassGridVisual = new GlassGridVisual(glassGridVisualTransform, candyGridCellPosition);
                
                glassGridDictionary[candyGridCellPosition] = glassGridVisual;
                //Instantiate(pfBackgroundGridVisual, grid.GetWorldPosition(x, y), Quaternion.identity);
                
            }
        }

        SetBusyState(0.1f, () => SetState(State.TryFindMatches));
        isSetup= true;

    }
    
    private void GridLogicSystem_OnCandyGridPositionDestroyed(object sender, EventArgs e)
    {   
        CandyGridCellPosition candyGridCellPosition = sender as CandyGridCellPosition;
        if (candyGridCellPosition!= null && candyGridCellPosition.GetCandyBlock() != null)
        {
            candyGridDictionary.Remove(candyGridCellPosition.GetCandyBlock());
        }
    }
    private void GridLogicSystem_OnNewCandyGridSpawned(object sender, GridLogicSystem.OnNewCandyGridSpawnedEventArgs e)
    {
        Vector3 position = e.candyGridCellPosition.GetWorldPosition();
        position = new Vector3(position.x,position.y);
        Transform candyGridVisualTransform = Instantiate(pfCandyGridVisual, position, Quaternion.identity);
    }

    private void Update()
    {
        if (!isSetup) { return; }
        UpdateVisual();

        switch (state)
        {
            case State.Busy:
                busyTimer -= Time.deltaTime;
                if (busyTimer <= 0f)
                {
                    onBusyTimerElapsedAction();
                }
                break;
            case State.WaitingForPlayer:
                break;
            case State.TryFindMatches:
                /*if (gridLogicSystem.TryFindMatches())
                {
                    SetBusyState(0.1f, () => SetState(State.WaitingForPlayer));
                }
                else
                {
                    SetState(State.WaitingForPlayer);
                }*/
                break;
            case State.GameOver:
                break;
        }

        if (Input.GetMouseButtonDown(0))
        {   Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 60f;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            grid.GetXY(worldPosition, out int x, out int y);
            Debug.Log(gridLogicSystem.AdjacentCandyGridCellPositionsSameColor(x, y));
            //gridLogicSystem.ClearConnectedSameColorCandyBlocks(x, y);
        }
        
    }

    private void UpdateVisual()
    {
        foreach (CandyOnGridCell candyGrid in candyGridDictionary.Keys)
        {
            candyGridDictionary[candyGrid].Update();
        }
        
    }

    private void SetBusyState(float busyTimer, Action onBusyTimerElapsedAction) {
        SetState(State.Busy);
        this.busyTimer = busyTimer;
        this.onBusyTimerElapsedAction = onBusyTimerElapsedAction;
    }
    private void SetState(State state) {
        this.state = state;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public State GetState() {
        return state;
    }

    #region CandyGridVisual

    public class CandyGridVisual
    {
        private Transform transform;
        private CandyOnGridCell candyOnGridCell;
        private GridLogicSystem gridLogicSystem;
        
        
        public CandyGridVisual(Transform transform, CandyOnGridCell candyOnGridCell, GridLogicSystem gridLogicSystem)
        {
            this.transform = transform;
            this.candyOnGridCell = candyOnGridCell;
            this.gridLogicSystem = gridLogicSystem;

            
            candyOnGridCell.OnDestroyed += CandyOnGridCell_OnDestroyed;
        }
        private void CandyOnGridCell_OnDestroyed(object sender, System.EventArgs e)
        {   //transform.GetComponent<Animation>().Play();
            Destroy(transform.gameObject); //,1f);
        }
        public void Update()
        {
            Vector3 targetPosition = candyOnGridCell.GetWorldPosition() * gridLogicSystem.grid.GetCellSize();
            Vector3 moveDir = (targetPosition - transform.position);
            float moveSpeed = 10f;
            transform.position += moveDir * moveSpeed * Time.deltaTime;
        }
        
    }

    #endregion

    #region GlassGridVisual
    public class GlassGridVisual
    {
        private Transform transform;
        private CandyGridCellPosition candyGridCellPosition;
        
        public GlassGridVisual(Transform transform, CandyGridCellPosition candyGridCellPosition)
        {
            this.transform = transform;
            this.candyGridCellPosition = candyGridCellPosition;
            
            transform.gameObject.SetActive(candyGridCellPosition.HasGlass());
            
            
            candyGridCellPosition.OnGlassDestroyed += CandyGridPosition_OnGlassDestroyed;
        }
        private void CandyGridPosition_OnGlassDestroyed(object sender, System.EventArgs e)
        {   
            transform.gameObject.SetActive(candyGridCellPosition.HasGlass());
        }
    }
    

    #endregion
    

}