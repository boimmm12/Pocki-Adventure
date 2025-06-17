using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using Unity.Collections;
using Unity.VisualScripting;

public class Portal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] int sceneToLoad = -1;
    [SerializeField] DestinationIdentifier destinationPortal;
    [SerializeField] Transform spawnPoint;

    PlayerController player;
    Fader fader;
    public void onPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.isMoving = false;
        this.player = player;
        StartCoroutine(SwitchScene());
    }

    private void Start()
    {
        fader = FindObjectOfType<Fader>();
    }

    IEnumerator SwitchScene()
    {
        DontDestroyOnLoad(gameObject);

        GameController.Instance.PauseGame(true);
        yield return fader.FadeIn(0.5f);

        yield return SceneManager.LoadSceneAsync(sceneToLoad);
        
        var destPortal = FindObjectsOfType<Portal>().First(x => x != this && x.destinationPortal == this.destinationPortal);
        if (destPortal == null)
        {
            Debug.LogError("No destination portal found!");
            yield break;
        }
        player.Character.SetPositionAndSnapToTile(destPortal.SpawnPoint.position);

        yield return fader.FadeOut(0.5f);
        GameController.Instance.PauseGame(false);

        Destroy(gameObject);
    }

    public Transform SpawnPoint => spawnPoint;

    public bool TriggerRepeatedly => false;
}

public enum DestinationIdentifier{A, B, C, D, E}
