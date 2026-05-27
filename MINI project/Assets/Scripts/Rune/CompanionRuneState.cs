using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 동료 룬 상태. 게임 시작 시 정해진 노드 순서대로 레벨업할 때마다 자동 해금.
    /// </summary>
    public sealed class CompanionRuneState
    {
        /// <summary>직업.</summary>
        public RuneClass ClassId;

        /// <summary>고정 트리.</summary>
        public RuneTree Tree;

        /// <summary>해금 순서 시퀀스(시작 룬 포함).</summary>
        public List<int> FixedSequence;

        /// <summary>현재까지 해금된 개수(시퀀스 인덱스).</summary>
        public int UnlockedCount;

        /// <summary>
        /// 시작 룬 1개 해금.
        /// </summary>
        public void UnlockStarter()
        {
            // 동작 요약: FixedSequence[0] 해금, UnlockedCount = 1.
            //TODO: if (FixedSequence.Count == 0) return;
            //TODO: if (Tree.AllNodes.TryGetValue(FixedSequence[0], out var starter))
            //TODO:     starter.Unlocked = true;
            //TODO: UnlockedCount = 1;
            if (FixedSequence == null || FixedSequence.Count == 0) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            if (Tree?.AllNodes != null && Tree.AllNodes.TryGetValue(FixedSequence[0], out RuneNode starter)) //Wave0write
            { //Wave0write
                starter.Unlocked = true; //Wave0write
            } //Wave0write

            UnlockedCount = 1; //Wave0write
        }

        /// <summary>
        /// 다음 노드 자동 해금. 레벨업 시 1회 호출.
        /// </summary>
        public bool UnlockNextNodeIfPossible()
        {
            // 동작 요약:
            // - UnlockedCount < FixedSequence.Count 검사.
            // - 다음 노드 Unlocked = true; UnlockedCount += 1.
            //TODO: if (UnlockedCount >= FixedSequence.Count) return false;
            //TODO: int nextId = FixedSequence[UnlockedCount];
            //TODO: if (Tree.AllNodes.TryGetValue(nextId, out var node))
            //TODO:     node.Unlocked = true;
            //TODO: UnlockedCount++;
            //TODO: return true;
            if (FixedSequence == null || UnlockedCount >= FixedSequence.Count) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            int nextId = FixedSequence[UnlockedCount]; //Wave0write
            if (Tree?.AllNodes != null && Tree.AllNodes.TryGetValue(nextId, out RuneNode node)) //Wave0write
            { //Wave0write
                node.Unlocked = true; //Wave0write
            } //Wave0write

            UnlockedCount++; //Wave0write
            return true; //Wave0write
        }

        /// <summary>
        /// 해금된 룬 노드의 스탯 보정 합산.
        /// UnlockSkill 타입 노드는 제외(패시브 스킬은 SyncPassivesFromRunes 경로).
        /// </summary>
        public EquipmentStatMod AggregateStatMod()
        {
            // 동작 요약:
            // - FixedSequence 앞 UnlockedCount개 순회 → Tree.AllNodes[id].Data 조회.
            // - EffectType 분기:
            //   AddMaxHP/AddMaxMP/AddATK/AddDEF/AddSPD → EffectValue 합산.
            //   UnlockSkill / DamageBoost / HealBoost → 스킵.
            // - 합산 결과 EquipmentStatMod 반환.
            //TODO: var result = new EquipmentStatMod();
            //TODO: for (int i = 0; i < UnlockedCount && i < FixedSequence.Count; i++)
            //TODO: {
            //TODO:     if (!Tree.AllNodes.TryGetValue(FixedSequence[i], out var node)) continue;
            //TODO:     switch (node.Data.EffectType)
            //TODO:     {
            //TODO:         case RuneEffectType.AddMaxHP:  result.HP  += (int)node.Data.EffectValue; break;
            //TODO:         case RuneEffectType.AddMaxMP:  result.MP  += (int)node.Data.EffectValue; break;
            //TODO:         case RuneEffectType.AddATK:    result.ATK += (int)node.Data.EffectValue; break;
            //TODO:         case RuneEffectType.AddDEF:    result.DEF += (int)node.Data.EffectValue; break;
            //TODO:         case RuneEffectType.AddSPD:    result.SPD += (int)node.Data.EffectValue; break;
            //TODO:     }
            //TODO: }
            //TODO: return result;
            var result = new EquipmentStatMod(); //Wave0write
            if (FixedSequence == null || Tree?.AllNodes == null) //Wave0write
            { //Wave0write
                return result; //Wave0write
            } //Wave0write

            for (int i = 0; i < UnlockedCount && i < FixedSequence.Count; i++) //Wave0write
            { //Wave0write
                if (!Tree.AllNodes.TryGetValue(FixedSequence[i], out RuneNode node) || node.Data == null) //Wave0write
                { //Wave0write
                    continue; //Wave0write
                } //Wave0write

                int value = (int)node.Data.EffectValue; //Wave0write
                switch (node.Data.EffectType) //Wave0write
                { //Wave0write
                    case RuneEffectType.AddMaxHP: result.HP += value; break; //Wave0write
                    case RuneEffectType.AddMaxMP: result.MP += value; break; //Wave0write
                    case RuneEffectType.AddATK: result.ATK += value; break; //Wave0write
                    case RuneEffectType.AddDEF: result.DEF += value; break; //Wave0write
                    case RuneEffectType.AddSPD: result.SPD += value; break; //Wave0write
                } //Wave0write
            } //Wave0write

            return result; //Wave0write
        }
    }
}

