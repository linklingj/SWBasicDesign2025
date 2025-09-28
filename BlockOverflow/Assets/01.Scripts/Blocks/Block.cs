using System;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor.Rendering;

// 블록의 기본 클래스
// 블록의 최대 크기는 4x4
public class Block : SerializedMonoBehaviour {
    [EnumPaging]
    public BlockType blockType;

    [TableMatrix(SquareCells = true, DrawElementMethod = "DrawColoredGrid")]
    public BlockElement[,] elements = new BlockElement[4,4];
    
    // 블록의 크기와 중심점
    public Vector2Int Size { private set; get; }
    public Vector2Int Center { private set; get; }
    
    // 블록이 인벤토리에 배치되었는지
    public bool IsPlaced { private set; get; } = false;

    public void Init(bool[,] shape, int width, int height, BlockType blockType, GameObject elementPrefab)
    {
        this.blockType = blockType;
        Size = new Vector2Int(width, height);
        int centerX = Mathf.FloorToInt(width * 0.5f);
        int centerY = Mathf.FloorToInt(height * 0.5f);
        Center = new Vector2Int(centerX, centerY);
        elements = new BlockElement[height, width];

        // shape 배열을 순회하며 true인 위치에 BlockElement 생성
        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                if (shape[r, c])
                {
                    // BlockElement 프리팹을 인스턴스화하고 위치 설정
                    GameObject elementObj = Instantiate(elementPrefab, new Vector3(r-centerY, -(c-centerX), 0), Quaternion.identity, transform);
                    elementObj.name = $"Element_{c}_{r}";
                    elements[r, c] = elementObj.GetComponent<BlockElement>();
                    elements[r, c].SetType(blockType);
                }
            }
        }
    }
    
    public void PlaceBlock(Vector2Int position)
    {
        transform.position = new Vector2(position.x, position.y);
        IsPlaced = true;
    }

    private BlockElement DrawColoredGrid(Rect rect, BlockElement value) => BlockCreator.DrawColoredGridEl(rect, value);
}
