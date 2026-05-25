using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 런 상태에서 참조되는 보조 모델 묶음. GameRunStatet에서 인용.
    /// </summary>

    /// <summary>
    /// 플레이어 런타임 상태 데이터(직렬화 가능). Playert MonoBehaviour는 이 객체를 참조한다.
    /// 전투 진입 시 MonoBehaviour로 매핑.
    /// </summary>
    [System.Serializable]
    public sealed class PlayerStatet
    {
        /// <summary>표시 이름.</summary>
        public string Name;

        /// <summary>레벨.</summary>
        public int Level = 1;

        /// <summary>현재 EXP.</summary>
        public int Exp;

        /// <summary>스탯 블록.</summary>
        public StatBlockt Stats;

        /// <summary>시작 직업.</summary>
        public RuneClasst StartingClass;

        /// <summary>룬 진행.</summary>
        public PlayerRuneStatet Rune;

        /// <summary>인벤토리.</summary>
        public InventoryStatet Inventory;

        /// <summary>장비.</summary>
        public EquipmentSlotst Equipment;

        /// <summary>전투 소모 4칸.</summary>
        public ConsumableSlotst Consumables;

        /// <summary>보관함.</summary>
        public LockerStatet Locker;
    }

    /// <summary>
    /// 동료 명부 상태.
    /// </summary>
    [System.Serializable]
    public sealed class CompanionRosterStatet
    {
        /// <summary>현재 합류 중인 동료(최대 3명).</summary>
        public List<CompanionInstancet> Active = new List<CompanionInstancet>();

        /// <summary>주점에서 모집된 후 길드에서 미편성된 대기 동료.</summary>
        public List<CompanionInstancet> Bench = new List<CompanionInstancet>();

        /// <summary>한 명 등록.</summary>
        public void Recruit(CompanionInstancet inst)
        {
            // 동작 요약: Bench에 추가.
        }

        /// <summary>대기 → 파티 합류.</summary>
        public bool Promote(int companionId)
        {
            // 동작 요약:
            // - Active.Count < 3 검사.
            // - Bench에서 제거 → Active로 이동.
            return false;
        }

        /// <summary>파티 → 대기 강등.</summary>
        public bool Demote(int companionId)
        {
            // 동작 요약: Active → Bench 이동.
            return false;
        }
    }

    /// <summary>
    /// 동료 1명의 직렬화 가능 인스턴스. TeamBaset MonoBehaviour의 데이터 본체.
    /// </summary>
    [System.Serializable]
    public sealed class CompanionInstancet
    {
        /// <summary>CompanionDatat ID.</summary>
        public int CompanionDataId;

        /// <summary>고정 시드(룬 트리 결정).</summary>
        public int Seed;

        /// <summary>레벨.</summary>
        public int Level = 1;

        /// <summary>EXP.</summary>
        public int Exp;

        /// <summary>스탯.</summary>
        public StatBlockt Stats;

        /// <summary>룬 진행.</summary>
        public CompanionRuneStatet Rune;

        /// <summary>장비.</summary>
        public EquipmentSlotst Equipment;
    }

    /// <summary>
    /// 안전지대 해금 상태. 보스 클리어로 해금, 침식 100%로 잠김.
    /// </summary>
    [System.Serializable]
    public sealed class SafeZoneUnlockStatet
    {
        /// <summary>인덱스 0~5의 해금 여부.</summary>
        public bool[] Unlocked = new bool[6];

        /// <summary>해금 처리.</summary>
        public void Unlock(int index)
        {
            // 동작 요약: 인덱스 범위 검사 후 Unlocked[index] = true.
        }

        /// <summary>잠금 처리(침식).</summary>
        public void Lock(int index)
        {
            // 동작 요약: Unlocked[index] = false.
        }

        /// <summary>해금 여부 조회.</summary>
        public bool IsUnlocked(int index)
        {
            // 동작 요약: 범위 검사 후 Unlocked[index] 반환.
            return false;
        }
    }

    /// <summary>
    /// 튜토리얼 진행 상태.
    /// </summary>
    [System.Serializable]
    public sealed class TutorialProgressStatet
    {
        /// <summary>완료된 시퀀스 키 목록.</summary>
        public List<string> CompletedSteps = new List<string>();

        /// <summary>완료 표시.</summary>
        public void MarkCompleted(string sequenceKey)
        {
            // 동작 요약: 중복 없이 추가.
        }

        /// <summary>완료 여부 조회.</summary>
        public bool IsCompleted(string sequenceKey)
        {
            // 동작 요약: Contains 반환.
            return false;
        }
    }
}
