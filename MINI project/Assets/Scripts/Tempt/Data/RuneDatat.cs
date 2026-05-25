using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 룬 정적 데이터. 직업별 시작 룬 + 연결 트리.
    /// </summary>
    public sealed class RuneDatat : DataTablet
    {
        /// <summary>직업 분류.</summary>
        public RuneClasst ClassId;

        /// <summary>시작 룬 여부(직업 최초 노드 1개).</summary>
        public bool IsStarter;

        /// <summary>연결되어 있는 다음 노드 ID 목록.</summary>
        public List<int> NextNodeIds;

        /// <summary>해금 비용(룬 포인트, 기본 1).</summary>
        public int PointCost;

        /// <summary>효과 설명 키(데미지 % 증가/방어 +N/회복 등 다양한 모디파이어).</summary>
        public RuneEffectt Effect;

        /// <inheritdoc/>
        public override void Parse(string[] cells)
        {
            // 동작 요약: 필드 파싱. NextNodeIds는 ';' 구분.
        }
    }

    /// <summary>직업 분류.</summary>
    public enum RuneClasst
    {
        /// <summary>딜러 — 단일 대상 강화 + 공속.</summary>
        Dealer,

        /// <summary>탱커 — 방어 + HP.</summary>
        Tanker,

        /// <summary>마법딜러 — 범위 스킬 다수.</summary>
        MagicDealer,

        /// <summary>지원가 — 회복 + 보조.</summary>
        Supporter,
    }

    /// <summary>룬 효과 모디파이어.</summary>
    public sealed class RuneEffectt
    {
        /// <summary>스탯 보정.</summary>
        public EquipmentStatModt StatMod;

        /// <summary>특정 스킬 ID 강화(0=없음).</summary>
        public int BoostSkillId;

        /// <summary>해당 스킬 데미지 +%.</summary>
        public float BoostSkillDamagePercent;

        /// <summary>패시브 효과 키(예: "OnHitChainLightning"). 데이터 주도.</summary>
        public string PassiveEffectKey;
    }
}
