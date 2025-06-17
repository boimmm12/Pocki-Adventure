using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScreenController : MonoBehaviour
{
    public static LoadingScreenController i;

    [SerializeField] GameObject loadingScreenObj;
    [SerializeField] private Text loadingText;
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private Text tipsText;

    [TextArea]
    [SerializeField] private List<string> tipsList;

    private int dotCount = 0;
    private float dotTimer = 0f;
    private float dotInterval = 0.5f;

    private bool isLoading = false;
    private float loadingTimer = 0f;

    private void Awake()
    {
        if (i == null)
        {
            i = this;
            DontDestroyOnLoad(gameObject);
            HideLoading();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (loadingScreenObj == null || !loadingScreenObj.activeSelf) return;

        AnimateLoadingText();
        loadingTimer += Time.deltaTime;

        if (IsScreenClicked())
        {
            ShowRandomTip();
        }
    }

    private bool IsScreenClicked()
    {
        if (Input.GetMouseButtonDown(0))
            return true;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            return true;

        return false;
    }

    private void AnimateLoadingText()
    {
        dotTimer += Time.deltaTime;
        if (dotTimer >= dotInterval)
        {
            dotCount = (dotCount + 1) % 4;
            loadingText.text = "Loading" + new string('.', dotCount);
            dotTimer = 0f;
        }
    }

    public void ShowLoading()
    {
        loadingScreenObj.SetActive(true);
        loadingText.text = "Loading";
        loadingSlider.value = 0f;
        ShowRandomTip();
        isLoading = true;
        loadingTimer = 0f;
    }

    public void HideLoading()
    {
        loadingScreenObj.SetActive(false);
        isLoading = false;
    }

    private void ShowRandomTip()
    {
        if (tipsList == null || tipsList.Count == 0)
        {
            tipsText.text = "";
            return;
        }

        int randomIndex = Random.Range(0, tipsList.Count);
        tipsText.text = tipsList[randomIndex];
    }

    public void SetProgress(float progress)
    {
        if (loadingSlider != null)
        {
            loadingSlider.value = progress;
        }
    }
    public void StartLoadingScene(int sceneIndex, System.Action onSceneLoaded = null)
    {
        StartCoroutine(LoadSceneRoutine(sceneIndex, onSceneLoaded));
    }

    IEnumerator LoadSceneRoutine(int sceneIndex, System.Action onSceneLoaded)
    {
        ShowLoading();
        yield return new WaitForSeconds(0.5f);

        var operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false;

        loadingTimer = 0f;
        while (operation.progress < 0.9f || loadingTimer < 5f)
        {
            loadingTimer += Time.deltaTime;

            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            SetProgress(progress);
            yield return null;
        }

        var mainMenuController = FindObjectOfType<MainMenuController>();
        if (mainMenuController != null)
        {
            Destroy(mainMenuController.transform.parent.gameObject);
        }

        operation.allowSceneActivation = true;
        yield return new WaitForEndOfFrame();


        onSceneLoaded?.Invoke();
        HideLoading();
    }

}
