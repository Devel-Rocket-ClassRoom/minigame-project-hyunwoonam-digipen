namespace Tempt
{
    // Guid3 §5 2026-05-27: 길드의 스킬 구매 / 가격 산식 단일 진입점.
    // Shop 과 같은 정책: BasePrice 위치는 SkillData.PurchasePrice, Inflation 은 Shop 산식 그대로 사용.
    // fallback 금지: run / data / Skill 데이터 누락은 LogError + 0 / false.
    /// <summary>
    /// 길드 스킬 거래. 구매 후 PlayerState.OwnedSkillIds 에 등록한다.
    /// 스킬 판매 / 환불은 Wave0 합의 범위 밖.
    /// </summary>
    public static class Guild
    {
        /// <summary>현재 단계 침식률을 적용한 스킬 구매가. 데이터/상태 누락 시 0.</summary>
        public static int GetSkillBuyPrice(int skillId, GameRunState run, DataManager data)
        {
            if (!TryResolvePriceInputs(skillId, run, data, out SkillData skill, out float inflation))
            {
                return 0;
            }

            if (skill.AcquireType != AcquireType.Shop)
            {
                return 0;
            }

            if (skill.PurchasePrice <= 0)
            {
                UnityEngine.Debug.LogError("[Guild.GetSkillBuyPrice] PurchasePrice 가 0 이하입니다: " + skillId);
                return 0;
            }

            return UnityEngine.Mathf.Max(1, UnityEngine.Mathf.RoundToInt(skill.PurchasePrice * inflation));
        }

        /// <summary>스킬이 현재 구매 가능한지(보유 여부 + 골드 + 데이터 검증). UI 버튼 활성 표시용.</summary>
        public static bool CanBuy(int skillId, GameRunState run, DataManager data)
        {
            if (!TryResolveTradeInputs(skillId, run, data, out SkillData skill))
            {
                return false;
            }

            if (skill.AcquireType != AcquireType.Shop)
            {
                return false;
            }

            if (run.Player.OwnedSkillIds != null && run.Player.OwnedSkillIds.Contains(skillId))
            {
                return false;
            }

            int price = GetSkillBuyPrice(skillId, run, data);
            return price > 0 && run.Gold >= price;
        }

        /// <summary>스킬을 골드로 구매. AcquireType == Shop 인 스킬만 허용. 실패 시 변동 없음.</summary>
        public static bool TryBuySkill(int skillId, GameRunState run, DataManager data)
        {
            if (!TryResolveTradeInputs(skillId, run, data, out SkillData skill))
            {
                return false;
            }

            if (skill.AcquireType != AcquireType.Shop)
            {
                return false;
            }

            if (skill.SkillType == SkillType.Passive)
            {
                return false;
            }

            if (run.Player.OwnedSkillIds == null)
            {
                run.Player.OwnedSkillIds = new System.Collections.Generic.HashSet<int>();
            }

            if (run.Player.OwnedSkillIds.Contains(skillId))
            {
                return false;
            }

            int price = GetSkillBuyPrice(skillId, run, data);
            if (price <= 0 || run.Gold < price)
            {
                return false;
            }

            run.Gold -= price;
            run.Player.OwnedSkillIds.Add(skillId);
            RaiseChanged(run);
            return true;
        }

        private static bool TryResolveTradeInputs(int skillId, GameRunState run, DataManager data, out SkillData skill)
        {
            skill = null;
            if (run?.Player == null)
            {
                UnityEngine.Debug.LogError("[Guild] run / Player 참조가 없습니다.");
                return false;
            }

            if (data?.Skills == null)
            {
                UnityEngine.Debug.LogError("[Guild] DataManager.Skills 참조가 없습니다.");
                return false;
            }

            if (!data.Skills.TryGetValue(skillId, out skill) || skill == null)
            {
                UnityEngine.Debug.LogError("[Guild] 스킬 ID 없음: " + skillId);
                return false;
            }

            return true;
        }

        private static bool TryResolvePriceInputs(int skillId, GameRunState run, DataManager data, out SkillData skill, out float inflation)
        {
            inflation = 1f;
            if (!TryResolveTradeInputs(skillId, run, data, out skill))
            {
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

        private static void RaiseChanged(GameRunState run)
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                gsm.Events?.RaiseGoldChanged(run != null ? run.Gold : 0);
                gsm.Events?.RaiseSkillsChanged();
            }
        }
    }
}
