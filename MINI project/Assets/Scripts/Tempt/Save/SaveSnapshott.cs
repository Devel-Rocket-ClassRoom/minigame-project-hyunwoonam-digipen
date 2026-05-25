using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 한 런의 전체 상태를 직렬화하기 위한 스냅샷.
    /// "종료 시점 전체 복원" 정책에 따라 모든 동적 상태를 포함.
    /// </summary>
    [System.Serializable]
    public sealed class SaveSnapshott
    {
        /// <summary>저장 일자(ISO).</summary>
        public string SavedAtIso;

        /// <summary>클리어 완료된 런 여부(true면 Continue 불가).</summary>
        public bool IsCompleted;

        /// <summary>현재 일자.</summary>
        public int CurrentDay;

        /// <summary>현재 위치한 층/씬.</summary>
        public SaveLocationt Location;

        /// <summary>플로어 맵 직렬화.</summary>
        public FloorMapSnapshott FloorMap;

        /// <summary>플레이어 상태.</summary>
        public PlayerSnapshott Player;

        /// <summary>동료 명부.</summary>
        public RosterSnapshott Roster;

        /// <summary>침식 상태(단계별 비율 + 잠금 상태).</summary>
        public ErosionSnapshott Erosion;

        /// <summary>안전지대 해금 상태.</summary>
        public List<bool> SafeUnlocks;

        /// <summary>골드.</summary>
        public int Gold;

        /// <summary>마석.</summary>
        public int ManaStone;

        /// <summary>튜토리얼 진행.</summary>
        public TutorialSnapshott Tutorial;

        /// <summary>옵션/언어/볼륨/화면모드.</summary>
        public OptionSnapshott Options;
    }

    /// <summary>현재 위치 정보.</summary>
    [System.Serializable]
    public sealed class SaveLocationt
    {
        /// <summary>마지막 씬 ID.</summary>
        public SceneIdt SceneId;

        /// <summary>안전지대 안 세부 위치 ID(주점/상점 등).</summary>
        public string SubLocationKey;
    }

    /// <summary>플레이어 직렬화 형태.</summary>
    [System.Serializable]
    public sealed class PlayerSnapshott
    {
        /// <summary>플레이어 이름.</summary>
        public string Name;

        /// <summary>레벨.</summary>
        public int Level;

        /// <summary>현재 EXP.</summary>
        public int Exp;

        /// <summary>HP/MP 등 현재 자원.</summary>
        public StatBlockSnapshott Stats;

        /// <summary>해금된 룬 ID 목록 + 미사용 룬 포인트.</summary>
        public PlayerRuneSnapshott Rune;

        /// <summary>인벤토리.</summary>
        public InventorySnapshott Inventory;

        /// <summary>장비.</summary>
        public EquipmentSnapshott Equipment;

        /// <summary>소모 4칸.</summary>
        public List<int> ConsumableSlots;

        /// <summary>보관함(주점 구매 후 활성).</summary>
        public LockerSnapshott Locker;
    }

    /// <summary>동료 명부 직렬화.</summary>
    [System.Serializable]
    public sealed class RosterSnapshott
    {
        /// <summary>현재 합류 중인 동료(최대 3).</summary>
        public List<CompanionSnapshott> Active;

        /// <summary>주점/길드에서 해금됐지만 대기 중인 동료.</summary>
        public List<CompanionSnapshott> Bench;
    }

    /// <summary>동료 1명 직렬화.</summary>
    [System.Serializable]
    public sealed class CompanionSnapshott
    {
        /// <summary>동료 데이터 ID.</summary>
        public int CompanionId;

        /// <summary>레벨/EXP.</summary>
        public int Level;

        /// <summary>EXP.</summary>
        public int Exp;

        /// <summary>고정 룬 트리(랜덤 생성 시드 또는 노드 ID 시퀀스).</summary>
        public List<int> FixedRuneSequence;

        /// <summary>현재까지 해금된 룬 노드 수.</summary>
        public int UnlockedCount;

        /// <summary>현재 HP/MP/스탯.</summary>
        public StatBlockSnapshott Stats;

        /// <summary>장비 슬롯.</summary>
        public EquipmentSnapshott Equipment;
    }

    /// <summary>스탯 직렬화.</summary>
    [System.Serializable]
    public sealed class StatBlockSnapshott
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
    public sealed class PlayerRuneSnapshott
    {
        /// <summary>선택한 시작 직업 룬(딜러/탱커/마법딜러/지원가 중).</summary>
        public int StartingClassRuneId;

        /// <summary>해금된 룬 노드 ID 목록.</summary>
        public List<int> UnlockedNodeIds;

        /// <summary>현재 보유 룬 포인트(레벨업으로 적립).</summary>
        public int RunePoints;
    }

    /// <summary>인벤토리 직렬화.</summary>
    [System.Serializable]
    public sealed class InventorySnapshott
    {
        /// <summary>아이템 ID → 보유 수량.</summary>
        public List<InventoryEntryt> Items;
    }

    /// <summary>인벤토리 1행.</summary>
    [System.Serializable]
    public sealed class InventoryEntryt
    {
        /// <summary>아이템 ID.</summary>
        public int ItemId;

        /// <summary>수량.</summary>
        public int Count;
    }

    /// <summary>장비 직렬화.</summary>
    [System.Serializable]
    public sealed class EquipmentSnapshott
    {
        /// <summary>무기 아이템 ID(0=없음).</summary>
        public int WeaponId;

        /// <summary>방어구(몸통) ID.</summary>
        public int ArmorBodyId;

        /// <summary>방어구(팔) ID.</summary>
        public int ArmorArmsId;

        /// <summary>방어구(다리) ID.</summary>
        public int ArmorLegsId;
    }

    /// <summary>보관함 직렬화.</summary>
    [System.Serializable]
    public sealed class LockerSnapshott
    {
        /// <summary>활성화 여부(주점에서 구매 시 true).</summary>
        public bool Unlocked;

        /// <summary>보관 중인 아이템.</summary>
        public List<InventoryEntryt> Items;
    }

    /// <summary>플로어 맵 직렬화.</summary>
    [System.Serializable]
    public sealed class FloorMapSnapshott
    {
        /// <summary>맵 생성에 사용된 시드.</summary>
        public int Seed;

        /// <summary>층별 노드 목록.</summary>
        public List<FloorRowSnapshott> Floors;

        /// <summary>클리어된 노드 ID 집합.</summary>
        public List<int> ClearedNodeIds;
    }

    /// <summary>한 층의 노드 행.</summary>
    [System.Serializable]
    public sealed class FloorRowSnapshott
    {
        /// <summary>층 번호.</summary>
        public int Floor;

        /// <summary>노드 ID 목록(1~3개).</summary>
        public List<int> NodeIds;
    }

    /// <summary>침식 직렬화.</summary>
    [System.Serializable]
    public sealed class ErosionSnapshott
    {
        /// <summary>각 단계 침식률(stage 1~6).</summary>
        public List<float> StageRates;

        /// <summary>침식이 시작된 가장 낮은 단계(설계상 안전지대 2 도달 시 시작).</summary>
        public int ErosionUnlockedAtStage;
    }

    /// <summary>튜토리얼 진행 직렬화.</summary>
    [System.Serializable]
    public sealed class TutorialSnapshott
    {
        /// <summary>완료된 단계 ID 목록.</summary>
        public List<string> CompletedSteps;
    }

    /// <summary>옵션 직렬화.</summary>
    [System.Serializable]
    public sealed class OptionSnapshott
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
