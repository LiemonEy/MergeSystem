using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MergeCell : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int cellId;
    public int itemId;

    public bool isDragging;
    public bool isEntered;

    [SerializeField] Image sprite;
    [SerializeField] Text textCount;
    

    public void UpdateCell()
    {
        sprite.gameObject.SetActive(itemId != 0);
        textCount.gameObject.SetActive(itemId != 0);

        if (itemId > 0)
        {
            textCount.text = itemId.ToString();

            int spriteIndex = Mathf.Min(itemId - 1, MergeManager.instance.itemSprites.Length - 1);
            sprite.sprite = MergeManager.instance.itemSprites[spriteIndex];
        }
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (itemId > 0)
        MergeManager.instance.cursorManager.CursorUpdate();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        MergeManager.instance.draggingCell = this;

        if (itemId > 0)
        {
            MergeManager.instance.cursorManager.CursorStart(itemId);
            sprite.color = Color.clear;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (itemId > 0)
        {
            MergeManager.instance.cursorManager.CursorEnd();
            sprite.color = Color.white;
        }

        if (MergeManager.instance.enteredCell && MergeManager.instance.enteredCell != this)
            MergeAction();

        if (MergeManager.instance.CanDeleteRocket())
        {
            itemId = 0;
            UpdateCell();
        }

        isDragging = false;
        MergeManager.instance.draggingCell = null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isEntered = true;
        MergeManager.instance.enteredCell = this;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isEntered = false;
        MergeManager.instance.enteredCell = null;
    }

    private void MergeAction()
    {
        if (itemId > 0)
        {
            if (MergeManager.instance.enteredCell.itemId == 0)
            {
                MergeManager.instance.enteredCell.itemId = itemId;
                itemId = 0;
            }
            else if (MergeManager.instance.enteredCell.itemId > 0)
            {
                if (MergeManager.instance.enteredCell.itemId == itemId)
                {
                    MergeManager.instance.enteredCell.itemId++;
                    itemId = 0;

                    SoundManager.instance.mergeRocket.Play();
                    VFXManager.instance.CreateMergeRocket(MergeManager.instance.enteredCell.transform.position, MergeManager.instance.enteredCell.itemId - 1);
                }
                else
                    (MergeManager.instance.enteredCell.itemId, itemId) = (itemId, MergeManager.instance.enteredCell.itemId);
            }

            UpdateCell();
            MergeManager.instance.enteredCell.UpdateCell();
            MergeManager.instance.MaxRocketChecker(itemId);
        }
    }
}
