using UnityEngine;
using UnityEngine.UI;

public class SearchButton : MonoBehaviour
{
    public TMPro.TMP_InputField authorField = null;
    public TMPro.TMP_InputField andField = null;
    public TMPro.TMP_InputField orField = null;
    public TMPro.TMP_InputField notField = null;
    public TMPro.TMP_Dropdown hasSong = null;
    public TMPro.TMP_Dropdown hasMorshu = null;
    public TMPro.TMP_Dropdown hasGrant = null;

    public QueryManager manager = null;

    private QueryManager.Option IndexToOption(int idx)
    {
        switch (idx)
        {
            case 0:
                return QueryManager.Option.Must;
            case 1:
                return QueryManager.Option.May;
            case 2:
                return QueryManager.Option.Not;
            default:
                throw new System.Exception("Unhandled option when casting to QueryManager.Option");
        }
    }

    public void PerformSearch()
    {
        string author = authorField.text.Trim();
        string and = andField.text.Trim();
        string or = orField.text.Trim();
        string not = notField.text.Trim();
        QueryManager.Option song = IndexToOption(hasSong.value);
        QueryManager.Option morshu = IndexToOption(hasMorshu.value);
        QueryManager.Option grant = IndexToOption(hasGrant.value);

        manager.MakeQuery(author, and, or, not, song, morshu, grant);
    }
}
