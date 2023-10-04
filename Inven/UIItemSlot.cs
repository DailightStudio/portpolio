using Rito.InventorySystem;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public enum CostType
{
    Gold, Diamond
}

public class UISlot
{    
    public UISlot DeepCopy()
    {
        UISlot newCopy = new UISlot();
        newCopy.itemData = itemData;
        newCopy.Amount = Amount;
        return newCopy;
    }

    public bool isTaken { get; set; }
    public ItemData itemData { get; set; }
    public int Amount { get; set; }
}


public class UIItemSlot : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerClickHandler
{    
    public UISlot uislot { get; set; } = new UISlot();
    public Image spr;
    public Text amountTxt;
    public GameObject slotObj { get; set; }
    public GameObject locked;


    public void OnPointerClick(PointerEventData eventData)
    {
        if (uislot.isTaken)
        {
            UIInventory.Instance.OnPointerClick(this);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null) return;
        if (uislot.isTaken)
        {
            UIInventory.Instance.OnBeginDrag(this);
        }
    }
    public void OnDrag(PointerEventData eventData)
    {
        UIInventory.Instance.OnDrag(this);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        UIInventory.Instance.OnEndDrag(this);
    }

    /// <summary> 해당 슬롯의 아이템 개수 텍스트 지정 </summary>
    public void SetItemAmountText(int amount)
    {
        // NOTE : amount가 1 이하일 경우 텍스트 미표시

        if (amount > 1)
        {
            amountTxt.gameObject.SetActive(true);
            amountTxt.text = amount.ToString();
        }
        else amountTxt.gameObject.SetActive(false);

    }

    public void SetAmount(UIItemSlot _item, int amount)
    {
        _item.uislot.Amount = Mathf.Clamp(amount, 0, UIInventory.Instance.itemMaxAmount);
    }

    /// <summary> 개수 추가 및 최대치 초과량 반환(초과량 없을 경우 0) </summary>
    public int AddAmountAndGetExcess(UIItemSlot _item, int amount)
    {
        int nextAmount = _item.uislot.Amount + amount;
        SetAmount(_item, nextAmount);

        return (nextAmount > UIInventory.Instance.itemMaxAmount) ? (nextAmount - UIInventory.Instance.itemMaxAmount) : 0;
    }
}
