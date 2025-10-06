using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Block))]
public class BlockDragHandler : MonoBehaviour {
    [SerializeField] private Inventory inventory;

    private Block block;
    private Camera mainCamera;
    private bool isDragging;
    private Vector3 dragOffset;
    private Vector3 originalPosition;
    private Vector2Int originalGridPosition;
    private bool hasOriginalGridPosition;
    private int rotationStepsDuringDrag;

    private void Awake()
    {
        block = GetComponent<Block>();
        mainCamera = Camera.main;

        if (inventory == null)
        {
            inventory = FindObjectOfType<Inventory>();
        }
    }

    private void Update()
    {
        if (inventory == null)
        {
            return;
        }

        if (Mouse.current == null)
        {
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
                if (mainCamera == null)
                {
                    return;
                }
            }
        }

        if (!isDragging)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                TryBeginDrag();
            }
        }
        else
        {
            FollowCursor();

            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                RotateWhileDragging();
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                EndDrag();
            }
        }
    }

    private void TryBeginDrag()
    {
        Vector2 mouseWorld = GetMouseWorldPosition();
        Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorld);
        Block hitBlock = null;

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] == null)
            {
                continue;
            }

            hitBlock = hits[i].GetComponent<Block>();

            if (hitBlock == null)
            {
                hitBlock = hits[i].GetComponentInParent<Block>();
            }

            if (hitBlock == block)
            {
                break;
            }
            hitBlock = null;
        }

        if (hitBlock != block)
        {
            return;
        }

        isDragging = true;
        originalPosition = transform.position;
        hasOriginalGridPosition = inventory.TryGetNearestGridPosition(originalPosition, out originalGridPosition);
        dragOffset = transform.position - (Vector3)mouseWorld;
        rotationStepsDuringDrag = 0;
    }

    private void FollowCursor()
    {
        Vector2 mouseWorld = GetMouseWorldPosition();
        Vector3 target = (Vector3)mouseWorld + dragOffset;
        transform.position = new Vector3(target.x, target.y, originalPosition.z);
    }

    private void EndDrag()
    {
        isDragging = false;

        Vector2Int gridPosition;
        bool placed = inventory.TryGetNearestGridPosition(transform.position, out gridPosition) && inventory.TrySet(block, gridPosition);

        if (!placed)
        {
            if (rotationStepsDuringDrag != 0)
            {
                RevertRotation();
            }

            transform.position = originalPosition;

            if (hasOriginalGridPosition)
            {
                inventory.TrySet(block, originalGridPosition);
            }
        }
        else
        {
            rotationStepsDuringDrag = 0;
        }
    }

    private Vector2 GetMouseWorldPosition()
    {
        Vector3 mouse = Mouse.current.position.ReadValue();
        float depth = Mathf.Abs(transform.position.z - mainCamera.transform.position.z);
        if (depth < 0.0001f)
        {
            depth = Mathf.Abs(mainCamera.transform.position.z);
        }

        mouse.z = depth;
        return mainCamera.ScreenToWorldPoint(mouse);
    }

    private void RotateWhileDragging()
    {
        block.RotateClockwise();
        rotationStepsDuringDrag = (rotationStepsDuringDrag + 1) % 4;
    }

    private void RevertRotation()
    {
        for (int i = 0; i < rotationStepsDuringDrag; i++)
        {
            block.RotateCounterClockwise();
        }

        rotationStepsDuringDrag = 0;
    }
}
