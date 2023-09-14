using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TinAungKhant.UIManagement;

public class UIResultPanel : UIBase
{
    [SerializeField] private ResultPanelManager _resultPanelManager;

    protected override void OnShow(UIBaseData data = null)
    {
        base.OnShow(data);

        if(data != null)
        {
            ResultData resultData = (ResultData)data;

            _resultPanelManager.ShowResult(resultData.winnerName, resultData.roomID);
        }
    }

    protected override void OnClose()
    {
        base.OnClose();

        _resultPanelManager.ClearPanels();
    }
}

public class ResultData : UIBaseData
{
    public string winnerName;
    public int roomID;

    public ResultData(string winnerName, int roomID)
    {
        this.winnerName = winnerName;
        this.roomID = roomID;
    }
}
