using UnityEngine;

public class KeyboardSafeAreaHandler : MonoBehaviour
{
    [SerializeField] private RectTransform uiRootToShift;
    [SerializeField] private float offset = 1000f;

    private bool keyboardVisible = false;
    private Vector2 originalAnchoredPos;

    private void Start()
    {
        if (uiRootToShift != null)
            originalAnchoredPos = uiRootToShift.anchoredPosition;
    }

    void Update()
    {
#if UNITY_ANDROID
        if (TouchScreenKeyboard.visible)
        {
            if (!keyboardVisible)
            {
                keyboardVisible = true;
                ShiftUIUp();
            }
        }
        else
        {
            if (keyboardVisible)
            {
                keyboardVisible = false;
                ResetUI();
            }
        }
#endif
    }

    void ShiftUIUp()
    {
        if (uiRootToShift != null)
            uiRootToShift.anchoredPosition = originalAnchoredPos + new Vector2(0, offset);
    }

    void ResetUI()
    {
        if (uiRootToShift != null)
            uiRootToShift.anchoredPosition = originalAnchoredPos;
    }
}
