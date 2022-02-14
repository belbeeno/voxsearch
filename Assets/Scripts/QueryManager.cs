using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class QueryManager : MonoBehaviour
{
    private static QueryManager _instance;
    public static QueryManager Get()
    {
        if (_instance == null) _instance = FindObjectOfType<QueryManager>();
        return _instance;
    }

    [Header("Query")]
    public CanvasGroup queryGroup = null;
    public string QueryPath = "https://belbeeno.com/rooktools/voxquery.php";
    public bool QueryComplete { get; private set; } = true;

    [Header("Results")]
    public SearchResult resultPrefab = null;
    public CanvasGroup resultGroup = null;
    public RectTransform resultHolder = null;

    [Header("Pages")]
    public int QueryPerPage = 50;
    public int CurrentPage = 0;
    public TMPro.TMP_Text PageDisplay = null;

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
    public List<QueryResult.Vox> CurrentResults { get; private set; }  = new List<QueryResult.Vox>();

    public void OnEnable()
    {
        _prevChildCount = -1;
        resultPrefab.CreatePool();
    }
    public void OnDisable()
    {
        resultPrefab.RemovePool();
    }
    private int _prevChildCount = -1;
    private bool _prevQueryComplete = true;
    private int _prevCurrentResults = 0;
    private void Update()
    {
        if (_prevChildCount == resultHolder.childCount && QueryComplete == _prevQueryComplete && _prevCurrentResults == CurrentResults.Count)
        {
            return;
        }

        if (CurrentResults.Count > 0)
        {
            queryGroup.blocksRaycasts = false;
            queryGroup.interactable = false;
            queryGroup.alpha = 0f;
            resultGroup.blocksRaycasts = true;
            resultGroup.interactable = QueryComplete;
            resultGroup.alpha = 1f;
        }
        else
        {
            queryGroup.blocksRaycasts = true;
            queryGroup.interactable = QueryComplete;
            queryGroup.alpha = 1f;
            resultGroup.blocksRaycasts= false;
            resultGroup.interactable = false;
            resultGroup.alpha = 0f;
        }
        PageDisplay.text = string.Format("{0} to {1} of {2}", CurrentPage * QueryPerPage, CurrentPage * QueryPerPage + resultHolder.childCount, CurrentResults.Count);
        _prevChildCount = resultHolder.childCount;
        _prevQueryComplete = QueryComplete;
        _prevCurrentResults = CurrentResults.Count;
    }

    public enum Option
    {
        May = 0,
        Must,
        Not,
    }

    public void MakeQuery(string author, string andQuery, string orQuery, string notQuery, Option isSong, Option isMorshu)
    {
        if (!QueryComplete)
        {
            Debug.LogError("Attempting to make multiple queries at once!  That's not allowed.");
        }
        else
        {
            QueryComplete = false;
            StartCoroutine(MakeQueryCoroutine(author, andQuery, orQuery, notQuery, isSong, isMorshu));
        }
    }

    private StringBuilder sb = new StringBuilder();
    private IEnumerator MakeQueryCoroutine(string author, string andQuery, string orQuery, string notQuery, Option isSong, Option isMorshu)
    {
        if (string.IsNullOrWhiteSpace(author)
            && string.IsNullOrWhiteSpace(andQuery)
            && string.IsNullOrWhiteSpace(orQuery)
            && string.IsNullOrWhiteSpace(notQuery)
            && isSong == Option.May
            && isMorshu == Option.May)
        {
            // Nothing to do, exit out.
            QueryComplete = true;
            yield break;
        }

        sb.Clear();
        sb.Append(QueryPath);
        char prefix = '?';
        void addStrParameter(string name, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                sb.AppendFormat("{0}{1}={2}", prefix, name, value.Replace(' ', '_'));
                prefix = '&';
            }
        }
        void addOptionParameter(string name, Option value)
        {
            if (value != Option.May)
            {
                sb.AppendFormat("{0}{1}={2}", prefix, name, (value == Option.Must).ToString().ToLower());
                prefix = '&';
            }
        }
        addStrParameter("author", author);
        addStrParameter("qa", andQuery);
        addStrParameter("qo", orQuery);
        addStrParameter("qn", notQuery);
        addOptionParameter("s", isSong);
        addOptionParameter("m", isMorshu);

        string query = sb.ToString();
        Debug.Log("Query: " + query, gameObject);

        using (UnityWebRequest req = UnityWebRequest.Get(query))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(string.Format("Could not access server.  Reason: [{0} Error code: {1}] - {2}", req.result.ToString() ,req.responseCode.ToString(), req.error));
                QueryComplete = true;
                yield break;
            }

            QueryResult result = JsonUtility.FromJson<QueryResult>(req.downloadHandler.text);
            CurrentResults.AddRange(result.content);
            yield return StartCoroutine(BuildEntries(0));
        }
    }

    private void AddSearchEntry(int index)
    {
        QueryResult.Vox currentResult = CurrentResults[index];
        SearchResult newResult = resultPrefab.Spawn(resultHolder);
        newResult.FillWithContent(int.Parse(CurrentResults[index].id), CurrentResults[index].author, CurrentResults[index].log_id, CurrentResults[index].content);
    }

    public void ClearEntries()
    {
        CurrentResults.Clear();
        DropAllEntries();

    }

    private void DropAllEntries()
    {
        while (resultHolder.childCount > 0)
        {
            SearchResult toRecycle = resultHolder.GetChild(resultHolder.childCount - 1).GetComponent<SearchResult>();
            toRecycle.Recycle();
        }
    }

    private IEnumerator BuildEntries(int page)
    {
        QueryComplete = false;
        CurrentPage = page;
        int startIndex = CurrentPage * QueryPerPage;
        int endIndex = Mathf.Min((CurrentPage + 1) * QueryPerPage, CurrentResults.Count);
        for (int currentResultIndex = startIndex; currentResultIndex < endIndex; ++currentResultIndex)
        {
            AddSearchEntry(currentResultIndex);
            yield return 0;
        }
        QueryComplete = true;
    }

    public void DropResult(SearchResult toDrop)
    {
        if (!QueryComplete) return;
        int idx = toDrop.transform.GetSiblingIndex();
        CurrentResults.RemoveAt(idx);
        toDrop.Recycle();

        if (QueryPerPage * (CurrentPage + 1) < CurrentResults.Count)
        {
            AddSearchEntry(QueryPerPage * (CurrentPage + 1) - 1);
        }
    }

    public void PageUp()
    {
        if ((CurrentPage + 1) * QueryPerPage >= CurrentResults.Count) return;
        if (!QueryComplete) return;
        DropAllEntries();
        StartCoroutine(BuildEntries(CurrentPage + 1));
    }

    public void PageDown()
    {
        if (CurrentPage == 0) return;
        if (!QueryComplete) return;
        DropAllEntries();
        StartCoroutine(BuildEntries(CurrentPage - 1));
    }
}
