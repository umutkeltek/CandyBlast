using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoSingleton<CameraManager>
{
    
    
    public void SetCameraOrthoSize(GridXY<CandyGridCellPosition> _grid, Transform _camera)
    {
        var desiredCameraWidth = _grid.GetColumnsCount() * 256f / 100f;
        float screenRatio = Screen.width / (float)Screen.height;
        var desiredCameraHeight = desiredCameraWidth / screenRatio;
        Camera.main.orthographicSize = desiredCameraHeight / 2f;
        _camera.transform.position = CalculateOrthoSize(
            _grid.GetWorldPosition(0, 0) - new Vector3(_grid.GetCellSize(), _grid.GetCellSize()),
            _grid.GetWorldPosition(_grid.GetColumnsCount() - 1, _grid.GetRowsCount() - 1) +
            new Vector3(_grid.GetCellSize(), _grid.GetCellSize())).center + Vector3.right * 1.22f + new Vector3(0,_grid.GetCellSize()/0.7f,-10);
        
        
    }
    public (Vector3 center, float size) CalculateOrthoSize(Vector3 positionA, Vector3 positionB)
    {
        Vector3 center = (positionA + positionB) / 2f;
        float size = Mathf.Max(Mathf.Abs(positionA.x - positionB.x), Mathf.Abs(positionA.y - positionB.y));
        return (center, size);
    }
}
