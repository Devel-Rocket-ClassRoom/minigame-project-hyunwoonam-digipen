namespace Tempt
{
    /// <summary>
    /// 스킬 정적 데이터. SkillTable.csv 1행.
    /// </summary>
    public sealed class SkillData : DataTable
    {
        /// <summary>스킬 종류(액티브 또는 패시브).</summary>
        public SkillType SkillType;

        /// <summary>스킬 획득 방식.</summary>
        public AcquireType AcquireType;

        /// <summary>상점 구매 시 필요 골드(AcquireType == Shop일 때만 의미).</summary>
        public int PurchasePrice;

        /// <summary>MP 소비량(Passive는 0).</summary>
        public int MpCost;

        /// <summary>기본 데미지 배수(0이면 공격 무관).</summary>
        public float DamageScale;

        /// <summary>회복량 배수(0이면 회복 없음).</summary>
        public float HealScale;

        /// <summary>보호막 배수(0이면 보호막 없음, DEF * scale 적용).</summary>
        public float ShieldScale;

        /// <summary>타겟 타입(Passive는 Self로 처리).</summary>
        public SkillTargetType TargetType;

        /// <summary>애니메이션 키.</summary>
        public string AnimationKey;

        /// <summary>이펙트 키.</summary>
        public string EffectKey;

        /// <summary>효과음 키(Resources/Sfx/{key}). 빈값이면 무음.</summary>
        public string SfxKey;

        /// <summary>실행 시간(초). 0 입력 시 ActionTimingt가 기본 0.1초 + 애니/이펙트 합산으로 보정.</summary>
        public float ActionDuration;

        /// <summary>쿨다운(라운드). Passive는 0.</summary>
        public int CooldownRounds;

        /// <summary>보호막 지속 라운드. ShieldScale 이 0이면 무시.</summary>
        public int ShieldDurationRounds;

        /// <summary>명시형 패시브 스탯 타입. None이면 기존 Scale 기반 패시브 해석을 사용한다.</summary>
        public PassiveStatType PassiveStatType;

        /// <summary>명시형 패시브 고정 보너스.</summary>
        public int PassiveFlatValue;

        /// <summary>명시형 패시브 비율 보너스. 0.1 = 기준 스탯 10%.</summary>
        public float PassivePercentValue;

        /// <summary>룬 보정으로 UI 설명에 덧붙일 런타임 전용 로컬라이즈 키.</summary>
        public string RuntimeDescAppendKey;

        /// <inheritdoc/>
        public override void Parse(string[] cells)
        {
            Id = cells.Length > 0 && CsvParser.TryParseInt(cells[0], out int id) ? id : 0;
            NameKey = cells.Length > 1 ? cells[1] : string.Empty;
            AcquireType = cells.Length > 2 && System.Enum.TryParse(cells[2], true, out AcquireType acquireType) ? acquireType : AcquireType.Default;
            PurchasePrice = cells.Length > 3 && CsvParser.TryParseInt(cells[3], out int price) ? price : 0;
            SkillType = cells.Length > 4 && System.Enum.TryParse(cells[4], true, out SkillType skillType) ? skillType : SkillType.Active;
            TargetType = cells.Length > 5 && System.Enum.TryParse(cells[5], true, out SkillTargetType targetType) ? targetType : SkillTargetType.EnemySingle;
            MpCost = cells.Length > 6 && CsvParser.TryParseInt(cells[6], out int mpCost) ? mpCost : 0;
        }

        public static SkillData FromRow(System.Collections.Generic.IDictionary<string, string> row)
        {
            if (!CsvParser.HasColumns(row, nameof(SkillData), "Id", "NameKey", "SkillType", "AcquireType", "MpCost", "TargetType"))
            {
                return null;
            }

            return new SkillData
            {
                Id = CsvParser.GetInt(row, "Id"),
                NameKey = CsvParser.GetString(row, "NameKey"),
                DescKey = CsvParser.GetString(row, "DescKey"),
                SkillType = CsvParser.GetEnum(row, "SkillType", SkillType.Active),
                AcquireType = CsvParser.GetEnum(row, "AcquireType", AcquireType.Default),
                PurchasePrice = CsvParser.GetInt(row, "PurchasePrice"),
                MpCost = CsvParser.GetInt(row, "MpCost"),
                DamageScale = CsvParser.GetFloat(row, "DamageScale"),
                HealScale = CsvParser.GetFloat(row, "HealScale"),
                ShieldScale = CsvParser.GetFloat(row, "ShieldScale"),
                TargetType = CsvParser.GetEnum(row, "TargetType", SkillTargetType.EnemySingle),
                AnimationKey = CsvParser.GetString(row, "AnimationKey"),
                EffectKey = CsvParser.GetString(row, "EffectKey"),
                SfxKey = CsvParser.GetString(row, "SfxKey"),
                ActionDuration = CsvParser.GetFloat(row, "ActionDuration"),
                CooldownRounds = CsvParser.GetInt(row, "CooldownRounds"),
                ShieldDurationRounds = CsvParser.GetInt(row, "ShieldDurationRounds", 1),
                PassiveStatType = CsvParser.GetEnum(row, "PassiveStatType", PassiveStatType.None),
                PassiveFlatValue = CsvParser.GetInt(row, "PassiveFlatValue"),
                PassivePercentValue = CsvParser.GetFloat(row, "PassivePercentValue"),
            };
        }
    }

    /// <summary>스킬 종류.</summary>
    public enum SkillType
    {
        /// <summary>사용형 — 전투 중 슬롯에 배치하여 능동적으로 사용.</summary>
        Active,

        /// <summary>
        /// 지속형 — 룬 해금으로만 취득, 직접 선택/제거 불가.
        /// 전투 시작 시 EntityBase.PassiveSkills에 자동 등록되어 상시 적용.
        /// </summary>
        Passive,
    }

    /// <summary>스킬 획득 방식.</summary>
    public enum AcquireType
    {
        /// <summary>기본 보유(캐릭터 초기 지급).</summary>
        Default,

        /// <summary>길드 상점에서 골드로 구매.</summary>
        Shop,

        /// <summary>룬 노드 해금 시 자동 취득(Passive 포함).</summary>
        Rune,

        /// <summary>몬스터 전용 — 플레이어/동료는 취득 불가.</summary>
        MonsterOnly,
    }

    /// <summary>패시브 스킬이 보정하는 스탯.</summary>
    public enum PassiveStatType
    {
        /// <summary>명시형 패시브 보정 없음. 기존 Scale 기반 패시브 해석을 사용한다.</summary>
        None,

        /// <summary>최대 HP.</summary>
        HP,

        /// <summary>최대 MP.</summary>
        MP,

        /// <summary>공격력.</summary>
        ATK,

        /// <summary>방어력.</summary>
        DEF,

        /// <summary>속도.</summary>
        SPD,
    }
}

