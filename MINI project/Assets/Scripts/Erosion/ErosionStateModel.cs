using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 침식 상태 모델. 단계별 침식률 + 활성 단계 + 시작 여부.
    /// </summary>
    public sealed class ErosionStateModel
    {
        /// <summary>각 단계 침식률(stage 1~6).</summary>
        public Dictionary<int, float> StageRates = new Dictionary<int, float>();

        /// <summary>현재 월드 데이터 기준 단계 수.</summary>
        public int StageCount;

        /// <summary>침식이 시작됐는가(Safe2 도달 시 true).</summary>
        public bool IsActivated;

        /// <summary>현재 침식이 누적되는 단계(1~MaxStage). 모든 단계 침식 완료 시 MaxStage.</summary>
        public int CurrentEroddingStage;

        public void EnsureStageCount(int stageCount)
        {
            StageCount = System.Math.Max(1, stageCount);
            if (StageRates == null)
            {
                StageRates = new Dictionary<int, float>();
            }

            for (int i = 1; i <= StageCount; i++)
            {
                if (!StageRates.ContainsKey(i))
                {
                    StageRates[i] = 0f;
                }
            }
        }

        public int GetStageCount(int fallback = 6)
        {
            if (StageCount > 0)
            {
                return StageCount;
            }

            if (StageRates != null && StageRates.Count > 0)
            {
                return StageRates.Count;
            }

            return System.Math.Max(1, fallback);
        }

        /// <summary>
        /// 단계 침식률 조회(0~100).
        /// </summary>
        public float GetRate(int stageIndex)
        {
            return StageRates != null && StageRates.TryGetValue(stageIndex, out float rate) ? rate : 0f;
        }

        /// <summary>
        /// 단계가 완전 침식됐는가.
        /// </summary>
        public bool IsStageFullyEroded(int stageIndex)
        {
            return GetRate(stageIndex) >= 100f;
        }
    }
}

