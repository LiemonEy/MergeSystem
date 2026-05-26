using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YG;
using static RocketAction;

public class MergeManager : MonoBehaviour
{
    public static MergeManager instance;
    public Sprite[] itemSprites;
    public Sprite[] bossSprites;

    [SerializeField] private BinScript binScript;
    [SerializeField] private GameObject prefabCell;
    private List<MergeCell> mergeCells = new();
    private int horizontalCellsCount = 5;
    private int verticalCellsCount = 5;

    public MergeCell enteredCell;
    public MergeCell draggingCell;
    public CursorManager cursorManager;

    [SerializeField] private List<RocketAction> actionList;
    public bool isWin = false;

    private int[] itemIdList = { 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    private int rocketBuyedCount = 0;
    private int maxRocketId = 1;

    private void Awake()
    {
        instance = this;
    }

    public void Initialize()
    {
        itemIdList = YG2.saves.itemIdList;
        rocketBuyedCount = YG2.saves.rocketBuyedCount;
        maxRocketId = itemIdList.Max();

        if (UIManager.instance.GetLevel() < 5)        
            verticalCellsCount = 2;        
        else if (UIManager.instance.GetLevel() < 25)        
            verticalCellsCount = 3;        
        else if (UIManager.instance.GetLevel() < 65)        
            verticalCellsCount = 4;        
        else        
            verticalCellsCount = 5;        

        GetComponent<RectTransform>().localPosition -= new Vector3(0, (5 - verticalCellsCount) * 90, 0);

        FillInventory();
        UpdateCells();
    }

    private void OnDisable()
    {
        SetSaves();
    }

    public void SetSaves()
    {
        YG2.saves.itemIdList = GetItemIdList();
        YG2.saves.rocketBuyedCount = rocketBuyedCount;
    }

    private int[] GetItemIdList()
    {
        for (int i = 0; i < verticalCellsCount * horizontalCellsCount; i++)
        {
            itemIdList[i] = mergeCells[i].itemId;
        }
        return itemIdList;
    }

    private void FillInventory()
    {
        for (int i = 0; i < verticalCellsCount * horizontalCellsCount; i++)
        {
            MergeCell cell = Instantiate(prefabCell, transform).GetComponent<MergeCell>();
            mergeCells.Add(cell);
            cell.cellId = i;
            cell.itemId = itemIdList[i];
        }
    }

    public bool GiftItem()
    {
        List<MergeCell> freeMergeCells = GetMergeCells();

        if (freeMergeCells.Count > 0)
        {
            SpawnItem(freeMergeCells, maxRocketId / 3 + UnityEngine.Random.Range(1, 3));
            SoundManager.instance.openGift.Play();

            return true;
        }

        return false;
    }

    public void BuyItem()
    {
        List<MergeCell> freeMergeCells = GetMergeCells();        

        if (freeMergeCells.Count > 0 && UIManager.instance.IsPurchase())
        {
            SpawnItem(freeMergeCells, UIManager.instance.GetRocketLevel());

            SoundManager.instance.buyRocket.Play();
            rocketBuyedCount++;
            UIManager.instance.CostRocketUpdate(rocketBuyedCount);
        }
        else if (freeMergeCells.Count == 0)
        {
            UIManager.instance.NoCellsAnimation();
        }
        else
        {
            UIManager.instance.NoMoneyAnimation();
        }
    }

    public void SpawnItem(List<MergeCell> freeMergeCells, int rocketId)
    {
        int index = UnityEngine.Random.Range(0, freeMergeCells.Count);
        freeMergeCells[index].itemId += rocketId;
        freeMergeCells[index].UpdateCell();
    }

    public void UpdateWeakRockets(int minRocketLevel)
    {
        for (int i = 0; i < mergeCells.Count; i++)
        {
            MergeCell cell = mergeCells[i];

            if (cell.itemId == 0)
                continue;

            if (cell.itemId < minRocketLevel)
            {
                cell.itemId = minRocketLevel;
                cell.UpdateCell();
            }
        }
    }

    public bool FreeCellsChecker()
    {
        List<MergeCell> freeMergeCells = GetMergeCells();

        if (freeMergeCells.Count > 0)
            return true;
        return false;
    }

    public List<MergeCell> GetMergeCells()
    {
        List<MergeCell> freeMergeCells = new();
        for (int i = 0; i < mergeCells.Count; i++)
        {
            if (mergeCells[i].itemId == 0)
            {
                freeMergeCells.Add(mergeCells[i]);
            }
        }
        return freeMergeCells;
    }

    public void StartRockets()
    {
        for (int i = 0; i < mergeCells.Count; i++)
        {
            RocketAction ra = mergeCells[i].GetComponentInChildren<RocketAction>();
            if (ra != null)
            {
                ra.StartInitialize();
                ra.SetState(RocketState.Flying);
                actionList.Add(ra);
            }
        }
    }

    public void RemoveRocketObject(RocketAction ra)
    {
        actionList.Remove(ra);

        if (actionList.Count == 0)
            UIManager.instance.MenuActivate(isWin);
    }

    public List<RocketAction> GetRocketList() => actionList;
    
    public void MaxRocketChecker(int id)
    {
        if (id > maxRocketId)
            maxRocketId = id;
    }
    public void UpdateCells()
    {
        for (int i = 0; i < mergeCells.Count; i++)
        {
            mergeCells[i].UpdateCell();
        }
    }

    public bool CanDeleteRocket()
    {
        return binScript.isEntered;
    }

    public int GetMaxRocket() => maxRocketId;
}
