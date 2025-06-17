using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class FishStorageUI : MonoBehaviour
{
    [SerializeField] private GameObject fishSlotPrefab;
    [SerializeField] private Transform fishListParent;
    [SerializeField] private Button exitButton;
    [SerializeField] private int maxFishSlot = 9;
    [SerializeField] private Text totalPriceText;
    [SerializeField] private GameObject baitSlotPrefab;
    [SerializeField] private Transform baitListParent;
    private List<GameObject> activeBaitSlots = new List<GameObject>();
    private List<GameObject> activeSlots = new List<GameObject>();

    public static FishStorageUI i { get; private set; }

    private void Awake()
    {
        if (i == null)
            i = this;
        else
            Destroy(gameObject);
        exitButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    private void OnEnable()
    {
        FishInventory.i.OnUpdated += RefreshUI;
        RefreshUI();
    }

    private void OnDisable()
    {
        FishInventory.i.OnUpdated -= RefreshUI;
    }

    private void RefreshUI()
    {
        Debug.Log("[FishStorageUI] RefreshUI dipanggil");

        // 🔄 Bersihkan semua slot ikan lama
        foreach (Transform child in fishListParent)
            Destroy(child.gameObject);
        activeSlots.Clear();

        var fishList = FishInventory.i.GetFishList();

        for (int i = 0; i < maxFishSlot; i++)
        {
            var go = Instantiate(fishSlotPrefab, fishListParent);
            var slotUI = go.GetComponent<FishSlotUI>();

            if (i < fishList.Count)
            {
                var fish = fishList[i];
                slotUI?.SetData(fish); // Tampilkan data ikan
            }
            else
            {
                slotUI?.ClearData(); // Kosongkan slot
            }

            activeSlots.Add(go);
        }

        // 💰 Hitung total harga ikan
        int totalPrice = fishList.Sum(f => f.GetPrice());
        totalPriceText.text = $"₵ {totalPrice:N0}";

        // 🔄 Bersihkan semua slot bait lama
        foreach (Transform child in baitListParent)
            Destroy(child.gameObject);
        activeBaitSlots.Clear();

        var baitList = FishInventory.i.GetAllBaits();

        foreach (var baitSlot in baitList)
        {
            var go = Instantiate(baitSlotPrefab, baitListParent);
            var baitSlotUI = go.GetComponent<BaitSlotUI>();
            baitSlotUI?.SetData(baitSlot);
            activeBaitSlots.Add(go);
        }
    }
    public void Show()
    {
        RefreshUI(); // Tambahkan ini!
        gameObject.SetActive(true);
    }

}
