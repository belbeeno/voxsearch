using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutocompleteEntry : MonoBehaviour
{
    public Text text;
    public Image bar;

    private void OnEnable()
    {
        text.text = string.Empty;
    }

    public void SetText(string newText, bool isEnd)
    {
        text.text = newText;
        bar.enabled = !isEnd;
    }
}
