using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 런 상태에서 참조되는 보조 모델 묶음. GameRunStatet에서 인용.
    /// </summary>

    /// <summary>
    /// 플레이어 런타임 상태 데이터(직렬화 가능). Player MonoBehaviour는 이 객체를 참조한다.
    /// 전투 진입 시 MonoBehaviour로 매핑.
    /// </summary>
    [System.Serializable]
    public sealed class PlayerState
    {
        /// <summary>표시 이름.</summary>
        public string Name;

        /// <summary>레벨.</summary>
        public int Level = 1;

        /// <summary>현재 EXP.</summary>
        public int Exp;

        /// <summary>스탯 블록.</summary>
        public StatBlock Stats;

        /// <summary>시작 직업.</summary>
        public RuneClass StartingClass;

        /// <summary>룬 진행.</summary>
        public PlayerRuneState Rune;

        /// <summary>인벤토리.</summary>
        public InventoryState Inventory;

        /// <summary>장비.</summary>
        public EquipmentSlots Equipment;

        /// <summary>전투 소모 4칸.</summary>
        public ConsumableSlots Consumables;

        /// <summary>보관함.</summary>
        public LockerState Locker;
    }

    /// <summary>
    /// 동료 명부 상태.
    /// </summary>
    [System.Serializable]
    public sealed class CompanionRosterState
    {
        /// <summary>현재 합류 중인 동료(최대 3명).</summary>
        public List<CompanionInstance> Active = new List<CompanionInstance>();

        /// <summary>주점에서 모집된 후 길드에서 미편성된 대기 동료.</summary>
        public List<CompanionInstance> Bench = new List<CompanionInstance>();

        /// <summary>한 명 등록.</summary>
        public void Recruit(CompanionInstance inst)
        {
            // 동작 요약: Bench에 추가.
            //TODO: Bench.Add(inst);
            if (inst != null) //Wave0write
            { //Wave0write
                Bench.Add(inst); //Wave0write
            } //Wave0write
        }

        /// <summary>대기 → 파티 합류.</summary>
        public bool Promote(int companionId)
        {
            // 동작 요약:
            // - Active.Count < 3 검사.
            // - Bench에서 제거 → Active로 이동.
            //TODO: if (Active.Count >= 3) return false;
            //TODO: var inst = Bench.Find(c => c.CompanionDataId == companionId);
            //TODO: if (inst == null) return false;
            //TODO: Bench.Remove(inst);
            //TODO: Active.Add(inst);
            //TODO: return true;
            if (Active.Count >= 3) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            CompanionInstance inst = Bench.Find(c => c.CompanionDataId == companionId); //Wave0write
            if (inst == null) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            Bench.Remove(inst); //Wave0write
            Active.Add(inst); //Wave0write
            return true; //Wave0write
        }

        /// <summary>파티 → 대기 강등.</summary>
        public bool Demote(int companionId)
        {
            // 동작 요약: Active → Bench 이동.
            //TODO: var inst = Active.Find(c => c.CompanionDataId == companionId);
            //TODO: if (inst == null) return false;
            //TODO: Active.Remove(inst);
            //TODO: Bench.Add(inst);
            //TODO: return true;
            CompanionInstance inst = Active.Find(c => c.CompanionDataId == companionId); //Wave0write
            if (inst == null) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            Active.Remove(inst); //Wave0write
            Bench.Add(inst); //Wave0write
            return true; //Wave0write
        }
    }

    /// <summary>
    /// 동료 1명의 직렬화 가능 인스턴스. TeamBase MonoBehaviour의 데이터 본체.
    /// </summary>
    [System.Serializable]
    public sealed class CompanionInstance
    {
        /// <summary>CompanionData ID.</summary>
        public int CompanionDataId;

        /// <summary>고정 시드(룬 트리 결정).</summary>
        public int Seed;

        /// <summary>레벨.</summary>
        public int Level = 1;

        /// <summary>EXP.</summary>
        public int Exp;

        /// <summary>스탯.</summary>
        public StatBlock Stats;

        /// <summary>룬 진행.</summary>
        public CompanionRuneState Rune;

        /// <summary>장비.</summary>
        public EquipmentSlots Equipment;
    }

    /// <summary>
    /// 안전지대 해금 상태. 보스 클리어로 해금, 침식 100%로 잠김.
    /// </summary>
    [System.Serializable]
    public sealed class SafeZoneUnlockState
    {
        /// <summary>인덱스 0~5의 해금 여부.</summary>
        public bool[] Unlocked = new bool[6];

        /// <summary>해금 처리.</summary>
        public void Unlock(int index)
        {
            // 동작 요약: 인덱스 범위 검사 후 Unlocked[index] = true.
            //TODO: if (index >= 0 && index < Unlocked.Length) Unlocked[index] = true;
            if (index >= 0 && index < Unlocked.Length) //Wave0write
            { //Wave0write
                Unlocked[index] = true; //Wave0write
            } //Wave0write
        }

        /// <summary>잠금 처리(침식).</summary>
        public void Lock(int index)
        {
            // 동작 요약: Unlocked[index] = false.
            //TODO: if (index >= 0 && index < Unlocked.Length) Unlocked[index] = false;
            if (index >= 0 && index < Unlocked.Length) //Wave0write
            { //Wave0write
                Unlocked[index] = false; //Wave0write
            } //Wave0write
        }

        /// <summary>해금 여부 조회.</summary>
        public bool IsUnlocked(int index)
        {
            // 동작 요약: 범위 검사 후 Unlocked[index] 반환.
            //TODO: return index >= 0 && index < Unlocked.Length && Unlocked[index];
            return index >= 0 && index < Unlocked.Length && Unlocked[index]; //Wave0write
        }
    }

    /// <summary>
    /// 튜토리얼 진행 상태.
    /// </summary>
    [System.Serializable]
    public sealed class TutorialProgressState
    {
        /// <summary>완료된 시퀀스 키 목록.</summary>
        public List<string> CompletedSteps = new List<string>();

        /// <summary>완료 표시.</summary>
        public void MarkCompleted(string sequenceKey)
        {
            // 동작 요약: 중복 없이 추가.
            //TODO: if (!CompletedSteps.Contains(sequenceKey)) CompletedSteps.Add(sequenceKey);
            if (!string.IsNullOrEmpty(sequenceKey) && !CompletedSteps.Contains(sequenceKey)) //Wave0write
            { //Wave0write
                CompletedSteps.Add(sequenceKey); //Wave0write
            } //Wave0write
        }

        /// <summary>완료 여부 조회.</summary>
        public bool IsCompleted(string sequenceKey)
        {
            // 동작 요약: Contains 반환.
            //TODO: return CompletedSteps.Contains(sequenceKey);
            return !string.IsNullOrEmpty(sequenceKey) && CompletedSteps.Contains(sequenceKey); //Wave0write
        }
    }
}

