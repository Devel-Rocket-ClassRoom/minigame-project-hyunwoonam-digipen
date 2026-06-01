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

        // Guid3 §9.A 2026-05-27: 길드 시스템을 위한 보유 스킬 풀 / 슬롯 매핑.
        // - OwnedSkillIds: 플레이어가 보유한 Active 스킬 ID 집합. Passive 스킬은 룬이 권위라 포함하지 않는다.
        // - ActiveSlotSkillIds: 두 슬롯의 SkillId. 0 은 빈 슬롯.
        // 직렬화는 SaveSnapshot 의 PlayerSnapshot 에서 List<int> 로 변환한다(HashSet 은 JsonUtility 미지원).
        /// <summary>보유한 Active 스킬 풀(길드 구매 누적).</summary>
        public System.Collections.Generic.HashSet<int> OwnedSkillIds = new System.Collections.Generic.HashSet<int>();

        /// <summary>활성 슬롯 2칸의 SkillId 매핑. 0 = 빈 슬롯.</summary>
        public int[] ActiveSlotSkillIds = new int[2];
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
            if (inst != null)
            {
                Bench.Add(inst);
            }
        }

        /// <summary>대기 → 파티 합류.</summary>
        public bool Promote(int companionId)
        {
            // 동작 요약:
            // - Active.Count < 3 검사.
            // - Bench에서 제거 → Active로 이동.
            if (Active.Count >= 3)
            {
                return false;
            }

            CompanionInstance inst = Bench.Find(c => c.CompanionDataId == companionId);
            if (inst == null)
            {
                return false;
            }

            Bench.Remove(inst);
            Active.Add(inst);

            return true;
        }

        /// <summary>파티 → 대기 강등.</summary>
        public bool Demote(int companionId)
        {
            // 동작 요약: Active → Bench 이동.
            CompanionInstance inst = Active.Find(c => c.CompanionDataId == companionId);
            if (inst == null)
            {
                return false;
            }

            Active.Remove(inst);
            Bench.Add(inst);

            return true;
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
        /// <summary>안전지대 인덱스별 해금 여부.</summary>
        public bool[] Unlocked = new bool[6];

        public SafeZoneUnlockState()
        {
        }

        public SafeZoneUnlockState(int safeZoneCount)
        {
            EnsureCapacity(safeZoneCount);
        }

        public void EnsureCapacity(int safeZoneCount)
        {
            int targetLength = System.Math.Max(1, safeZoneCount);

            if (Unlocked != null && Unlocked.Length == targetLength)
            {
                return;
            }

            bool[] next = new bool[targetLength];

            if (Unlocked != null)
            {
                int copyLength = System.Math.Min(Unlocked.Length, next.Length);

                for (int i = 0; i < copyLength; i++)
                {
                    next[i] = Unlocked[i];
                }
            }

            Unlocked = next;
        }

        /// <summary>해금 처리.</summary>
        public void Unlock(int index)
        {
            // 동작 요약: 인덱스 범위 검사 후 Unlocked[index] = true.
            if (index >= 0 && index < Unlocked.Length)
            {
                Unlocked[index] = true;
            }
        }

        /// <summary>잠금 처리(침식).</summary>
        public void Lock(int index)
        {
            // 동작 요약: Unlocked[index] = false.
            if (index >= 0 && index < Unlocked.Length)
            {
                Unlocked[index] = false;
            }
        }

        /// <summary>해금 여부 조회.</summary>
        public bool IsUnlocked(int index)
        {
            // 동작 요약: 범위 검사 후 Unlocked[index] 반환.
            return index >= 0 && index < Unlocked.Length && Unlocked[index];
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
            if (!string.IsNullOrEmpty(sequenceKey) && !CompletedSteps.Contains(sequenceKey))
            {
                CompletedSteps.Add(sequenceKey);
            }
        }

        /// <summary>완료 여부 조회.</summary>
        public bool IsCompleted(string sequenceKey)
        {
            // 동작 요약: Contains 반환.
            return !string.IsNullOrEmpty(sequenceKey) && CompletedSteps.Contains(sequenceKey);
        }
    }

    /// <summary>
    /// 안전지대 상점의 런별 재고 상태. 구매 이력은 이 모델에 남고 SaveSnapshot에 저장된다.
    /// </summary>
    [System.Serializable]
    public sealed class ShopStockState
    {
        public const int UnlimitedCount = -1;
        private const string Safe1DefaultUnlockKey = "Safe1Default";

        /// <summary>상점에 등록된 모든 재고. Available=false 이면 현재 목록에 표시하지 않는다.</summary>
        public List<ShopStockEntry> Entries = new List<ShopStockEntry>();

        public static ShopStockState CreateDefaultSafe1Stock()
        {
            var state = new ShopStockState();
            state.EnsureDefaultSafe1Stock();

            return state;
        }

        /// <summary>현재 테스트용 Safe1 기본 재고를 누락분만 보강한다. 기존 구매/비활성 상태는 덮어쓰지 않는다.</summary>
        public void EnsureDefaultSafe1Stock()
        {
            if (Entries == null)
            {
                Entries = new List<ShopStockEntry>();
            }

            EnsureEntry(1, UnlimitedCount, 1, Safe1DefaultUnlockKey, true);
            EnsureEntry(2, UnlimitedCount, 1, Safe1DefaultUnlockKey, true);
            EnsureEntry(901, 1, 1, Safe1DefaultUnlockKey, true);
            EnsureEntry(902, 1, 1, Safe1DefaultUnlockKey, true);
            EnsureEntry(903, 1, 1, Safe1DefaultUnlockKey, true);
            EnsureEntry(904, 1, 1, Safe1DefaultUnlockKey, true);
        }

        public ShopStockEntry Find(int itemId)
        {
            if (Entries == null)
            {
                return null;
            }

            for (int i = 0; i < Entries.Count; i++)
            {
                ShopStockEntry entry = Entries[i];

                if (entry != null && entry.ItemId == itemId)
                {
                    return entry;
                }
            }

            return null;
        }

        public void ActivateByUnlockKey(string unlockKey)
        {
            if (string.IsNullOrEmpty(unlockKey) || Entries == null)
            {
                return;
            }

            for (int i = 0; i < Entries.Count; i++)
            {
                ShopStockEntry entry = Entries[i];

                if (entry == null || entry.UnlockKey != unlockKey)
                {
                    continue;
                }

                entry.Available = true;

                if (!entry.IsUnlimited && entry.RemainingCount <= 0)
                {
                    entry.RemainingCount = System.Math.Max(1, entry.InitialCount);
                }
            }
        }

        private void EnsureEntry(int itemId, int remainingCount, int unitPrice, string unlockKey, bool available)
        {
            if (Find(itemId) != null)
            {
                return;
            }

            Entries.Add(new ShopStockEntry
            {
                ItemId = itemId,
                Available = available,
                RemainingCount = remainingCount,
                InitialCount = remainingCount,
                UnitPrice = unitPrice,
                UnlockKey = unlockKey,
            });
        }
    }

    /// <summary>상점 재고 1행. RemainingCount=-1 이면 무제한 구매 가능.</summary>
    [System.Serializable]
    public sealed class ShopStockEntry
    {
        public int ItemId;
        public bool Available = true;
        public int RemainingCount = 1;
        public int InitialCount = 1;
        public int UnitPrice = 1;
        public string UnlockKey;

        public bool IsUnlimited => RemainingCount == ShopStockState.UnlimitedCount;
        public bool CanPurchase => Available && (IsUnlimited || RemainingCount > 0);

        public void ConsumeOne()
        {
            if (IsUnlimited)
            {
                return;
            }

            RemainingCount = System.Math.Max(0, RemainingCount - 1);

            if (RemainingCount <= 0)
            {
                Available = false;
            }
        }
    }
}

