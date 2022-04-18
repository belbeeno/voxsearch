using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

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

    private string _latestVox = string.Empty;
    public UnityEvent<string> OnLatestVoxAssigned;
    public string LatestVox
    {
        get => _latestVox;
        private set
        {
            _latestVox = value;
            if (OnLatestVoxAssigned != null)
            {
                OnLatestVoxAssigned.Invoke(value);
            }
        }
    }

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

    private Regex paddingRegex = new Regex(@"((?<= )|^)([a-zA-Z0-9_']{1,2})(?=($|[\r\n\s ]))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private string PadShortQueries(string query) => paddingRegex.Replace(query, (Match match) => match.Result("$2").PadRight(3, '_'));

    // Should probably find a way to embed this in the dictionary.
    private readonly List<KeyValuePair<string, string>> shorthandLookup = new List<KeyValuePair<string, string>>()
    {
        new KeyValuePair<string, string>(@"\bn1\b", "cnote" ),
        new KeyValuePair<string, string>(@"\bn2\b", "catnote" ),
        new KeyValuePair<string, string>(@"\bn3\b", "cuicanote" ),
        new KeyValuePair<string, string>(@"\bn4\b", "dootnote" ),
        new KeyValuePair<string, string>(@"\bn5\b", "yossynote" ),
        new KeyValuePair<string, string>(@"\bn6\b", "puhnote" ),
        new KeyValuePair<string, string>(@"\bn7\b", "bupnote" ),
        new KeyValuePair<string, string>(@"\bn8\b", "dantnote" ),
        new KeyValuePair<string, string>(@"\bn9\b", "downote" ),
        new KeyValuePair<string, string>(@"\bn10\b", "slapnote" ),
        new KeyValuePair<string, string>(@"\bn11\b", "jarnote" ),
        new KeyValuePair<string, string>(@"\bn12\b", "orchnote" ),
        new KeyValuePair<string, string>(@"\bn13\b", "shynote" ),
        new KeyValuePair<string, string>(@"\bn14\b", "morshunote" ),
        new KeyValuePair<string, string>(@"\bn15\b", "hazymazenote" ),
        new KeyValuePair<string, string>(@"\bn16\b", "hauntnote" ),
        new KeyValuePair<string, string>(@"\bn17\b", "pizzicatonote" ),
        new KeyValuePair<string, string>(@"\bn18\b", "zunnote" ),
        new KeyValuePair<string, string>(@"\bn19\b", "banjonote" ),
        new KeyValuePair<string, string>(@"\bn20\b", "banjonote2" ),
        new KeyValuePair<string, string>(@"\bn21\b", "banjonote3" ),
        new KeyValuePair<string, string>(@"\bn22\b", "diddynote" ),
        new KeyValuePair<string, string>(@"\bn23\b", "diddynote2" ),
        new KeyValuePair<string, string>(@"\bn24\b", "diddynote3" ),
        new KeyValuePair<string, string>(@"\bkk1\b", "kk_na" ),
        new KeyValuePair<string, string>(@"\bkk2\b", "kk_mi" ),
        new KeyValuePair<string, string>(@"\bkk3\b", "kk_me" ),
        new KeyValuePair<string, string>(@"\bkk4\b", "kk_o" ),
        new KeyValuePair<string, string>(@"\bkk5\b", "kk_oh" ),
        new KeyValuePair<string, string>(@"\bkk6\b", "kk_way" ),
        new KeyValuePair<string, string>(@"\bkk7\b", "kk_now" ),
        new KeyValuePair<string, string>(@"\bkk8\b", "kk_whistle" ),
        new KeyValuePair<string, string>(@"\bkk9\b", "kk_howl" ),
        new KeyValuePair<string, string>(@"\bkk10\b", "kk_hm" ),
        new KeyValuePair<string, string>(@"\bkk11\b", "kk_hmlow" ),
        new KeyValuePair<string, string>(@"\bkk12\b", "kk_snare" ),
        new KeyValuePair<string, string>(@"\bkk13\b", "kk_snare2" ),
        new KeyValuePair<string, string>(@"\bkk14\b", "kk_hat" ),
        new KeyValuePair<string, string>(@"\bd1\b", "sonic_snare" ),
        new KeyValuePair<string, string>(@"\bd2\b", "sonic_kick" ),
        new KeyValuePair<string, string>(@"\bd3\b", "sonic_go" ),
        new KeyValuePair<string, string>(@"\bd4\b", "hazymazedrum" ),
        new KeyValuePair<string, string>(@"\bd5\b", "hazymazewood" ),
        new KeyValuePair<string, string>(@"\bd6\b", "yosbongonote" ),
        new KeyValuePair<string, string>(@"\brn\b", "restnote" ),
    };

    private string PreprocessQuery(string query)
    {
        string retVal = query;
        foreach (KeyValuePair<string, string> pair in shorthandLookup) {
            Regex shorthandRx = new Regex(pair.Key);
            retVal = shorthandRx.Replace(retVal, pair.Value);
        }

        return PadShortQueries(retVal);
    }

    public void MakeQuery(string author, string andQuery, string orQuery, string notQuery, Option isSong, Option isMorshu, Option isGrant)
    {
        if (!QueryComplete)
        {
            //Debug.LogError("Attempting to make multiple queries at once!  That's not allowed.");
            GlobalOverlay.AppendMessage("Multiple Queries not allowed!");
        }
        else
        {
            QueryComplete = false;
            StartCoroutine(MakeQueryCoroutine(author, PreprocessQuery(andQuery), PreprocessQuery(orQuery), PreprocessQuery(notQuery), isSong, isMorshu, isGrant));
        }
    }

    private StringBuilder sb = new StringBuilder();
    private IEnumerator MakeQueryCoroutine(string author, string andQuery, string orQuery, string notQuery, Option isSong, Option isMorshu, Option isGrant)
    {
        if (string.IsNullOrWhiteSpace(author)
            && string.IsNullOrWhiteSpace(andQuery)
            && string.IsNullOrWhiteSpace(orQuery)
            && string.IsNullOrWhiteSpace(notQuery)
            && isSong == Option.May
            && isMorshu == Option.May)
        {
            // Nothing to do, exit out.
            GlobalOverlay.AppendMessage("Empty query not allowed!");
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
                sb.AppendFormat("{0}{1}={2}", prefix, name, value.Replace(' ', ';'));
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
        addOptionParameter("g", isGrant);

        string query = sb.ToString();
        Debug.Log("Query: " + query, gameObject);

        using (UnityWebRequest req = UnityWebRequest.Get(query))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                string error = string.Format("Could not access server.  Reason: [{0} Error code: {1}]", req.result.ToString(), req.responseCode.ToString());
                GlobalOverlay.AppendMessage(error);
                QueryComplete = true;
                yield break;
            }

            QueryResult result = JsonUtility.FromJson<QueryResult>(req.downloadHandler.text);
            CurrentResults.AddRange(result.content);

            if (CurrentResults.Count == 0)
            {
                GlobalOverlay.AppendMessage("0 entries found.");
                yield return 0;
            }
            yield return StartCoroutine(BuildEntries(0));
        }
    }

    public void RequestLatest()
    {
        QueryComplete = false;
        StartCoroutine(RequestLatestCoroutine());
    }

    private IEnumerator RequestLatestCoroutine()
    {
        string query = QueryPath + "?latest=1";
        using (UnityWebRequest req = UnityWebRequest.Get(query))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                string error = string.Format("Could not access the server.  Reason: [{0} ErrorCode: {1}]", req.result.ToString(), req.responseCode.ToString());
                GlobalOverlay.AppendMessage(error);
                QueryComplete = true;
            }

            LatestVox = req.downloadHandler.text;
        }

        QueryComplete = true;
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
