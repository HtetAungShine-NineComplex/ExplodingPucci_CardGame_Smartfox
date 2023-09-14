using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DeckHandler : MonoBehaviour
{
    [SerializeField] private TMP_Text _deckCardAmontTxt;

    public void SetDeckAmount(int cardLeft)
    {
        _deckCardAmontTxt.text = "" + cardLeft;
    }
}
