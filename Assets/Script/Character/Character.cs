using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Character : MonoBehaviour
{
    public float moveSpeed;
    public bool isMoving { get; private set; }
    public float offsetY { get; private set; } = 0.3f;
    CharacterAnimator animator;

    private void Awake()
    {
        animator = GetComponent<CharacterAnimator>();
        SetPositionAndSnapToTile(transform.position);
    }

    public void SetPositionAndSnapToTile(Vector2 pos)
    {
        pos.x = Mathf.Floor(pos.x) + 0.5f;
        pos.y = Mathf.Floor(pos.y) + 0.5f + offsetY;

        transform.position = pos;
    }
    public IEnumerator Move(Vector2 moveVec, Action OnMoveOver = null, bool checkCollisions = true)
    {
        animator.MoveX = Mathf.Clamp(moveVec.x, -1f, 1f);
        animator.MoveY = Mathf.Clamp(moveVec.y, -1f, 1f);

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + new Vector3(moveVec.x, moveVec.y, 0);

        var ledge = CheckForLedge(targetPos);
        if (ledge != null)
        {
            if (ledge.TryToJump(this, moveVec))
                yield break;
        }

        if (checkCollisions && !IsPathClear(targetPos))
            yield break;

        if (animator.isSurfing && Physics2D.OverlapCircle(targetPos, 0.3f, GameLayer.i.WaterLayer) == null)
            animator.isSurfing = false;

        isMoving = true;

        // Loop movement
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // Snap to target position (important)
        transform.position = new Vector3(
            Mathf.Round(targetPos.x * 100f) / 100f,
            Mathf.Round(targetPos.y * 100f) / 100f,
            transform.position.z
        );

        isMoving = false;
        OnMoveOver?.Invoke();
    }


    public void HandleUpdate()
    {
        animator.isMoving = isMoving;
    }

    private bool IsPathClear(Vector3 targetPos)
    {
        var diff = targetPos - transform.position;
        var dir = diff.normalized;

        var collisionLayer = GameLayer.i.SolidLayer | GameLayer.i.InteractableLayer | GameLayer.i.PlayerLayer;

        if (!animator.isSurfing)
            collisionLayer = collisionLayer | GameLayer.i.WaterLayer;

        if (Physics2D.BoxCast(transform.position + dir, new Vector2(0.2f, 0.2f), 0f, dir, diff.magnitude - 1, collisionLayer) == true)
            return false;

        return true;
    }


    private bool isWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, GameLayer.i.SolidLayer | GameLayer.i.InteractableLayer) != null)
        {
            return false;
        }

        return true;
    }

    Ledge CheckForLedge(Vector3 targetPos)
    {
        var collider = Physics2D.OverlapCircle(targetPos, 0.15f, GameLayer.i.LedgeLayer);
        return collider?.GetComponent<Ledge>();
    }

    public void LookToward(Vector3 targetPos)
    {
        var xdiff = Mathf.Floor(targetPos.x) - Mathf.Floor(transform.position.x);
        var ydiff = Mathf.Floor(targetPos.y) - Mathf.Floor(transform.position.y);

        if (xdiff == 0 || ydiff == 0)
        {
            animator.MoveX = Mathf.Clamp(xdiff, -1f, 1f);
            animator.MoveY = Mathf.Clamp(ydiff, -1f, 1f);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.05f);
    }

    public CharacterAnimator Animator
    {
        get => animator;
    }
}
