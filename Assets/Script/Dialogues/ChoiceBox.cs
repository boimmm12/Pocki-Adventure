using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]

public class ChoiceBox : MonoBehaviour
{
    [SerializeField] ChoiceText choiceTextPrefab;

    bool choiceSelected = false;

    List<ChoiceText> choiceTexts;
    int currentChoice;

    public IEnumerator ShowChoices(List<string> choices, Action<int> onChoiceSelected, Action onCancel)
    {
        choiceSelected = false;
        gameObject.SetActive(true);
        currentChoice = 0;

        foreach (Transform child in transform)
            Destroy(child.gameObject);

        choiceTexts = new List<ChoiceText>();

        for (int i = 0; i < choices.Count; i++)
        {
            var choiceTextObj = Instantiate(choiceTextPrefab, transform);
            choiceTextObj.TextField.text = choices[i];
            choiceTexts.Add(choiceTextObj);

            int choiceIndex = i;

            choiceTextObj.Button.onClick.RemoveAllListeners();
            choiceTextObj.Button.onClick.AddListener(() =>
            {
                currentChoice = choiceIndex;
                choiceSelected = true;
            });
        }
        yield return new WaitUntil(() => choiceSelected == true);

        onChoiceSelected?.Invoke(currentChoice);
        gameObject.SetActive(false);
    }
}