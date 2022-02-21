using UnityEngine;

[RequireComponent(typeof(Animation))]
public class RequestLatestLog : MonoBehaviour
{
    public TMPro.TMP_Text text = null;

    public void OnEnable()
    {
        QueryManager.Get().OnLatestVoxAssigned.AddListener(OnLatextVoxFound);
        QueryManager.Get().RequestLatest();
    }

    public void OnDisable()
    {
        if (QueryManager.Get())
        {
            QueryManager.Get().OnLatestVoxAssigned.RemoveListener(OnLatextVoxFound);
        }
    }

    public void OnLatextVoxFound(string vox)
    {
        text.text = vox;
        Animation anim = GetComponent<Animation>();
        anim.Play();
    }
}
