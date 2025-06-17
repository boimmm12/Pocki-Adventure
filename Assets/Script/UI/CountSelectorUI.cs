using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CountSelectorUI : MonoBehaviour
{
    [SerializeField] Text countText;
    [SerializeField] Text priceText;
    [SerializeField] Button Up;
    [SerializeField] Button Down;
    [SerializeField] Button Confirm;

    bool selected;
    int currentCount;
    int maxCount;
    float pricePerUnit;

    void Start()
    {
        Up.onClick.RemoveAllListeners();
        Down.onClick.RemoveAllListeners();
        Confirm.onClick.RemoveAllListeners();

        Up.onClick.AddListener(() =>
        {
            ++currentCount;
            SetValues();
        });

        Down.onClick.AddListener(() =>
        {
            --currentCount;
            SetValues();
        });

        Confirm.onClick.AddListener(() =>
        {
            selected = true;
        });
    }


    public IEnumerator ShowSelector(int maxCount, float pricePerUnit,
    Action<int> onCountSelected)
{
    this.maxCount = maxCount;
    this.pricePerUnit = pricePerUnit;

    this.currentCount = 1;

    selected = false;

    gameObject.SetActive(true);
    SetValues();

    yield return new WaitUntil(() => selected == true);

    onCountSelected?.Invoke(currentCount);
    gameObject.SetActive(false);
}

    void Update()
    {
        int prevCount = currentCount;

        currentCount = Mathf.Clamp(currentCount, 1, maxCount);

        if (currentCount != prevCount)
            SetValues();
    }

    void SetValues()
    {
        countText.text = "x " + currentCount;
        priceText.text = "$ " + pricePerUnit * currentCount;
    }
}
