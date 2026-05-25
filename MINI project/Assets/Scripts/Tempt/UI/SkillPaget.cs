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
        }

        /// <inheritdoc/>
        public override void OnClose()
        {
            // 동작 요약: 정리.
        }
    }
}
