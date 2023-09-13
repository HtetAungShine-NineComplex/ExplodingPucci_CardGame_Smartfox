using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AvatorManager : MonoBehaviour
{
    [SerializeField] private AvatorData avatorData;
    [SerializeField] private Image avatorImg;
    [SerializeField] private TextMeshProUGUI avatorTxt;
    [SerializeField] private Button nextAvatorBtn;
    [SerializeField] private Button backAvatorBtn;

    int selectedIndex=0;
    Avator myAvator;
    private void Start()
    {
        updateAvator(selectedIndex);
        nextAvatorBtn.onClick.AddListener(nextOption);
        backAvatorBtn.onClick.AddListener(backOption);
    }

    private void updateAvator(int index)
    {
        myAvator=avatorData.GetAvator(index);
        avatorImg.sprite = myAvator.AvatorSprite;
        avatorTxt.text=myAvator.AvatorName;
    }

    private void nextOption()
    {
        selectedIndex++;

        if(selectedIndex >= avatorData.AvatorCount)
        {
            selectedIndex = 0;
        }
        updateAvator(selectedIndex);
    }

    private void backOption()
    {
        selectedIndex--;

        if (selectedIndex < 0)
        {
            selectedIndex = avatorData.AvatorCount-1;
        }
        updateAvator(selectedIndex);
    }
}
