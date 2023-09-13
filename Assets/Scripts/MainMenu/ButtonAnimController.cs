using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;

public class ButtonAnimController : MonoBehaviour
{
    [SerializeField] private RectTransform PW_Friends_TF;
    [SerializeField] private RectTransform PW_Stranger_TF;
    [SerializeField] private RectTransform PW_Yourself_TF;
    [SerializeField] private RectTransform Tutorial_TF;
    [SerializeField] private RectTransform Setting_TF;

    [SerializeField] private Vector2 PW_Friends_TargetTF;
    [SerializeField] private Vector2 PW_Stranger_TargetTF;
    [SerializeField] private Vector2 PW_Yourself_TargetTF;
    [SerializeField] private Vector2 Tutorial_TargetTF;
    [SerializeField] private Vector2 Setting_TargetTF;

    private void Start()
    {
        //buttonAnim();
    }
    public void buttonAnim()
    {
        PW_Friends_TF.DOAnchorPos(PW_Friends_TargetTF,0.25f,true);
        PW_Stranger_TF.DOAnchorPos(PW_Stranger_TargetTF, 0.5f,true);
        PW_Yourself_TF.DOAnchorPos(PW_Yourself_TargetTF, 0.75f, true);
        Tutorial_TF.DOAnchorPos(Tutorial_TargetTF, 1f,true);
        Setting_TF.DOAnchorPos(Setting_TargetTF, 1.25f, true);
    }
}
