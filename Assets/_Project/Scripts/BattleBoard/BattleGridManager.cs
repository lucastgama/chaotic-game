using System.Collections.Generic;
using UnityEngine;

//Asicionar no BattleBoard
public class BattleGridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [Tooltip("Se 0, será obtido automaticamente do GameManager")]
    public int gridSize = 0;
    public GameObject gridPrefab;

    [Header("Spacing Settings")]
    public float horizontalSpacing = 2.5f;
    public float verticalSpacing = 2.0f;
    public float teamDistance = 6.0f;
    public float gridHeight = 1.01f;

    private List<GameObject> battleGrid = new List<GameObject>();
    private Dictionary<int, List<Vector2>> formationLayouts = new Dictionary<int, List<Vector2>>();

    // Armazenar valores anteriores para detectar mudanças
    private float lastHorizontalSpacing;
    private float lastVerticalSpacing;
    private float lastTeamDistance;
    private float lastGridHeight;

    void Awake()
    {
        formationLayouts[1] = new List<Vector2> { new Vector2(0, 0) };

        formationLayouts[3] = new List<Vector2>
        {
            new Vector2(-0.92f, 0),
            new Vector2(1, 0),
            new Vector2(0, 1)
        };

        // Formação 6v6
        formationLayouts[6] = new List<Vector2>
        {
            new Vector2(-0.92f, 0),
            new Vector2(1, 0),
            new Vector2(2.92f, 0), // Linha da frente
            new Vector2(0.04f, 1),
            new Vector2(1.97f, 1), // Linha do meio
            new Vector2(1, 2.045f) // Linha da final
        };

        // Formação 10v10
        formationLayouts[10] = new List<Vector2>
        {
            new Vector2(-2.92f, 0),
            new Vector2(-0.92f, 0),
            new Vector2(1, 0),
            new Vector2(2.92f, 0), // Linha de trás
            new Vector2(-1.97f, 1),
            new Vector2(0, 1),
            new Vector2(1.97f, 1), // Linha do meio
            new Vector2(-0.97f, 2),
            new Vector2(0.97f, 2), // Terceira linha
            new Vector2(0, 3.045f) // Linha da frente
        };

        // Inicializar valores anteriores
        lastHorizontalSpacing = horizontalSpacing;
        lastVerticalSpacing = verticalSpacing;
        lastTeamDistance = teamDistance;
        lastGridHeight = gridHeight;
    }

    void Start()
    {
        if (gridSize == 0 && GameManager.Instance != null)
        {
            gridSize = GameManager.Instance.GetMaxCreaturesPerPlayer();
        }

        GenerateGrid(gridSize);
    }

    void Update()
    {
        // Verificar se algum valor mudou
        if (HasSpacingChanged())
        {
            // Atualizar grid se estiver em modo de desenvolvimento
#if UNITY_EDITOR
            if (gridSize > 0)
            {
                GenerateGrid(gridSize);
            }
#endif
            // Atualizar valores anteriores
            UpdateLastValues();
        }
    }

    // Alternativa: usar OnValidate para atualizar apenas no Editor
    void OnValidate()
    {
#if UNITY_EDITOR
        if (Application.isPlaying && gridSize > 0)
        {
            GenerateGrid(gridSize);
        }
#endif
    }

    public void GenerateGrid(int size)
    {
        ClearGrid();

        if (!formationLayouts.ContainsKey(size))
        {
            Debug.LogError($"Formação para tamanho {size} não definida!");
            return;
        }

        GenerateTeamFormation(size, -1);

        GenerateTeamFormation(size, 1);
    }

    private void GenerateTeamFormation(int size, int teamDirection)
    {
        float zOffset = teamDirection * teamDistance / 2;

        // Usa a formação pré-definida para este tamanho
        foreach (Vector2 relativePos in formationLayouts[size])
        {
            // Multiplica pelas configurações de espaçamento atuais
            float xPos = relativePos.x * horizontalSpacing;
            float zPos = zOffset + teamDirection * relativePos.y * verticalSpacing;

            // Cria a grid na posição calculada
            CreateGridAtPosition(new Vector3(xPos, gridHeight, zPos));
        }
    }

    private void CreateGridAtPosition(Vector3 position)
    {
        GameObject newGrid = Instantiate(gridPrefab, position, Quaternion.identity);
        newGrid.transform.parent = transform;
        battleGrid.Add(newGrid);
    }

    private void ClearGrid()
    {
        foreach (GameObject grid in battleGrid)
        {
            Destroy(grid);
        }
        battleGrid.Clear();
    }

    private bool HasSpacingChanged()
    {
        return lastHorizontalSpacing != horizontalSpacing
            || lastVerticalSpacing != verticalSpacing
            || lastTeamDistance != teamDistance
            || lastGridHeight != gridHeight;
    }

    private void UpdateLastValues()
    {
        lastHorizontalSpacing = horizontalSpacing;
        lastVerticalSpacing = verticalSpacing;
        lastTeamDistance = teamDistance;
        lastGridHeight = gridHeight;
    }
}
