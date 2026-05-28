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
            //TODO: Tree.Starter.Unlocked = true;
            //TODO: UnlockedIds.Add(Tree.Starter.Data.Id);
            if (UnlockedIds == null) //Wave0write
            { //Wave0write
                UnlockedIds = new HashSet<int>(); //Wave0write
            } //Wave0write

            if (Tree?.Starter?.Data == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            Tree.Starter.Unlocked = true; //Wave0write
            UnlockedIds.Add(Tree.Starter.Data.Id); //Wave0write
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
            //TODO: if (!Tree.AllNodes.TryGetValue(nodeId, out var node)) return false;
            //TODO: if (!Tree.CanUnlock(node)) return false;
            //TODO: if (RunePoints < node.Data.PointCost) return false;
            //TODO: RunePoints -= node.Data.PointCost;
            //TODO: node.Unlocked = true;
            //TODO: UnlockedIds.Add(nodeId);
            //TODO: return true;
            if (Tree?.AllNodes == null || UnlockedIds == null) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            if (!Tree.AllNodes.TryGetValue(nodeId, out RuneNode node) || node.Unlocked || !Tree.CanUnlock(node)) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            int cost = System.Math.Max(0, node.Data.PointCost); //Wave0write
            if (RunePoints < cost) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            RunePoints -= cost; //Wave0write
            node.Unlocked = true; //Wave0write
            UnlockedIds.Add(nodeId); //Wave0write
            return true; //Wave0write
        }

        /// <summary>
        /// 신전에서 룬 초기화. 모든 노드 잠금 + 포인트 환급.
        /// </summary>
        public void ResetTree()
        {
            // 동작 요약:
            // - 모든 노드 Unlocked = false.
            // - 시작 룬은 다시 자동 해금.
            // - 환급 정책(전액 환급 / 일부 손실)은 BalanceDatat로.
            //TODO: // 해금된 포인트 합 계산 후 환급
            //TODO: int refundable = 0;
            //TODO: foreach (var id in UnlockedIds)
            //TODO:     if (Tree.AllNodes.TryGetValue(id, out var n) && n != Tree.Starter)
            //TODO:         refundable += n.Data.PointCost;
            //TODO: RunePoints += UnityEngine.Mathf.RoundToInt(refundable * BalanceData.RuneResetRefundRate);
            //TODO: // 모든 노드 잠금
            //TODO: foreach (var node in Tree.AllNodes.Values) node.Unlocked = false;
            //TODO: UnlockedIds.Clear();
            //TODO: UnlockStarter(); // 시작 룬 재해금
            RunePoints += UnlockedIds != null ? System.Math.Max(0, UnlockedIds.Count - 1) : 0; //Wave0write
            if (Tree?.AllNodes != null) //Wave0write
            { //Wave0write
                foreach (RuneNode node in Tree.AllNodes.Values) //Wave0write
                { //Wave0write
                    node.Unlocked = false; //Wave0write
                } //Wave0write
            } //Wave0write

            if (UnlockedIds == null) //Wave0write
            { //Wave0write
                UnlockedIds = new HashSet<int>(); //Wave0write
            } //Wave0write

            UnlockedIds.Clear(); //Wave0write
            UnlockStarter(); //Wave0write
        }

        /// <summary>
        /// 레벨업 시 포인트 적립.
        /// </summary>
        public void AddRunePoint(int amount)
        {
            // 동작 요약: RunePoints += amount.
            //TODO: RunePoints += amount;
            RunePoints += System.Math.Max(0, amount); //Wave0write
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
            //TODO: var result = new EquipmentStatMod();
            //TODO: foreach (var id in UnlockedIds)
            //TODO: {
            //TODO:     if (!Tree.AllNodes.TryGetValue(id, out var node)) continue;
            //TODO:     switch (node.Data.EffectType)
            //TODO:     {
            //TODO:         case RuneEffectType.AddMaxHP:  result.HP  += (int)node.Data.EffectValue; break;
            //TODO:         case RuneEffectType.AddMaxMP:  result.MP  += (int)node.Data.EffectValue; break;
            //TODO:         case RuneEffectType.AddATK:    result.ATK += (int)node.Data.EffectValue; break;
            //TODO:         case RuneEffectType.AddDEF:    result.DEF += (int)node.Data.EffectValue; break;
            //TODO:         case RuneEffectType.AddSPD:    result.SPD += (int)node.Data.EffectValue; break;
            //TODO:         // UnlockSkill, DamageBoost, HealBoost는 스탯 합산 대상 아님
            //TODO:     }
            //TODO: }
            //TODO: return result;
            var result = new EquipmentStatMod(); //Wave0write
            if (UnlockedIds == null || Tree?.AllNodes == null) //Wave0write
            { //Wave0write
                return result; //Wave0write
            } //Wave0write

            foreach (int id in UnlockedIds) //Wave0write
            { //Wave0write
                if (!Tree.AllNodes.TryGetValue(id, out RuneNode node) || node.Data == null) //Wave0write
                { //Wave0write
                    continue; //Wave0write
                } //Wave0write

                AddRuneMod(result, node.Data); //Wave0write
            } //Wave0write

            return result; //Wave0write
        }

        private static void AddRuneMod(EquipmentStatMod target, RuneData data) //Wave0write
        { //Wave0write
            int value = (int)data.EffectValue; //Wave0write
            switch (data.EffectType) //Wave0write
            { //Wave0write
                case RuneEffectType.AddMaxHP: target.HP += value; break; //Wave0write
                case RuneEffectType.AddMaxMP: target.MP += value; break; //Wave0write
                case RuneEffectType.AddATK: target.ATK += value; break; //Wave0write
                case RuneEffectType.AddDEF: target.DEF += value; break; //Wave0write
                case RuneEffectType.AddSPD: target.SPD += value; break; //Wave0write
            } //Wave0write
        } //Wave0write
    }
}

