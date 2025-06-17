using GDEUtils.StateMachine;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CameraState : State<GameController>
{
    public static CameraState i { get; private set; }
    [SerializeField] GameObject cameraB;
    [SerializeField] RawImage cameraPreview;
    [SerializeField] Button takePhotoButton;
    [SerializeField] Button backButton;
    [SerializeField] ImageClassifier classifier;
    [SerializeField] Image blocker;

    private WebCamTexture webcamTexture;
    private Texture2D capturedPhoto;

    private void Awake()
    {
        i = this;

        takePhotoButton.onClick.AddListener(() => StartCoroutine(CaptureAndClassify()));
        backButton.onClick.AddListener(() => GameController.Instance.StateMachine.Pop());
    }

    public override void Enter(GameController owner)
    {
        cameraB.gameObject.SetActive(true);
        blocker.gameObject.SetActive(true);
        StartCoroutine(StartCamera());
    }

    public override void Exit()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
            webcamTexture.Stop();

        cameraPreview.texture = null;
        cameraB.gameObject.SetActive(false);
        blocker.gameObject.SetActive(false);
    }

    IEnumerator StartCamera()
    {
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.LogWarning("Izin kamera ditolak.");
            yield break;
        }
        var devices = WebCamTexture.devices;
        foreach (var device in devices)
            Debug.Log($"Device: {device.name}, front: {device.isFrontFacing}");

        WebCamDevice backCam = devices.FirstOrDefault(d => !d.isFrontFacing);
        if (backCam.name == null)
        {
            if (WebCamTexture.devices.Length == 0)
            {
                yield break;
            }

            backCam = WebCamTexture.devices[0]; // fallback ke kamera pertama
        }

        webcamTexture = new WebCamTexture(backCam.name, 1280, 720);
        webcamTexture.Play();

        cameraPreview.texture = webcamTexture;
        cameraPreview.rectTransform.localEulerAngles = new Vector3(0, 0, -webcamTexture.videoRotationAngle);

        // 🔧 Auto-resize RawImage sesuai rasio
        float aspectRatio = (float)webcamTexture.width / webcamTexture.height;

        AspectRatioFitter fitter = cameraPreview.GetComponent<AspectRatioFitter>();
        if (fitter != null)
        {
            fitter.aspectRatio = aspectRatio;
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        }
    }

    IEnumerator CaptureAndClassify()
    {
        yield return new WaitForEndOfFrame();

        capturedPhoto = new Texture2D(webcamTexture.width, webcamTexture.height);
        capturedPhoto.SetPixels(webcamTexture.GetPixels());
        capturedPhoto.Apply();

        webcamTexture.Stop();

        yield return classifier.Classify(capturedPhoto);
        string hasil = classifier.LastPrediction;
        Debug.Log("Klasifikasi: " + hasil);

        GameController.Instance.StateMachine.Pop(); // Keluar dari CameraState
        GameController.Instance.StartClassifiedBattle(hasil); // Mulai battle
    }
}
