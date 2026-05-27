using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 게임 전역 밸런스 곡선. JSON.
    /// </summary>
    public sealed class BalanceData
    {
        /// <summary>레벨별 필요 EXP 표(인덱스 = 레벨, 값 = 필요량).</summary>
        public List<int> ExpToNextLevel;

        /// <summary>레벨업 시 지급 룬 포인트.</summary>
        public int RunePointPerLevel;

        /// <summary>침식 곡선(일자 → 추가 비율 %, 지수함수).</summary>
        public ErosionCurve ErosionCurve;

        /// <summary>침식 시 몬스터 능력치 배수.</summary>
        public float ErosionMonsterMultiplier;

        /// <summary>가격 인플레이션 계수.</summary>
        public float InflationCoef;

        /// <summary>광산 단계별 일일 마석 지급량(SafeZone 3~5).</summary>
        public List<int> MineDailyGain;

        /// <summary>성소 1회 사용 시 침식률 차감량.</summary>
        public float ErosionAltarReduction;

        /// <summary>성소 사용 마석 비용.</summary>
        public int ErosionAltarCost;

        /// <summary>행동 타이밍 기본 최소(초).</summary>
        public float MinActionTimeSec;
    }

    /// <summary>침식 지수함수 파라미터.</summary>
    public sealed class ErosionCurve
    {
        /// <summary>일일 기본 증가량(초기).</summary>
        public float DailyBase;

        /// <summary>지수 base(>1).</summary>
        public float ExpBase;

        /// <summary>변곡 시점(일자).</summary>
        public int InflectionDay;
    }
}

