using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 플레이어 룬 진행 상태. 사용자가 직접 노드 선택 + 룬 포인트 사용으로 해금.
    /// </summary>
    public sealed class PlayerRuneState
    {
        /// <summary>직업.</summary>
        public RuneClass ClassId;

        /// <summary>전체 트리.</summary>
        public RuneTree Tree;

        /// <summary>보유 룬 포인트.</summary>
        public int RunePoints;

        /// <summary>해금된 노드 ID 집합.</summary>
        public HashSet<int> UnlockedIds;

        /// <summary>
        /// 시작 룬 1개 해금. ApplyStartingClass 시점 호출.
        /// </summary>
        public void UnlockStarter()
        {
            // 동작 요약: Tree.Starter.Unlocked = true; UnlockedIds.Add(Tree.Starter.Data.Id).
            if (UnlockedIds == null)
            {
                UnlockedIds = new HashSet<int>();
            }

            if (Tree?.Starter?.Data == null)
            {
                return;
            }

            Tree.Starter.Unlocked = true;
            UnlockedIds.Add(Tree.Starter.Data.Id);
        }

        /// <summary>
        /// 사용자가 노드 1개를 해금 시도. 신전(룬 변경/초기화)과 자동 호출 흐름에서 사용.
        /// </summary>
        public bool TryUnlock(int nodeId)
        {
            // 동작 요약:
            // - Tree.AllNodes[nodeId] 조회.
            // - Tree.CanUnlock(node) 검사.
            // - RunePoints >= node.Data.PointCost 검사.
            // - 성공 시 차감 + Unlocked 표시.
            if (Tree?.AllNodes == null || UnlockedIds == null)
            {
                return false;
            }

            if (!Tree.AllNodes.TryGetValue(nodeId, out RuneNode node) || node.Unlocked || !Tree.CanUnlock(node))
            {
                return false;
            }

            int cost = System.Math.Max(0, node.Data.PointCost);
            if (RunePoints < cost)
            {
                return false;
            }

            RunePoints -= cost;
            node.Unlocked = true;
            UnlockedIds.Add(nodeId);
            return true;
        }

        /// <summary>
        /// 신전에서 룬 초기화. 모든 노드 잠금 + 포인트 환급.
        /// </summary>
        public void ResetTree(BalanceData balance = null)
        {
            float refundRate = balance != null ? UnityEngine.Mathf.Clamp01(balance.RuneResetRefundRate) : 1f;
            RunePoints += ComputeResetRefund(refundRate);
            if (Tree?.AllNodes != null)
            {
                foreach (RuneNode node in Tree.AllNodes.Values)
                {
                    node.Unlocked = false;
                }
            }

            if (UnlockedIds == null)
            {
                UnlockedIds = new HashSet<int>();
            }

            UnlockedIds.Clear();
            UnlockStarter();
        }

        private int ComputeResetRefund(float refundRate)
        {
            if (UnlockedIds == null || Tree?.AllNodes == null)
            {
                return 0;
            }

            int refund = 0;
            foreach (int id in UnlockedIds)
            {
                if (!Tree.AllNodes.TryGetValue(id, out RuneNode node) || node?.Data == null || node == Tree.Starter)
                {
                    continue;
                }

                refund += UnityEngine.Mathf.RoundToInt(System.Math.Max(0, node.Data.PointCost) * refundRate);
            }

            return refund;
        }

        /// <summary>
        /// 레벨업 시 포인트 적립.
        /// </summary>
        public void AddRunePoint(int amount)
        {
            // 동작 요약: RunePoints += amount.
            RunePoints += System.Math.Max(0, amount);
        }

        /// <summary>
        /// 현재 해금된 룬 노드의 스탯 보정 합산.
        /// UnlockSkill 타입 노드는 이 메서드에서 스탯에 반영하지 않는다(패시브 스킬은 SyncPassivesFromRunes 경로).
        /// </summary>
        public EquipmentStatMod AggregateStatMod()
        {
            // 동작 요약:
            // - UnlockedIds 순회 → Tree.AllNodes[id].Data 조회.
            // - EffectType 분기:
            //   AddMaxHP  → result.HP  += (int)EffectValue
            //   AddMaxMP  → result.MP  += (int)EffectValue
            //   AddATK    → result.ATK += (int)EffectValue
            //   AddDEF    → result.DEF += (int)EffectValue
            //   AddSPD    → result.SPD += (int)EffectValue
            //   UnlockSkill / DamageBoost / HealBoost → 스탯 합산 대상 아님, 스킵.
            // - 합산 결과 EquipmentStatMod 반환.
            var result = new EquipmentStatMod();
            if (UnlockedIds == null || Tree?.AllNodes == null)
            {
                return result;
            }

            foreach (int id in UnlockedIds)
            {
                if (!Tree.AllNodes.TryGetValue(id, out RuneNode node) || node.Data == null)
                {
                    continue;
                }

                AddRuneMod(result, node.Data);
            }

            return result;
        }

        private static void AddRuneMod(EquipmentStatMod target, RuneData data)
        {
            int value = (int)data.EffectValue;
            switch (data.EffectType)
            {
                case RuneEffectType.AddMaxHP: target.HP += value; break;
                case RuneEffectType.AddMaxMP: target.MP += value; break;
                case RuneEffectType.AddATK: target.ATK += value; break;
                case RuneEffectType.AddDEF: target.DEF += value; break;
                case RuneEffectType.AddSPD: target.SPD += value; break;
            }
        }
    }
}

