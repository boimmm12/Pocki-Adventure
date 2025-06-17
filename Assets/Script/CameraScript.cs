using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public float referenceWidth = 1080f;
    public float referenceHeight = 1920f;

    void Start()
    {
        float targetAspect = referenceWidth / referenceHeight;
        float screenAspect = (float)Screen.width / (float)Screen.height;

        if (screenAspect >= targetAspect)
        {
            Camera.main.orthographicSize = referenceHeight / 200f / 2f;
        }
        else
        {
            float differenceInSize = targetAspect / screenAspect;
            Camera.main.orthographicSize = referenceHeight / 200f / 2f * differenceInSize;
        }
    }
}