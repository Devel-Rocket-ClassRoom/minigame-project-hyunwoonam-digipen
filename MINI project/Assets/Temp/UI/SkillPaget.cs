namespace Tempt
{
    /// <summary>
    /// K 단축키 페이지. 액티브 스킬 2칸 확인 + 동료 스킬 확인.
    /// </summary>
    public sealed class SkillPaget : UIPageControllerBaset
    {
        /// <inheritdoc/>
        public override void OnOpen()
        {
            // 동작 요약: Player + 동료 액티브 스킬 슬롯, 쿨다운, 설명 표시.
            //TODO: var run = GameSystemManagert.Instance.CurrentRun;
            //TODO: PlayerSkillPanel.Bind(run.Player.ActiveSkills); // Slot1/Slot2 쿨다운 포함
            //TODO: foreach (var companion in run.Roster.Active)
            //TODO:     CompanionSkillPanel.AddCompanionRow(companion.Data.Id, companion.ActiveSkills);
        }

        /// <inheritdoc/>
        public override void OnClose()
        {
            // 동작 요약: 정리.
            //TODO: CompanionSkillPanel.ClearRows();
        }
    }
}
