using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class QueryManager : MonoBehaviour
{
    public SearchResult testResult = null;
    public string QueryPath = "https://belbeeno.com/rooktools/voxquery.php";

    [System.Serializable]
    public struct QueryResult
    {
        [System.Serializable]
        public struct Vox
        {
            public string id;
            public string author;
            public string log_id;
            public string date;
            public string content;
        }
        public List<Vox> content;
        public override string ToString()
        {
            StringBuilder _sb = new StringBuilder();
            for (int i = 0; i < content.Count; ++i)
            {
                _sb.AppendFormat("Vox Entry {0} by {1} in {2}:\n{3}\n", content[i].id, content[i].author, content[i].log_id, content[i].content);
            }
            return _sb.ToString();
        }
    }

    public void Start()
    {
        StartCoroutine(MakeQuery("belbeeno"));
    }

    private StringBuilder sb = new StringBuilder();
    public IEnumerator MakeQuery(string author)
    {
        sb.Clear();
        sb.Append(QueryPath);
        sb.AppendFormat("?author={0}", author);
        string query = sb.ToString();

        using (UnityWebRequest req = UnityWebRequest.Get(query))
        {
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                //JsonUtility.FromJson<>(req.downloadHandler.text);
                QueryResult result = JsonUtility.FromJson<QueryResult>(req.downloadHandler.text);
                testResult.FillWithContent(result.content[0].author, result.content[0].log_id, result.content[0].content);
            }
            else
            {
                Debug.LogError("Could not access server.  Reason: " + req.result.ToString() + ", Error code: " + req.responseCode.ToString());
            }
        }
    }
}
