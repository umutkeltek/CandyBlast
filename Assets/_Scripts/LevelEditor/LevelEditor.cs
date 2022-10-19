using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;

//This is the class is responsible for creating level design for the real game. It is not the actual game. Desired elements are adjusted and saved into LevelSO s
public class LevelEditor : MonoBehaviour
{
    [SerializeField] private LevelSO levelSo;
    [SerializeField] private Transform pfCandyGridVisual;
    [SerializeField] private Transform pfGlassGridVisual;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI selectedCell;
    

    
    
    private Vector3 _mousePosition;
    private Vector3 _mousePositionWorld;

    private GridXY<GridPosition> _grid;
    
    
    
    private void Awake()
    {
        _grid = new GridXY<GridPosition>(levelSo.columns, levelSo.rows, 2.2f, Vector3.zero, (GridXY<GridPosition> g, int x, int y) => new GridPosition(levelSo, g, x, y));
        levelText.text = levelSo.name;
        
        

        if (levelSo.candyGridPositionsList == null || levelSo.candyGridPositionsList.Count != levelSo.columns*levelSo.rows) 
        {
            Debug.Log("Creating new level...");
            levelSo.candyGridPositionsList = new List<LevelSO.LevelGridPosition>();
            for (int x = 0; x < _grid.GetColumnsCount(); x++)
            {
                for (int y = 0; y < _grid.GetRowsCount(); y++)
                {
                    CandyBlockSO candyBlock = levelSo.candyBlocksList[Random.Range(0,levelSo.candyBlocksList.Count)];
                    LevelSO.LevelGridPosition levelGridPosition = new LevelSO.LevelGridPosition{candyBlockSO = candyBlock, x = x, y = y};
                    levelSo.candyGridPositionsList.Add(levelGridPosition);
                    
                    CreateVisual(_grid.GetGridObject(x,y),levelGridPosition);
                    _grid.GetGridObject(x,y).SpriteRenderer.sortingOrder = y;
                }
            }
            
        }
        else
        {
            Debug.Log("loading level...");
            for (int x = 0; x < _grid.GetColumnsCount(); x++)
            {
                for (int y = 0; y < _grid.GetRowsCount(); y++)
                {
                    LevelSO.LevelGridPosition levelGridPosition = null;
                    foreach (LevelSO.LevelGridPosition tempLevelGridPosition in levelSo.candyGridPositionsList)
                    {
                        if (tempLevelGridPosition.x == x && tempLevelGridPosition.y == y)
                        {
                            levelGridPosition = tempLevelGridPosition;
                            break;
                        }
                    }
                    if (levelGridPosition == null)
                    {
                        Debug.LogError("LevelGridPosition not found");
                    }
                    CreateVisual(_grid.GetGridObject(x,y), levelGridPosition);
                    _grid.GetGridObject(x,y).SpriteRenderer.sortingOrder = y;
                }
            }
        }
        
        SetCameraOrthoSize();
    }

    

    private (Vector3 center, float size) CalculateOrthoSize(Vector3 positionA, Vector3 positionB)
    {
        Vector3 center = (positionA + positionB) / 2f;
        float size = Mathf.Max(Mathf.Abs(positionA.x - positionB.x), Mathf.Abs(positionA.y - positionB.y));
        return (center, size);
    }
    
    private void SetCameraOrthoSize()
    {
        var desiredCameraWidth = _grid.GetColumnsCount() * 256f / 48f;
        float screenRatio = Screen.width / (float)Screen.height;
        var desiredCameraHeight = desiredCameraWidth / screenRatio;
        Camera.main.orthographicSize = desiredCameraHeight / 2f;
        cameraTransform.position = CalculateOrthoSize(
            _grid.GetWorldPosition(0, 0) - new Vector3(_grid.GetCellSize(), _grid.GetCellSize()),
            _grid.GetWorldPosition(_grid.GetColumnsCount() - 1, _grid.GetRowsCount() - 1) +
            new Vector3(_grid.GetCellSize(), _grid.GetCellSize())).center + Vector3.right * 1.22f + new Vector3(0,_grid.GetCellSize()/2.56f,-10);
        
        
    }
    
    
    

