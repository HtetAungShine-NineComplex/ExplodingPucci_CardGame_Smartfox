using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sfs2X.Requests;

public class LoginUIManager : MonoBehaviour
{
    [SerializeField] private Button playAsGuest_Btn;
    [SerializeField] private Button logIn_Btn;
    [SerializeField] private Image loading_Img;
    [SerializeField] private ButtonAnimController AnimController_Btn;
    [SerializeField] private GameObject Login_Go;
    [SerializeField] private GameObject Signup_Go;
    [SerializeField] private Button createAnAcc_Btn;


    [SerializeField] private RectTransform avator_TF;
    [SerializeField] private Vector2 avator_TargetTF;

    private float loadingTime = 2f;

    private void Start()
    {
        playAsGuest_Btn.onClick.AddListener(() => OnClickPlayAsGuest());
        logIn_Btn.onClick.AddListener(() => onClickLogIn());
        createAnAcc_Btn.onClick.AddListener(() => onClickCreateAnAcc());
    }

    private void onClickCreateAnAcc()
    {
        
       // GlobalManager.Instance.CreateSfsClient();
/*        GlobalManager.Instance.GetSfsClient().Send(new LogoutRequest());
        GlobalManager.Instance.GetSfsClient().Send(new LoginRequest("", "", "CardGame"));*/
        Login_Go.gameObject.SetActive(false);
        Signup_Go.gameObject.SetActive(true);
        
    }
    private void OnClickPlayAsGuest()
    {
        disableBtns();
        StartCoroutine(IAvatorAnim());
        
    }

    private void onClickLogIn()
    {

        disableBtns();
        StartCoroutine(ILogInAnim());
    }

    IEnumerator IAvatorAnim()
    {
        loading_Img.gameObject.SetActive(true);
        yield return new WaitForSeconds(loadingTime);
        loading_Img.gameObject.SetActive(false);
        avator_TF.DOAnchorPos(avator_TargetTF,0.5f,true);
        AnimController_Btn.buttonAnim();
    }
    
    IEnumerator ILogInAnim()
    {
        loading_Img.gameObject.SetActive(true);
        yield return new WaitForSeconds(loadingTime);
        loading_Img.gameObject.SetActive(false);
        avator_TF.DOAnchorPos(avator_TargetTF, 0.5f, true);
        Login_Go.gameObject.SetActive(true);
    }

    private void disableBtns()
    {
        playAsGuest_Btn.gameObject.SetActive(false);
        logIn_Btn.gameObject.SetActive(false);
    }
}
