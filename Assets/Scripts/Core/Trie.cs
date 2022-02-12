using System.Collections.Generic;
using UnityEngine;
using TrieDict = System.Collections.Generic.Dictionary<char, Trie>;

public sealed class Trie
{
    private TrieDict children = new TrieDict();

    public int Count { get; private set; } = 0;
    public bool IsWord { get; private set; } = false;
    private void UpdateCount()
    {
        Count = (IsWord ? 1 : 0);
        TrieDict.ValueCollection childrenNodes = children.Values;
        foreach (Trie child in childrenNodes)
        {
            Count += child.Count;
        }
    }

    public int Weight(string key)
    {
        return Weight(key, 0);
    }
    private int Weight(string key, int keyIndex)
    {
        if (keyIndex >= key.Length)
        {
            // At the end of the key... how many words beneath me do we have?
            return Count;
        }

        Trie child;
        if (children.TryGetValue(key[keyIndex], out child))
        {
            return child.Weight(key, keyIndex + 1);
        }

        // No match found;
        return 0;
    }

    public bool Search(string key)
    {
        return Search(key, 0);
    }
    private bool Search(string key, int keyIndex)
    {
        if (keyIndex < key.Length)
        {
            Trie child;
            if (children.TryGetValue(key[keyIndex], out child))
            {
                return child.Search(key, keyIndex + 1);
            }
            else
            {
                // We've gone off trie, guess I'll die
                return false;
            }
        }

        // at the end of the key... is this a word?
        return IsWord;
    }

    public bool GetPossibleResults(string key, int capacity, ref List<string> retVal)
    {
        retVal.Clear();
        return GetPossibleResults(key, 0, capacity, ref retVal);
    }
    private bool GetPossibleResults(string key, int keyIndex, int capacity, ref List<string> retVal)
    {
        Trie childTrie;
        if (children.TryGetValue(key[keyIndex], out childTrie))
        {
            if (keyIndex >= key.Length - 1)
            {
                // We're at the end of the key.... well, is this a word?
                childTrie.CollectChildren(key, capacity, ref retVal);

                return retVal.Count > 0;
            }
            return childTrie.GetPossibleResults(key, keyIndex + 1, capacity, ref retVal);
        }

        // This word segment isn't in the vocabulary
        retVal.Clear();
        return false;
    }
    private void CollectChildren(string key, int capacity, ref List<string> retVal)
    {
        if (IsWord) retVal.Add(key);
        if (retVal.Count > capacity) return;

        // Still not full?  Try collecting from children
        foreach (char keyChar in children.Keys)
        {
            children[keyChar].CollectChildren(key + keyChar, capacity, ref retVal);
            if (retVal.Count > capacity) return;
        }
    }

    public void Add(string key)
    {
        Add(key, 0);
    }
    private void Add(string key, int keyIndex)
    {
        if (keyIndex >= key.Length)
        {
            IsWord = true;
            UpdateCount();
            return;
        }

        char currentKey = key[keyIndex];
        if (!children.ContainsKey(currentKey))
        {
            children.Add(currentKey, new Trie());
        }
        if (keyIndex < key.Length)
        {
            children[currentKey].Add(key, keyIndex + 1);
        }
        UpdateCount();
    }

    public void Remove(string key)
    {
        Remove(key, 0);
    }
    private bool Remove(string key, int keyIndex)
    {
        if (keyIndex >= key.Length)
        {
            return true;
        }
        char currentKey = key[keyIndex];
        if (children.ContainsKey(currentKey))
        {
            if (Remove(key, keyIndex + 1))
            {
                if (children[currentKey].Count == 0)
                {
                    children.Remove(currentKey);
                }
                UpdateCount();
                return true;
            }
        }
        return false;
    }
}
