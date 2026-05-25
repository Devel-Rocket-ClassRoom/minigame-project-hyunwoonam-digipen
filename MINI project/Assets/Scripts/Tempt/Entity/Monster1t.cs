namespace Tempt
{
    /// <summary>
    /// 1단계 일반 몬스터 예시. 데이터 주도가 기본이므로 코드 특화는 최소화.
    /// </summary>
    public sealed class Monster1t : MonsterBaset
    {
        /// <summary>
        /// 데이터 외 특수 동작이 있을 경우 PrepareForCombat에서 override.
        /// </summary>
        public override void PrepareForCombat()
        {
            // 동작 요약: base.PrepareForCombat() 호출 + 이 몬스터 고유 초기화(필요 시).
        }
    }
}
