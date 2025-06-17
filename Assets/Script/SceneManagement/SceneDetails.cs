using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDetails : MonoBehaviour
{
    [SerializeField] List<SceneDetails> connectedScenes;
    [SerializeField] AudioClip sceneMusic;
    [SerializeField] bool useLoadingScreen = false;

    public bool IsLoaded { get; private set; }

    List<SavableEntity> savableEntities;

    // NEW: Static tracking scene
    private static HashSet<string> loadedScenes = new HashSet<string>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            LoadScene();
            GameController.Instance.SetCurrentScene(this);

            if (sceneMusic != null)
                AudioManager.i.PlayMusic(sceneMusic, fade: true);

            foreach (var scene in connectedScenes)
            {
                scene.LoadScene();
            }

            var prevScene = GameController.Instance.PrevScene;
            if (prevScene != null)
            {
                var previouslyLoadedScenes = prevScene.connectedScenes;
                foreach (var scene in previouslyLoadedScenes)
                {
                    if (!connectedScenes.Contains(scene) && scene != this)
                        scene.UnloadScene();
                }

                if (!connectedScenes.Contains(prevScene))
                    prevScene.UnloadScene();
            }
        }
    }

    public void LoadScene()
    {
        if (!IsLoaded)
        {
            bool firstTimeLoad = !loadedScenes.Contains(gameObject.name);

            if (useLoadingScreen && firstTimeLoad)
                StartCoroutine(LoadSceneWithLoading());
            else
                StartCoroutine(LoadSceneDirect());

            loadedScenes.Add(gameObject.name);
        }
    }

    IEnumerator LoadSceneWithLoading()
    {
        LoadingScreenController.i.ShowLoading();
        yield return new WaitForSeconds(0.5f);

        var operation = SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
        IsLoaded = true;

        float timer = 0f;
        while (!operation.isDone || timer < 2f)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            LoadingScreenController.i.SetProgress(progress);
            yield return null;
        }

        LoadingScreenController.i.HideLoading();

        savableEntities = GetSavableEntitiesInScene();
        SavingSystem.i.RestoreEntityStates(savableEntities);
    }

    IEnumerator LoadSceneDirect()
    {
        var operation = SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
        IsLoaded = true;

        operation.completed += (op) =>
        {
            savableEntities = GetSavableEntitiesInScene();
            SavingSystem.i.RestoreEntityStates(savableEntities);
        };

        yield return null;
    }

    public void UnloadScene()
    {
        if (IsLoaded)
        {
            SavingSystem.i.CaptureEntityStates(savableEntities);
            SceneManager.UnloadSceneAsync(gameObject.name);
            IsLoaded = false;
        }
    }

    List<SavableEntity> GetSavableEntitiesInScene()
    {
        var currScene = SceneManager.GetSceneByName(gameObject.name);
        var savableEntities = FindObjectsOfType<SavableEntity>().Where(x => x.gameObject.scene == currScene).ToList();
        return savableEntities;
    }

    public AudioClip SceneMusic => sceneMusic;
}
