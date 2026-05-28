namespace Tempt
{
    /// <summary>
    /// 스킬 정적 데이터. SkillTable.csv 1행.
    /// </summary>
    public sealed class SkillDatat : DataTablet
    {
        /// <summary>스킬 종류(액티브 또는 패시브).</summary>
        public SkillTypet SkillType;

        /// <summary>스킬 획득 방식.</summary>
        public AcquireTypet AcquireType;

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
        public SkillTargetTypet TargetType;

        /// <summary>애니메이션 키.</summary>
        public string AnimationKey;

        /// <summary>이펙트 키.</summary>
        public string EffectKey;

        /// <summary>실행 시간(초). 0 입력 시 ActionTimingt가 기본 0.1초 + 애니/이펙트 합산으로 보정.</summary>
        public float ActionDuration;

        /// <summary>쿨다운(라운드). Passive는 0.</summary>
        public int CooldownRounds;

        /// <inheritdoc/>
        public override void Parse(string[] cells)
        {
            // 동작 요약: SkillTable.csv 열 순서대로 파싱.
            // - cells[0] = SkillID → Id
            // - cells[1] = NameKey
            // - cells[2] = AcquireType (enum 문자열 → AcquireTypet)
            // - cells[3] = PurchasePrice (int)
            // - cells[4] = SkillType (enum 문자열 → SkillTypet)
            // - cells[5] = TargetType (enum 문자열 → SkillTargetTypet)
            // - cells[6] = MpCost (int)
            // - cells[7] = EffectFormula → DamageScale / HealScale / ShieldScale 분기 파싱
            // - cells[8] = EffectValue (float)
            // - cells[9] = DescriptionKey
            // - AnimationKey, EffectKey, ActionDuration, CooldownRounds는 별도 컬럼 또는 기본값 처리
            //TODO: Id            = int.Parse(cells[0]);
            //TODO: NameKey       = cells[1];
            //TODO: AcquireType   = (AcquireTypet)System.Enum.Parse(typeof(AcquireTypet), cells[2]);
            //TODO: PurchasePrice = int.Parse(cells[3]);
            //TODO: SkillType     = (SkillTypet)System.Enum.Parse(typeof(SkillTypet), cells[4]);
            //TODO: TargetType    = (SkillTargetTypet)System.Enum.Parse(typeof(SkillTargetTypet), cells[5]);
            //TODO: MpCost        = int.Parse(cells[6]);
            //TODO: // cells[7] = 효과 분류("Damage"/"Heal"/"Shield"), cells[8] = scale 수치
            //TODO: float effectVal = float.Parse(cells[8]);
            //TODO: switch (cells[7].Trim())
            //TODO: {
            //TODO:     case "Damage": DamageScale = effectVal; break;
            //TODO:     case "Heal":   HealScale   = effectVal; break;
            //TODO:     case "Shield": ShieldScale = effectVal; break;
            //TODO: }
            //TODO: DescKey        = cells[9];
            //TODO: CooldownRounds = cells.Length > 10 ? int.Parse(cells[10]) : 0;
            //TODO: ActionDuration = cells.Length > 11 ? float.Parse(cells[11]) : 0f;
        }
    }

    /// <summary>스킬 종류.</summary>
    public enum SkillTypet
    {
        /// <summary>사용형 — 전투 중 슬롯에 배치하여 능동적으로 사용.</summary>
        Active,

        /// <summary>
        /// 지속형 — 룬 해금으로만 취득, 직접 선택/제거 불가.
        /// 전투 시작 시 EntityBaset.PassiveSkills에 자동 등록되어 상시 적용.
        /// </summary>
        Passive,
    }

    /// <summary>스킬 획득 방식.</summary>
    public enum AcquireTypet
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
}
