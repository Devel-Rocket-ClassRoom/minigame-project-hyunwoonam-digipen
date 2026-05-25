namespace Tempt
{
    /// <summary>
    /// 안전지대 2: 성소(침식 관리). 단계 침식률을 마석으로 차감 가능. 100% 차감은 불가.
    /// </summary>
    public sealed class Safe2Controllert : SafeZoneControllerBaset
    {
        /// <summary>성소.</summary>
        public ErosionAltart Altar;

        /// <inheritdoc/>
        protected override void SetupZoneFeatures()
        {
            // 동작 요약:
            // - 안전지대 2 도달 시 침식 시스템 활성화 시점 → ErosionSystemt.Activate().
            // - Altar 인터랙션 등록.
        }
    }
}
