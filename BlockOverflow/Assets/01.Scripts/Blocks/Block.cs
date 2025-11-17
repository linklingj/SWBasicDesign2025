using System;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor.Rendering;

// 블록의 기본 클래스
// 블록의 최대 크기는 4x4
public class Block : SerializedMonoBehaviour {
    [EnumPaging]
    public BlockType blockType;
    
    [SerializeReference, InlineProperty, HideLabel]
    public IBlockEffect blockEffect;

    [TableMatrix(SquareCells = true, DrawElementMethod = "DrawColoredGrid")]
    public BlockElement[,] elements = new BlockElement[4,4];
    
    public string blockID;
    
    // 블록의 크기와 중심점
    public Vector2Int size;
    public Vector2Int center;
    public int rotationState { private set; get; } = 0;
    public void SetRotationState(int rotationState) => this.rotationState = rotationState % 4;
    
    // 블록 선택 모드
    public bool IsSelectMode { private set; get; }
    
    // 블록이 인벤토리에 배치되었는지
    public bool IsPlaced { private set; get; } = false;
    
    private BlockAnimator _blockAnimator;

    private void Awake()
    {
        _blockAnimator = GetComponent<BlockAnimator>();
    }

    // 처음 블록이 생성되었을 때 설정 (주로 에디터에서)
    public void Init(bool[,] shape, int width, int height, BlockType blockType, GameObject elementPrefab)
    {
        this.blockType = blockType;
        size = new Vector2Int(width, height);
        center = CalculateCenter(size);
        rotationState = 0;
        elements = new BlockElement[height, width];

        // shape 배열을 순회하며 true인 위치에 BlockElement 생성
        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                if (shape[r, c])
                {
                    // BlockElement 프리팹을 인스턴스화하고 위치 설정
                    GameObject elementObj = Instantiate(elementPrefab, transform);
                    elementObj.name = $"Element_{c}_{r}";
                    elements[r, c] = elementObj.GetComponent<BlockElement>();
                    elements[r, c].SetType(blockType);
                    elements[r, c].transform.localPosition = GetElementLocalPosition(r, c);
                }
            }
        }
    }

    // 처음 보상으로 등장
    public void Appear(int idx, int total)
    {
        IsSelectMode = true;
        _blockAnimator.FirstAppearAnim(idx, total);
    }

    // 보상 중에 선택됨
    public void Selected()
    {
        IsSelectMode = false;
        _blockAnimator.SelectedAnim();
    }

    public void NotSelected()
    {
        _blockAnimator.NotSelectedAnim();
    }
    
    // 블록 놓아짐
    public void PlaceBlock(Vector2Int position, Vector2 lu, bool imidiate = false)
    {
        Debug.Log("place:" + position);
        Vector3 targetPos = new Vector3(lu.x + position.y + 0.5f, lu.y - position.x - 0.5f, transform.position.z);
        IsPlaced = true;
        if (!_blockAnimator) _blockAnimator = GetComponent<BlockAnimator>();
        
        if (imidiate) SetBlockPosInstant(position, lu);
        else _blockAnimator.PlacedAnim(targetPos);
    }
<<<<<<< Updated upstream
=======
    
    public void SetBlockPosInstant(Vector2Int position, Vector2 lu)
    {
        placedPosition = position;
        _blockAnimator.interactable = false;
        Vector3 targetPos = new Vector3(lu.x + position.y + 0.5f, lu.y - position.x - 0.5f, transform.position.z);
        transform.position = targetPos;
        IsPlaced = true;
    }
>>>>>>> Stashed changes

    public void RotateClockwise()
    {
        ApplyRotation(true);
        rotationState = (rotationState + 1) % 4;
    }

    public void RotateCounterClockwise()
    {
        ApplyRotation(false);
        rotationState = (rotationState + 3) % 4;
    }

    private void ApplyRotation(bool clockwise)
    {
        if (elements == null)
        {
            return;
        }

        int originalHeight = elements.GetLength(0);
        int originalWidth = elements.GetLength(1);
        if (originalHeight == 0 || originalWidth == 0)
        {
            return;
        }
        BlockElement[,] rotated = new BlockElement[originalWidth, originalHeight];

        for (int r = 0; r < originalHeight; r++)
        {
            for (int c = 0; c < originalWidth; c++)
            {
                BlockElement element = elements[r, c];
                if (element == null) continue;

                int newRow;
                int newCol;

                if (clockwise)
                {
                    newRow = c;
                    newCol = originalHeight - 1 - r;
                }
                else
                {
                    newRow = originalWidth - 1 - c;
                    newCol = r;
                }

                rotated[newRow, newCol] = element;
            }
        }

        elements = rotated;
        size = new Vector2Int(rotated.GetLength(1), rotated.GetLength(0));
        center = CalculateCenter(size);
        UpdateElementLocalPositions();
    }

    private Vector2Int CalculateCenter(Vector2Int currentSize)
    {
        int centerX = Mathf.FloorToInt(currentSize.x * 0.5f);
        int centerY = Mathf.FloorToInt(currentSize.y * 0.5f);
        return new Vector2Int(centerX, centerY);
    }

    private Vector3 GetElementLocalPosition(int row, int column)
    {
        float x = row - center.y;
        float y = -(column - center.x);
        return new Vector3(x, y, 0f);
    }

    private void UpdateElementLocalPositions()
    {
        int height = elements.GetLength(0);
        int width = elements.GetLength(1);

        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                BlockElement element = elements[r, c];
                if (element == null) continue;

                element.transform.localPosition = GetElementLocalPosition(r, c);
            }
        }
    }

    public void SetPresetBlock()
    {
        _blockAnimator.interactable = false;
    }

    private BlockElement DrawColoredGrid(Rect rect, BlockElement value) => BlockCreator.DrawColoredGridEl(rect, value);
    
    [Button] public void PrintInfo() => Debug.Log($"center: {center}, size: {size}");
}
