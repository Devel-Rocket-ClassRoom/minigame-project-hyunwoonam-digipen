namespace Tempt
{
    /// <summary>
    /// 6대 핵심 스탯 + 현재 자원. EntityBaset의 런타임 수치를 표현.
    /// 8스탯 시스템은 제거됨. EXP는 CharacterBaset 전용으로 분리.
    /// </summary>
    public sealed class StatBlockt
    {
        /// <summary>기본 최대 HP(레벨/몬스터 데이터 원본).</summary>
        public int BaseMaxHP;

        /// <summary>기본 최대 MP(레벨/몬스터 데이터 원본).</summary>
        public int BaseMaxMP;

        /// <summary>기본 공격력(레벨/몬스터 데이터 원본).</summary>
        public int BaseATK;

        /// <summary>기본 방어력(레벨/몬스터 데이터 원본).</summary>
        public int BaseDEF;

        /// <summary>기본 공격속도(레벨/몬스터 데이터 원본).</summary>
        public int BaseSPD;

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

        /// <summary>공격속도. 라운드 큐 순서/추가 공격 확률에 영향.</summary>
        public int SPD;

        /// <summary>장비로 얻는 보정값.</summary>
        public EquipmentStatModt EquipmentBonus = new EquipmentStatModt();

        /// <summary>룬 노드로 얻는 보정값.</summary>
        public EquipmentStatModt RuneBonus = new EquipmentStatModt();

        /// <summary>패시브 스킬로 얻는 보정값. 전투 준비 시 재계산한다.</summary>
        public EquipmentStatModt PassiveBonus = new EquipmentStatModt();

        /// <summary>
        /// 현재 최종 스탯을 기본 스탯으로 캡처한다.
        /// 레거시 초기화 코드가 MaxHP/ATK 같은 최종 필드만 채웠을 때 사용한다.
        /// </summary>
        public void CaptureBaseFromCurrent()
        {
            // 동작 요약:
            // - 현재 최종 필드를 기본 스탯 원본으로 복사.
            // - 이후 장비/룬/패시브 보정은 이 기본값 위에 누적된다.
            BaseMaxHP = MaxHP;
            BaseMaxMP = MaxMP;
            BaseATK = ATK;
            BaseDEF = DEF;
            BaseSPD = SPD;
        }

        /// <summary>
        /// 기본 스탯을 설정하고 최종 스탯을 재계산한다.
        /// </summary>
        public void SetBaseStats(int maxHP, int maxMP, int atk, int def, int spd)
        {
            // 동작 요약: 기본 스탯을 저장한 뒤 RecalculateFinalStats().
            BaseMaxHP = maxHP;
            BaseMaxMP = maxMP;
            BaseATK = atk;
            BaseDEF = def;
            BaseSPD = spd;
            RecalculateFinalStats();
        }

        /// <summary>
        /// 장비 보정을 초기화한다.
        /// </summary>
        public void ResetEquipmentBonuses()
        {
            // 동작 요약: EquipmentBonus를 0으로 초기화 후 최종 스탯 재계산.
            EquipmentBonus = new EquipmentStatModt();
            RecalculateFinalStats();
        }

        /// <summary>
        /// 장비 보정을 추가한다.
        /// </summary>
        public void ApplyEquipmentBonus(EquipmentStatModt mod)
        {
            // 동작 요약: 장비 보정 누적 후 최종 스탯 재계산.
            AddMod(EquipmentBonus, mod);
            RecalculateFinalStats();
        }

        /// <summary>
        /// 룬 보정을 초기화한다.
        /// </summary>
        public void ResetRuneBonuses()
        {
            // 동작 요약: RuneBonus를 0으로 초기화 후 최종 스탯 재계산.
            RuneBonus = new EquipmentStatModt();
            RecalculateFinalStats();
        }

        /// <summary>
        /// 룬 보정을 추가한다.
        /// </summary>
        public void ApplyRuneBonus(StatTypet statType, int amount)
        {
            // 동작 요약: statType에 맞는 룬 보정 누적 후 최종 스탯 재계산.
            AddByStatType(RuneBonus, statType, amount);
            RecalculateFinalStats();
        }

        /// <summary>
        /// 패시브 보정을 초기화한다.
        /// </summary>
        public void ResetPassiveBonuses()
        {
            // 동작 요약: PassiveBonus를 0으로 초기화 후 최종 스탯 재계산.
            PassiveBonus = new EquipmentStatModt();
            RecalculateFinalStats();
        }

        /// <summary>
        /// 패시브 보정을 추가한다.
        /// </summary>
        public void ApplyPassiveBonus(StatTypet statType, int amount)
        {
            // 동작 요약: statType에 맞는 패시브 보정 누적 후 최종 스탯 재계산.
            AddByStatType(PassiveBonus, statType, amount);
            RecalculateFinalStats();
        }

        /// <summary>
        /// 기본 스탯 + 장비 보정 + 룬 보정 + 패시브 보정을 합산해 최종 스탯을 갱신한다.
        /// </summary>
        public void RecalculateFinalStats()
        {
            // 동작 요약:
            // - 기본 스탯이 아직 없고 최종 스탯만 있으면 현재 값을 기본 스탯으로 캡처.
            // - 보정 3종을 합산해 MaxHP/MaxMP/ATK/DEF/SPD를 갱신.
            // - 현재 HP/MP는 새 최대치를 넘지 않도록 보정.
            if (BaseMaxHP == 0 && BaseMaxMP == 0 && BaseATK == 0 && BaseDEF == 0 && BaseSPD == 0)
            {
                CaptureBaseFromCurrent();
            }

            MaxHP = System.Math.Max(1, BaseMaxHP + EquipmentBonus.HP + RuneBonus.HP + PassiveBonus.HP);
            MaxMP = System.Math.Max(0, BaseMaxMP + EquipmentBonus.MP + RuneBonus.MP + PassiveBonus.MP);
            ATK = System.Math.Max(0, BaseATK + EquipmentBonus.ATK + RuneBonus.ATK + PassiveBonus.ATK);
            DEF = System.Math.Max(0, BaseDEF + EquipmentBonus.DEF + RuneBonus.DEF + PassiveBonus.DEF);
            SPD = System.Math.Max(0, BaseSPD + EquipmentBonus.SPD + RuneBonus.SPD + PassiveBonus.SPD);
            ClampToMax();
        }

        /// <summary>
        /// 최대치 변경 후 현재값을 범위 내로 보정.
        /// </summary>
        public void ClampToMax()
        {
            // 동작 요약: CurrentHP/CurrentMP를 [0, max] 범위로 제한.
            CurrentHP = System.Math.Min(System.Math.Max(CurrentHP, 0), MaxHP);
            CurrentMP = System.Math.Min(System.Math.Max(CurrentMP, 0), MaxMP);
        }

        /// <summary>
        /// HP/MP를 완전 회복.
        /// </summary>
        public void RestoreToFull()
        {
            // 동작 요약: CurrentHP = MaxHP, CurrentMP = MaxMP.
            CurrentHP = MaxHP;
            CurrentMP = MaxMP;
        }

        /// <summary>
        /// 입력 데미지를 적용하고 실제 차감 피해 반환.
        /// </summary>
        public int TakeDamage(int damage, bool isDefending)
        {
            // 동작 요약:
            // - DamageCalculatort가 이미 방어 적용 여부를 처리한 경우 isDefending=false로 호출.
            // - isDefending=true이면 DEF만큼 추가 경감.
            // - 결과 데미지를 CurrentHP에서 차감.
            int actual = isDefending ? System.Math.Max(1, damage - DEF / 2) : System.Math.Max(0, damage);
            CurrentHP = System.Math.Max(0, CurrentHP - actual);
            return actual;
        }

        /// <summary>
        /// MP가 충분하면 차감.
        /// </summary>
        public bool TrySpendMP(int cost)
        {
            // 동작 요약: CurrentMP >= cost일 때만 차감하고 true 반환.
            if (cost <= 0)
            {
                return true;
            }

            if (CurrentMP < cost)
            {
                return false;
            }

            CurrentMP -= cost;
            return true;
        }

        private static void AddMod(EquipmentStatModt target, EquipmentStatModt source)
        {
            if (target == null || source == null)
            {
                return;
            }

            target.HP += source.HP;
            target.MP += source.MP;
            target.ATK += source.ATK;
            target.DEF += source.DEF;
            target.SPD += source.SPD;
        }

        private static void AddByStatType(EquipmentStatModt target, StatTypet statType, int amount)
        {
            if (target == null)
            {
                return;
            }

            switch (statType)
            {
                case StatTypet.HP:
                    target.HP += amount;
                    break;
                case StatTypet.MP:
                    target.MP += amount;
                    break;
                case StatTypet.ATK:
                    target.ATK += amount;
                    break;
                case StatTypet.DEF:
                    target.DEF += amount;
                    break;
                case StatTypet.SPD:
                    target.SPD += amount;
                    break;
            }
        }
    }

    /// <summary>스탯 타입.</summary>
    public enum StatTypet
    {
        /// <summary>HP.</summary>
        HP,

        /// <summary>MP.</summary>
        MP,

        /// <summary>공격력.</summary>
        ATK,

        /// <summary>방어력.</summary>
        DEF,

        /// <summary>공격속도.</summary>
        SPD,
    }
}
