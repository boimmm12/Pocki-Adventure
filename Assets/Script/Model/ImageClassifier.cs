using UnityEngine;
using Unity.Barracuda;
using System;
using System.Collections;
using System.Linq;

public class ImageClassifier : MonoBehaviour
{
    [Header("Model & Labels")]
    public NNModel modelAsset;
    public string[] labels;

    private Model runtimeModel;
    private IWorker worker;

    public string LastPrediction { get; private set; }

    void Awake()
    {
        runtimeModel = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, runtimeModel);
    }

    public IEnumerator Classify(Texture2D texture)
    {
        Texture2D resized = ResizeTexture(texture, 224, 224); // Sesuaikan dengan input model
        Tensor input = new Tensor(resized, 3); // 3 = RGB

        worker.Execute(input);
        Tensor output = worker.PeekOutput();

        float[] result = output.ToReadOnlyArray();
        int predictedIndex = Array.IndexOf(result, result.Max());

        LastPrediction = labels[predictedIndex];
        Debug.Log("Prediksi: " + LastPrediction);

        input.Dispose();
        output.Dispose();

        yield return null;
    }

    private Texture2D ResizeTexture(Texture2D source, int width, int height)
    {
        RenderTexture rt = RenderTexture.GetTemporary(width, height);
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        Texture2D result = new Texture2D(width, height, TextureFormat.RGB24, false);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return result;
    }

    private void OnDestroy()
    {
        worker?.Dispose();
    }
}
