using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchResult : MonoBehaviour
{
    public Text authorTextBox = null;
    public Text linkTextBox = null;
    public TMPro.TextMeshProUGUI contentTextBox = null;
    public void FillWithContent(string author, string log_id, string content)
    {
        authorTextBox.text = author;
        linkTextBox.text = log_id;
        contentTextBox.text = content;
    }
}
