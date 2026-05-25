namespace Tempt
{
    /// <summary>
    /// 침식 시스템. 일자 진행 시 단계 침식률 증가, 100% 도달 시 안전지대 잠금, 보스 재클리어로 해금.
    /// 안전지대 2 도달 후 활성화.
    /// </summary>
    public sealed class ErosionSystemt
    {
        /// <summary>모델.</summary>
        public ErosionStateModelt Model;

        /// <summary>이벤트 버스.</summary>
        public EventBust Events;

        /// <summary>
        /// Safe2 도달 시 호출. 침식 시작.
        /// </summary>
        public void Activate()
        {
            // 동작 요약:
            // - Model.IsActivated = true.
            // - Model.CurrentEroddingStage = 1.
            // - 이벤트 발행.
        }

        /// <summary>
        /// 일자 진행. 노드 진입마다 호출.
        /// </summary>
        public void AdvanceDay(int currentDay)
        {
            // 동작 요약:
            // - IsActivated가 false면 return.
            // - 현재 침식 진행 단계에 BalanceDatat.ErosionCurve 기반 증가량 적용(지수).
            // - 100 도달 시:
            //   * 해당 단계의 안전지대 잠금: GameRunStatet.SafeUnlocks[idx] = false.
            //   * Events.RaiseSafeZoneLockChanged.
            //   * 다음 단계로 침식 진행(CurrentEroddingStage += 1).
            // - 각 단계 침식 변화 이벤트.
        }

        /// <summary>
        /// 단계 침식률 차감(성소 사용).
        /// </summary>
        public void Reduce(int stageIndex, float amount)
        {
            // 동작 요약:
            // - 현재 침식률에서 amount 차감, 최소 0.
            // - 단, 100 도달 후 차감은 불가(설계: 100% 전에만 사용 가능).
        }

        /// <summary>
        /// 보스 재클리어 시 호출. 해당 단계 침식률 0 초기화 + 안전지대 재해금.
        /// </summary>
        public void OnBossRecleared(int stageIndex)
        {
            // 동작 요약:
            // - Model.StageRates[stageIndex] = 0.
            // - GameRunStatet.SafeUnlocks[해당 안전지대] = true.
            // - CurrentEroddingStage 재계산(가장 낮은 미침식 + 1 단계).
        }

        /// <summary>
        /// 현재 단계 침식률 기반 몬스터 보정 배수.
        /// </summary>
        public float ComputeMonsterMultiplier(int stageIndex)
        {
            // 동작 요약:
            // - 침식률이 0이면 1.0.
            // - 침식률 > 0이면 BalanceDatat.ErosionMonsterMultiplier(기본 1.5) 반환.
            return 1f;
        }
    }
}
