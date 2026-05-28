namespace Tempt
{
    /// <summary>
    /// 상점. 아이템 구매/판매. 단계별 가격 인플레이션 적용.
    /// </summary>
    public sealed class Shopt
    {
        /// <summary>이번 진입 가격 보정(단계 침식률 기반).</summary>
        public float CurrentInflation = 1f;

        /// <summary>
        /// 상점 UI 열기.
        /// </summary>
        public void Open()
        {
            // 동작 요약:
            // - 인플레이션 계산(현재 안전지대 단계, 현재 침식률).
            // - 판매 목록 UI 그리기.
            //TODO: var run = GameSystemManagert.Instance.CurrentRun;
            //TODO: int stageIdx = run.CurrentFloor / 8;
            //TODO: float erosionRate = run.Erosion.Model.GetRate(stageIdx);
            //TODO: CurrentInflation = GameSystemManagert.Instance.Data.ComputeInflation(stageIdx, erosionRate);
            //TODO: ShopUI.Open(GameSystemManagert.Instance.Data.Items.Values, CurrentInflation, onBuy: TryBuy, onSell: TrySell);
        }

        /// <summary>
        /// 구매.
        /// </summary>
        public bool TryBuy(int itemId, int count)
        {
            // 동작 요약:
            // - price = data.BaseBuyPrice * CurrentInflation * count.
            // - CurrentRun.Gold 차감 → Inventory.Add.
            //TODO: var run = GameSystemManagert.Instance.CurrentRun;
            //TODO: var data = GameSystemManagert.Instance.Data.Items[itemId];
            //TODO: int price = UnityEngine.Mathf.RoundToInt(data.BaseBuyPrice * CurrentInflation * count);
            //TODO: if (run.Gold < price) return false;
            //TODO: if (!run.Player.Inventory.TryAdd(itemId, count)) return false;
            //TODO: run.Gold -= price;
            //TODO: return true;
            return false;
        }

        /// <summary>
        /// 판매.
        /// </summary>
        public bool TrySell(int itemId, int count)
        {
            // 동작 요약:
            // - Inventory.Remove(itemId, count) → CurrentRun.Gold += BaseSellPrice * count.
            //TODO: var run = GameSystemManagert.Instance.CurrentRun;
            //TODO: var data = GameSystemManagert.Instance.Data.Items[itemId];
            //TODO: if (run.Player.Inventory.CountOf(itemId) < count) return false;
            //TODO: run.Player.Inventory.Remove(itemId, count);
            //TODO: run.Gold += UnityEngine.Mathf.RoundToInt(data.BaseSellPrice * count);
            //TODO: return true;
            return false;
        }
    }
}
