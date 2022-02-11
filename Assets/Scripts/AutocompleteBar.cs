using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutocompleteBar : MonoBehaviour
{
    public RectTransform myRect;
    public Vector2 anchoredStart = new Vector2(0f, 0f);
    public Vector2 anchoredEnd = new Vector2(0f, 1f);
    public AnimationCurve hideCurve;
    public AutocompleteEntry[] entries = new AutocompleteEntry[10];

    public bool hidden = true;
    private float hideTimer = 0f;

    private void OnEnable()
    {
        hidden = true;
        hideTimer = 0f;
        myRect.anchoredPosition = Vector2.LerpUnclamped(anchoredStart, anchoredEnd, hideCurve.Evaluate(hideTimer));
        ClearBar();
    }

    public void ClearBar()
    {
        for (int i = 0; i < entries.Length; ++i)
        {
            entries[i].gameObject.SetActive(false);
        }
    }

    public void Assign(List<string> terms)
    {
        for (int i = 0; i < entries.Length; ++i)
        {
            if (i < terms.Count)
            {
                entries[i].gameObject.SetActive(true);
                entries[i].SetText(terms[i], i == terms.Count - 1);
            }
            else
            {
                entries[i].gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        Debug.Assert(hideCurve.length > 0);

        float end = hideCurve.keys[hideCurve.length - 1].time;
        if (hidden)
        {
            if (hideTimer <= 0f) {
                return;
            }
            hideTimer = Mathf.Max(hideTimer - Time.deltaTime, 0f);
            if (hideTimer <= 0f)
            {
                ClearBar();
            }
        }
        else
        {
            if (hideTimer >= end) {
                return;
            }
            hideTimer = Mathf.Min(hideTimer + Time.deltaTime, end);
        }

        myRect.anchoredPosition = Vector2.LerpUnclamped(anchoredStart, anchoredEnd, hideCurve.Evaluate(hideTimer));
    }

    public void OnBarSelected()
    {
        hidden = false;
    }

    public void OnBarDeselected()
    {
        hidden = true;
    }
}
