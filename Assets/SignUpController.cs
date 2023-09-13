using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SignUpController : MonoBehaviour
{
    private GameObject signup_Go;
    [SerializeField] private GameObject login_Go;     

    [SerializeField] private Button back_Btn;
    [SerializeField] private Button signup_Btn;

    private void Start()
    {
        signup_Go=this.gameObject;
        back_Btn.onClick.AddListener(() =>OnClickBack());
    }

    private void OnClickBack()
    {
        signup_Go.SetActive(false);
        login_Go.SetActive(true);
    }
    //Animations at OnEnable and OnDisable
}
