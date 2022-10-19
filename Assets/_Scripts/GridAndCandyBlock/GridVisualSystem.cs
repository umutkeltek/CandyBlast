using System;
using System.Collections.Generic;
using CodeMonkey.Utils;
using UnityEngine;

//Visual Representation of the grid logic
public class GridVisualSystem : MonoBehaviour
{   
    public event EventHandler OnVisualSetupComplete; //Event to notify when the visual setup is complete to game manager
    [SerializeField] private Transform pfCandyGridVisual;
    [SerializeField] private Transform pfGlassGridVisual;
    [SerializeField] private Transform pfBackgroundGridVisual; //it will be used if it is wanted to add a background to the grid
    [SerializeField] private Transform cameraTransform;
    [SerializeField] public GridLogicSystem gridLogicSystem;

    private GridXY<CandyGridCellPosition> _grid;
    private Dictionary<CandyOnGridCell, CandyGridVisual> _candyGridDictionary;
    private Dictionary<CandyGridCellPosition, GlassGridVisual> _glassGridDictionary;
    
    public void Setup(GridLogicSystem gridLogicSystem, GridXY<CandyGridCellPosition> grid)
    {
        this.gridLogicSystem = gridLogicSystem;
        this._grid = grid;
        
        //float cameraYOffset = 1f;
        //cameraTransform.position = new Vector3(grid.GetColumnsCount() * .5f, grid.GetRowsCount() * .5f + cameraYOffset, cameraTransform.position.z);
        
        gridLogicSystem.OnCandyGridPositionDestroyed += GridLogicSystem_OnCandyGridPositionDestroyed;
        gridLogicSystem.OnNewCandyGridSpawned += GridLogicSystem_OnNewCandyGridSpawned;
        
        
        _candyGridDictionary = new Dictionary<CandyOnGridCell, CandyGridVisual>(); //we create a dictionary to store the candy grid visual matched with the candy grid logic
        _glassGridDictionary = new Dictionary<CandyGridCellPosition, GlassGridVisual>(); //we create a dictionary to store the glass grid visual matched with grid cell position

        for (int x = 0; x < grid.GetColumnsCount(); x++)
        {
            for (int y = 0; y < grid.GetRowsCount(); y++)
            {
                CandyGridCellPosition candyGridCellPosition = grid.GetGridObject(x, y); //we get the grid cell position from the grid logic system
                CandyOnGridCell candyOnGridCell = candyGridCellPosition.GetCandyBlock(); //we get the candy block from the grid cell position

                Vector3 position = grid.GetWorldPosition(x, y); //we get the world position of the grid cell position
                position = new Vector3(position.x,20); //we set the position of the grid cell position to be 20 units above the grid cell position to make it look like it is falling from the sky
                
                //VisualTransform
                Transform candyGridVisualTransform = Instantiate(pfCandyGridVisual, position, Quaternion.identity); //we instantiate the candy grid visual prefab
                candyGridVisualTransform.GetChild(0).GetComponent<SpriteRenderer>().sprite = candyOnGridCell.GetCandyBlockSo().defaultCandySprite; //we set the sprite of the candy grid visual to be the sprite of the candy block 
                candyGridVisualTransform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = y; //it is set to y otherwise the candy grid visual will be hidden behind the top candy grid visual
                CandyGridVisual candyGridVisual = new CandyGridVisual(candyGridVisualTransform, candyOnGridCell,gridLogicSystem); //we create a new candy grid visual object and pass the transform, candy block and grid logic system
                
                _candyGridDictionary[candyOnGridCell] = candyGridVisual;
                
                Transform glassGridVisualTransform = Instantiate(pfGlassGridVisual, grid.GetWorldPosition(x,y), Quaternion.identity);
                GlassGridVisual glassGridVisual = new GlassGridVisual(glassGridVisualTransform, candyGridCellPosition);
                
                _glassGridDictionary[candyGridCellPosition] = glassGridVisual;
                //Instantiate(pfBackgroundGridVisual, grid.GetWorldPosition(x, y), Quaternion.identity);
                
            }
        }
        this.gridLogicSystem.ChangeAllCandyBlocksState(); //we change the state of all the candy blocks so icons could change based on the state of the candy block
        //Debug.Log(gridLogicSystem.GetAllPossibleMoves().Count.ToString());
        OnVisualSetupComplete?.Invoke(this, EventArgs.Empty);
        
        

    }
    
    //whenever a candy block is destroyed, we remove the candy grid visual from the dictionary
    private void GridLogicSystem_OnCandyGridPositionDestroyed(object sender, EventArgs e)
    {
        if (sender is CandyGridCellPosition candyGridCellPosition && candyGridCellPosition.GetCandyBlock() != null)
        {
            _candyGridDictionary.Remove(candyGridCellPosition.GetCandyBlock()); //we remove the candy grid visual from the dictionary when the candy block is destroyed
        }
    }
    //whenever new candy spawned we instantiate a new candy grid visual and add to library
    private void GridLogicSystem_OnNewCandyGridSpawned(object sender, GridLogicSystem.OnNewCandyGridSpawnedEventArgs e) 
    {   //whenever a new candy block is spawned, we instantiate a new candy grid visual and add it to the dictionary
        Vector3 position = e.CandyGridCellPosition.GetWorldPosition();
        position = new Vector3(position.x,20); //we set the position of the grid cell position to be 20 units above the grid cell position to make it look like it is falling from the sky
        
        Transform candyGridVisualTransform = Instantiate(pfCandyGridVisual, position, Quaternion.identity);
        candyGridVisualTransform.GetChild(0).GetComponent<SpriteRenderer>().sprite = e.CandyOnGridCell.GetCandyBlockSo().defaultCandySprite; 
        candyGridVisualTransform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = e.CandyGridCellPosition.GetY();
        CandyGridVisual candyGridVisual = new CandyGridVisual(candyGridVisualTransform, e.CandyOnGridCell,gridLogicSystem);
        _candyGridDictionary[e.CandyOnGridCell] = candyGridVisual;
    }
    public void UpdateVisual()
    {
        foreach (CandyOnGridCell candyGrid in _candyGridDictionary.Keys)
        {
            _candyGridDictionary[candyGrid].Update();
        }
        
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
