using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class SearchTrieBar : MonoBehaviour
{
    public float searchCooldown = 1f;
    public TMPro.TMP_InputField textfield = null;
    public AutocompleteBar autocomplete = null;
    public UnityEngine.UI.Image loading = null;

    private Trie vocab = new Trie();
    private float timer = -1f;
    private string SearchQuery { get; set; } = string.Empty;
    private List<string> activeAutocompletes = new List<string>();

    public void Start()
    {
        TextAsset vox_db = Resources.Load<TextAsset>("vox_db");
        int counter = 0;
        using (StringReader sr = new StringReader(vox_db.text))
        {
            string word = sr.ReadLine();
            while (word != null)
            {
                vocab.Add(word);
                ++counter;
                word = sr.ReadLine();
            }
        }
        Debug.Log("Added " + counter + " entries to vocab, of size " + vocab.Count);
        Resources.UnloadAsset(vox_db);
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
                vocab.GetPossibleResults(SearchQuery, 10, ref activeAutocompletes);
                if (activeAutocompletes.Count > 0)
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

    public void OnValueChanged(string content)
    {
        if (string.IsNullOrEmpty(content) || content.EndsWith(' '))
        {
            // Ignore.
            SearchQuery = string.Empty;
            timer = searchCooldown;
            return;
        }

        int firstChar = content.LastIndexOf(' ') + 1;
        SearchQuery = content.Substring(firstChar);
        timer = searchCooldown;
    }
}