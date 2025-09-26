using System;
using UnityEngine;
using Sirenix.OdinInspector;

// 블록의 기본 클래스
// 블록의 최대 크기는 4x4
public class Block : SerializedMonoBehaviour {
    [EnumPaging]
    public BlockType blockType;

    [TableMatrix(SquareCells = true)]
    public BlockElement[,] elements;
    
    public Vector2 Size { private set; get; }
    public Vector2 Center { private set; get; }

    public void Init(bool[,] shape, int width, int height, BlockType blockType, GameObject elementPrefab)
    {
        this.blockType = blockType;
        Size = new Vector2(width, height);
        int centerX = Mathf.FloorToInt(width * 0.5f);
        int centerY = Mathf.FloorToInt(height * 0.5f);
        Center = new Vector2(centerX, centerY);
        elements = new BlockElement[height, width];

        // shape 배열을 순회하며 true인 위치에 BlockElement 생성
        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                if (shape[r, c])
                {
                    // BlockElement 프리팹을 인스턴스화하고 위치 설정
                    GameObject elementObj = Instantiate(elementPrefab, new Vector3(c, -r, 0), Quaternion.identity, transform);
                    elementObj.name = $"Element_{r}_{c}";
                    elements[r, c] = elementObj.GetComponent<BlockElement>();
                    elements[r, c].SetType(blockType);
                }
            }
        }
    }
}
