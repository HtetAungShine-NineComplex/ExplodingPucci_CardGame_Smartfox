using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class AvatorData : ScriptableObject
{
    public Avator[] avators;

    public int AvatorCount
    {
        get
        {
            return avators.Length;
        }
        
    }

    public Avator GetAvator(int index)
    {
        return avators[index];
    }
}
