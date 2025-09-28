using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.Utilities;

public class BlockCreator : SerializedMonoBehaviour {
    [SerializeField] GameObject blockPrefab;
    [SerializeField] GameObject elementPrefab;
    
    [TableMatrix(SquareCells = true, DrawElementMethod = "DrawColoredGridBool")] public bool[,] blockCreationGrid = new bool[4, 4];
    [EnumButtons] public BlockType blockType;

    [Button] public void CreateBlock() => GetNewBlock(blockCreationGrid, blockType);
    
    // 랜덤 블록 생성 (인스펙터용) 최소 1개 ~ 최대 4개의 블록을 생성한다
    [Button] public void CreateRandomBlock() => GetRandomBlock(Random.Range(1,5));
    

    // elementCount 개수만큼 랜덤하게 설정된 블록을 반환
    private Block GetRandomBlock(int elementCount)
    {
        elementCount = Mathf.Clamp(elementCount, 1, 16);

        bool[,] shape = new bool[4, 4];

        HashSet<int> chosen = new HashSet<int>();
        List<int> frontier = new List<int>();
        HashSet<int> frontierSet = new HashSet<int>();

        // Helpers
        System.Func<int, int, bool> InRange = (r, c) => (r >= 0 && r < 4 && c >= 0 && c < 4);
        System.Action<int, int> PushNeighbors = (r, c) =>
        {
            int nr, nc, key;
            // up
            nr = r - 1; nc = c; if (InRange(nr, nc)) { key = nr * 4 + nc; if (!chosen.Contains(key) && frontierSet.Add(key)) frontier.Add(key); }
            // down
            nr = r + 1; nc = c; if (InRange(nr, nc)) { key = nr * 4 + nc; if (!chosen.Contains(key) && frontierSet.Add(key)) frontier.Add(key); }
            // left
            nr = r; nc = c - 1; if (InRange(nr, nc)) { key = nr * 4 + nc; if (!chosen.Contains(key) && frontierSet.Add(key)) frontier.Add(key); }
            // right
            nr = r; nc = c + 1; if (InRange(nr, nc)) { key = nr * 4 + nc; if (!chosen.Contains(key) && frontierSet.Add(key)) frontier.Add(key); }
        };

        int sr = Random.Range(0, 4);
        int sc = Random.Range(0, 4);
        int sKey = sr * 4 + sc;
        chosen.Add(sKey);
        shape[sr, sc] = true;
        PushNeighbors(sr, sc);

        while (chosen.Count < elementCount)
        {
            if (frontier.Count == 0)
            {
                foreach (int key in chosen)
                {
                    int r = key / 4; int c = key % 4;
                    PushNeighbors(r, c);
                }
                if (frontier.Count == 0)
                {
                    break;
                }
            }

            int pick = Random.Range(0, frontier.Count);
            int cell = frontier[pick];
            int last = frontier[frontier.Count - 1];
            frontier[pick] = last;
            frontier.RemoveAt(frontier.Count - 1);
            frontierSet.Remove(cell);

            if (!chosen.Contains(cell))
            {
                chosen.Add(cell);
                int r = cell / 4; int c = cell % 4;
                shape[r, c] = true;
                PushNeighbors(r, c);
            }
        }

        return GetNewBlock(shape, (BlockType)Random.Range(0, 5));
    }
    
    private Block GetNewBlock(bool[,] shape, BlockType blockType)
    {
        int rows = 4;
        int cols = 4;

        int minR = rows, minC = cols;
        int maxR = -1,  maxC = -1;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (!shape[r, c]) continue;

                if (r < minR) minR = r;
                if (r > maxR) maxR = r;
                if (c < minC) minC = c;
                if (c > maxC) maxC = c;
            }
        }

        // true가 하나도 없으면 생성 의미가 없음 → null 반환
        if (maxR == -1)
        {
            Debug.LogWarning("선택된 칸 없음");
            return null;
        }

        // 바운딩 박스 크기 → height, width 계산
        int height = maxR - minR + 1;
        int width  = maxC - minC + 1;

        // 5) 바운딩 박스 영역만 복사해 만든 "정규화된" 새 배열
        //    cropped[row, col] : 0..height-1, 0..width-1
        //    원본의 (minR, minC)을 (0,0)으로 평행이동한 형태
        
        bool[,] cropped = new bool[height, width];
        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                cropped[r, c] = shape[minR + r, minC + c];
            }
        }

        Debug.Log(height);
        Debug.Log(width);
        GameObject blockObj = Instantiate(blockPrefab, transform);
        Block block = blockObj.GetComponent<Block>();

        block.Init(cropped, width, height, blockType, elementPrefab);

        return block;
    }
    
    [OnInspectorInit]
    private void CreateData()
    {
        blockCreationGrid = new bool[4, 4]
        {
            { false, false, false, false },
            { false, false, false, false },
            { false, false, false, false },
            { false, false, false, false }
        };
    }
    
    public static bool DrawColoredGridBool(Rect rect, bool value)
    {
        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
            value = !value;
            GUI.changed = true;
            Event.current.Use();
        }

        UnityEditor.EditorGUI.DrawRect(rect.Padding(1), value ? new Color(0.1f, 0.8f, 0.2f) : new Color(0, 0, 0, 0.5f));

        return value;
    }
    
    public static BlockElement DrawColoredGridEl(Rect rect, BlockElement value)
    {
        UnityEditor.EditorGUI.DrawRect(rect.Padding(1), value ? BlockType.BasicStats.ToColor() : new Color(0, 0, 0, 0.5f));

        return value;
    }
}