using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Xml.Schema;
using UnityEngine.EventSystems;
using System;

public class UIInventory : Singleton_General<UIInventory>
{
    [SerializeField] Canvas canvas;
    [SerializeField] Transform ui;
    [SerializeField] Sprite emptySpr;

    Transform slotTransform;
    int usableSlot = 10;
    int fullySlot = 30;
    public int itemMaxAmount { get; set; } = 10;

    public List<UIItemSlot> uiItemSlotList = new List<UIItemSlot>();

    private void Start()
    {
        DOTween.Init();
        SetInven();
        DragInit();

        _gr = canvas.GetComponent<GraphicRaycaster>();
    }


    #region UI관련 함수

    public void OnPointerClick(UIItemSlot _slot)
    {
        ShowDiscriptions(_slot);
    }

    bool _isCountable;
    void ShowDiscriptions(UIItemSlot _slot)
    {
        GameObject ui_discriptions = ObjectPool.Instance.PopFromPool("UIItem_discriptions");
        ui_discriptions.SetActive(true);
        ui_discriptions.transform.SetParent(ui);
        ui_discriptions.transform.localPosition = Vector3.zero;
        ui_discriptions.transform.localScale = Vector3.one;
        RectTransform _rect = ui_discriptions.GetComponent<RectTransform>();
        _rect.offsetMax = Vector2.zero;
        _rect.offsetMin = Vector2.zero;

        UIItem_discriptions _uidis = ui_discriptions.GetComponent<UIItem_discriptions>();
        _uidis.itemArmor = 0;
        _uidis.itemDamageMin = 0;
        _uidis.itemDamageMax = 0;
        _uidis.itemFreindShip = 0;
        _isCountable = false;
        switch (_slot.uislot.itemData)
        {
            case Hats:
                Hats _hats = _slot.uislot.itemData as Hats;
                _uidis.itemArmor = Convert.ToInt32(_hats.armor.ToString());
                break;
            case Clothes:
                Clothes _closthes = _slot.uislot.itemData as Clothes;
                _uidis.itemArmor = Convert.ToInt32(_closthes.armor.ToString());
                break;
            case Tools:
                Tools _tools = _slot.uislot.itemData as Tools;
                _uidis.itemDamageMin = Convert.ToInt32(_tools.damageMin.ToString());
                _uidis.itemDamageMax = Convert.ToInt32(_tools.damageMax.ToString());
                break;
            case Countable:
                Countable _countable = _slot.uislot.itemData as Countable;
                _uidis.itemFreindShip = Convert.ToInt32(_countable.friendship.ToString());
                _isCountable = true;
                break;
        }

        switch (_slot.uislot.itemData.itemCostType)
        {
            case CostType.Gold:
                _uidis.itemCostImg.sprite = InsteadLoadManager.Instance.goldSpr;
                break;
            case CostType.Diamond:
                _uidis.itemCostImg.sprite = InsteadLoadManager.Instance.diamondSpr;
                break;
        }
        _uidis.itemCostTxt.text = _slot.uislot.itemData.cost.ToString();
        _uidis.itemImg.sprite = InsteadLoadManager.Instance.ItemSprite(_slot.uislot.itemData.item_code);
        _uidis.itemClassTxt.text = $"[{InsteadLoadManager.Instance.ChangeLocalazation(_slot.uislot.itemData.item_code, itemLocalazationType.ItemType)}]";
        _uidis.itemTitleTxt.text = InsteadLoadManager.Instance.ChangeLocalazation(_slot.uislot.itemData.item_code, itemLocalazationType.Name);
        _uidis.itemContextTxt.text = InsteadLoadManager.Instance.ChangeLocalazation(_slot.uislot.itemData.item_code, itemLocalazationType.Content);
        _uidis.itemArmorTitleTxt.text = InsteadLoadManager.Instance.ChangeLocalazation(LocalazationList.Localazation, 4);
        _uidis.itemDamageTitleTxt.text = InsteadLoadManager.Instance.ChangeLocalazation(LocalazationList.Localazation, 5);
        _uidis.itemFreindShipTitleTxt.text = InsteadLoadManager.Instance.ChangeLocalazation(LocalazationList.Localazation, 6);
        _uidis.itemArmorTxt.text = _uidis.itemArmor.ToString();
        _uidis.itemFreindShipTxt.text = _uidis.itemFreindShip.ToString();
        _uidis.itemDamageTxt.text = $"{_uidis.itemDamageMin}~{_uidis.itemDamageMax}";
        if (_isCountable) _uidis.itemEquipTxt.text = InsteadLoadManager.Instance.ChangeLocalazation(LocalazationList.Localazation, 9);
        else _uidis.itemEquipTxt.text = InsteadLoadManager.Instance.ChangeLocalazation(LocalazationList.Localazation, 8);
        _uidis.itemSellTxt.text = InsteadLoadManager.Instance.ChangeLocalazation(LocalazationList.Localazation, 10);

        _uidis.itemArmorGroup.SetActive(_uidis.itemArmor > 0 ? true : false);
        _uidis.itemDamageGroup.SetActive(_uidis.itemDamageMax > 0 ? true : false);
        _uidis.itemFriendshipGroup.SetActive(_uidis.itemFreindShip > 0 ? true : false);
        _uidis.itemDiscriptionLine.SetActive((!_uidis.itemArmorGroup.activeSelf && !_uidis.itemDamageGroup.activeSelf && !_uidis.itemFriendshipGroup.activeSelf) ? false : true);

        RectTransform uiRectTransform = ui_discriptions.transform.GetChild(0).GetComponent<RectTransform>();
        LanguageManager.Instance.LayoutRootRefresh(uiRectTransform);

        float uiWidth = uiRectTransform.rect.width;
        float uiHeight = uiRectTransform.rect.height;

        // 화면 너비와 높이 값
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // 피봇 초기화
        Vector2 currentPivot = Vector2.up;

        // UI 요소의 크기가 화면의 상단을 벗어나는지 확인하고 pivot 변경
        if (uiRectTransform.anchoredPosition.y + uiHeight / 2 > screenHeight / 2)
        {
            currentPivot.y = 1; // 상단으로 변경
        }

        // UI 요소의 크기가 화면의 하단을 벗어나는지 확인하고 pivot 변경
        if (uiRectTransform.anchoredPosition.y - uiHeight / 2 < -screenHeight / 2)
        {
            currentPivot.y = 0; // 하단으로 변경
        }

        // UI 요소의 크기가 화면의 왼쪽을 벗어나는지 확인하고 pivot 변경
        if (uiRectTransform.anchoredPosition.x - uiWidth / 2 < -screenWidth / 2)
        {
            currentPivot.x = 0; // 왼쪽으로 변경
        }

        // UI 요소의 크기가 화면의 오른쪽을 벗어나는지 확인하고 pivot 변경
        if (uiRectTransform.anchoredPosition.x + uiWidth / 2 > screenWidth / 2)
        {
            currentPivot.x = 1; // 오른쪽으로 변경
        }

        // 변경된 pivot 값을 UI 요소에 적용
        uiRectTransform.pivot = currentPivot;
        uiRectTransform.transform.position = _slot.slotObj.transform.position;

        var seq = DOTween.Sequence();
        seq.Append(uiRectTransform.transform.DOScale(1.1f, 0.1f));
        seq.Append(uiRectTransform.transform.DOScale(1f, 0.1f));
    }



