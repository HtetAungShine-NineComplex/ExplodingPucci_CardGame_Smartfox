using System.Collections;
using System.Collections.Generic;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;
using UnityEngine;
using UnityEngine.UI;

public class PutCardHandler : MonoBehaviour
{
    [SerializeField] private Button _topBtn;
    [SerializeField] private Button _secondBtn;
    [SerializeField] private Button _thirdBtn;
    [SerializeField] private Button _lastBtn;

    private void Start()
    {
        _topBtn.onClick.AddListener(() => PutCardBackOnDeck(ExtensionEventNames.FIRST));
        _secondBtn.onClick.AddListener(() => PutCardBackOnDeck(ExtensionEventNames.SECOND));
        _thirdBtn.onClick.AddListener(() => PutCardBackOnDeck(ExtensionEventNames.THIRD));
        _lastBtn.onClick.AddListener(() => PutCardBackOnDeck(ExtensionEventNames.LAST));
    }

    private void PutCardBackOnDeck(string position)
    {
        ISFSObject requestObject = new SFSObject();
        requestObject.PutUtfString(ExtensionEventNames.CARD_POSITION, position);

        ExtensionRequest extensionRequest = new ExtensionRequest(ExtensionEventNames.PUT_CARD_ON_DECK, requestObject);
        GlobalManager.Instance.GetSfsClient().Send(extensionRequest);
    }
}
