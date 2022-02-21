using UnityEngine;
using UnityEngine.UI;

public class FollowLinkButton : MonoBehaviour
{
    public string rootPath = "https://rook.zone/voxlogs/";
    public TMPro.TMP_InputField logEntry = null;
    public Button button = null;

    public void FollowPath()
    {
        Application.OpenURL(rootPath + logEntry.text);
    }

    public void OnTextChanged(string text)
    {
        button.interactable = (logEntry != null && text.Contains("-voxLog.txt"));
    }
}
