namespace Tempt
{
    /// <summary>
    /// 침식 시스템. 자기 모델의 침식률 변이와 도메인 이벤트 발행만 책임진다.
    /// </summary>
    public sealed class ErosionSystem
    {
        /// <summary>모델.</summary>
        public ErosionStateModel Model;

        /// <summary>이벤트 버스.</summary>
        public EventBus Events;

        /// <summary>침식 증가/몬스터 보정 밸런스.</summary>
        public BalanceData Balance;

        /// <summary>단계 수 권위.</summary>
        public WorldData World;

        public int MaxStage => GetMaxStage(World);

        public ErosionSystem()
        {
        }

        public ErosionSystem(ErosionStateModel model, EventBus events, BalanceData balance = null, WorldData world = null)
        {
            Model = model;
            Events = events;
            Balance = balance;
            World = world;
            Model?.EnsureStageCount(MaxStage);
        }

        public static int GetMaxStage(WorldData world)
        {
            return world?.Stages != null && world.Stages.Count > 0 ? world.Stages.Count : 6;
        }

        /// <summary>Safe2 도달 시 호출. 침식 시작.</summary>
        public void Activate()
        {
            // Guid4 §6.B 2026-05-29
            if (Model == null || Model.IsActivated)
            {
                return;
            }

            Model.IsActivated = true;
            Model.CurrentEroddingStage = 1;
            Model.EnsureStageCount(MaxStage);
            for (int i = 1; i <= MaxStage; i++)
            {
                Model.StageRates[i] = 0f;
            }

            Events?.RaiseErosionActivated();
        }

        /// <summary>일자 진행. 노드 진입마다 호출.</summary>
        public void AdvanceDay(int currentDay)
        {
            // Guid4 §6.C 2026-05-29
            if (Model == null || !Model.IsActivated)
            {
                return;
            }

            if (Balance?.ErosionCurve == null)
            {
                UnityEngine.Debug.LogError("[ErosionSystem] BalanceData.ErosionCurve 참조가 없습니다.");
                return;
            }

            int stage = UnityEngine.Mathf.Clamp(Model.CurrentEroddingStage, 1, MaxStage);
            if (Model.GetRate(stage) >= 100f)
            {
                return;
            }

            float dayFactor = UnityEngine.Mathf.Pow(
                Balance.ErosionCurve.ExpBase,
                System.Math.Max(0, currentDay - Balance.ErosionCurve.InflectionDay));
            float nextRate = UnityEngine.Mathf.Clamp(
                Model.GetRate(stage) + Balance.ErosionCurve.DailyBase * dayFactor,
                0f,
                100f);
            Model.StageRates[stage] = nextRate;
            Events?.RaiseStageErosionChanged(stage, nextRate);

            if (nextRate < 100f)
            {
                return;
            }

            Events?.RaiseStageFullyEroded(stage);
            if (stage < MaxStage)
            {
                Model.CurrentEroddingStage = stage + 1;
            }
            else
            {
                Events?.RaiseAllStagesEroded();
            }
        }

        /// <summary>단계 침식률 차감(성소 사용).</summary>
        public void Reduce(int stageIndex, float amount)
        {
            // Guid4 §6.D 2026-05-29
            if (Model == null || amount <= 0f || Model.IsStageFullyEroded(stageIndex))
            {
                return;
            }

            float nextRate = UnityEngine.Mathf.Max(0f, Model.GetRate(stageIndex) - amount);
            Model.StageRates[stageIndex] = nextRate;
            Events?.RaiseStageErosionChanged(stageIndex, nextRate);
        }

        /// <summary>보스 재클리어 시 호출. 해당 단계 침식률 0 초기화.</summary>
        public void Reset(int stageIndex)
        {
            // Guid4 §6.E 2026-05-29
            if (Model == null)
            {
                return;
            }

            int stage = UnityEngine.Mathf.Clamp(stageIndex, 1, MaxStage);
            Model.StageRates[stage] = 0f;
            Events?.RaiseStageErosionChanged(stage, 0f);

            for (int i = 1; i <= MaxStage; i++)
            {
                if (Model.GetRate(i) < 100f)
                {
                    Model.CurrentEroddingStage = i;
                    return;
                }
            }

            Model.CurrentEroddingStage = MaxStage;
        }

        /// <summary>현재 단계 침식률 기반 몬스터 보정 배수.</summary>
        public float ComputeMonsterMultiplier(int stageIndex)
        {
            // Guid4 §6.F 2026-05-29
            float rate = Model != null ? Model.GetRate(stageIndex) : 0f;
            if (rate <= 0f)
            {
                return 1f;
            }

            if (Balance == null)
            {
                UnityEngine.Debug.LogError("[ErosionSystem] BalanceData 참조가 없습니다.");
                return 1f;
            }

            float maxMultiplier = UnityEngine.Mathf.Max(1f, Balance.ErosionMonsterMultiplier);
            if (!Balance.UseErosionMonsterMultiplierCurve)
            {
                return maxMultiplier;
            }

            float t = UnityEngine.Mathf.Clamp01(rate / 100f);
            float power = Balance.ErosionMonsterMultiplierCurvePower > 0f ? Balance.ErosionMonsterMultiplierCurvePower : 1f;
            t = UnityEngine.Mathf.Pow(t, power);
            return UnityEngine.Mathf.Lerp(1f, maxMultiplier, t);
        }

        /// <summary>단계 완전 침식 여부.</summary>
        public bool IsStageFullyEroded(int stageIndex)
        {
            // Guid4 §6.G 2026-05-29
            return Model != null && Model.IsStageFullyEroded(stageIndex);
        }
    }
}

