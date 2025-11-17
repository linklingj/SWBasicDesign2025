using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class BlockCellData
{
    public int row;
    public int column;
    public string blockId; // Unique ID per Block prefab or instance
}

<<<<<<< Updated upstream
[CreateAssetMenu(fileName = "PlayerData", menuName = "Game/PlayerData")]
=======
[Serializable]
public class BlockData
{
    public string blockId;
    public Vector2Int placedPosition;
    public int rotationState;
    public BlockData(string id, Vector2Int position, int rotationState)
    {
        blockId = id;
        placedPosition = position;
        this.rotationState = rotationState;
    }
}

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
>>>>>>> Stashed changes
public class PlayerData : ScriptableObject
{
    public List<BlockCellData> placedBlocks = new List<BlockCellData>();
    public List<string> ownedBlockIds = new List<string>();

    public void SaveInventory(Inventory inventory)
    {
        placedBlocks.Clear();
        ownedBlockIds.Clear();

        for (int r = 0; r < Inventory.InventoryHeight; r++)
        {
            for (int c = 0; c < Inventory.InventoryWidth; c++)
            {
                BlockElement element = inventory.blockPlacedGrid[r, c];
                if (element == null) continue;

                Block owner = element.GetComponentInParent<Block>();
                if (owner == null) continue;

                placedBlocks.Add(new BlockCellData
                {
                    row = r,
                    column = c,
                    blockId = owner.blockID // blockId must exist in Block
                });

                if (!ownedBlockIds.Contains(owner.blockID))
                {
<<<<<<< Updated upstream
                    ownedBlockIds.Add(owner.blockID);
=======
                    ownedBlocks.Add(new BlockData(owner.blockID, owner.placedPosition, owner.rotationState));
>>>>>>> Stashed changes
                }
            }
        }
    }

    public void LoadInventory(Inventory inventory, Func<string, Block> blockFactory)
    {
        inventory.ResetGrid();

        Dictionary<string, Block> created = new Dictionary<string, Block>();

        foreach (var cell in placedBlocks)
        {
<<<<<<< Updated upstream
            if (!created.TryGetValue(cell.blockId, out Block block))
            {
                block = blockFactory(cell.blockId);
                created[cell.blockId] = block;
            }

            inventory.blockPlacedGrid[cell.row, cell.column] = block.elements[cell.row, cell.column];
        }
=======
            Block b = Instantiate(blockFactory(block.blockId), inventory.transform);
            inventory.blocks.Add(b);
            inventory.Set(b, block.placedPosition, block.rotationState);
            b.SetPresetBlock();
            //b.SetBlockPosInstant(block.placedPosition, inventory.GetInventoryLeftUp());
            //b.PlaceBlock(block.placedPosition, inventory.GetInventoryLeftUp());
        }
        
        // foreach (var e in placedBlocks)
        // {
        //     inventory.blockPlacedGrid[e.row, e.column] = blockFactory(e.blockId).element;
        // }

>>>>>>> Stashed changes
    }
}
