using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RankingInput : MonoBehaviour
{
    public List<Toggle> stars;

    private int index = 0;
    private bool skipEvent;

    public UnityEvent<int> onRankingChange;

    void Start()
    {
        stars[0].onValueChanged.AddListener(delegate { OnStarToggled(0, stars[0].isOn); });
		stars[1].onValueChanged.AddListener(delegate { OnStarToggled(1, stars[1].isOn); });
		stars[2].onValueChanged.AddListener(delegate { OnStarToggled(2, stars[2].isOn); });
		stars[3].onValueChanged.AddListener(delegate { OnStarToggled(3, stars[3].isOn); });
		stars[4].onValueChanged.AddListener(delegate { OnStarToggled(4, stars[4].isOn); });
	}

    public int Ranking
    {
        get {
            return index + 1;
        }

        set {
            SetIndex(value - 1);
        }
    }

    private void SetIndex(int value)
    {
        index = value;
            
        for (int i = 0; i < stars.Count; i++)
		{
            if (!stars[i].isOn && i <= index)
                stars[i].isOn = true;

            if (stars[i].isOn && i > index)
                stars[i].isOn = false;
        }
    }

    private void OnStarToggled(int num, bool isOn)
	{
        if (!skipEvent)
        {
            skipEvent = true;
            
            if (isOn)
                SetIndex(num);
            else
                SetIndex(num - 1);

            // Fire custom event
            onRankingChange.Invoke(Ranking);

            skipEvent = false;
        }
	}
}
