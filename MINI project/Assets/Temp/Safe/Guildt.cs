namespace Tempt
{
    /// <summary>
    /// 길드. 동료 등록(파티 구성), 스킬 구매/장착. 최대 3명까지 파티 편성.
    /// </summary>
    public sealed class Guildt
    {
        /// <summary>
        /// 길드 UI 열기.
        /// </summary>
        public void Open()
        {
            // 동작 요약:
            // - Roster.Bench와 Roster.Active 표시.
            // - 파티 슬롯 3칸(Active.Count ≤ 3) 편성 UI.
            // - 스킬 구매 목록(SkillDatat에서 캐릭터 직업으로 필터).
            //TODO: var roster = GameSystemManagert.Instance.CurrentRun.Roster;
            //TODO: PartyUI.Bind(roster.Active, roster.Bench, onAdd: TryAddToParty, onRemove: TryRemoveFromParty);
            //TODO: var player = GameSystemManagert.Instance.CurrentRun.Player;
            //TODO: var skills = new System.Collections.Generic.List<SkillDatat>();
            //TODO: foreach (var s in GameSystemManagert.Instance.Data.Skills.Values)
            //TODO:     if (s.AcquireType == SkillAcquireTypet.Purchase && s.RuneClass == player.StartingClass)
            //TODO:         skills.Add(s);
            //TODO: SkillShopUI.Bind(skills, onBuy: TryBuySkill);
        }

        /// <summary>
        /// 동료 파티 합류.
        /// </summary>
        public bool TryAddToParty(int companionId)
        {
            // 동작 요약:
            // - Roster.Active.Count < 3 검사.
            // - Bench → Active 이동.
            //TODO: var roster = GameSystemManagert.Instance.CurrentRun.Roster;
            //TODO: if (roster.Active.Count >= 3) return false;
            //TODO: var companion = roster.Bench.Find(c => c.Data.Id == companionId);
            //TODO: if (companion == null) return false;
            //TODO: roster.Bench.Remove(companion);
            //TODO: roster.Active.Add(companion);
            //TODO: return true;
            return false;
        }

        /// <summary>
        /// 동료 파티 제외.
        /// </summary>
        public bool TryRemoveFromParty(int companionId)
        {
            // 동작 요약: Active → Bench 이동.
            //TODO: var roster = GameSystemManagert.Instance.CurrentRun.Roster;
            //TODO: var companion = roster.Active.Find(c => c.Data.Id == companionId);
            //TODO: if (companion == null) return false;
            //TODO: roster.Active.Remove(companion);
            //TODO: roster.Bench.Add(companion);
            //TODO: return true;
            return false;
        }

        /// <summary>
        /// 스킬 구매(플레이어 또는 동료에게 학습 — 설계 보류 항목).
        /// </summary>
        public bool TryBuySkill(int skillId, int targetCharacterId)
        {
            // 동작 요약:
            // - 가격 차감.
            // - 대상 캐릭터의 액티브 슬롯에 장착 가능 여부 확인 후 SetActiveSkill.
            //TODO: var data = GameSystemManagert.Instance.Data.Skills[skillId];
            //TODO: var run = GameSystemManagert.Instance.CurrentRun;
            //TODO: if (run.Player.Gold < data.PurchasePrice) return false;
            //TODO: // 대상 캐릭터 찾기 (플레이어 or 동료)
            //TODO: CharacterBaset target = run.Player.Id == targetCharacterId ? (CharacterBaset)run.Player
            //TODO:     : run.Roster.Active.Find(c => c.Id == targetCharacterId) ?? run.Roster.Bench.Find(c => c.Id == targetCharacterId);
            //TODO: if (target == null) return false;
            //TODO: if (!target.CanLearnSkill(skillId)) return false;
            //TODO: run.Player.Gold -= data.PurchasePrice;
            //TODO: target.LearnSkill(skillId);
            //TODO: return true;
            return false;
        }
    }
}
