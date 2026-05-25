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
        }

        /// <inheritdoc/>
        public override void OnClose()
        {
            // 동작 요약: 정리.
        }
    }
}
