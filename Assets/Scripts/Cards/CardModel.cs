using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardModel : MonoBehaviour
{
    [SerializeField] private Sprite[] _faceSprites;
    [SerializeField] private Sprite _backSprite;

    [SerializeField] private Image _image;

    public void ShowFace(string cardName)
    {
        switch (cardName)
        {
            case CardNames.EXPLODE:
                _image.sprite = _faceSprites[0];
                break;

            case CardNames.DEFUSE:
                _image.sprite = _faceSprites[1];
                break;

            case CardNames.POWER:
                _image.sprite = _faceSprites[2];
                break;

            default:
                break;
        }
    }

    public void ShowBack()
    {
        _image.sprite = _backSprite;
    }
}
