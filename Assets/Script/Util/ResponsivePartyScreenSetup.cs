// ResponsivePartyScreenSetup.cs
// Attach this script to a GameObject with a Canvas to programmatically set up a responsive PartyScreen layout.

using UnityEngine;
using UnityEngine.UI;

public class ResponsivePartyScreenSetup : MonoBehaviour
{
    public Canvas canvas;
    public RectTransform partyContainer;
    public RectTransform boxContainer;

    void Start()
    {
        SetupCanvasScaler();
        SetupPartyGrid();
        SetupBoxGrid();
    }

    void SetupCanvasScaler()
    {
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    void SetupPartyGrid()
    {
        var grid = partyContainer.GetComponent<GridLayoutGroup>();
        if (grid == null) grid = partyContainer.gameObject.AddComponent<GridLayoutGroup>();

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.cellSize = new Vector2(250, 250);
        grid.spacing = new Vector2(20, 20);
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;

        var fitter = partyContainer.GetComponent<ContentSizeFitter>();
        if (fitter == null) fitter = partyContainer.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    void SetupBoxGrid()
    {
        var grid = boxContainer.GetComponent<GridLayoutGroup>();
        if (grid == null) grid = boxContainer.gameObject.AddComponent<GridLayoutGroup>();

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;
        grid.cellSize = new Vector2(180, 180);
        grid.spacing = new Vector2(15, 15);
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;

        var fitter = boxContainer.GetComponent<ContentSizeFitter>();
        if (fitter == null) fitter = boxContainer.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }
}
