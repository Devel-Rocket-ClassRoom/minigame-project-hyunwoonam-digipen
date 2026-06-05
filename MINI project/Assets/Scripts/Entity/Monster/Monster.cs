namespace Tempt
{
    /// <summary>
    /// 데이터 테이블로 초기화되는 공통 몬스터 런타임.
    /// </summary>
    public sealed class Monster : MonsterBase
    {
        /// <summary>
        /// 데이터 외 특수 동작이 있을 경우 PrepareForCombat에서 override.
        /// </summary>
        public override void PrepareForCombat()
        {
            // 동작 요약: base.PrepareForCombat() 호출 + 이 몬스터 고유 초기화(필요 시).
            base.PrepareForCombat();
        }
    }
}

