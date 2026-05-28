namespace Tempt
{
    /// <summary>
    /// 1단계 일반 몬스터 예시. 데이터 주도가 기본이므로 코드 특화는 최소화.
    /// </summary>
    public sealed class Monster1 : MonsterBase
    {
        /// <summary>
        /// 데이터 외 특수 동작이 있을 경우 PrepareForCombat에서 override.
        /// </summary>
        public override void PrepareForCombat()
        {
            // 동작 요약: base.PrepareForCombat() 호출 + 이 몬스터 고유 초기화(필요 시).
            //TODO: base.PrepareForCombat();
            //TODO: // Monster1 고유 특수 동작이 없으면 base 호출만으로 충분
            base.PrepareForCombat(); //Wave0write
        }
    }
}

