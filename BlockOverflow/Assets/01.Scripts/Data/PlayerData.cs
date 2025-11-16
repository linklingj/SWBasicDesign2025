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

[CreateAssetMenu(fileName = "PlayerData", menuName = "Game/PlayerData")]
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
                    ownedBlockIds.Add(owner.blockID);
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
            if (!created.TryGetValue(cell.blockId, out Block block))
            {
                block = blockFactory(cell.blockId);
                created[cell.blockId] = block;
            }

            inventory.blockPlacedGrid[cell.row, cell.column] = block.elements[cell.row, cell.column];
        }
    }
}
