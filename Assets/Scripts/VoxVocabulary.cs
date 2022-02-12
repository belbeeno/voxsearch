using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class VoxVocabulary
{
    public static VoxVocabulary _instance = null;
    public static VoxVocabulary Get()
    {
        if (_instance == null)
        {
            _instance = new VoxVocabulary();
        }
        return _instance;
    }
    public static bool HasWord(string word)
    {
        return Get().AllWords.Contains(word);
    }

    public Trie PrefixTree { get; private set; } = new Trie();
    public HashSet<string> AllWords { get; private set; } = new HashSet<string>();

    private VoxVocabulary()
    {
        TextAsset vox_db = Resources.Load<TextAsset>("vox_db");
        int counter = 0;
        using (StringReader sr = new StringReader(vox_db.text))
        {
            string word = sr.ReadLine();
            while (word != null)
            {
                PrefixTree.Add(word);
                AllWords.Add(word);
                ++counter;
                word = sr.ReadLine();
            }
        }
        Resources.UnloadAsset(vox_db);
    }
}