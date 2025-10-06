using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[Serializable]
public class Inventory : SerializedMonoBehaviour {
    public const int InventoryWidth = 4;
    public const int InventoryHeight = 4;
    public List<Block> blocks;
    
    [TableMatrix(SquareCells = true, DrawElementMethod = "DrawColoredGrid")]
    public BlockElement[,] blockPlacedGrid = new BlockElement[InventoryHeight, InventoryWidth];



    [Button]
    public void ResetGrid()
    {
        blockPlacedGrid = new BlockElement[InventoryHeight, InventoryWidth];
        blocks = new List<Block>();
        //odin inspector 새로고침
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }
    
    private Vector2 GetInventoryLeftUp()
    {
        Vector2 center = transform.position;
        return center + new Vector2(-(float)InventoryWidth / 2f, (float)InventoryHeight / 2f);
    }
    
    //block를 배치할 수 있는 상태인지 검사
    public bool CheckViability(Block block, Vector2Int position)
    {
        if (block == null) return false;

        BlockElement[,] elements = block.elements;
        if (elements == null) return false;

        int originX = position.x - block.center.x;
        int originY = position.y - block.center.y;

        int height = elements.GetLength(0);
        int width = elements.GetLength(1);

        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                BlockElement element = elements[r, c];
                if (element == null) continue;

                int gridY = originY + r;
                int gridX = originX + c;

                // 경계 검사
                if (gridY < 0 || gridY >= InventoryHeight || gridX < 0 || gridX >= InventoryWidth)
                {
                    return false;
                }

                BlockElement occupied = blockPlacedGrid[gridY, gridX];
                if (occupied != null && occupied != element)
                {
                    Block occupiedOwner = occupied.GetComponentInParent<Block>();
                    if (occupiedOwner != block)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    //block를 배치
    [Button]
    public void Set(Block block, Vector2Int position)
    {
        if (!CheckViability(block, position)) return;

        if (blocks == null)
        {
            blocks = new List<Block>();
        }

        // 기존 위치에서 제거
        for (int r = 0; r < InventoryHeight; r++)
        {
            for (int c = 0; c < InventoryWidth; c++)
            {
                BlockElement element = blockPlacedGrid[r, c];
                if (element == null) continue;

                Block owner = element.GetComponentInParent<Block>();
                if (owner == block)
                {
                    blockPlacedGrid[r, c] = null;
                }
            }
        }

        int originX = position.x - block.center.x;
        int originY = position.y - block.center.y;

        BlockElement[,] elements = block.elements;
        int height = elements.GetLength(0);
        int width = elements.GetLength(1);

        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                BlockElement element = elements[r, c];
                if (element == null) continue;

                int gridY = originY + r;
                int gridX = originX + c;
                blockPlacedGrid[gridY, gridX] = element;
            }
        }

        if (!blocks.Contains(block))
        {
            blocks.Add(block);
        }

        block.PlaceBlock(position, GetInventoryLeftUp());
        
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        
    }

    public bool TrySet(Block block, Vector2Int position)
    {
        if (!CheckViability(block, position))
        {
            return false;
        }

        Set(block, position);
        return true;
    }

    public Vector2 GridToWorldCenter(Vector2Int gridPosition)
    {
        Vector2 leftUp = GetInventoryLeftUp();
        return new Vector2(leftUp.x + gridPosition.y + 0.5f, leftUp.y - gridPosition.x - 0.5f);
    }

    public bool TryGetNearestGridPosition(Vector2 worldPosition, out Vector2Int gridPosition)
    {
        Vector2 leftUp = GetInventoryLeftUp();
        float rowApprox = leftUp.y - worldPosition.y - 0.5f;
        float columnApprox = worldPosition.x - leftUp.x - 0.5f;

        gridPosition = new Vector2Int(Mathf.RoundToInt(rowApprox), Mathf.RoundToInt(columnApprox));

        if (gridPosition.x < 0 || gridPosition.x >= InventoryWidth || gridPosition.y < 0 || gridPosition.y >= InventoryHeight)
        {
            return false;
        }
        return true;
    }
    
    private BlockElement DrawColoredGrid(Rect rect, BlockElement value) => BlockCreator.DrawColoredGridEl(rect, value);
}
