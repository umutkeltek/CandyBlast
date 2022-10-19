using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GridLogicSystem gridLogicSystem;

    [SerializeField] private TextMeshProUGUI movesText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI targetScoreText;
    [SerializeField] private TextMeshProUGUI glassText;
    [SerializeField] private Transform winPanel;
    [SerializeField] private Transform losePanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Image glassImage;
    
    

    private void Awake()
    {
        winPanel.gameObject.SetActive(false);
        losePanel.gameObject.SetActive(false);
        
        gridLogicSystem.OnLevelSet += GridLogicSystem_OnLevelSet;
        LevelManager.Instance.OnMoveCountChanged += LevelManager_OnMoveCountChanged;
        LevelManager.Instance.OnScoreAmountAdded += LevelManager_OnScoreAmountAdded;
        LevelManager.Instance.OnCurrentGlassCountChanged += LevelManager_OnCurrentGlassCountChanged;
        LevelManager.Instance.OnWin += LevelManager_OnWin;
        LevelManager.Instance.OnLose += LevelManager_OnLose;
         
    }

    private void LevelManager_OnLose(object sender, EventArgs e)
    {
        losePanel.gameObject.SetActive(true);
    }

    private void LevelManager_OnWin(object sender, EventArgs e)
    {
        winPanel.gameObject.SetActive(true);
    }

    private void LevelManager_OnCurrentGlassCountChanged(object sender, EventArgs e)
    {
        UpdateText();
    }

    private void LevelManager_OnScoreAmountAdded(object sender, EventArgs e)
    {
        UpdateText();
    }

    private void LevelManager_OnMoveCountChanged(object sender, EventArgs e)
    {
        UpdateText();
    }

    private void GridLogicSystem_OnLevelSet(object sender, System.EventArgs e)
    {   UpdateText();
        LevelSO levelSo = gridLogicSystem.GetLevelSo();
        
        switch (levelSo.winCondition)
        {
            case LevelSO.WinCondition.ReachSpecificScore:
                glassText.gameObject.SetActive(false);
                glassImage.gameObject.SetActive(false);
                targetScoreText.gameObject.SetActive(true);
                targetScoreText.text = levelSo.targetScore.ToString();
                break;
            case LevelSO.WinCondition.RemoveAllGlassBlocks:
                glassText.gameObject.SetActive(true);
                glassImage.gameObject.SetActive(true);
                targetScoreText.gameObject.SetActive(false);
                
                break;
        }

        
    }
    
    private void UpdateText()
    {
        movesText.text = LevelManager.Instance.GetMoveCount().ToString();
        scoreText.text = LevelManager.Instance.GetScore().ToString();
        glassText.text = LevelManager.Instance.GetCurrentGlassAmount().ToString();
    }
}
