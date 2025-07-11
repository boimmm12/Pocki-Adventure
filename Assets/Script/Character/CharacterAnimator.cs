using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] List<Sprite> walkDownSprites;
    [SerializeField] List<Sprite> walkUpSprites;
    [SerializeField] List<Sprite> walkRightSprites;
    [SerializeField] List<Sprite> walkLeftSprites;
    [SerializeField] List<Sprite> surfSprites;
    [SerializeField] Sprite fishingSprite;
    [SerializeField] FacingDirection defaultDirection = FacingDirection.Down;

    // Parameters
    public float MoveX { get; set; }
    public float MoveY { get; set; }
    public bool isMoving { get; set; }
    public bool isJumping { get; set; }
    public bool isSurfing { get; set; }
    public bool IsFishing { get; set; }

    // States
    SpriteAnimator walkDownAnim;
    SpriteAnimator walkUpAnim;
    SpriteAnimator walkRightAnim;
    SpriteAnimator walkLeftAnim;

    SpriteAnimator currentAnim;
    bool wasPreviouslyMoving;

    // Refrences
    SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        walkDownAnim = new SpriteAnimator(walkDownSprites, spriteRenderer);
        walkUpAnim = new SpriteAnimator(walkUpSprites, spriteRenderer);
        walkRightAnim = new SpriteAnimator(walkRightSprites, spriteRenderer);
        walkLeftAnim = new SpriteAnimator(walkLeftSprites, spriteRenderer);
        SetFacingDirection(defaultDirection);

        currentAnim = walkDownAnim;
    }

    private void Update()
    {
        var prevAnim = currentAnim;

        if (IsFishing)
        {
            spriteRenderer.sprite = fishingSprite;
            return;
        }

        if (!isSurfing)
        {
            if (MoveX == 1)
                currentAnim = walkRightAnim;
            else if (MoveX == -1)
                currentAnim = walkLeftAnim;
            else if (MoveY == 1)
                currentAnim = walkUpAnim;
            else if (MoveY == -1)
                currentAnim = walkDownAnim;

            if (currentAnim != prevAnim || isMoving != wasPreviouslyMoving)
                currentAnim.Start();

            if (isJumping)
                spriteRenderer.sprite = currentAnim.Frames[currentAnim.Frames.Count - 1];
            else if (isMoving)
                currentAnim.HandleUpdate();
            else
                spriteRenderer.sprite = currentAnim.Frames[0];
        }
        else
        {
            if (MoveX == 1)
                spriteRenderer.sprite = surfSprites[2];
            else if (MoveX == -1)
                spriteRenderer.sprite = surfSprites[3];
            else if (MoveY == 1)
                spriteRenderer.sprite = surfSprites[1];
            else if (MoveY == -1)
                spriteRenderer.sprite = surfSprites[0];
        }

        wasPreviouslyMoving = isMoving;
    }

    public void SetFacingDirection(FacingDirection dir)
    {
        if (dir == FacingDirection.Right)
            MoveX = 1;
        else if (dir == FacingDirection.Left)
            MoveX = -1;
        else if (dir == FacingDirection.Down)
            MoveY = -1;
        else if (dir == FacingDirection.Up)
            MoveY = 1;
    }

    public void SetCharacterPreset(CharacterPreset preset)
    {
        walkDownAnim = new SpriteAnimator(preset.walkDownSprites, spriteRenderer);
        walkUpAnim = new SpriteAnimator(preset.walkUpSprites, spriteRenderer);
        walkRightAnim = new SpriteAnimator(preset.walkRightSprites, spriteRenderer);
        walkLeftAnim = new SpriteAnimator(preset.walkLeftSprites, spriteRenderer);

        currentAnim = walkDownAnim;
    }

    public FacingDirection DefaultDirection
    {
        get => defaultDirection;
    }
    public void SetFishingAnimation(bool status)
    {
        IsFishing = status;
    }
}

public enum FacingDirection { Up, Down, Left, Right }
