using UnityEngine;

public class DropZoneFactory : MonoBehaviour
{
    [Header("Drop Zone Prefab")]
    public GameObject dropZonePrefab;
    public Transform dropZoneContainer;

    [Header("Card Size (base)")]
    public float cardWidth = 125f;
    public float cardHeight = 175f;

    [Header("Layout")]
    public float baseYOffset = -200f;
    public float spacingMultiplier = 1.0f;
    public float verticalSpacingMultiplier = 1.0f;

    [Header("Battle Mode")]
    public int battleMode;

    [Header("Offsets")]
    public float battlegearOffsetY = 20f;
    public float battlegearOffsetX = -10f;

    void Awake()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        switch (battleMode)
        {
            case 1:
                Setup1v1();
                break;
            case 3:
                SetupPyramid(rows: 2, scale: 1.1f);
                break;
            case 6:
                SetupPyramid(rows: 3, scale: 0.8f);
                break;
            default:
                SetupPyramid(rows: 2, scale: 0.8f);
                break;
        }
        // SetupPyramid(rows: CalculateRowsForPyramid(battleMode), scale: 0.5f);
    }

    void Setup1v1()
    {
        float scale = 2f;
        CreateDropZone(DropZoneType.Creature, new Vector2(25, -25), scale, 1);
        CreateDropZone(DropZoneType.Battlegear, new Vector2(-25, 25), scale, 1);
    }

    void SetupPyramid(int rows, float scale)
    {
        float scaledW = cardWidth * scale;
        float scaledH = cardHeight * scale;

        float hSpacing = scaledW * 1.3f;
        float vSpacing = scaledH * 1.3f;

        int index = 1;

        for (int row = 1; row <= rows; row++)
        {
            int cardsInRow = row;

            float totalWidth = (cardsInRow - 1) * hSpacing;
            float startX = -totalWidth / 2f;

            float y = baseYOffset + (row - 1) * vSpacing;

            for (int col = 0; col < cardsInRow; col++)
            {
                float x = (startX + col * hSpacing) + 10.0f;

                Vector2 creaturePos = new Vector2(x, y);
                Vector2 battlegearPos =
                    creaturePos + new Vector2(battlegearOffsetX, battlegearOffsetY);

                CreateDropZone(DropZoneType.Battlegear, battlegearPos, scale, index);
                CreateDropZone(DropZoneType.Creature, creaturePos, scale, index);
                index++;
            }
        }
    }

    void CreateDropZone(DropZoneType type, Vector2 position, float scale, int index)
    {
        if (dropZonePrefab == null || dropZoneContainer == null)
        {
            Debug.LogError("dropZonePrefab ou dropZoneContainer não atribuído!");
            return;
        }

        GameObject dropZoneObj = Instantiate(dropZonePrefab, dropZoneContainer);
        dropZoneObj.name = $"{type} DropZone {index}";

        DropZone dz = dropZoneObj.GetComponent<DropZone>();
        if (dz != null)
        {
            dz.dropZoneId = $"grid-{type}-{index}".ToLower();
            dz.acceptOnlyCardType = type;
        }

        RectTransform rect = dropZoneObj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = position;
            rect.localScale = new Vector3(scale, scale, 1f);
        }
    }

    int CalculateRowsForPyramid(int totalCards)
    {
        return Mathf.CeilToInt((-1 + Mathf.Sqrt(1 + 8 * totalCards)) / 2);
    }
}
