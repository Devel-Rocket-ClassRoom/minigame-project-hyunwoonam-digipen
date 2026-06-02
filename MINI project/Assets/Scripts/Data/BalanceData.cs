using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 게임 전역 밸런스 곡선. JSON.
    /// </summary>
    [System.Serializable]
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

        /// <summary>판매가 비율. 판매가 = BasePrice * SellRatio * Inflation.</summary>
        public float SellRatio;

        /// <summary>장비 강화 1단계당 스탯 배율 증가량.</summary>
        public float EnhanceMultiplier;

        /// <summary>강화 비용 기본 계수. 비용 = BasePrice * (EnhanceCostBase + level * EnhanceCostPerLevel).</summary>
        public float EnhanceCostBase;

        /// <summary>강화 비용 레벨당 증가 계수.</summary>
        public float EnhanceCostPerLevel;

        /// <summary>강화 기본 성공률(0~1).</summary>
        public float EnhanceBaseSuccessRate;

        /// <summary>강화 레벨당 성공률 감소량.</summary>
        public float EnhanceSuccessRateDecayPerLevel;

        /// <summary>강화 성공률 최소 하한(0~1).</summary>
        public float EnhanceMinSuccessRate;

        /// <summary>이 횟수만큼 연속 실패하면 다음 강화가 확정 성공한다.</summary>
        public int EnhancePityFailCount;

        /// <summary>광산 단계별 일일 골드 지급량(SafeZone 3~5).</summary>
        public List<int> MineDailyGain;

        /// <summary>광산 활성화 기본 골드 비용.</summary>
        public int MineActivationCost;

        /// <summary>성소 1회 사용 시 침식률 차감량.</summary>
        public float ErosionAltarReduction;

        /// <summary>성소 정화 골드 비용.</summary>
        public int ErosionAltarCost;

        /// <summary>행동 타이밍 기본 최소(초).</summary>
        public float MinActionTimeSec;

        /// <summary>룬 초기화 시 환급 비율(0~1).</summary>
        public float RuneResetRefundRate;

        /// <summary>룬 트리 초기화 골드 비용.</summary>
        public int RuneResetCostGold;

        /// <summary>룬 직업 변경 골드 비용.</summary>
        public int RuneClassChangeCostGold;

        /// <summary>첫 AI 행동 시작 전 대기 시간(초).</summary>
        public float FirstNonPlayerActionDelaySec;

        /// <summary>기본 공격 추가 행동 시간(초).</summary>
        public float AttackActionTimeSec;

        /// <summary>스킬 데이터에 행동 시간이 없을 때 사용할 기본 시간(초).</summary>
        public float SkillActionFallbackSec;

        /// <summary>방어 행동 추가 시간(초).</summary>
        public float DefendActionTimeSec;

        /// <summary>런타임 대체 전투 유닛 스프라이트 크기(px).</summary>
        public int CombatGeneratedSpriteSize;

        /// <summary>런타임 대체 전투 유닛 스프라이트 PPU.</summary>
        public float CombatGeneratedSpritePixelsPerUnit;

        /// <summary>침식률 비례 몬스터 배수 곡선 사용 여부.</summary>
        public bool UseErosionMonsterMultiplierCurve;

        /// <summary>침식률 배수 곡선 지수. 1이면 선형.</summary>
        public float ErosionMonsterMultiplierCurvePower;
    }

    /// <summary>침식 지수함수 파라미터.</summary>
    [System.Serializable]
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