    Image selectedItemImg;
    Text selectedItemTxt;
    public void OnBeginDrag(UIItemSlot _slot)
    {
        if (!_slot.uislot.isTaken) return;
        _slot.spr.color = Color.clear;
        selectedItemImg.sprite = _slot.spr.sprite;
        selectedItemImg.color = Color.white;
        selectedItemTxt.gameObject.SetActive(_slot.amountTxt.gameObject.activeSelf);
        _slot.amountTxt.gameObject.SetActive(false);
        selectedItemTxt.text = _slot.amountTxt.text;
    }

    public void OnDrag(UIItemSlot _slot)
    {
        selectedItemImg.transform.position = Input.mousePosition;
    }


    public void OnEndDrag(UIItemSlot _slot)
    {
        _slot.amountTxt.gameObject.SetActive(selectedItemTxt.gameObject.activeSelf);
        DragInit(_slot);

        UIItemSlot curSlot = RaycastAndGetFirstComponent<UIItemSlot>();
        if (curSlot == null || curSlot.locked.activeSelf) return;
        else
        {
            if (!curSlot.uislot.isTaken && curSlot != null)
            {
                CopyFrom(curSlot, _slot.uislot);
                RemoveItem(_slot);
                UpdateSlot(curSlot);
            }
            else
            {
                TrySwapItems(curSlot, _slot);
            }
        }
    }

