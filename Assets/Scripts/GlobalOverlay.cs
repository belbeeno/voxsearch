using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animation))]
public class GlobalOverlay : MonoBehaviour
{
    public Text textfield = null;
    public Animation anim = null;

    public string[] greetings = { "Greetis...", "Luppy!", "Luppy luppy!", "Gamer's greetings to you!", "Hi!", "Hi hi!", "Heyo!", "Hello!", "Howdy!", "Helo!", "Rally-huh...??", "Bengal Hello" };

    private static GlobalOverlay _instance = null;
    public static GlobalOverlay Get()
    {
        if (_instance == null)
        {
            _instance = GameObject.FindWithTag("GlobalOverlay").GetComponent<GlobalOverlay>();
        }
        return _instance;
    }

    private Queue<string> messages = new Queue<string>();

    private void Start()
    {
        if (anim == null) anim = GetComponent<Animation>();
        textfield.text = greetings[Random.Range(0, greetings.Length)];
    }

    public static void AppendMessage(string msg)
    {
        Get().messages.Enqueue(msg);
    }

    private void Update()
    {
        if (anim.isPlaying) return;
        if (messages.Count > 0)
        {
            textfield.text = messages.Dequeue();
            anim.Rewind();
            anim.Play();
        }
    }
}
