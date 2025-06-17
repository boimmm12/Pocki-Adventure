using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;  // New Input System
using UnityEngine.UI;
using UnityEngine.SceneManagement;



public class PlayerController : MonoBehaviour, ISavable
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;
    [SerializeField] public Button interactButton;
    [SerializeField] private Button cameraButton;
    [SerializeField] private Button fishingButton;

    [SerializeField] private ItemBase cameraItem;

    private Vector2 input;
    private int presetIndex;
    public int PresetIndex => presetIndex;

    private string playerName;
    private Vector2 lastInput;

    public static PlayerController i { get; private set; }

    private Character character;

    private InputSystem_Actions playerInputActions;
    private bool isInputEnabled = true; // Track input state
    private Inventory inventory;


    private void Awake()
    {
        i = this;
        playerInputActions = new InputSystem_Actions(); // Initialize input actions
        character = GetComponent<Character>();
        inventory = GetComponent<Inventory>();
    }

    public void SetNameAndSprite(string newName, Sprite newSprite)
    {
        name = newName;
        sprite = newSprite;
        GetComponent<SpriteRenderer>().sprite = newSprite;
    }

    private void OnEnable()
    {
        playerInputActions.Enable();
        playerInputActions.Player.Move.performed += ctx => OnMove(ctx.ReadValue<Vector2>());
        playerInputActions.Player.Move.canceled += ctx => input = Vector2.zero;
    }

    private void OnDisable()
    {
        playerInputActions.Disable();
    }

    public void EnableInput()
    {
        isInputEnabled = true;
        playerInputActions.Enable();
    }

    public void DisableInput()
    {
        isInputEnabled = false;
        playerInputActions.Disable();
    }

    public void OnMove(Vector2 moveInput)
    {
        if (!isInputEnabled) return;

        if (moveInput == Vector2.zero)
        {
            input = Vector2.zero;
            return;
        }

        if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
        {
            input = new Vector2(Mathf.Sign(moveInput.x), 0);
        }
        else
        {
            input = new Vector2(0, Mathf.Sign(moveInput.y));
        }
    }

    private void Start()
    {
        if (PlayerSetupUI.i != null)
        {
            SetNameAndPreset(name, PlayerSetupUI.i.characterPresets[PlayerSetupUI.i.CurrentPresetIndex]);
        }

        if (interactButton != null)
        {
            interactButton.onClick.AddListener(() =>
            {
                Debug.Log("inter tombol diklik!");
                OnInteractButtonClicked();
            });
        }
        if (cameraButton != null)
        {
            cameraButton.onClick.AddListener(() =>
            {
                Debug.Log("[UpdateCameraButton] Camera button diklik.");
                GameController.Instance.EnterCameraState();
            });
        }

        if (fishingButton != null)
        {
            fishingButton.onClick.AddListener(() =>
            {
                StartCoroutine(Fish());
            });
        }

    }
    private void OnInteractButtonClicked()
    {
        if (character.isMoving || !interactButton.interactable) return;

        interactButton.interactable = false;
        StartCoroutine(Interact());
    }

    public void HandleUpdate()
    {
        if (!character.isMoving && input != Vector2.zero)
        {
            lastInput = input;
            StartCoroutine(character.Move(input, OnMoveOver));
        }

        CheckForInteractables();
        character.HandleUpdate();
    }

    IEnumerator Interact()
    {
        var interactPos = transform.position + (Vector3)lastInput.normalized;
        var collider = Physics2D.OverlapCircle(interactPos, 0.5f, GameLayer.i.InteractableLayer | GameLayer.i.WaterLayer);
        if (collider != null)
        {
            var interactable = collider.GetComponent<Interactable>();

            if (interactable != null && !(interactable is FishableWater))
            {
                yield return interactable.Interact(transform);
            }
        }

        yield return new WaitForSeconds(0.2f);
        interactButton.interactable = true;
    }

    private void CheckForInteractables()
    {
        var checkPos = transform.position + (Vector3)lastInput.normalized;
        var colliders = Physics2D.OverlapCircleAll(checkPos, 0.5f, GameLayer.i.InteractableLayer | GameLayer.i.WaterLayer);

        interactButton.gameObject.SetActive(false);
        fishingButton.gameObject.SetActive(false);

        foreach (var col in colliders)
        {
            if (col.GetComponent<FishableWater>() != null && inventory.HasItemByName("FishingRod"))
            {
                fishingButton.gameObject.SetActive(true);
            }

            var interactable = col.GetComponent<Interactable>();
            if (interactable != null && !(interactable is FishableWater))
            {
                interactButton.gameObject.SetActive(true);
            }
        }
    }

    public void UpdateCameraButton()
    {
        if (inventory == null)
        {
            inventory = GetComponent<Inventory>();
        }

        bool hasItem = inventory.HasItemByName("StrangeDevice");

        cameraButton.gameObject.SetActive(hasItem);
    }

    private IEnumerator Fish()
    {
        if (character.isMoving || !fishingButton.interactable) yield break;

        fishingButton.interactable = false;

        var interactPos = transform.position + (Vector3)lastInput.normalized;
        var collider = Physics2D.OverlapCircle(interactPos, 0.5f, GameLayer.i.WaterLayer);

        var fishable = collider?.GetComponent<FishableWater>();
        if (fishable != null)
        {
            yield return fishable.Interact(transform);
        }

        yield return new WaitForSeconds(0.2f);
        fishingButton.interactable = true;
    }

    IPlayerTriggerable currentlyInTrigger;
    private void OnMoveOver()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.offsetY), 0.2f, GameLayer.i.TriggerableLayers);

        IPlayerTriggerable triggerable = null;
        foreach (var collider in colliders)
        {
            triggerable = collider.GetComponent<IPlayerTriggerable>();
            if (triggerable != null)
            {
                if (triggerable == currentlyInTrigger && !triggerable.TriggerRepeatedly)
                    break;

                triggerable.onPlayerTriggered(this);
                currentlyInTrigger = triggerable;
                break;
            }
        }

        if (colliders.Count() == 0 || triggerable != currentlyInTrigger)
            currentlyInTrigger = null;
    }

    public void SetNameAndPreset(string name, CharacterPreset preset)
    {
        this.name = name;
        playerName = name; // Save name lokal
        sprite = preset.idleDown;
        presetIndex = PlayerSetupUI.i != null ? PlayerSetupUI.i.CurrentPresetIndex : 0;

        var animator = GetComponent<CharacterAnimator>();
        animator.SetCharacterPreset(preset);
    }


    public object CaptureState()
    {
        var saveData = new PlayerSaveData()
        {
            position = new float[] { transform.position.x, transform.position.y },
            pokemons = GetComponent<PockiParty>().Pockies.Select(p => p.GetSaveData()).ToList(),
            playerName = playerName,
            presetIndex = presetIndex,
            currentScene = SceneManager.GetActiveScene().name,
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = (PlayerSaveData)state;

        var pos = saveData.position;
        transform.position = new Vector3(pos[0], pos[1]);

        var party = GetComponent<PockiParty>();
        party.Pockies.Clear(); // 🧼 Hapus semua yang lama

        party.Pockies = saveData.pokemons.Select(s => new Pocki(s)).ToList();
        party.PartyUpdated(); // ⬅️ Penting untuk refresh UI/logic lain

        this.playerName = saveData.playerName;
        this.name = saveData.playerName;
        this.presetIndex = saveData.presetIndex;

        var preset = GlobalCharacterPresets.i.GetPreset(presetIndex);
        if (preset != null)
        {
            sprite = preset.idleDown;
            GetComponent<CharacterAnimator>().SetCharacterPreset(preset);
        }
    }

    public string Name
    {
        get => name;
    }

    public Sprite Sprite
    {
        get => sprite;
    }
    public Character Character => character;
    public PockiParty Party => GetComponent<PockiParty>();

}

[Serializable]
public class PlayerSaveData
{
    public float[] position;
    public List<PockiSaveData> pokemons;

    public string playerName;
    public int presetIndex;
    public string currentScene;

}
