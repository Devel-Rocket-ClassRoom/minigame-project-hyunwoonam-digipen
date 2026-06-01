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

        /// <summary>노드별 현재 투자 포인트. JsonUtility 저장은 SaveSnapshot의 리스트 변환 경로를 사용한다.</summary>
        public Dictionary<int, int> InvestedPointsByNode;

        /// <summary>
        /// 시작 룬 1개 해금. ApplyStartingClass 시점 호출.
        /// </summary>
        public void UnlockStarter()
        {
            // 동작 요약: 루트는 비용 0의 시작 노드이므로 마스터 처리하고 진행 목록에 포함한다.
            EnsureProgressCollections();

            if (Tree?.Starter?.Data == null)
            {
                return;
            }

            Tree.Starter.Unlocked = true;
            Tree.Starter.InvestedPoints = 0;
            UnlockedIds.Add(Tree.Starter.Data.Id);
            InvestedPointsByNode[Tree.Starter.Data.Id] = 0;
        }

        /// <summary>
        /// 사용자가 노드에 룬 포인트 1개를 투자 시도. 신전(룬 변경/초기화)과 자동 호출 흐름에서 사용.
        /// </summary>
        public bool TryUnlock(int nodeId)
        {
            // 동작 요약:
            // - Tree.AllNodes[nodeId] 조회.
            // - Tree.CanUnlock(node) 검사.
            // - RunePoints >= 1 검사.
            // - 성공 시 1포인트 차감 + InvestedPoints 증가.
            // - InvestedPoints >= PointCost 이면 마스터(Unlocked) 처리.
            if (Tree?.AllNodes == null)
            {
                return false;
            }

            EnsureProgressCollections();
            SyncTreeStateFromProgress();
            if (!Tree.AllNodes.TryGetValue(nodeId, out RuneNode node) || !Tree.CanUnlock(node))
            {
                return false;
            }

            int cost = System.Math.Max(0, node.Data.PointCost);
            int invested = GetInvestedPoints(nodeId);
            if (cost <= 0 || invested >= cost || RunePoints <= 0)
            {
                return false;
            }

            RunePoints -= 1;
            invested += 1;
            SetInvestedPoints(node, invested);
            UnlockedIds.Add(nodeId);
            RaiseRuneNodeUnlocked(nodeId, RunePoints);
            RaiseRunePointsChanged(RunePoints);
            return true;
        }

        /// <summary>
        /// 신전에서 룬 초기화. 모든 노드 잠금 + 포인트 환급.
        /// </summary>
        public void ResetTree(BalanceData balance = null)
        {
            float refundRate =
                balance != null ? UnityEngine.Mathf.Clamp01(balance.RuneResetRefundRate) : 1f;
            int refundedPoints = PreviewResetRefund(refundRate);
            RunePoints += refundedPoints;
            if (Tree?.AllNodes != null)
            {
                foreach (RuneNode node in Tree.AllNodes.Values)
                {
                    node.Unlocked = false;
                    node.InvestedPoints = 0;
                }
            }

            EnsureProgressCollections();
            UnlockedIds.Clear();
            InvestedPointsByNode.Clear();
            UnlockStarter();
            RaiseRuneReset(refundedPoints, RunePoints);
            RaiseRunePointsChanged(RunePoints);
        }

        /// <summary>
        /// 신전에서 룬 직업을 변경한다. 골드 검증과 차감은 호출부 책임이다.
        /// </summary>
        public bool ChangeRuneClass(
            RuneClass newClass,
            BalanceData balance,
            IEnumerable<RuneData> allRunes
        )
        {
            if (newClass == RuneClass.None || newClass == ClassId)
            {
                return false;
            }

            RuneTree nextTree = RuneTree.BuildFromData(newClass, allRunes);
            if (nextTree?.Starter?.Data == null)
            {
                return false;
            }

            ResetTree(balance);
            ClassId = newClass;
            Tree = nextTree;
            EnsureProgressCollections();
            UnlockedIds.Clear();
            InvestedPointsByNode.Clear();
            UnlockStarter();
            RaiseRuneClassChanged(newClass);
            return true;
        }

        public int PreviewResetRefund(BalanceData balance)
        {
            float refundRate =
                balance != null ? UnityEngine.Mathf.Clamp01(balance.RuneResetRefundRate) : 1f;
            return PreviewResetRefund(refundRate);
        }

        public bool HasResettableInvestment()
        {
            if (Tree?.AllNodes == null)
            {
                return false;
            }

            EnsureProgressCollections();
            foreach (RuneNode node in Tree.AllNodes.Values)
            {
                if (node != null && node != Tree.Starter && GetInvestedPoints(node.Data.Id) > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private int PreviewResetRefund(float refundRate)
        {
            if (Tree?.AllNodes == null)
            {
                return 0;
            }

            EnsureProgressCollections();
            int refund = 0;
            foreach (RuneNode node in Tree.AllNodes.Values)
            {
                if (node?.Data == null || node == Tree.Starter)
                {
                    continue;
                }

                refund += UnityEngine.Mathf.RoundToInt(
                    GetInvestedPoints(node.Data.Id) * refundRate
                );
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
            if (Tree?.AllNodes == null)
            {
                return result;
            }

            EnsureProgressCollections();
            SyncTreeStateFromProgress();
            foreach (RuneNode node in Tree.AllNodes.Values)
            {
                if (node?.Data == null)
                {
                    continue;
                }

                float ratio = GetInvestmentRatio(node);
                if (ratio > 0f)
                {
                    AddRuneMod(result, node.Data, ratio);
                }
            }

            return result;
        }

        public int GetInvestedPoints(int nodeId)
        {
            EnsureProgressCollections();
            return InvestedPointsByNode.TryGetValue(nodeId, out int invested)
                ? System.Math.Max(0, invested)
                : 0;
        }

        public int GetRequiredPoints(int nodeId)
        {
            if (Tree?.AllNodes == null || !Tree.AllNodes.TryGetValue(nodeId, out RuneNode node))
            {
                return 0;
            }

            return node.RequiredPoints;
        }

        public bool IsNodeMastered(int nodeId)
        {
            if (Tree?.AllNodes == null || !Tree.AllNodes.TryGetValue(nodeId, out RuneNode node))
            {
                return false;
            }

            int required = node.RequiredPoints;
            return required <= 0 ? node.Unlocked : GetInvestedPoints(nodeId) >= required;
        }

        public List<int> GetMasteredNodeIds()
        {
            var ids = new List<int>();
            if (Tree?.AllNodes == null)
            {
                return ids;
            }

            EnsureProgressCollections();
            SyncTreeStateFromProgress();
            foreach (RuneNode node in Tree.AllNodes.Values)
            {
                if (node?.Data != null && node.Unlocked)
                {
                    ids.Add(node.Data.Id);
                }
            }

            return ids;
        }

        public void SyncTreeStateFromProgress()
        {
            EnsureProgressCollections();
            if (Tree?.AllNodes == null)
            {
                return;
            }

            bool hasInvestments = HasAnyStoredInvestment();
            if (!hasInvestments && UnlockedIds.Count > 0)
            {
                foreach (int id in UnlockedIds)
                {
                    if (!Tree.AllNodes.TryGetValue(id, out RuneNode legacyNode))
                    {
                        continue;
                    }

                    InvestedPointsByNode[id] = legacyNode.RequiredPoints;
                }
            }

            UnlockedIds.Clear();
            foreach (RuneNode node in Tree.AllNodes.Values)
            {
                if (node?.Data == null)
                {
                    continue;
                }

                int required = node.RequiredPoints;
                int invested = GetInvestedPoints(node.Data.Id);
                invested = required > 0 ? UnityEngine.Mathf.Clamp(invested, 0, required) : 0;
                node.InvestedPoints = invested;
                node.Unlocked =
                    required <= 0 ? node.Data.RequiredRuneId == 0 : invested >= required;

                if (node.Unlocked || invested > 0)
                {
                    UnlockedIds.Add(node.Data.Id);
                }
            }

            if (Tree.Starter?.Data != null)
            {
                UnlockStarter();
            }
        }

        private void SetInvestedPoints(RuneNode node, int invested)
        {
            if (node?.Data == null)
            {
                return;
            }

            int required = node.RequiredPoints;
            int clamped = required > 0 ? UnityEngine.Mathf.Clamp(invested, 0, required) : 0;
            node.InvestedPoints = clamped;
            node.Unlocked = required <= 0 || clamped >= required;
            InvestedPointsByNode[node.Data.Id] = clamped;
        }

        private float GetInvestmentRatio(RuneNode node)
        {
            if (node?.Data == null)
            {
                return 0f;
            }

            int required = node.RequiredPoints;
            if (required <= 0)
            {
                return node.Unlocked ? 1f : 0f;
            }

            return UnityEngine.Mathf.Clamp01(GetInvestedPoints(node.Data.Id) / (float)required);
        }

        private bool HasAnyStoredInvestment()
        {
            foreach (int value in InvestedPointsByNode.Values)
            {
                if (value > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private void EnsureProgressCollections()
        {
            if (UnlockedIds == null)
            {
                UnlockedIds = new HashSet<int>();
            }

            if (InvestedPointsByNode == null)
            {
                InvestedPointsByNode = new Dictionary<int, int>();
            }
        }

        private static void AddRuneMod(EquipmentStatMod target, RuneData data, float ratio)
        {
            int value = UnityEngine.Mathf.RoundToInt((int)data.EffectValue * ratio);
            switch (data.EffectType)
            {
                case RuneEffectType.AddMaxHP:
                    target.HP += value;
                    break;
                case RuneEffectType.AddMaxMP:
                    target.MP += value;
                    break;
                case RuneEffectType.AddATK:
                    target.ATK += value;
                    break;
                case RuneEffectType.AddDEF:
                    target.DEF += value;
                    break;
                case RuneEffectType.AddSPD:
                    target.SPD += value;
                    break;
            }
        }

        private static void RaiseRuneNodeUnlocked(int nodeId, int remainingPoints)
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                gsm.Events?.RaiseRuneNodeUnlocked(nodeId, remainingPoints);
            }
        }

        private static void RaiseRunePointsChanged(int currentPoints)
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                gsm.Events?.RaiseRunePointsChanged(currentPoints);
            }
        }

        private static void RaiseRuneClassChanged(RuneClass newClass)
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                gsm.Events?.RaiseRuneClassChanged(newClass);
            }
        }

        private static void RaiseRuneReset(int refundedPoints, int currentPoints)
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                gsm.Events?.RaiseRuneReset(refundedPoints, currentPoints);
            }
        }
    }
}
