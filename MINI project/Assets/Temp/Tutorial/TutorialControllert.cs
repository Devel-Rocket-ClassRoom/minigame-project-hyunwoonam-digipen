using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 튜토리얼 진행 컨트롤러. Safe1 첫 진입, 던전 1층/2층/3층 등 정해진 시퀀스 진행.
    /// 완료된 시퀀스는 GameRunStatet.Tutorial에 기록되어 같은 런 안에서는 반복하지 않는다.
    /// </summary>
    public sealed class TutorialControllert
    {
        /// <summary>현재 진행 중인 시퀀스의 단계들.</summary>
        public List<TutorialStept> CurrentSteps;

        /// <summary>현재 단계 인덱스.</summary>
        public int CurrentIndex;

        /// <summary>
        /// 시퀀스 시작. 이미 완료된 시퀀스는 무시.
        /// </summary>
        public void StartIfNotCompleted(string sequenceKey)
        {
            // 동작 요약:
            // - GameRunStatet.Tutorial.CompletedSteps에 sequenceKey 포함이면 return.
            // - 데이터(JSON 또는 코드 상수)에서 sequenceKey의 step 목록 로드.
            // - CurrentSteps 설정, CurrentIndex = 0.
            // - 첫 단계 표시.
            //TODO: var progress = GameSystemManagert.Instance.CurrentRun.Tutorial;
            //TODO: if (progress.IsCompleted(sequenceKey)) return;
            //TODO: CurrentSteps = TutorialDatat.GetSteps(sequenceKey); // 데이터에서 단계 목록 로드
            //TODO: CurrentIndex = 0;
            //TODO: TutorialUI.Show(CurrentSteps[0]);
        }

        /// <summary>
        /// 다음 단계 진행 트리거(Enter 등).
        /// </summary>
        public void Advance()
        {
            // 동작 요약:
            // - CurrentIndex += 1.
            // - CurrentSteps.Count 도달 시 완료 처리: CompletedSteps에 추가, UI 닫기.
            // - 아니면 다음 단계 표시.
            //TODO: if (CurrentSteps == null) return;
            //TODO: CurrentIndex++;
            //TODO: if (CurrentIndex >= CurrentSteps.Count)
            //TODO: {
            //TODO:     GameSystemManagert.Instance.CurrentRun.Tutorial.MarkCompleted(CurrentSteps[0].StepKey.Split('.')[0]);
            //TODO:     CurrentSteps = null;
            //TODO:     TutorialUI.Hide();
            //TODO: }
            //TODO: else TutorialUI.Show(CurrentSteps[CurrentIndex]);
        }

        /// <summary>
        /// 매 프레임 호출. 키 입력 감지.
        /// </summary>
        public void Tick()
        {
            // 동작 요약:
            // - CurrentSteps가 null이면 return.
            // - 현재 단계의 AdvanceTrigger에 맞는 입력 발생 시 Advance().
            //TODO: if (CurrentSteps == null || CurrentIndex >= CurrentSteps.Count) return;
            //TODO: var trigger = CurrentSteps[CurrentIndex].AdvanceTrigger;
            //TODO: if (trigger == TutorialAdvanceTriggert.EnterKey && UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Return))
            //TODO:     Advance();
            //TODO: else if (trigger == TutorialAdvanceTriggert.AnyKey && UnityEngine.Input.anyKeyDown)
            //TODO:     Advance();
            //TODO: // TargetClick은 FloorNodeUIt 또는 상호작용 UI에서 직접 Advance() 호출
        }
    }
}
