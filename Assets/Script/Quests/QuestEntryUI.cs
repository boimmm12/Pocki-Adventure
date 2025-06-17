using UnityEngine;
using UnityEngine.UI;

public class QuestEntryUI : MonoBehaviour
{
    [SerializeField] private Text questNameText;
    [SerializeField] private Text questDescText;

    public void SetData(Quest quest)
    {
        questNameText.text = quest.Base.Name;
        questDescText.text = quest.Base.Description;
    }
}
