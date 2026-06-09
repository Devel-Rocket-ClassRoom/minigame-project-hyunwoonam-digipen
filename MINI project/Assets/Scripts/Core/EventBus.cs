using System;

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

    /// <summary>단계 침식률 100% 도달. (stageIndex)</summary>
    public event Action<int> OnStageFullyEroded;

    /// <summary>모든 단계 침식 완료.</summary>
    public event Action OnAllStagesEroded;

    /// <summary>침식 시스템 활성화.</summary>
    public event Action OnErosionActivated;

    /// <summary>골드 변동. (newGold)</summary>
    public event Action<int> OnGoldChanged;

    /// <summary>게임 내 일자 변경. (newDay)</summary>
    public event Action<int> OnDayChanged;

    /// <summary>마석 변동. (newManaStone)</summary>
    public event Action<int> OnManaStoneChanged;

    /// <summary>인벤토리 변경.</summary>
    public event Action OnInventoryChanged;

    /// <summary>장비 변경.</summary>
    public event Action OnEquipmentChanged;

    /// <summary>강화 시도 결과.</summary>
    public event Action<EnhanceResult> OnEnhanceResult;

    // Guid3 §9.E 2026-05-27: 길드 시스템의 보유 스킬 / ActiveSlotSkillIds 변경 이벤트.
    /// <summary>보유 스킬 풀 또는 ActiveSlotSkillIds 변경.</summary>
    public event Action OnSkillsChanged;

    /// <summary>룬 노드 해금. (nodeId, remainingPoints)</summary>
    public event Action<int, int> OnRuneNodeUnlocked;

    /// <summary>룬 포인트 변경. (currentPoints)</summary>
    public event Action<int> OnRunePointsChanged;

    /// <summary>룬 직업 변경. (newClass)</summary>
    public event Action<RuneClass> OnRuneClassChanged;

    /// <summary>룬 트리 리셋. (refundedPoints, currentPoints)</summary>
    public event Action<int, int> OnRuneReset;

    /// <summary>동료 모집/해고. (companionId, joined)</summary>
    public event Action<int, bool> OnRosterChanged;

    /// <summary>언어 변경 후 즉시 UI 재구성용. (newLang)</summary>
    public event Action<string> OnLanguageChanged;

    /// <summary>플레이어 레벨업 발행.</summary>
    public void RaisePlayerLevelUp(int newLevel)
    {
        // 동작 요약: OnPlayerLevelUp?.Invoke(newLevel).
        OnPlayerLevelUp?.Invoke(newLevel);
    }

    /// <summary>플레이어 EXP 변경 발행.</summary>
    public void RaisePlayerExpChanged(int current, int required)
    {
        // 동작 요약: OnPlayerExpChanged?.Invoke(current, required).
        OnPlayerExpChanged?.Invoke(current, required);
    }

    /// <summary>단계 침식 변경 발행.</summary>
    public void RaiseStageErosionChanged(int stage, float rate)
    {
        // 동작 요약: OnStageErosionChanged?.Invoke(stage, rate).
        OnStageErosionChanged?.Invoke(stage, rate);
    }

    /// <summary>안전지대 잠금 변화 발행.</summary>
    public void RaiseSafeZoneLockChanged(int idx, bool locked)
    {
        // 동작 요약: OnSafeZoneLockChanged?.Invoke(idx, locked).
        OnSafeZoneLockChanged?.Invoke(idx, locked);
    }

    // Guid4 §8.A 2026-05-29: 단계 100% 도달 통지.
    public void RaiseStageFullyEroded(int stage)
    {
        OnStageFullyEroded?.Invoke(stage);
    }

    // Guid4 §8.B 2026-05-29: 모든 단계 침식 완료 통지.
    public void RaiseAllStagesEroded()
    {
        OnAllStagesEroded?.Invoke();
    }

    // Guid4 §8.C 2026-05-29: 침식 시스템 활성화 통지.
    public void RaiseErosionActivated()
    {
        OnErosionActivated?.Invoke();
    }

    /// <summary>골드 변동 발행.</summary>
    public void RaiseGoldChanged(int v)
    {
        // 동작 요약: OnGoldChanged?.Invoke(v).
        OnGoldChanged?.Invoke(v);
    }

    // Guid4 §9.K 2026-05-29: SafeZone DayText 갱신용 일자 변경 발행.
    public void RaiseDayChanged(int day)
    {
        OnDayChanged?.Invoke(day);
    }

    /// <summary>마석 변동 발행.</summary>
    public void RaiseManaStoneChanged(int v)
    {
        // 동작 요약: OnManaStoneChanged?.Invoke(v).
        OnManaStoneChanged?.Invoke(v);
    }

    /// <summary>인벤토리 변경 발행.</summary>
    public void RaiseInventoryChanged()
    {
        // 동작 요약: OnInventoryChanged?.Invoke().
        OnInventoryChanged?.Invoke();
    }

    /// <summary>장비 변경 발행.</summary>
    public void RaiseEquipmentChanged()
    {
        // 동작 요약: OnEquipmentChanged?.Invoke().
        OnEquipmentChanged?.Invoke();
    }

    /// <summary>강화 결과 발행.</summary>
    public void RaiseEnhanceResult(EnhanceResult result)
    {
        OnEnhanceResult?.Invoke(result);
    }

    // Guid3 §9.E 2026-05-27: 보유 스킬 / ActiveSlotSkillIds 변경 발행.
    /// <summary>스킬 풀 또는 슬롯 매핑 변경 발행.</summary>
    public void RaiseSkillsChanged()
    {
        OnSkillsChanged?.Invoke();
    }

    /// <summary>룬 노드 해금 발행.</summary>
    public void RaiseRuneNodeUnlocked(int nodeId, int remainingPoints)
    {
        OnRuneNodeUnlocked?.Invoke(nodeId, remainingPoints);
    }

    /// <summary>룬 포인트 변경 발행.</summary>
    public void RaiseRunePointsChanged(int currentPoints)
    {
        OnRunePointsChanged?.Invoke(currentPoints);
    }

    /// <summary>룬 직업 변경 발행.</summary>
    public void RaiseRuneClassChanged(RuneClass newClass)
    {
        OnRuneClassChanged?.Invoke(newClass);
    }

    /// <summary>룬 트리 리셋 발행.</summary>
    public void RaiseRuneReset(int refundedPoints, int currentPoints)
    {
        OnRuneReset?.Invoke(refundedPoints, currentPoints);
    }

    /// <summary>동료 변경 발행.</summary>
    public void RaiseRosterChanged(int companionId, bool joined)
    {
        // 동작 요약: OnRosterChanged?.Invoke(companionId, joined).
        OnRosterChanged?.Invoke(companionId, joined);
    }

    /// <summary>언어 변경 발행.</summary>
    public void RaiseLanguageChanged(string lang)
    {
        // 동작 요약: OnLanguageChanged?.Invoke(lang).
        OnLanguageChanged?.Invoke(lang);
    }
}