    /// <summary> 두 슬롯의 아이템 교환 </summary>
    private void TrySwapItems(UIItemSlot from, UIItemSlot to)
    {
        if (from == to) return;
        if (from.uislot.itemData == null || to.uislot.itemData == null) return;

        Swap(from, to);
    }

    void CopyFrom(UIItemSlot _to, UISlot _from)
    {
        _to.uislot.itemData = _from.itemData; // 아이템 데이터 복사
        _to.uislot.isTaken = true;           // isTaken 속성 설정
        _to.uislot.Amount = _from.Amount;     // Amount 속성 복사
        _to.amountTxt.gameObject.SetActive(_to.uislot.Amount > 1 ? true : false);
    }

    GraphicRaycaster _gr;
    PointerEventData _ped = new PointerEventData(null);
    List<RaycastResult> _rrList = new List<RaycastResult>();
    /// <summary> 레이캐스트하여 얻은 첫 번째 UI에서 컴포넌트 찾아 리턴 </summary>
    private T RaycastAndGetFirstComponent<T>() where T : Component
    {
        _ped.position = Input.mousePosition;
        _rrList.Clear();

        _gr.Raycast(_ped, _rrList);

        if (_rrList.Count == 0) return null;
        return _rrList[0].gameObject.GetComponent<T>();
    }

    void DragInit(UIItemSlot _slot = null)
    {
        if (_slot != null)
        {
            _slot.spr.color = Color.white;
        }

        selectedItemImg.color = Color.clear;
        selectedItemTxt.gameObject.SetActive(false);
    }

    /// <summary> 두 인덱스의 아이템 위치를 서로 교체</summary>
    public void Swap(UIItemSlot curSlot, UIItemSlot _beginSlot)
    {
        bool _isSum = false;
        // 1. 셀 수 있는 아이템이고, 동일한 아이템일 경우, 개수 합치기
        if (curSlot.uislot.itemData.item_code == _beginSlot.uislot.itemData.item_code
            && curSlot.uislot.itemData is Countable && _beginSlot.uislot.itemData is Countable)
        {
            if (_beginSlot.uislot.Amount + curSlot.uislot.Amount < itemMaxAmount)
                _isSum = true;
        }

        if (_isSum)
        {
            int sum = curSlot.uislot.Amount + _beginSlot.uislot.Amount;

            if (sum <= itemMaxAmount)
            {
                curSlot.SetAmount(curSlot, 0);
                _beginSlot.SetAmount(_beginSlot, sum);
            }
            else
            {
                curSlot.SetAmount(curSlot, sum - itemMaxAmount);
                _beginSlot.SetAmount(_beginSlot, itemMaxAmount);
            }
        }

        // 2. 일반적인 경우 : 슬롯 교체
        else
        {
            UISlot _itemA = curSlot.uislot.DeepCopy();
            UISlot _itemB = _beginSlot.uislot.DeepCopy();
            CopyFrom(curSlot, _itemB);
            CopyFrom(_beginSlot, _itemA);
        }

        // 두 슬롯 정보 갱신
        UpdateSlot(curSlot, _beginSlot);
    }
    #endregion

