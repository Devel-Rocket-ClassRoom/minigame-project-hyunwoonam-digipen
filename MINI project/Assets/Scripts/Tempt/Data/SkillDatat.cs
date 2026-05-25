namespace Tempt
{
    /// <summary>
    /// 스킬 정적 데이터. CSV 1행.
    /// </summary>
    public sealed class SkillDatat : DataTablet
    {
        /// <summary>MP 소비량.</summary>
        public int MpCost;

        /// <summary>기본 데미지 배수(0이면 공격 무관).</summary>
        public float DamageScale;

        /// <summary>회복량 배수(0이면 회복 없음).</summary>
        public float HealScale;

        /// <summary>보호막 배수(0이면 보호막 없음, DEF * scale 적용).</summary>
        public float ShieldScale;

        /// <summary>타겟 타입.</summary>
        public SkillTargetTypet TargetType;

        /// <summary>애니메이션 키.</summary>
        public string AnimationKey;

        /// <summary>이펙트 키.</summary>
        public string EffectKey;

        /// <summary>실행 시간(초). 0 입력 시 ActionTimingt가 기본 0.1초 + 애니/이펙트 합산으로 보정.</summary>
        public float ActionDuration;

        /// <summary>쿨다운(라운드).</summary>
        public int CooldownRounds;

        /// <inheritdoc/>
        public override void Parse(string[] cells)
        {
            // 동작 요약: 모든 필드 순서대로 파싱.
        }
    }
}
