using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomToggleGroupUI : MonoBehaviour
{
    [System.Serializable]
    public class ToggleWindowPair
    {
        public Toggle toggle;
        public ToggleWindow window;
    }

    public ToggleWindowPair[] toggleWindowPairs;

    private void Start()
    {
        foreach (var pair in toggleWindowPairs)
        {
            pair.toggle.onValueChanged.AddListener((isOn) => OnToggleValueChanged(pair, isOn));
        }

        foreach (var pair in toggleWindowPairs)
        {
            pair.window.Active(pair.toggle.isOn);
        }
    }

    private void OnToggleValueChanged(ToggleWindowPair pair, bool isOn)
    {
        if(isOn)
        {
            AudioManager.Instance.PlaySFXNumber(0);
        }
        pair.window.Active(isOn);
    }
}
