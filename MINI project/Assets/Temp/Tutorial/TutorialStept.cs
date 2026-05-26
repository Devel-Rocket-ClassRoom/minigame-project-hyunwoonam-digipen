namespace Tempt
{
    /// <summary>
    /// 튜토리얼 한 단계 데이터. 화살표 대상 + 설명 문자열 키 + 완료 트리거.
    /// </summary>
    public sealed class TutorialStept
    {
        /// <summary>고유 키.</summary>
        public string StepKey;

        /// <summary>화살표가 가리킬 대상 키(예: "Safe1.Inn").</summary>
        public string ArrowTargetKey;

        /// <summary>설명 언어 키.</summary>
        public string DescriptionKey;

        /// <summary>다음 단계로 가는 트리거(기본: Enter).</summary>
        public TutorialAdvanceTriggert AdvanceTrigger;
    }

    /// <summary>튜토리얼 진행 트리거.</summary>
    public enum TutorialAdvanceTriggert
    {
        /// <summary>Enter 키.</summary>
        EnterKey,

        /// <summary>임의 키.</summary>
        AnyKey,

        /// <summary>특정 UI 클릭.</summary>
        TargetClick,
    }
}
