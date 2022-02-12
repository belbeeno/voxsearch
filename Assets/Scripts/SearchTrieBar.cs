using System.Collections.Generic;
using UnityEngine;

public class SearchTrieBar : MonoBehaviour
{
    public float searchCooldown = 1f;
    public TMPro.TMP_InputField textfield = null;
    public UnityEngine.UI.Text placeholder = null;
    public AutocompleteBar autocomplete = null;
    public UnityEngine.UI.Image loading = null;

    private float timer = -1f;
    private string SearchQuery { get; set; } = string.Empty;
    private List<string> activeAutocompletes = new List<string>();
    private string defaultPlaceholderText = string.Empty;

    private void Start()
    {
        defaultPlaceholderText = placeholder.text;
    }

    private void Update()
    {
        autocomplete.hidden = !textfield.isFocused;
        if (timer <= 0f)
        {
            return;
        }

        timer -= Time.deltaTime;
        loading.enabled = timer > 0f;
        if (timer <= 0f)
        {
            if (string.IsNullOrEmpty(SearchQuery))
            {
                autocomplete.ClearBar();
            }
            else
            {
                if (VoxVocabulary.Get().PrefixTree.GetPossibleResults(SearchQuery, 10, ref activeAutocompletes))
                {
                    autocomplete.Assign(activeAutocompletes);
                }
                else
                {
                    autocomplete.ClearBar();
                }
            }
        }
    }

    private System.Text.StringBuilder _sb = new System.Text.StringBuilder();
    public void OnValueChanged(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            // Ignore.
            SearchQuery = string.Empty;
            timer = searchCooldown;
            return;
        }
        else if (content.EndsWith(' '))
        {
            string[] split_query = content.Split(' ');
            _sb.Clear();
            for (int i = 0; i < split_query.Length - 2; ++i)
            {
                _sb.Append(split_query[i] + " ");
            }

            string lastWord = split_query[split_query.Length - 2].ToLower();
            if (!string.IsNullOrWhiteSpace(lastWord) && !VoxVocabulary.HasWord(lastWord))
            {
                string suggestion = autocomplete.GetTopSuggestion();
                if (!string.IsNullOrEmpty(suggestion))
                {
                    Debug.Assert(suggestion.StartsWith(lastWord));
                    _sb.Append(suggestion + " ");
                }
                // else, just drop it.  idk what it is lol

                textfield.text = _sb.ToString();
                textfield.MoveToEndOfLine(false, false);
                SearchQuery = string.Empty;
                timer = searchCooldown;
                return;
            }

        }

        int firstChar = content.LastIndexOf(' ') + 1;
        SearchQuery = content.Substring(firstChar);
        timer = searchCooldown;
    }

    public void ValidateString(string content)
    {
        //Debug.Log("validating...");
        string[] split_query = content.Split(' ');
        _sb.Clear();
        List<string> suggestions = new List<string>();
        for (int i = 0; i < split_query.Length; ++i)
        {
            string currentWord = split_query[i];
            if (VoxVocabulary.HasWord(currentWord))
            {
                _sb.Append(currentWord + " ");
            }
            else if (!string.IsNullOrWhiteSpace(currentWord))
            {
                // Alright, any suggestions?
                if (VoxVocabulary.Get().PrefixTree.GetPossibleResults(currentWord, 1, ref suggestions))
                {
                    _sb.Append(suggestions[0] + " ");
                }
            }
        }

        textfield.text = _sb.ToString();
    }
}