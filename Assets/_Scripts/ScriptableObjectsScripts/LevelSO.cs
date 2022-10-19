using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class LevelSO : ScriptableObject
{
    public enum WinCondition
    {
        ReachSpecificScore,
        BlastSpecificAmountOfCandyBlocks,
        RemoveAllGlassBlocks,
    }
    
    public List<CandyBlockSO> candyBlocksList;
    public List<LevelGridPosition> candyGridPositionsList;
    public int columns;
    public int rows;
    public int moveAmount;
    public int targetScore;
    public int scorePerCandy;
    public WinCondition winCondition;
    
    [Header("Conditions To Change Icon Level")]
    public int conditionThreshold1; 
    public int conditionThreshold2;
    public int conditionThreshold3;
    
    
    [System.Serializable]
    public class LevelGridPosition
    {
        public CandyBlockSO candyBlockSO;
        public int x;
        public int y;
        public bool hasGlass;

    }

}
