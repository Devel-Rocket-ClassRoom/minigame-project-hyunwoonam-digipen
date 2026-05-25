namespace Tempt
{
    /// <summary>
    /// 첫 번째 동료 예시 구현. 직업별 특수화는 별도 파생 클래스에서 처리.
    /// 실제 직업이 4종(Dealer/Tanker/MagicDealer/Supporter)이면 각각 별도 파일을 만들거나
    /// 데이터 주도로 한 클래스에서 처리해도 된다(설계 결정 보류).
    /// </summary>
    public sealed class TeamMember1t : TeamBaset
    {
        /// <summary>
        /// 데이터 적용 후 초기 셋업.
        /// </summary>
        protected override void OnLeveledUp()
        {
            // 동작 요약: base.OnLeveledUp() 호출 + 이 동료 고유 성장 보정.
        }
    }
}
