using System.Collections;
using System.Collections.Generic;
using TinAungKhant.UIManagement;
using UnityEngine;

public class GoToFindRoom : MonoBehaviour
{
    public void onClickGotoFindRoom()
    {
        UIManager.Instance.CloseUI(GLOBALCONST.MAINMENU_UI);
        UIManager.Instance.ShowUI(GLOBALCONST.FINDROOM_UI);
    }
}
