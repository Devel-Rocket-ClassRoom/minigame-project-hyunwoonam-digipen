using System.Collections.Generic;

namespace Tempt
{

    /// <summary>현재 위치 정보.</summary>
    [System.Serializable]
    public sealed class SaveLocation
    {
        /// <summary>마지막 씬 ID.</summary>
        public SceneId SceneId;

        /// <summary>안전지대 안 세부 위치 ID(주점/상점 등).</summary>
        public string SubLocationKey;
    }

    /// <summary>플레이어 직렬화 형태.</summary>
    [System.Serializable]
    public sealed class PlayerSnapshot
    {
        /// <summary>플레이어 이름.</summary>
        public string Name;

        /// <summary>레벨.</summary>
        public int Level;

        /// <summary>현재 EXP.</summary>
        public int Exp;

        /// <summary>HP/MP 등 현재 자원.</summary>
        public StatBlockSnapshot Stats;

        /// <summary>해금된 룬 ID 목록 + 미사용 룬 포인트.</summary>
        public PlayerRuneSnapshot Rune;

        /// <summary>인벤토리(소모/재료 + 장비 별도).</summary>
        public InventorySnapshot Inventory;

        /// <summary>장착 중인 장비 슬롯.</summary>
        public EquipmentSnapshot Equipment;

        /// <summary>소모 4칸 아이템 ID(0=비어있음).</summary>
        public List<int> ConsumableSlots;

        /// <summary>보관함(주점 구매 후 활성).</summary>
        public LockerSnapshot Locker;

        /// <summary>길드에서 구매/획득한 Active 스킬 ID 집합.</summary>
        public List<int> OwnedSkillIds;

        /// <summary>활성 스킬 슬롯 2칸. 0 = 비어있음.</summary>
        public List<int> ActiveSlotSkillIds;
    }

    /// <summary>동료 명부 직렬화.</summary>
    [System.Serializable]
    public sealed class RosterSnapshot
    {
        /// <summary>현재 합류 중인 동료(최대 3).</summary>
        public List<CompanionSnapshot> Active;

        /// <summary>주점/길드에서 해금됐지만 대기 중인 동료.</summary>
        public List<CompanionSnapshot> Bench;
    }

    /// <summary>동료 1명 직렬화.</summary>
    [System.Serializable]
    public sealed class CompanionSnapshot
    {
        /// <summary>동료 데이터 ID.</summary>
        public int CompanionId;

        /// <summary>동료별 자동 룬 선택 시드.</summary>
        public int Seed;

        /// <summary>레벨.</summary>
        public int Level;

        /// <summary>EXP.</summary>
        public int Exp;

        /// <summary>자동 룬 투자 이력. 시작 룬을 포함하며 같은 노드가 중복될 수 있다.</summary>
        public List<int> FixedRuneSequence;

        /// <summary>현재까지 적용된 자동 룬 투자 이력 수.</summary>
        public int UnlockedCount;

        /// <summary>현재 HP/MP/스탯.</summary>
        public StatBlockSnapshot Stats;

        /// <summary>장착 중인 장비.</summary>
        public EquipmentSnapshot Equipment;
    }

    /// <summary>스탯 직렬화.</summary>
    [System.Serializable]
    public sealed class StatBlockSnapshot
    {
        /// <summary>최대 HP.</summary>
        public int MaxHP;

        /// <summary>현재 HP.</summary>
        public int CurrentHP;

        /// <summary>최대 MP.</summary>
        public int MaxMP;

        /// <summary>현재 MP.</summary>
        public int CurrentMP;

        /// <summary>공격력.</summary>
        public int ATK;

        /// <summary>방어력.</summary>
        public int DEF;

        /// <summary>공격속도.</summary>
        public int SPD;
    }

    /// <summary>플레이어 룬 진행 직렬화.</summary>
    [System.Serializable]
    public sealed class PlayerRuneSnapshot
    {
        /// <summary>선택한 시작 직업 룬 ID(딜러/탱커/마법딜러/지원가 중).</summary>
        public int StartingClassRuneId;

        /// <summary>해금된 룬 노드 ID 목록.</summary>
        public List<int> UnlockedNodeIds;

        /// <summary>노드별 투자 포인트 목록.</summary>
        public List<RuneNodeInvestmentSnapshot> NodeInvestments;

        /// <summary>현재 보유 룬 포인트(레벨업으로 적립).</summary>
        public int RunePoints;
    }

    [System.Serializable]
    public sealed class RuneNodeInvestmentSnapshot
    {
        public int NodeId;

        public int InvestedPoints;
    }

    /// <summary>인벤토리 직렬화.</summary>
    [System.Serializable]
    public sealed class InventorySnapshot
    {
        /// <summary>소모/재료 아이템(itemId → 수량).</summary>
        public List<InventoryEntry> StackableItems;

        /// <summary>장비 아이템(강화 단계 포함).</summary>
        public List<EquipItemEntry> EquipItems;
    }

    /// <summary>소모/재료 아이템 1행(itemId + 수량).</summary>
    [System.Serializable]
    public sealed class InventoryEntry
    {
        /// <summary>아이템 ID.</summary>
        public int ItemId;

        /// <summary>수량.</summary>
        public int Count;
    }

    /// <summary>
    /// 장비 아이템 1개(itemId + 강화 단계).
    /// 장비는 Stackable=false이므로 Count 개념 없음.
    /// </summary>
    [System.Serializable]
    public sealed class EquipItemEntry
    {
        /// <summary>아이템 ID.</summary>
        public int ItemId;

        /// <summary>강화 단계(0=기본).</summary>
        public int Enhancement;

        /// <summary>연속 강화 실패 횟수.</summary>
        public int EnhanceFailStreak;
    }

