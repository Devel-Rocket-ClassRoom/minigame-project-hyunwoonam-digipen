namespace Tempt
{
    /// <summary>
    /// 전투 전용 HUD. 행동 패널(공격/스킬1/스킬2/방어 + 소모 4칸), 라운드 표시, 적 의도 표시.
    /// </summary>
    public sealed class CombatHudt : UIPageControllerBaset
    {
        /// <summary>참조 컨트롤러.</summary>
        public CombatControllert Controller;

        /// <inheritdoc/>
        public override void OnOpen()
        {
            // 동작 요약:
            // - 행동 버튼 OnClick 바인딩(Controller.PlayerPickAttack 등).
            // - 소모 4칸 버튼 바인딩(Controller.PlayerUseItem).
            // - 라운드 / 적 의도 / Player HP/MP 표시 갱신.
        }

        /// <inheritdoc/>
        public override void OnClose()
        {
            // 동작 요약: 바인딩 해제.
        }

        /// <summary>플레이어 행동 패널 표시.</summary>
        public void ShowPlayerActionPanel(EntityBaset actor)
        {
            // 동작 요약: actor가 Player면 활성.
        }

        /// <summary>플레이어 행동 패널 숨김.</summary>
        public void HidePlayerActionPanel()
        {
            // 동작 요약: 비활성.
        }
    }
}
