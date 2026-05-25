namespace Tempt
{
    /// <summary>
    /// 안전지대 0: 시작 안식처. 호수/모래밭, 비석/묘비, 최초 룬 선택.
    /// </summary>
    public sealed class Safe0Controllert : SafeZoneControllerBaset
    {
        /// <summary>
        /// 최초 시작 룬 선택 UI 준비.
        /// </summary>
        protected override void SetupZoneFeatures()
        {
            // 동작 요약:
            // - 비석/묘비 데이터 렌더링(RecordBookt 사용, 글자 크기 누적에 따라 작아짐).
            // - 플레이어가 룬을 한 번도 선택하지 않은 상태이면 룬 선택 UI를 강제 표시.
            // - 룬 선택 완료 콜백 → Playert.ApplyStartingClass.
            // - 출발 버튼 활성(룬 선택 후).
        }
    }
}