    /// <summary>장착 슬롯 직렬화(강화 단계 포함).</summary>
    [System.Serializable]
    public sealed class EquipmentSnapshot
    {
        /// <summary>무기 아이템 ID(0=없음).</summary>
        public int WeaponId;

        /// <summary>무기 강화 단계.</summary>
        public int WeaponEnhancement;

        /// <summary>무기 연속 강화 실패 횟수.</summary>
        public int WeaponEnhanceFailStreak;

        /// <summary>방어구(몸통) ID.</summary>
        public int ArmorBodyId;

        /// <summary>방어구(몸통) 강화 단계.</summary>
        public int ArmorBodyEnhancement;

        /// <summary>방어구(몸통) 연속 강화 실패 횟수.</summary>
        public int ArmorBodyEnhanceFailStreak;

        /// <summary>방어구(팔) ID.</summary>
        public int ArmorArmsId;

        /// <summary>방어구(팔) 강화 단계.</summary>
        public int ArmorArmsEnhancement;

        /// <summary>방어구(팔) 연속 강화 실패 횟수.</summary>
        public int ArmorArmsEnhanceFailStreak;

        /// <summary>방어구(다리) ID.</summary>
        public int ArmorLegsId;

        /// <summary>방어구(다리) 강화 단계.</summary>
        public int ArmorLegsEnhancement;

        /// <summary>방어구(다리) 연속 강화 실패 횟수.</summary>
        public int ArmorLegsEnhanceFailStreak;
    }

    /// <summary>보관함 직렬화.</summary>
    [System.Serializable]
    public sealed class LockerSnapshot
    {
        /// <summary>활성화 여부(주점에서 구매 시 true).</summary>
        public bool Unlocked;

        /// <summary>현재 보관함 슬롯 수.</summary>
        public int Capacity;

        /// <summary>보관 중인 소모/재료 아이템.</summary>
        public List<InventoryEntry> StackableItems;

        /// <summary>보관 중인 장비 아이템(강화 단계 포함).</summary>
        public List<EquipItemEntry> EquipItems;
    }

    /// <summary>상점 재고 1행 직렬화.</summary>
    [System.Serializable]
    public sealed class ShopStockEntrySnapshot
    {
        public int ItemId;
        public bool Available;
        public int RemainingCount;
        public int InitialCount;
        public int UnitPrice;
        public string UnlockKey;
    }

    /// <summary>
    /// 플로어 맵 직렬화.
    /// seed를 저장해 재생성하지 않고, 저장 시점의 전체 노드 구조를 JSON에 직접 기록한다.
    /// </summary>
    [System.Serializable]
    public sealed class FloorMapSnapshot
    {
        /// <summary>다음 선택 가능 층. FloorMapModel.NextSelectableFloor와 동일.</summary>
        public int NextSelectableFloor;

        /// <summary>
        /// 저장 시점의 전체 노드 목록.
        /// 각 노드는 층, 단계, 난이도, 보스 여부, 클리어 여부, 다음 노드 연결을 포함한다.
        /// </summary>
        public List<FloorNodeSnapshot> Nodes;
    }

    /// <summary>플로어 맵 노드 1개의 직렬화 형태.</summary>
    [System.Serializable]
    public sealed class FloorNodeSnapshot
    {
        /// <summary>전역 고유 노드 ID.</summary>
        public int NodeId;

        /// <summary>층 번호(1~49).</summary>
        public int Floor;

        /// <summary>해당 단계(1~6).</summary>
        public int StageIndex;

        /// <summary>난이도(생성 시 고정).</summary>
        public int Difficulty;

        /// <summary>몬스터 수(1~3, 생성 시 고정).</summary>
        public int MonsterCount;

        /// <summary>보스 노드 여부.</summary>
        public bool IsBoss;

        /// <summary>안전지대 표시 노드 여부.</summary>
        public bool IsSafeZone;

        /// <summary>클리어 여부.</summary>
        public bool IsCleared;

        /// <summary>다음 층의 연결 노드 ID 목록.</summary>
        public List<int> NextNodeIds;
    }

    /// <summary>침식 직렬화.</summary>
    [System.Serializable]
    public sealed class ErosionSnapshot
    {
        /// <summary>
        /// 각 단계의 침식률(단계 1~6). 인덱스 0 = 단계 1.
        /// 0.0 = 비침식, 100.0 = 완전침식.
        /// </summary>
        public List<float> StageRates;

        /// <summary>
        /// 침식 활성화 여부. Safe2 도달 전까지는 false.
        /// </summary>
        public bool ErosionStarted;

        /// <summary>
        /// 현재 침식이 누적되는 단계(1~6).
        /// </summary>
        public int CurrentEroddingStage;

        /// <summary>
        /// 각 단계의 안전지대 침식 잠금 여부(침식률 100%에 도달 시 true).
        /// 인덱스 0 = 단계 1의 안전지대(Safe1).
        /// </summary>
        public List<bool> StageSafeLocked;
    }

    /// <summary>튜토리얼 진행 직렬화.</summary>
    [System.Serializable]
    public sealed class TutorialSnapshot
    {
        /// <summary>완료된 단계 ID 목록.</summary>
        public List<string> CompletedSteps;
    }

    /// <summary>옵션 직렬화.</summary>
    [System.Serializable]
    public sealed class OptionSnapshot
    {
        /// <summary>언어 코드(ko/en 등).</summary>
        public string LanguageCode;

        /// <summary>마스터 볼륨(0~1).</summary>
        public float MasterVolume;

        /// <summary>풀스크린 여부.</summary>
        public bool Fullscreen;

        /// <summary>해상도 가로.</summary>
        public int ResolutionWidth;

        /// <summary>해상도 세로.</summary>
        public int ResolutionHeight;
    }
}

