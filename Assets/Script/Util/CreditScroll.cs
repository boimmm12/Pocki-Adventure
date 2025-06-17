using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CreditScroll : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private float scrollSpeed = 10f;
    [SerializeField] private float resetDelay = 5f;
    private bool isUserScrolling = false;
    private float idleTimer = 0f;

    void Update()
    {
        if (isUserScrolling)
        {
            idleTimer += Time.unscaledDeltaTime;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                transform.root.Find("Credit").gameObject.SetActive(false);
                return;
            }
            if (idleTimer >= resetDelay)
            {
                isUserScrolling = false;
                idleTimer = 0f;
            }
        }

        if (!isUserScrolling)
        {
            float newPos = scrollRect.verticalNormalizedPosition - scrollSpeed * Time.unscaledDeltaTime;
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(newPos);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isUserScrolling = true;
        idleTimer = 0f;
    }
}
