using UnityEngine;
using UnityEngine.UI;

public class SearchResult : PooledMonoBehaviour
{
    public int id = -1;
    public Text authorTextBox = null;
    public Text linkTextBox = null;
    public TMPro.TMP_InputField contentTextBox = null;

    public override void OnSpawn()
    {
        transform.localScale = Vector3.one;

        id = -1;
        authorTextBox.text = string.Empty;
        linkTextBox.text = string.Empty;
        contentTextBox.text = string.Empty;
    }

    public void FillWithContent(int _id, string author, string log_id, string content)
    {
        id = _id;
        authorTextBox.text = author;
        linkTextBox.text = log_id;
        contentTextBox.text = content;
    }

    public void Drop()
    {
        QueryManager.Get().DropResult(this);
    }
}
