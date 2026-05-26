using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static RocketAction;

public class MergeManager : MonoBehaviour
{
    public static MergeManager Instance;

    [Header("Visuals")]
    public Sprite[] itemSprites;
    public Sprite[] bossSprites;

    [Header("References")]
    [SerializeField] private BinScript binScript;
    [SerializeField] private GameObject prefabCell;
    [SerializeField] private CursorManager cursorManager;

    [Header("Grid Settings")]
    [SerializeField] private int horizontalCellsCount = 5;
    [SerializeField] private int verticalCellsCount = 5;

    private readonly List<MergeCell> mergeCells = new();
    private readonly List<RocketAction> activeRockets = new();

    private readonly int[] cellItemIds =
    {
        1, 1, 0, 0, 0,
        0, 0, 0, 0, 0,
        0, 0, 0, 0, 0,
        0, 0, 0, 0, 0,
        0, 0, 0, 0, 0
    };

    public MergeCell enteredCell;
    public MergeCell draggingCell;

    public bool isWin = false;

    private int maxRocketId = 1;

    private void Awake()
    {
        Instance = this;
    }

    public void Initialize()
    {
        maxRocketId = cellItemIds.Max();

        ConfigureGridSize();

        FillInventory();
        UpdateCells();
    }

    private void ConfigureGridSize()
    {
        int level = UIManager.instance.GetLevel();

        if (level < 5)
            verticalCellsCount = 2;
        else if (level < 25)
            verticalCellsCount = 3;
        else if (level < 65)
            verticalCellsCount = 4;
        else
            verticalCellsCount = 5;

        GetComponent<RectTransform>().localPosition -=
            new Vector3(0, (5 - verticalCellsCount) * 90, 0);
    }

    private void FillInventory()
    {
        int totalCells = verticalCellsCount * horizontalCellsCount;

        for (int i = 0; i < totalCells; i++)
        {
            MergeCell cell = Instantiate(prefabCell, transform)
                .GetComponent<MergeCell>();

            mergeCells.Add(cell);

            cell.cellId = i;
            cell.itemId = cellItemIds[i];
        }
    }

    public void SpawnItem(List<MergeCell> freeCells, int rocketId)
    {
        int randomIndex = Random.Range(0, freeCells.Count);

        MergeCell targetCell = freeCells[randomIndex];

        targetCell.itemId += rocketId;
        targetCell.UpdateCell();
    }

    public List<MergeCell> GetFreeCells()
    {
        List<MergeCell> freeCells = new();

        for (int i = 0; i < mergeCells.Count; i++)
        {
            if (mergeCells[i].itemId == 0)
            {
                freeCells.Add(mergeCells[i]);
            }
        }

        return freeCells;
    }

    public bool HasFreeCells()
    {
        return GetFreeCells().Count > 0;
    }

    public void StartRockets()
    {
        for (int i = 0; i < mergeCells.Count; i++)
        {
            RocketAction rocket = mergeCells[i]
                .GetComponentInChildren<RocketAction>();

            if (rocket == null)
                continue;

            rocket.StartInitialize();
            rocket.SetState(RocketState.Flying);

            activeRockets.Add(rocket);
        }
    }

    public void RemoveRocket(RocketAction rocket)
    {
        activeRockets.Remove(rocket);

        if (activeRockets.Count == 0)
        {
            UIManager.instance.MenuActivate(isWin);
        }
    }

    public void UpdateCells()
    {
        for (int i = 0; i < mergeCells.Count; i++)
        {
            mergeCells[i].UpdateCell();
        }
    }

    public void CheckMaxRocket(int id)
    {
        if (id > maxRocketId)
        {
            maxRocketId = id;
        }
    }

    public bool CanDeleteRocket()
    {
        return binScript.isEntered;
    }

    public int GetMaxRocket()
    {
        return maxRocketId;
    }

    public List<RocketAction> GetActiveRockets()
    {
        return activeRockets;
    }
}
