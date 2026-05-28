namespace Tempt
{
    // Guid2 §8 2026-05-27: 구매 / 판매 / 가격 산식 단일 진입점.
    // Inflation = DataManager.ComputeInflation(stageIndex, erosionRate).
    // 실패 시 Gold / Inventory 의 어떤 상태도 변경하지 않는다.
    /// <summary>
    /// 상점 거래. 가격은 BasePrice * Inflation, 판매가는 BasePrice * SellRatio * Inflation.
    /// </summary>
    public static class Shop
    {
        /// <summary>현재 단계 침식률을 적용한 구매가. 데이터/상태 누락 시 0.</summary>
        public static int GetBuyPrice(int itemId, GameRunState run, DataManager data)
        {
            if (!TryResolvePriceInputs(itemId, run, data, out ItemData item, out float inflation))
            {
                return 0;
            }

            return UnityEngine.Mathf.Max(1, UnityEngine.Mathf.RoundToInt(item.BasePrice * inflation));
        }

        /// <summary>현재 단계 침식률을 적용한 판매가. 데이터/상태 누락 시 0.</summary>
        public static int GetSellPrice(int itemId, GameRunState run, DataManager data, BalanceData balance)
        {
            if (balance == null)
            {
                UnityEngine.Debug.LogError("[Shop.GetSellPrice] BalanceData 참조가 없습니다.");
                return 0;
            }

            if (!TryResolvePriceInputs(itemId, run, data, out ItemData item, out float inflation))
            {
                return 0;
            }

            float sellRatio = UnityEngine.Mathf.Clamp01(balance.SellRatio);
            return UnityEngine.Mathf.Max(1, UnityEngine.Mathf.RoundToInt(item.BasePrice * sellRatio * inflation));
        }

        /// <summary>재고 행에 지정된 구매가. UnitPrice가 있으면 침식 인플레이션을 적용하지 않는다.</summary>
        public static int GetStockBuyPrice(ShopStockEntry stock, GameRunState run, DataManager data)
        {
            if (stock == null)
            {
                UnityEngine.Debug.LogError("[Shop.GetStockBuyPrice] stock 참조가 없습니다.");
                return 0;
            }

            return stock.UnitPrice > 0 ? stock.UnitPrice : GetBuyPrice(stock.ItemId, run, data);
        }

        /// <summary>상점 재고에서 1개 구매. 무제한 재고는 유지, 제한 재고는 구매 후 비활성화될 수 있다.</summary>
        public static bool TryBuyStock(ShopStockEntry stock, GameRunState run, DataManager data)
        {
            if (stock == null)
            {
                UnityEngine.Debug.LogError("[Shop.TryBuyStock] stock 참조가 없습니다.");
                return false;
            }

            if (!stock.CanPurchase)
            {
                return false;
            }

            if (!TryResolveTradeInputs(stock.ItemId, run, data, out ItemData item, out InventoryState inventory))
            {
                return false;
            }

            int price = GetStockBuyPrice(stock, run, data);
            if (price <= 0 || run.Gold < price)
            {
                return false;
            }

            bool added = item.Stackable
                ? inventory.TryAdd(item, 1)
                : inventory.TryAddEquip(new Item { Data = item, Enhancement = 0 });
            if (!added)
            {
                return false;
            }

            run.Gold -= price;
            stock.ConsumeOne();
            RaiseGoldChanged(run);
            return true;
        }

        /// <summary>스택 가능 아이템 구매(count 개). 실패 시 Gold / Inventory 변동 없음.</summary>
        public static bool TryBuy(int itemId, int count, GameRunState run, DataManager data)
        {
            if (!TryResolveTradeInputs(itemId, run, data, out ItemData item, out InventoryState inventory))
            {
                return false;
            }

            if (count <= 0)
            {
                UnityEngine.Debug.LogError("[Shop.TryBuy] count <= 0.");
                return false;
            }

            if (item.Category == ItemCategory.Equipment || !item.Stackable)
            {
                if (count != 1)
                {
                    UnityEngine.Debug.LogError("[Shop.TryBuy] 장비 구매는 count == 1 이어야 합니다.");
                    return false;
                }

                return TryBuyEquip(itemId, run, data);
            }

            int unitPrice = GetBuyPrice(itemId, run, data);
            int totalPrice = unitPrice * count;
            if (unitPrice <= 0 || run.Gold < totalPrice)
            {
                return false;
            }

            if (!inventory.TryAdd(item, count))
            {
                return false;
            }

            run.Gold -= totalPrice;
            RaiseGoldChanged(run);
            return true;
        }

        /// <summary>장비 1개 인스턴스 구매(Enhancement=0). 실패 시 Gold / Inventory 변동 없음.</summary>
        public static bool TryBuyEquip(int itemId, GameRunState run, DataManager data)
        {
            if (!TryResolveTradeInputs(itemId, run, data, out ItemData item, out InventoryState inventory))
            {
                return false;
            }

            if (item.Category != ItemCategory.Equipment || item.Stackable)
            {
                UnityEngine.Debug.LogError("[Shop.TryBuyEquip] 장비 아이템이 아닙니다: " + itemId);
                return false;
            }

            int price = GetBuyPrice(itemId, run, data);
            if (price <= 0 || run.Gold < price)
            {
                return false;
            }

            var instance = new Item { Data = item, Enhancement = 0 };
            if (!inventory.TryAddEquip(instance))
            {
                return false;
            }

            run.Gold -= price;
            RaiseGoldChanged(run);
            return true;
        }

        /// <summary>스택 가능 아이템 판매. 실패 시 Inventory / Gold 변동 없음.</summary>
        public static bool TrySell(int itemId, int count, GameRunState run, DataManager data, BalanceData balance)
        {
            if (!TryResolveTradeInputs(itemId, run, data, out ItemData item, out InventoryState inventory))
            {
                return false;
            }

            if (count <= 0)
            {
                UnityEngine.Debug.LogError("[Shop.TrySell] count <= 0.");
                return false;
            }

            if (item.Category == ItemCategory.Equipment || !item.Stackable)
            {
                UnityEngine.Debug.LogError("[Shop.TrySell] 장비는 TrySellEquip 을 사용해야 합니다.");
                return false;
            }

            int unitPrice = GetSellPrice(itemId, run, data, balance);
            if (unitPrice <= 0 || inventory.CountOf(itemId) < count)
            {
                return false;
            }

            if (!inventory.Remove(itemId, count))
            {
                return false;
            }

            run.Gold += unitPrice * count;
            RaiseGoldChanged(run);
            return true;
        }

        /// <summary>장비 인스턴스 판매. 실패 시 변동 없음.</summary>
        public static bool TrySellEquip(Item item, GameRunState run, DataManager data, BalanceData balance)
        {
            if (item?.Data == null)
            {
                UnityEngine.Debug.LogError("[Shop.TrySellEquip] item/Data 참조가 없습니다.");
                return false;
            }

            if (!TryResolveTradeInputs(item.Data.Id, run, data, out ItemData itemData, out InventoryState inventory))
            {
                return false;
            }

            if (itemData.Category != ItemCategory.Equipment || itemData.Stackable)
            {
                UnityEngine.Debug.LogError("[Shop.TrySellEquip] 장비 아이템이 아닙니다: " + itemData.Id);
                return false;
            }

            int price = GetSellPrice(itemData.Id, run, data, balance);
            if (price <= 0 || !inventory.RemoveEquip(item))
            {
                return false;
            }

            run.Gold += price;
            RaiseGoldChanged(run);
            return true;
        }

        /// <summary>스택 가능 아이템을 고정 단가로 판매. Safe1 임시 상점처럼 가격 정책이 고정된 UI에서 사용한다.</summary>
        public static bool TrySellForPrice(int itemId, int count, int unitPrice, GameRunState run, DataManager data)
        {
            if (!TryResolveTradeInputs(itemId, run, data, out ItemData item, out InventoryState inventory))
            {
                return false;
            }

            if (count <= 0)
            {
                UnityEngine.Debug.LogError("[Shop.TrySellForPrice] count <= 0.");
                return false;
            }

            if (unitPrice <= 0)
            {
                UnityEngine.Debug.LogError("[Shop.TrySellForPrice] unitPrice <= 0.");
                return false;
            }

            if (item.Category == ItemCategory.Equipment || !item.Stackable)
            {
                UnityEngine.Debug.LogError("[Shop.TrySellForPrice] 장비는 TrySellEquipForPrice 를 사용해야 합니다.");
                return false;
            }

            if (inventory.CountOf(itemId) < count || !inventory.Remove(itemId, count))
            {
                return false;
            }

            run.Gold += unitPrice * count;
            RaiseGoldChanged(run);
            return true;
        }

        /// <summary>장비 인스턴스를 고정 가격으로 판매. 실패 시 변동 없음.</summary>
        public static bool TrySellEquipForPrice(Item item, int price, GameRunState run, DataManager data)
        {
            if (item?.Data == null)
            {
                UnityEngine.Debug.LogError("[Shop.TrySellEquipForPrice] item/Data 참조가 없습니다.");
                return false;
            }

            if (price <= 0)
            {
                UnityEngine.Debug.LogError("[Shop.TrySellEquipForPrice] price <= 0.");
                return false;
            }

            if (!TryResolveTradeInputs(item.Data.Id, run, data, out ItemData itemData, out InventoryState inventory))
            {
                return false;
            }

            if (itemData.Category != ItemCategory.Equipment || itemData.Stackable)
            {
                UnityEngine.Debug.LogError("[Shop.TrySellEquipForPrice] 장비 아이템이 아닙니다: " + itemData.Id);
                return false;
            }

            if (!inventory.RemoveEquip(item))
            {
                return false;
            }

            run.Gold += price;
            RaiseGoldChanged(run);
            return true;
        }

        private static bool TryResolveTradeInputs(int itemId, GameRunState run, DataManager data, out ItemData item, out InventoryState inventory)
        {
            item = null;
            inventory = null;
            if (run?.Player?.Inventory == null)
            {
                UnityEngine.Debug.LogError("[Shop] run / Player / Inventory 참조가 없습니다.");
                return false;
            }

            if (data?.Items == null)
            {
                UnityEngine.Debug.LogError("[Shop] DataManager.Items 참조가 없습니다.");
                return false;
            }

            if (!data.Items.TryGetValue(itemId, out item) || item == null)
            {
                UnityEngine.Debug.LogError("[Shop] 아이템 ID 없음: " + itemId);
                return false;
            }

            inventory = run.Player.Inventory;
            return true;
        }

        private static bool TryResolvePriceInputs(int itemId, GameRunState run, DataManager data, out ItemData item, out float inflation)
        {
            item = null;
            inflation = 1f;
            if (run == null || data?.Items == null)
            {
                UnityEngine.Debug.LogError("[Shop] run 또는 DataManager.Items 참조가 없습니다.");
                return false;
            }

            if (!data.Items.TryGetValue(itemId, out item) || item == null)
            {
                UnityEngine.Debug.LogError("[Shop] 아이템 ID 없음: " + itemId);
                return false;
            }

            int stageIndex = StageIndexFromFloor(run.CurrentFloor);
            float erosionRate = run.Erosion != null ? run.Erosion.GetRate(stageIndex) : 0f;
            inflation = data.ComputeInflation(stageIndex, erosionRate);
            return true;
        }

        private static int StageIndexFromFloor(int floor)
        {
            if (floor <= 3) return 1;
            if (floor <= 11) return 2;
            if (floor <= 19) return 3;
            if (floor <= 29) return 4;
            if (floor <= 39) return 5;
            return 6;
        }

        private static void RaiseGoldChanged(GameRunState run)
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                gsm.Events?.RaiseGoldChanged(run != null ? run.Gold : 0);
            }
        }
    }
}
