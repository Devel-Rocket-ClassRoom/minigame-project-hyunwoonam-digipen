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
            //TODO: var run = GameSystemManagert.Instance.CurrentRun;
            //TODO: var ev = GameSystemManagert.Instance.Events;
            //TODO: GoldLabel.text  = run.Player.Gold.ToString();
            //TODO: StoneLabel.text = run.ManaStone.ToString();
            //TODO: FloorLabel.text = run.CurrentFloor.ToString();
            //TODO: DayLabel.text   = run.CurrentDay.ToString();
            //TODO: ev.OnGoldChanged       += (g) => GoldLabel.text  = g.ToString();
            //TODO: ev.OnManaStoneChanged  += (s) => StoneLabel.text = s.ToString();
            //TODO: ev.OnErosionRateChanged += (idx, rate) => ErosionHUD.SetRate(idx, rate);
            //TODO: ev.OnPlayerLevelUp     += (lv) => PlayerLevelLabel.text = lv.ToString();
        }

        /// <inheritdoc/>
        public override void OnClose()
        {
            // 동작 요약: 이벤트 구독 해제.
            //TODO: var ev = GameSystemManagert.Instance.Events;
            //TODO: ev.OnGoldChanged        = null;
            //TODO: ev.OnManaStoneChanged   = null;
            //TODO: ev.OnErosionRateChanged = null;
            //TODO: ev.OnPlayerLevelUp      = null;
        }
    }
}
