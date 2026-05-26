namespace Tempt
{
    /// <summary>
    /// S 단축키 페이지. 스탯(HP/MP/ATK/SPD/DEF) + 룬 트리 확인.
    /// 룬은 신전(Templet) 외에는 변경 불가, 이 페이지에서는 보기만.
    /// </summary>
    public sealed class StatRunePaget : UIPageControllerBaset
    {
        /// <inheritdoc/>
        public override void OnOpen()
        {
            // 동작 요약: Player.Stats, Player.Rune.Tree 시각화. 동료 탭도 제공.
            //TODO: var run = GameSystemManagert.Instance.CurrentRun;
            //TODO: StatPanel.Bind(run.Player.Stats, run.Player.BonusStats);
            //TODO: RuneTreeView.Render(run.Player.Rune, interactive: false); // view-only
            //TODO: CompanionTabGroup.Clear();
            //TODO: foreach (var c in run.Roster.Active)
            //TODO:     CompanionTabGroup.AddTab(c.Data.NameKey, () => StatPanel.Bind(c.Stats, c.BonusStats));
        }

        /// <inheritdoc/>
        public override void OnClose()
        {
            // 동작 요약: 정리.
            //TODO: CompanionTabGroup.Clear();
        }
    }
}
