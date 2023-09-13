using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginUI : UIBase
{
    public Button GuestBtn;
    [Header("Login Components")]
    public TMP_InputField Login_Username;
    public TMP_InputField Login_password;
    public Button Login_Btn;

    [Header("Signup components")]
    public TMP_InputField Signup_Username;
    public TMP_InputField Signup_Email;
    public TMP_InputField Signup_Password;
    public Button Signup_Btn;

    protected override void OnShow(UIBaseData Data = null)
    {
        base.OnShow(Data);
    }
}
