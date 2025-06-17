using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceText : MonoBehaviour
{
    public Button Button;
    public Button ConfirmButton;
    Text text;
    private void Awake()
    {
        text = GetComponent<Text>();
    }
    public Text TextField => text;
}
