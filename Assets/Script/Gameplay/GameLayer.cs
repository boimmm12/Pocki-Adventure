using UnityEngine;

public class GameLayer : MonoBehaviour
{
    [SerializeField] public LayerMask solidObjectLayer;
    [SerializeField] public LayerMask interactableLayer;
    [SerializeField] public LayerMask grassLayer;
    [SerializeField] public LayerMask playerLayer;
    [SerializeField] public LayerMask fovLayer;
    [SerializeField] public LayerMask portalLayer;
    [SerializeField] public LayerMask triggersLayer;
    [SerializeField] public LayerMask ledgeLayer;
    [SerializeField] public LayerMask waterLayer;
    [SerializeField] public LayerMask sandLayer;
    [SerializeField] public LayerMask snowLayer;

    public static GameLayer i { get; set; }
    private void Awake()
    {
        i = this;
    }

    public LayerMask SolidLayer
    {
        get => solidObjectLayer | waterLayer;
    }

    public LayerMask InteractableLayer
    {
        get => interactableLayer;
    }

    public LayerMask GrassLayer
    {
        get => grassLayer;
    }

    public LayerMask PlayerLayer
    {
        get => playerLayer;
    }

    public LayerMask FovLayer
    {
        get => fovLayer;
    }
    public LayerMask PortalLayer
    {
        get => portalLayer;
    }
    public LayerMask LedgeLayer
    {
        get => ledgeLayer;
    }

    public LayerMask WaterLayer
    {
        get => waterLayer;
    }
    public LayerMask SandLayer
    {
        get => sandLayer;
    }
        public LayerMask SnowLayer
    {
        get => snowLayer;
    }
    public LayerMask TriggerableLayers
    {
        get => grassLayer | fovLayer | portalLayer | triggersLayer | waterLayer | sandLayer | snowLayer;
    }
}