    private void Update()
    {
        _mousePosition = Input.mousePosition;
        Camera main;
        _mousePositionWorld = (main = Camera.main).ScreenToWorldPoint(_mousePosition);
        _mousePositionWorld.z = main.nearClipPlane -3f;
        _grid.GetXY(_mousePositionWorld, out int x, out int y);
        if (x<0 || y<0 || x>=_grid.GetColumnsCount() || y>=_grid.GetRowsCount())
        {
            selectedCell.text = "Out of bounds";
        }
        else
        {
            selectedCell.text = "Selected Grid Cell: \n" + x + " : " + y;
        }
        
        if (Screen.height != 1080 || Screen.width != 1920)
        {
            
            //Screen.SetResolution(1920,1080,true); 
            
            
            Debug.Log("Please set resolution to 1920 1080 in order to use level editor!");
            
        }
        
        
        
        
        
        if (IsValidPosition(x,y))
        {

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _grid.GetGridObject(x, y).SetCandySo(levelSo.candyBlocksList[0]);
                _grid.GetGridObject(x, y).SpriteRenderer.sortingOrder = y;
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _grid.GetGridObject(x, y).SetCandySo(levelSo.candyBlocksList[1]);
                _grid.GetGridObject(x, y).SpriteRenderer.sortingOrder = y;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                _grid.GetGridObject(x, y).SetCandySo(levelSo.candyBlocksList[2]);
                _grid.GetGridObject(x, y).SpriteRenderer.sortingOrder = y;
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                _grid.GetGridObject(x, y).SetCandySo(levelSo.candyBlocksList[3]);
                _grid.GetGridObject(x, y).SpriteRenderer.sortingOrder = y;
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                _grid.GetGridObject(x, y).SetCandySo(levelSo.candyBlocksList[4]);
                _grid.GetGridObject(x, y).SpriteRenderer.sortingOrder = y;
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                _grid.GetGridObject(x, y).SetCandySo(levelSo.candyBlocksList[5]);
                _grid.GetGridObject(x, y).SpriteRenderer.sortingOrder = y;
            }
            

            if (Input.GetMouseButtonDown(1)) {
                _grid.GetGridObject(x, y).SetHasGlass(!_grid.GetGridObject(x, y).GetHasGlass());
            }
        }
    }
    public LevelSO GetLevelSo()
    {
        return levelSo;
    }

    private void CreateVisual(GridPosition gridPosition, LevelSO.LevelGridPosition levelGridPosition)
    {
        Transform candyGridVisualTransform = Instantiate(pfCandyGridVisual, gridPosition.GetWorldPosition(), Quaternion.identity);
        Transform glassGridVisualTransform = Instantiate(pfGlassGridVisual, gridPosition.GetWorldPosition(), Quaternion.identity);
        
        gridPosition.SpriteRenderer = candyGridVisualTransform.Find("sprite").GetComponent<SpriteRenderer>();
        gridPosition.GlassVisualGameObject = glassGridVisualTransform.gameObject;
        gridPosition.LevelGridPosition = levelGridPosition;

        gridPosition.SetCandySo(levelGridPosition.candyBlockSO);
        gridPosition.SetHasGlass(levelGridPosition.hasGlass);

    }

    private bool IsValidPosition(int x, int y)
    {
        if (x < 0 || x >= _grid.GetColumnsCount() || y < 0 || y >= _grid.GetRowsCount())
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    #region GridPositionClass

    private class GridPosition //Dummy grid position class to create levels using level editor 
    {
        public SpriteRenderer SpriteRenderer;
        public LevelSO.LevelGridPosition LevelGridPosition;
        public GameObject GlassVisualGameObject;

        private LevelSO _levelScriptableObject;
        private GridXY<GridPosition> _grid;
        private int _x;
        private int _y;
        
        public GridPosition(LevelSO levelScriptableObject, GridXY<GridPosition> grid, int x, int y)
        {
            this._levelScriptableObject = levelScriptableObject;
            this._grid = grid;
            this._x = x;
            this._y = y;
        }
        public Vector3 GetWorldPosition()
        {
            return _grid.GetWorldPosition(_x, _y);
        }
        public void SetCandySo(CandyBlockSO candySo)
        {
            SpriteRenderer.sprite = candySo.defaultCandySprite;
            LevelGridPosition.candyBlockSO = candySo;
            
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(_levelScriptableObject);
#endif
        }

        public void SetHasGlass(bool hasGlass)
        {
            LevelGridPosition.hasGlass = hasGlass;
            GlassVisualGameObject.SetActive(hasGlass);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(_levelScriptableObject);
#endif
        }
        public bool GetHasGlass()
        {
            return LevelGridPosition.hasGlass;
        }
    }

    #endregion
    
}


