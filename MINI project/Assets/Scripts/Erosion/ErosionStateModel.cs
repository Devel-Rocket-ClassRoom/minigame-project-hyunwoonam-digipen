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

        /// <summary>침식이 시작됐는가(Safe2 도달 시 true).</summary>
        public bool IsActivated;

        /// <summary>현재 침식 진행 중인 가장 낮은 단계(아래부터 침식).</summary>
        public int CurrentEroddingStage;

        /// <summary>
        /// 단계 침식률 조회(0~100).
        /// </summary>
        public float GetRate(int stageIndex)
        {
            // 동작 요약: StageRates.TryGetValue 반환.
            //TODO: return StageRates.TryGetValue(stageIndex, out float rate) ? rate : 0f;
            return StageRates.TryGetValue(stageIndex, out float rate) ? rate : 0f; //Wave0write
        }

        /// <summary>
        /// 단계가 완전 침식됐는가.
        /// </summary>
        public bool IsStageFullyEroded(int stageIndex)
        {
            // 동작 요약: GetRate(stageIndex) >= 100.
            //TODO: return GetRate(stageIndex) >= 100f;
            return GetRate(stageIndex) >= 100f; //Wave0write
        }
    }
}

