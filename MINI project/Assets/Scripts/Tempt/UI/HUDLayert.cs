namespace Tempt
{
    /// <summary>
    /// 항상 표시되는 HUD 레이어. 골드/마석/현재 층/일자/단계 침식 표시.
    /// 전투/단축키 페이지가 열려 있어도 가시.
    /// </summary>
    public sealed class HUDLayert : UIPageControllerBaset
    {
        /// <inheritdoc/>
        public override void OnOpen()
        {
            // 동작 요약:
            // - EventBust 구독: OnGoldChanged, OnManaStoneChanged, OnStageErosionChanged, OnPlayerLevelUp.
            // - 현재 값으로 초기 표시.
        }

        /// <inheritdoc/>
        public override void OnClose()
        {
            // 동작 요약: 이벤트 구독 해제.
        }
    }
}
