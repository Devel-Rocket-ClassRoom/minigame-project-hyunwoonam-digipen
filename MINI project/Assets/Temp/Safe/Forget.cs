namespace Tempt
{
    /// <summary>
    /// 대장간. 장비 강화. 가격은 단계 인플레이션 영향.
    /// </summary>
    public sealed class Forget
    {
        /// <summary>
        /// 대장간 UI 열기.
        /// </summary>
        public void Open()
        {
            // 동작 요약:
            // - 인벤토리 + 장비 슬롯 합쳐 강화 가능 목록 표시.
            // - 현재 단계 강화 등급 상한(SafeZoneDeft 또는 BalanceDatat) 표시.
            //TODO: var player = GameSystemManagert.Instance.CurrentRun.Player;
            //TODO: var enhanceable = new System.Collections.Generic.List<Itemt>();
            //TODO: enhanceable.AddRange(player.Inventory.EquipItems);
            //TODO: foreach (var slot in new[] { EquipmentSlotIdt.Weapon, EquipmentSlotIdt.ArmorBody, EquipmentSlotIdt.ArmorArms, EquipmentSlotIdt.ArmorLegs })
            //TODO: {
            //TODO:     var equipped = player.Equipment.GetSlot(slot);
            //TODO:     if (equipped != null) enhanceable.Add(equipped);
            //TODO: }
            //TODO: int stageIdx = GameSystemManagert.Instance.CurrentRun.CurrentFloor / 8;
            //TODO: int maxEnhance = BalanceDatat.MaxEnhancePerStage[stageIdx];
            //TODO: ForgeUI.Open(enhanceable, maxEnhance, onEnhance: TryEnhance);
        }

        /// <summary>
        /// 강화 시도.
        /// </summary>
        public bool TryEnhance(Itemt target)
        {
            // 동작 요약:
            // - 가격 = base * (level + 1) * inflation.
            // - 골드 차감.
            // - target.Enhancement += 1.
            // - 상한(SafeIndex별) 초과 거부.
            // - 성공 시 EventBust.RaiseEquipmentChanged.
            //TODO: var run = GameSystemManagert.Instance.CurrentRun;
            //TODO: int stageIdx = run.CurrentFloor / 8;
            //TODO: int maxEnhance = BalanceDatat.MaxEnhancePerStage[stageIdx];
            //TODO: if (target.Enhancement >= maxEnhance) return false;
            //TODO: float inflation = GameSystemManagert.Instance.Data.ComputeInflation(stageIdx, run.Erosion.Model.GetRate(stageIdx));
            //TODO: int price = UnityEngine.Mathf.RoundToInt(target.Data.BaseEnhanceCost * (target.Enhancement + 1) * inflation);
            //TODO: if (run.Player.Gold < price) return false;
            //TODO: run.Player.Gold -= price;
            //TODO: target.Enhancement++;
            //TODO: run.Player.RecalcBonusStats();
            //TODO: GameSystemManagert.Instance.Events.RaiseEquipmentChanged(run.Player);
            //TODO: return true;
            return false;
        }
    }
}
