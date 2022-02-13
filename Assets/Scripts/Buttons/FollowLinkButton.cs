using UnityEngine;
using UnityEngine.UI;

public class FollowLinkButton : MonoBehaviour
{
    public string rootPath = "https://rook.zone/vox_logs/";
    public Text logEntry = null;
    public Button button = null;

    public void FollowPath()
    {
        if (!string.IsNullOrWhiteSpace(logEntry.text))
        {
            Application.OpenURL(rootPath + logEntry.text);
        }
    }

    private void Update()
    {
        button.interactable = (logEntry == null || string.IsNullOrWhiteSpace(logEntry.text));
    }
}
