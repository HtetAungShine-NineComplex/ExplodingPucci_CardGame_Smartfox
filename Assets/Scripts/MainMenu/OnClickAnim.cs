using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnClickAnim : MonoBehaviour
{
    public void OnPressed()
    {
        GetComponent<Animation>().Play("Pressed");
    }
}
