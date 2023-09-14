using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TinAungKhant.UIManagement;

public class UIGameRoom : UIBase
{
    [SerializeField] private TableManager _tableManager;

    protected override void OnInit()
    {
        base.OnInit();

        _tableManager.Init();
    }

    protected override void OnShow(UIBaseData data = null)
    {
        base.OnShow(data);

        _tableManager.ListenSfsEvents();
    }

    protected override void OnClose()
    {
        base.OnClose();

        _tableManager.RemoveSfsEvents();
    }
}
