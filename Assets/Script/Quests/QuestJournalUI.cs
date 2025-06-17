using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestJournalUI : MonoBehaviour
{
    [SerializeField] private GameObject questEntryPrefab;
    [SerializeField] private Transform questListContent;
    [SerializeField] private GameObject panel;
    [SerializeField] private Button back;

    private QuestList questList;

    private void Awake()
    {
        questList = QuestList.GetQuestList();
        back.onClick.AddListener(() => panel.gameObject.SetActive(false));
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        // Clear UI dulu
        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }

        // Tambahkan semua quest aktif
        foreach (var quest in questList.GetAllActiveQuests())
        {
            var questEntryObj = Instantiate(questEntryPrefab, questListContent);
            var questEntryUI = questEntryObj.GetComponent<QuestEntryUI>();
            questEntryUI.SetData(quest);
        }
    }
    
}
