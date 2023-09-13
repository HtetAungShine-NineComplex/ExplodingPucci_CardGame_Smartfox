using System.Collections;
using System.Collections.Generic;
using TinAungKhant.UIManagement;
using UnityEngine;

public class SpawnFirstUI : MonoBehaviour
{

    void Start()
    {
        UIManager.Instance.ShowUI(GLOBALCONST.LOGIN_UI);
    }

}