    /// <summary> 인덱스가 수용 범위 내에 있는지 검사 </summary>
    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < usableSlot;
    }

    /// <summary> 앞에서부터 비어있는 슬롯 인덱스 탐색 </summary>
    private int FindEmptySlotIndex()
    {
        int _num = -1;
        for (int i = 0; i < usableSlot; i++)
        {
            if (!uiItemSlotList[i].uislot.isTaken)
            {
                _num = i;
                break;
            }
        }

        return _num;
    }

    /// <summary> 앞에서부터 개수 여유가 있는 Countable 아이템의 슬롯 인덱스 탐색 </summary>
    private int FindCountableItemSlotIndex(ItemData target)
    {
        int _num = -1;
        for (int i = 0; i < usableSlot; i++)
        {
            UIItemSlot current = uiItemSlotList[i];
            if (current.uislot.isTaken)
            {
                // 아이템 종류 일치, 개수 여유 확인
                if (current.uislot.itemData.item_code == target.item_code)
                {
                    if (itemMaxAmount > current.uislot.Amount)
                    {
                        _num = i;
                        break;
                    }
                }
            }
        }
        return _num;
    }

    /// <summary> 해당하는 인덱스의 슬롯 상태 및 UI 갱신 </summary>
    private void UpdateSlot(UIItemSlot _slot)
    {
        // 1. 아이템이 슬롯에 존재하는 경우
        if (_slot.uislot.isTaken && _slot.uislot.itemData != null)
        {
            // 아이콘 등록
            _slot.spr.sprite = InsteadLoadManager.Instance.ItemSprite(_slot.uislot.itemData.item_code);
            // 1-1. 셀 수 있는 아이템
            if (_slot.uislot.itemData is Countable)
            {
                // 1-1-1. 수량이 0인 경우, 아이템 제거
                if (_slot.uislot.Amount == 0)
                {
                    RemoveItem(_slot);
                    return;
                }
                // 1-1-2. 수량 텍스트 표시
                else
                {
                    _slot.SetItemAmountText(_slot.uislot.Amount);
                }
            }
        }
        // 2. 빈 슬롯인 경우 : 아이콘 제거
        else
        {
            RemoveItem(_slot);
        }
    }

    // 아이템 제거하기
    void RemoveItem(UIItemSlot _slot)
    {
        _slot.amountTxt.gameObject.SetActive(false);
        _slot.uislot.Amount = 0;
        _slot.uislot.isTaken = false;
        _slot.spr.sprite = emptySpr;
        _slot.uislot.itemData = null;
    }

    /// <summary> 해당하는 인덱스의 슬롯들의 상태 및 UI 갱신 </summary>
    private void UpdateSlot(params UIItemSlot[] indices)
    {
        foreach (var i in indices)
        {
            UpdateSlot(i);
        }
    }

    public void Add(string _itemCode, int _amount = 1)
    {
        int index;
        InsteadLoadManager.Instance.itemDict.TryGetValue(_itemCode, out ItemData itemData);

        // 1. 수량이 있는 아이템
        if (itemData is Countable)
        {
            bool findNextCountable = true;
            index = -1;

            while (_amount > 0)
            {
                // 1-1. 이미 해당 아이템이 인벤토리 내에 존재하고, 개수 여유 있는지 검사
                if (findNextCountable)
                {
                    index = FindCountableItemSlotIndex(itemData);

                    // 개수 여유있는 기존재 슬롯이 더이상 없다고 판단될 경우, 빈 슬롯부터 탐색 시작
                    if (index == -1)
                    {
                        findNextCountable = false;
                    }
                    // 기존재 슬롯을 찾은 경우, 양 증가시키고 초과량 존재 시 amount에 초기화
                    else
                    {
                        _amount = uiItemSlotList[index].AddAmountAndGetExcess(uiItemSlotList[index], _amount);
                        UpdateSlot(uiItemSlotList[index]);
                    }
                }
                // 1-2. 빈 슬롯 탐색
                else
                {
                    index = FindEmptySlotIndex();

                    // 빈 슬롯조차 없는 경우 종료
                    if (index == -1)
                    {
                        Debug.Log("inventory is full.");
                        break;
                    }
                    // 빈 슬롯 발견 시, 슬롯에 아이템 추가 및 잉여량 계산
                    else
                    {
                        // 새로운 아이템 생성
                        uiItemSlotList[index].uislot.itemData = itemData;
                        uiItemSlotList[index].uislot.isTaken = true;
                        uiItemSlotList[index].uislot.Amount = 0;
                        uiItemSlotList[index].SetAmount(uiItemSlotList[index], _amount);

                        // 남은 개수 계산          
                        _amount = (_amount > itemMaxAmount) ? (_amount - itemMaxAmount) : 0;

                        UpdateSlot(uiItemSlotList[index]);
                    }
                }
            }
        }
        // 2. 수량이 없는 아이템
        else
        {
            // 2-1. 1개만 넣는 경우, 간단히 수행
            if (_amount == 1)
            {
                index = FindEmptySlotIndex();
                if (index != -1)
                {
                    // 아이템을 생성하여 슬롯에 추가
                    uiItemSlotList[index].uislot.itemData = itemData;
                    uiItemSlotList[index].uislot.isTaken = true;
                    _amount = 0;

                    UpdateSlot(uiItemSlotList[index]);
                }
            }

            // 2-2. 2개 이상의 수량 없는 아이템을 동시에 추가하는 경우
            index = -1;
            for (; _amount > 0; _amount--)
            {
                // 아이템 넣은 인덱스의 다음 인덱스부터 슬롯 탐색
                index = FindEmptySlotIndex();

                // 다 넣지 못한 경우 루프 종료
                if (index == -1) break;

                // 아이템을 생성하여 슬롯에 추가
                uiItemSlotList[index].uislot.itemData = itemData;
                uiItemSlotList[index].uislot.isTaken = true;

                UpdateSlot(uiItemSlotList[index]);
            }
        }
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Add("Fruits_8");
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            Add("Tools_2");
        }
    }

    bool isInventoryOn;
    GameObject inventory;

    void SetInven()
    {
        InvenPosInit();

        MovableHeaderUI _movable = inventory.GetComponent<MovableHeaderUI>();
        slotTransform = _movable.slotTrf;
        selectedItemImg = _movable.selectedItemImg;
        selectedItemTxt = _movable.selectedItemTxt;
        _movable._targetTr.position = Vector3.zero;
        _movable.cancel.onClick.AddListener(InventoryCancel);
        ObjectPool.Instance.PushToPool(inventory);

        SlotSettings();
    }

    void InvenPosInit()
    {
        inventory = ObjectPool.Instance.PopFromPool("UIInventory");
        inventory.SetActive(true);
        inventory.transform.SetParent(ui);
        inventory.transform.localPosition = Vector3.zero;
        inventory.transform.localScale = Vector3.one;
        inventory.transform.GetChild(0).transform.localPosition = Vector3.zero;
        RectTransform _rect = inventory.GetComponent<RectTransform>();
        _rect.offsetMax = Vector2.zero;
        _rect.offsetMin = Vector2.zero;
        Vector2 _anchoredPosition = _rect.anchoredPosition;
        _anchoredPosition.y = 50f;
        _rect.anchoredPosition = _anchoredPosition;
    }

    void SlotSettings()
    {
        for (int i = 0; i < fullySlot; i++)
        {
            GameObject _slot = ObjectPool.Instance.PopFromPool("ItemSlot");
            _slot.SetActive(true);
            _slot.transform.SetParent(slotTransform);
            _slot.transform.localPosition = Vector3.zero;
            _slot.transform.localScale = Vector3.one;

            UIItemSlot _slotItem = _slot.GetComponent<UIItemSlot>();
            _slotItem.spr = _slot.transform.GetChild(0).GetComponent<Image>();
            _slotItem.locked = _slot.transform.GetChild(1).gameObject;
            _slotItem.amountTxt = _slot.transform.GetChild(2).GetComponent<Text>();
            _slotItem.slotObj = _slot;
            _slotItem.locked.SetActive((i >= usableSlot) ? true : false);
            uiItemSlotList.Add(_slotItem);
        }
    }

    public void InventoryOn()
    {
        if (isInventoryOn)
        {
            InventoryCancel();
            return;
        }
        InvenPosInit();
        isInventoryOn = true;
        PlayerController.Instance.Direction(Vector2.down);
        AudioManager.Instance.PlaySound(FXEnum.Bubble);

        var seq = DOTween.Sequence();
        seq.Append(inventory.transform.DOScale(1.1f, 0.1f));
        seq.Append(inventory.transform.DOScale(1f, 0.1f));
    }

    public void InventoryCancel()
    {
        isInventoryOn = false;
        AudioManager.Instance.PlaySound(FXEnum.Bubble);

        var seq = DOTween.Sequence();
        seq.Append(inventory.transform.DOScale(0f, 0.1f));
        seq.Play().OnComplete(() =>
        {
            ObjectPool.Instance.PushToPool(inventory);
        });
    }
}