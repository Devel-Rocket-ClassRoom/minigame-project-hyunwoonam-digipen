namespace Tempt
{
    /// <summary>
    /// I 단축키 페이지. 인벤토리 + 장비 슬롯 + 소모 4칸 슬롯.
    /// 전투 중에는 view-only 모드(소모 4칸 변경 차단).
    /// 인벤/보관함/상점에서 아이템 클릭 시 공통 아이템 상세 팝업을 표시한다.
    /// </summary>
    public sealed class InventoryPaget : UIPageControllerBaset
    {
        /// <inheritdoc/>
        public override void OnOpen()
        {
            // 동작 요약:
            // - Player.Inventory / Equipment / ConsumableSlots 데이터 바인딩.
            // - Player.Locker 잠금 여부 확인 후 보관함 탭 활성 여부 결정.
            // - EventBust 구독(인벤/장비 변경 시 다시 그림).
            // - IsViewOnly = !UIManagert.ConsumablesEditable 일 때 소모 4칸 비활성.
            //TODO: var player = GameSystemManagert.Instance.CurrentRun.Player;
            //TODO: BindInventory(player.Inventory);
            //TODO: BindEquipment(player.Equipment);
            //TODO: BindConsumableSlots(player.Consumables, player.Inventory);
            //TODO: LockerTab.SetActive(player.Locker.Unlocked);
            //TODO: GameSystemManagert.Instance.Events.OnInventoryChanged  += OnInventoryChanged;
            //TODO: GameSystemManagert.Instance.Events.OnEquipmentChanged  += OnEquipmentChanged;
            //TODO: SetViewOnly(!UIManagert.Instance.ConsumablesEditable);
        }

        /// <inheritdoc/>
        public override void OnClose()
        {
            // 동작 요약: EventBust 구독 해제.
            //TODO: GameSystemManagert.Instance.Events.OnInventoryChanged  -= OnInventoryChanged;
            //TODO: GameSystemManagert.Instance.Events.OnEquipmentChanged  -= OnEquipmentChanged;
        }

        /// <inheritdoc/>
        public override void OnEditableChanged(bool editable)
        {
            // 동작 요약: 소모 4칸 슬롯 드래그/클릭 활성화 토글.
            //TODO: SetViewOnly(!editable);
        }

        /// <summary>
        /// 아이템 클릭 시 호출. 인벤토리, 보관함, 상점 어디서든 동일하게 사용.
        /// DataManagert.Items에서 ItemDatat를 조회하여 상세 팝업을 표시한다.
        /// </summary>
        /// <param name="itemId">선택된 아이템 ID.</param>
        /// <param name="context">클릭 출처(Inventory / Locker / Shop).</param>
        public void ShowItemDetail(int itemId, ItemDetailContextt context)
        {
            // 동작 요약:
            // - DataManagert.Items.TryGetValue(itemId, out ItemDatat data) 검사.
            // - ItemDetailInfot info = BuildDetailInfo(data, context).
            // - ItemDetailPopupt(또는 공통 팝업 UI)를 UIManagert.PushPage로 열기.
            // - context에 따라 가능한 행동(장착/폐기/판매/이동) 버튼 활성화 분기.
            //TODO: if (!GameSystemManagert.Instance.Data.Items.TryGetValue(itemId, out var data)) return;
            //TODO: ItemDetailInfot info = BuildDetailInfo(data, context);
            //TODO: ItemDetailPopup.Show(info, context,
            //TODO:     onDiscard:    () => OnDiscardClicked(itemId, 1),
            //TODO:     onEquip:      () => { var player = GameSystemManagert.Instance.CurrentRun.Player; OnEquipToggleClicked(player.Inventory.EquipItems.Find(i => i.Data.Id == itemId)); },
            //TODO:     onMoveToLocker: () => OnMoveClicked(itemId, 1, true));
        }

        /// <summary>
        /// 아이템 상세 팝업에 표시할 정보 구성.
        /// </summary>
        private ItemDetailInfot BuildDetailInfo(ItemDatat data, ItemDetailContextt context)
        {
            // 동작 요약:
            // - data.NameKey → LanguageServicet.Get(NameKey).
            // - data.DescKey → LanguageServicet.Get(DescKey).
            // - 장비라면 EquipMod 스탯 변화 표시용 텍스트 생성.
            // - context == Shop 이면 구매 가격(인플레이션 포함) 추가.
            // - context == Inventory/Locker 이면 판매 가격 추가.
            //TODO: var info = new ItemDetailInfot
            //TODO: {
            //TODO:     Name        = LanguageServicet.Get(data.NameKey),
            //TODO:     Description = LanguageServicet.Get(data.DescKey),
            //TODO:     Category    = data.Category,
            //TODO:     ParamText   = data.ParamValue > 0 ? $"+{data.ParamValue}" : string.Empty,
            //TODO: };
            //TODO: if (data.Category == ItemCategoryt.Equipment && data.EquipMod != null)
            //TODO:     info.StatModSummary = $"ATK+{data.EquipMod.ATK} DEF+{data.EquipMod.DEF} HP+{data.EquipMod.HP}";
            //TODO: var run = GameSystemManagert.Instance.CurrentRun;
            //TODO: float inflation = GameSystemManagert.Instance.Data.ComputeInflation(run.CurrentFloor / 8, run.Erosion.GetRate(run.CurrentFloor / 8));
            //TODO: if (context == ItemDetailContextt.Shop) info.BuyPrice  = UnityEngine.Mathf.RoundToInt(data.BaseBuyPrice  * inflation);
            //TODO: else                                     info.SellPrice = UnityEngine.Mathf.RoundToInt(data.BaseSellPrice * inflation);
            //TODO: return info;
            return null;
        }

        /// <summary>
        /// 아이템 폐기 버튼 처리. UI에서 수량 입력 후 호출.
        /// </summary>
        /// <param name="itemId">폐기할 아이템 ID(소모/재료).</param>
        /// <param name="count">폐기 수량.</param>
        public void OnDiscardClicked(int itemId, int count)
        {
            // 동작 요약:
            // - Player.Inventory.Discard(itemId, count).
            // - 팝업 닫기.
            //TODO: GameSystemManagert.Instance.CurrentRun.Player.Inventory.Discard(itemId, count);
            //TODO: ItemDetailPopup.Hide();
        }

        /// <summary>
        /// 장비 폐기 버튼 처리.
        /// </summary>
        public void OnDiscardEquipClicked(Itemt item)
        {
            // 동작 요약:
            // - Player.Inventory.DiscardEquip(item).
            // - 팝업 닫기.
            //TODO: GameSystemManagert.Instance.CurrentRun.Player.Inventory.DiscardEquip(item);
            //TODO: ItemDetailPopup.Hide();
        }

        /// <summary>
        /// 인벤 ↔ 보관함 이동 버튼 처리.
        /// </summary>
        /// <param name="itemId">이동할 아이템 ID.</param>
        /// <param name="count">이동 수량.</param>
        /// <param name="toLocker">true=인벤→보관함, false=보관함→인벤.</param>
        public void OnMoveClicked(int itemId, int count, bool toLocker)
        {
            // 동작 요약:
            // - toLocker: Player.Inventory.MoveToLocker(Player.Locker, itemId, count).
            // - !toLocker: Player.Inventory.MoveFromLocker(Player.Locker, itemId, count).
            //TODO: var player = GameSystemManagert.Instance.CurrentRun.Player;
            //TODO: if (toLocker) player.Inventory.MoveToLocker(player.Locker, itemId, count);
            //TODO: else          player.Inventory.MoveFromLocker(player.Locker, itemId, count);
        }

        /// <summary>
        /// 장비 장착/해제 버튼 처리.
        /// </summary>
        public void OnEquipToggleClicked(Itemt item)
        {
            // 동작 요약:
            // - 이미 장착 중이면 Player.Equipment.Unequip(slot) → Player.Inventory.AddEquip(반환된 아이템).
            // - 미장착이면 Player.Equipment.Equip(slot, item) → 기존 장비 있으면 Player.Inventory.AddEquip.
            // - 장착 후 Player.SyncPassivesFromRunes() 호출 불필요(장비는 패시브에 영향 없음, 스탯은 AggregateStatMod).
            //TODO: var player = GameSystemManagert.Instance.CurrentRun.Player;
            //TODO: EquipmentSlotIdt slot = item.Data.EquipSlot;
            //TODO: // 현재 장착 중인지 확인
            //TODO: bool isEquipped = IsCurrentlyEquipped(player.Equipment, item);
            //TODO: if (isEquipped)
            //TODO: {
            //TODO:     Itemt removed = player.Equipment.Unequip(slot);
            //TODO:     if (removed != null) player.Inventory.AddEquip(removed);
            //TODO: }
            //TODO: else
            //TODO: {
            //TODO:     Itemt displaced = player.Equipment.Equip(slot, item);
            //TODO:     player.Inventory.RemoveEquip(item); // 인벤에서 제거
            //TODO:     if (displaced != null) player.Inventory.AddEquip(displaced); // 기존 장비 인벤 복귀
            //TODO: }
            //TODO: player.RecalcBonusStats();
        }
    }

    /// <summary>아이템 상세 팝업에 표시할 구조화된 정보.</summary>
    public sealed class ItemDetailInfot
    {
        /// <summary>아이템 이름(현재 언어).</summary>
        public string Name;

        /// <summary>설명(현재 언어).</summary>
        public string Description;

        /// <summary>카테고리(소모/장비/재료).</summary>
        public ItemCategoryt Category;

        /// <summary>주요 수치 텍스트(회복량/스탯 수치 등).</summary>
        public string ParamText;

        /// <summary>구매 가격(인플레이션 포함, 0이면 표시 안 함).</summary>
        public int BuyPrice;

        /// <summary>판매 가격(0이면 표시 안 함).</summary>
        public int SellPrice;

        /// <summary>장비 스탯 보정 요약 텍스트(장비만, 빈 문자열이면 비표시).</summary>
        public string StatModSummary;
    }

    /// <summary>아이템 클릭 출처. ShowItemDetail()에서 가능한 행동 분기에 사용.</summary>
    public enum ItemDetailContextt
    {
        /// <summary>인벤토리에서 클릭.</summary>
        Inventory,

        /// <summary>보관함에서 클릭.</summary>
        Locker,

        /// <summary>상점에서 클릭.</summary>
        Shop,
    }
}
