using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorUI : MonoBehaviour
{   
    private List<Sprite> candyGridVisualList;
    [SerializeField] private List<Image> levelImageList;
    [SerializeField] private LevelEditor levelEditor;
    private void Start()
    {   candyGridVisualList = new List<Sprite>();
        LevelSO levelSO = levelEditor.GetLevelSO();
        for (int i = 0; i <levelSO.candyBlocksList.Count; i++)
        {
            CandyBlockSO candyBlock = levelSO.candyBlocksList[i];
            candyGridVisualList.Add(candyBlock.defaultCandySprite);
        }
        int index = 0;
        for (int i = 0; i < levelImageList.Count; i++)
        {
            if (index<candyGridVisualList.Count)
            {   levelImageList[i].gameObject.SetActive(true);
                levelImageList[i].sprite = candyGridVisualList[index];
                index++;
            }
            else
            {
                levelImageList[i].gameObject.SetActive(false);
            }
        }
    }
}
