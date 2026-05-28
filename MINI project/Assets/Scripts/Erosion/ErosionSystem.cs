namespace Tempt
{
    /// <summary>
    /// 침식 시스템. 일자 진행 시 단계 침식률 증가, 100% 도달 시 안전지대 잠금, 보스 재클리어로 해금.
    /// 안전지대 2 도달 후 활성화.
    /// </summary>
    public sealed class ErosionSystem
    {
        /// <summary>모델.</summary>
        public ErosionStateModel Model;

        /// <summary>이벤트 버스.</summary>
        public EventBus Events;

        public ErosionSystem(ErosionStateModel model, EventBus events)
        {
            Model = model;
            Events = events;
        }

        /// <summary>
        /// Safe2 도달 시 호출. 침식 시작.
        /// </summary>
        public void Activate()
        {
            // 동작 요약:
            // - Model.IsActivated = true.
            // - Model.CurrentEroddingStage = 1.
            // - 이벤트 발행.
            //TODO: if (Model.IsActivated) return;
            //TODO: Model.IsActivated = true;
            //TODO: Model.CurrentEroddingStage = 1;
            //TODO: for (int i = 1; i <= 6; i++) Model.StageRates[i] = 0f;
            //TODO: Events.RaiseErosionActivated();
            if (Model == null || Model.IsActivated) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            Model.IsActivated = true; //Wave0write
            Model.CurrentEroddingStage = 1; //Wave0write
            for (int i = 1; i <= 6; i++) //Wave0write
            { //Wave0write
                if (!Model.StageRates.ContainsKey(i)) //Wave0write
                { //Wave0write
                    Model.StageRates[i] = 0f; //Wave0write
                } //Wave0write
            } //Wave0write
        }

        /// <summary>
        /// 일자 진행. 노드 진입마다 호출.
        /// </summary>
        public void AdvanceDay(int currentDay)
        {
            // 동작 요약:
            // - IsActivated가 false면 return.
            // - 현재 침식 진행 단계에 BalanceData.ErosionCurve 기반 증가량 적용(지수).
            // - 100 도달 시:
            //   * 해당 단계의 안전지대 잠금: GameRunState.SafeUnlocks[idx] = false.
            //   * Events.RaiseSafeZoneLockChanged.
            //   * 다음 단계로 침식 진행(CurrentEroddingStage += 1).
            // - 각 단계 침식 변화 이벤트.
            //TODO: if (!Model.IsActivated) return;
            //TODO: int s = Model.CurrentEroddingStage;
            //TODO: float increment = BalanceData.ErosionDailyIncrement * UnityEngine.Mathf.Pow(BalanceData.ErosionExponent, s - 1);
            //TODO: Model.StageRates[s] = UnityEngine.Mathf.Min(100f, Model.StageRates[s] + increment);
            //TODO: Events.RaiseErosionRateChanged(s, Model.StageRates[s]);
            //TODO: if (Model.StageRates[s] >= 100f)
            //TODO: {
            //TODO:     // 해당 단계 안전지대 잠금
            //TODO:     var run = GameSystemManager.Instance.CurrentRun;
            //TODO:     if (run.SafeUnlocks.ContainsKey(s)) run.SafeUnlocks[s] = false;
            //TODO:     Events.RaiseSafeZoneLockChanged(s, false);
            //TODO:     if (s < 6) Model.CurrentEroddingStage++;
            //TODO: }
            if (Model == null || !Model.IsActivated) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            int stage = System.Math.Max(1, Model.CurrentEroddingStage); //Wave0write
            BalanceData balance = GameSystemManager.TryGetInstance(out GameSystemManager gsm) ? gsm.Data?.Balance : null; //Wave0write
            float dailyBase = balance?.ErosionCurve != null ? balance.ErosionCurve.DailyBase : 1f; //Wave0write
            float expBase = balance?.ErosionCurve != null ? balance.ErosionCurve.ExpBase : 1f; //Wave0write
            int inflection = balance?.ErosionCurve != null ? balance.ErosionCurve.InflectionDay : 1; //Wave0write
            float dayFactor = UnityEngine.Mathf.Pow(expBase, System.Math.Max(0, currentDay - inflection)); //Wave0write
            float nextRate = UnityEngine.Mathf.Clamp(Model.GetRate(stage) + dailyBase * dayFactor, 0f, 100f); //Wave0write
            Model.StageRates[stage] = nextRate; //Wave0write
            Events?.RaiseStageErosionChanged(stage, nextRate); //Wave0write

            if (nextRate >= 100f && gsm != null && gsm.CurrentRun?.SafeUnlocks != null) //Wave0write
            { //Wave0write
                gsm.CurrentRun.SafeUnlocks.Lock(stage); //Wave0write
                Events?.RaiseSafeZoneLockChanged(stage, true); //Wave0write
                if (stage < 6) //Wave0write
                { //Wave0write
                    Model.CurrentEroddingStage = stage + 1; //Wave0write
                } //Wave0write
            } //Wave0write
        }

        /// <summary>
        /// 단계 침식률 차감(성소 사용).
        /// </summary>
        public void Reduce(int stageIndex, float amount)
        {
            // 동작 요약:
            // - 현재 침식률에서 amount 차감, 최소 0.
            // - 단, 100 도달 후 차감은 불가(설계: 100% 전에만 사용 가능).
            //TODO: if (!Model.StageRates.ContainsKey(stageIndex)) return;
            //TODO: if (Model.StageRates[stageIndex] >= 100f) return; // 완전 침식 후 차감 불가
            //TODO: Model.StageRates[stageIndex] = UnityEngine.Mathf.Max(0f, Model.StageRates[stageIndex] - amount);
            //TODO: Events.RaiseErosionRateChanged(stageIndex, Model.StageRates[stageIndex]);
            if (Model == null || Model.GetRate(stageIndex) >= 100f) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            Model.StageRates[stageIndex] = UnityEngine.Mathf.Max(0f, Model.GetRate(stageIndex) - amount); //Wave0write
            Events?.RaiseStageErosionChanged(stageIndex, Model.StageRates[stageIndex]); //Wave0write
        }

        /// <summary>
        /// 보스 재클리어 시 호출. 해당 단계 침식률 0 초기화 + 안전지대 재해금.
        /// </summary>
        public void OnBossRecleared(int stageIndex)
        {
            // 동작 요약:
            // - Model.StageRates[stageIndex] = 0.
            // - GameRunState.SafeUnlocks[해당 안전지대] = true.
            // - CurrentEroddingStage 재계산(가장 낮은 미침식 + 1 단계).
            //TODO: Model.StageRates[stageIndex] = 0f;
            //TODO: var run = GameSystemManager.Instance.CurrentRun;
            //TODO: if (run.SafeUnlocks.ContainsKey(stageIndex)) run.SafeUnlocks[stageIndex] = true;
            //TODO: Events.RaiseSafeZoneLockChanged(stageIndex, true);
            //TODO: // CurrentEroddingStage = 침식률 100 미만인 가장 낮은 단계
            //TODO: for (int i = 1; i <= 6; i++)
            //TODO: {
            //TODO:     if (!Model.StageRates.ContainsKey(i) || Model.StageRates[i] < 100f)
            //TODO:     { Model.CurrentEroddingStage = i; break; }
            //TODO: }
            //TODO: Events.RaiseErosionRateChanged(stageIndex, 0f);
            if (Model == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            Model.StageRates[stageIndex] = 0f; //Wave0write
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.CurrentRun?.SafeUnlocks != null) //Wave0write
            { //Wave0write
                gsm.CurrentRun.SafeUnlocks.Unlock(stageIndex); //Wave0write
            } //Wave0write

            Events?.RaiseSafeZoneLockChanged(stageIndex, false); //Wave0write
            Events?.RaiseStageErosionChanged(stageIndex, 0f); //Wave0write
            for (int i = 1; i <= 6; i++) //Wave0write
            { //Wave0write
                if (Model.GetRate(i) < 100f) //Wave0write
                { //Wave0write
                    Model.CurrentEroddingStage = i; //Wave0write
                    break; //Wave0write
                } //Wave0write
            } //Wave0write
        }

        /// <summary>
        /// 현재 단계 침식률 기반 몬스터 보정 배수.
        /// </summary>
        public float ComputeMonsterMultiplier(int stageIndex)
        {
            // 동작 요약:
            // - 침식률이 0이면 1.0.
            // - 침식률 > 0이면 BalanceData.ErosionMonsterMultiplier(기본 1.5) 반환.
            //TODO: float rate = Model.GetRate(stageIndex);
            //TODO: return rate > 0f ? BalanceData.ErosionMonsterMultiplier : 1f;
            float rate = Model != null ? Model.GetRate(stageIndex) : 0f; //Wave0write
            if (rate <= 0f) //Wave0write
            { //Wave0write
                return 1f; //Wave0write
            } //Wave0write

            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Data?.Balance != null) //Wave0write
            { //Wave0write
                return UnityEngine.Mathf.Max(1f, gsm.Data.Balance.ErosionMonsterMultiplier); //Wave0write
            } //Wave0write

            return 1.5f; //Wave0write
        }
    }
}

