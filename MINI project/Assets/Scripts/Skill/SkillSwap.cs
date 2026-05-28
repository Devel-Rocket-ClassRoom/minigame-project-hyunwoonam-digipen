namespace Tempt
{
    // Guid3 §6 2026-05-27: ActiveSkills 슬롯 교체/해제 단일 헬퍼.
    // - OwnedSkillIds 검사 + SkillType / AcquireType 검사 + 전투 중 가드.
    // - PlayerState.ActiveSlotSkillIds 와 Player MonoBehaviour 의 ActiveSkills 양쪽을 동기화한다.
    // - ActivePlayer (GameSystemManager) 가 null 인 비전투 상황에서는 PlayerState 만 갱신 후
    //   다음 BindState 가 EntityBase.ActiveSkills 를 따라잡는다(Guid3 §10 W-G3-1 정책).
    /// <summary>
    /// ActiveSkills 슬롯 2칸의 장착 / 해제 단일 진입점.
    /// </summary>
    public static class SkillSwap
    {
        /// <summary>슬롯에 OwnedSkillIds 의 스킬을 장착. 다른 슬롯에 같은 스킬이 있으면 그 슬롯을 비운다.</summary>
        public static bool TrySetSlot(Player player, int slotIndex, int skillId)
        {
            // 본문 의사코드: Guid3 §6.3 TrySetSlot.
            // 검증 순서:
            //  1) slotIndex 범위(0/1).
            //  2) skillId != 0 (빈 슬롯은 TryClearSlot 사용).
            //  3) GameSystemManager.CombatContext != null 이면 false (전투 중 차단).
            //  4) PlayerState.OwnedSkillIds.Contains(skillId).
            //  5) data.Skills.TryGetValue → SkillType != Passive && AcquireType != MonsterOnly.
            //  6) 다른 슬롯에 같은 skillId 가 있으면 그 슬롯 = 0.
            //  7) PlayerState.ActiveSlotSkillIds[slotIndex] = skillId.
            //  8) player != null 이면 player.SetActiveSkill(slotIndex, new Skill(data)).
            //  9) EventBus.RaiseSkillsChanged().
            return false;
        }

        /// <summary>슬롯을 비운다. 이미 0 이면 false.</summary>
        public static bool TryClearSlot(Player player, int slotIndex)
        {
            // 본문 의사코드: Guid3 §6.3 TryClearSlot.
            // 1) slotIndex 범위 + 전투 중 가드.
            // 2) PlayerState.ActiveSlotSkillIds[slotIndex] == 0 이면 false.
            // 3) PlayerState.ActiveSlotSkillIds[slotIndex] = 0.
            // 4) player != null 이면 player.SetActiveSkill(slotIndex, null).
            // 5) EventBus.RaiseSkillsChanged().
            return false;
        }

        /// <summary>현재 슬롯에 들어 있는 SkillId. 슬롯 범위 벗어나면 0.</summary>
        public static int GetSlotSkillId(Player player, int slotIndex)
        {
            // 본문 의사코드: Guid3 §6.3 GetSlotSkillId.
            return 0;
        }
    }
}
