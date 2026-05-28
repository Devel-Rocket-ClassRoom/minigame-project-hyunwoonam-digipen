using System;

namespace Tempt
{
    /// <summary>
    /// 도메인 이벤트 발행/구독. UI ↔ 도메인의 직접 참조를 끊기 위해 사용.
    /// 발행은 도메인이, 구독은 UI/매니저가 한다.
    /// </summary>
    public sealed class EventBus
    {
        /// <summary>플레이어 레벨업 발생. (newLevel)</summary>
        public event Action<int> OnPlayerLevelUp;

        /// <summary>플레이어 EXP 변경. (currentExp, requiredExp)</summary>
        public event Action<int, int> OnPlayerExpChanged;

        /// <summary>단계 침식률 변경. (stageIndex, rate)</summary>
        public event Action<int, float> OnStageErosionChanged;

        /// <summary>안전지대 잠금 변화. (safeZoneIndex, locked)</summary>
        public event Action<int, bool> OnSafeZoneLockChanged;

        /// <summary>골드 변동. (newGold)</summary>
        public event Action<int> OnGoldChanged;

        /// <summary>마석 변동. (newManaStone)</summary>
        public event Action<int> OnManaStoneChanged;

        /// <summary>인벤토리 변경.</summary>
        public event Action OnInventoryChanged;

        /// <summary>장비 변경.</summary>
        public event Action OnEquipmentChanged;

        // Guid3 §9.E 2026-05-27: 길드 시스템의 보유 스킬 / ActiveSlotSkillIds 변경 이벤트.
        /// <summary>보유 스킬 풀 또는 ActiveSlotSkillIds 변경.</summary>
        public event Action OnSkillsChanged;

        /// <summary>동료 모집/해고. (companionId, joined)</summary>
        public event Action<int, bool> OnRosterChanged;

        /// <summary>언어 변경 후 즉시 UI 재구성용. (newLang)</summary>
        public event Action<string> OnLanguageChanged;

        /// <summary>플레이어 레벨업 발행.</summary>
        public void RaisePlayerLevelUp(int newLevel)
        {
            // 동작 요약: OnPlayerLevelUp?.Invoke(newLevel).
            //TODO: OnPlayerLevelUp?.Invoke(newLevel);
            OnPlayerLevelUp?.Invoke(newLevel); //Wave0write
        }

        /// <summary>플레이어 EXP 변경 발행.</summary>
        public void RaisePlayerExpChanged(int current, int required)
        {
            // 동작 요약: OnPlayerExpChanged?.Invoke(current, required).
            //TODO: OnPlayerExpChanged?.Invoke(current, required);
            OnPlayerExpChanged?.Invoke(current, required); //Wave0write
        }

        /// <summary>단계 침식 변경 발행.</summary>
        public void RaiseStageErosionChanged(int stage, float rate)
        {
            // 동작 요약: OnStageErosionChanged?.Invoke(stage, rate).
            //TODO: OnStageErosionChanged?.Invoke(stage, rate);
            OnStageErosionChanged?.Invoke(stage, rate); //Wave0write
        }

        /// <summary>안전지대 잠금 변화 발행.</summary>
        public void RaiseSafeZoneLockChanged(int idx, bool locked)
        {
            // 동작 요약: OnSafeZoneLockChanged?.Invoke(idx, locked).
            //TODO: OnSafeZoneLockChanged?.Invoke(idx, locked);
            OnSafeZoneLockChanged?.Invoke(idx, locked); //Wave0write
        }

        /// <summary>골드 변동 발행.</summary>
        public void RaiseGoldChanged(int v)
        {
            // 동작 요약: OnGoldChanged?.Invoke(v).
            //TODO: OnGoldChanged?.Invoke(v);
            OnGoldChanged?.Invoke(v); //Wave0write
        }

        /// <summary>마석 변동 발행.</summary>
        public void RaiseManaStoneChanged(int v)
        {
            // 동작 요약: OnManaStoneChanged?.Invoke(v).
            //TODO: OnManaStoneChanged?.Invoke(v);
            OnManaStoneChanged?.Invoke(v); //Wave0write
        }

        /// <summary>인벤토리 변경 발행.</summary>
        public void RaiseInventoryChanged()
        {
            // 동작 요약: OnInventoryChanged?.Invoke().
            //TODO: OnInventoryChanged?.Invoke();
            OnInventoryChanged?.Invoke(); //Wave0write
        }

        /// <summary>장비 변경 발행.</summary>
        public void RaiseEquipmentChanged()
        {
            // 동작 요약: OnEquipmentChanged?.Invoke().
            //TODO: OnEquipmentChanged?.Invoke();
            OnEquipmentChanged?.Invoke(); //Wave0write
        }

        // Guid3 §9.E 2026-05-27: 보유 스킬 / ActiveSlotSkillIds 변경 발행.
        /// <summary>스킬 풀 또는 슬롯 매핑 변경 발행.</summary>
        public void RaiseSkillsChanged()
        {
            OnSkillsChanged?.Invoke();
        }

        /// <summary>동료 변경 발행.</summary>
        public void RaiseRosterChanged(int companionId, bool joined)
        {
            // 동작 요약: OnRosterChanged?.Invoke(companionId, joined).
            //TODO: OnRosterChanged?.Invoke(companionId, joined);
            OnRosterChanged?.Invoke(companionId, joined); //Wave0write
        }

        /// <summary>언어 변경 발행.</summary>
        public void RaiseLanguageChanged(string lang)
        {
            // 동작 요약: OnLanguageChanged?.Invoke(lang).
            //TODO: OnLanguageChanged?.Invoke(lang);
            OnLanguageChanged?.Invoke(lang); //Wave0write
        }
    }
}

