using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 룬 정적 데이터. RuneTable.csv 1행.
    /// 플레이어는 포인트로 자유롭게 해금, 동료는 시드 기반 고정 트리를 레벨업마다 순서대로 자동 해금.
    /// </summary>
    public sealed class RuneDatat : DataTablet
    {
        /// <summary>룬 타입(메인 노드 또는 보조 파편).</summary>
        public RuneNodeTypet RuneType;

        /// <summary>직업 분류(MainNode일 때만 의미).</summary>
        public RuneClasst ClassId;

        /// <summary>
        /// 선행(부모) 노드 ID. 0이면 시작 노드.
        /// 이 노드를 해금하려면 RequiredRuneId 노드가 먼저 해금되어야 한다.
        /// </summary>
        public int RequiredRuneId;

        /// <summary>해금 비용(룬 포인트, 기본 1).</summary>
        public int PointCost;

        /// <summary>
        /// 이 노드가 부여하는 효과 종류.
        /// CharacterBaset.SyncPassivesFromRunes()가 UnlockSkill 타입을 감지하여
        /// EffectValue를 skillId로 변환한 뒤 Passive 스킬을 자동 등록한다.
        /// </summary>
        public RuneEffectTypet EffectType;

        /// <summary>
        /// 효과 수치 또는 해금 스킬 ID.
        /// EffectType == UnlockSkill 이면 이 값을 int로 캐스팅해 skillId로 사용.
        /// EffectType == AddMaxHP 등 스탯 계열이면 보정 수치.
        /// </summary>
        public float EffectValue;

        /// <inheritdoc/>
        public override void Parse(string[] cells)
        {
            // 동작 요약: RuneTable.csv 열 순서대로 파싱.
            // - cells[0] = RuneID → Id
            // - cells[1] = NameKey
            // - cells[2] = RuneType (enum 문자열 → RuneNodeTypet)
            // - cells[3] = RequiredRuneID → RequiredRuneId (int, 0이면 시작점)
            // - cells[4] = EffectType (enum 문자열 → RuneEffectTypet)
            // - cells[5] = EffectValue (float)
            // - cells[6] = DescriptionKey → DescKey
            // - ClassId는 시작 노드(RequiredRuneId == 0) 구분을 위해 별도 컬럼 또는 규칙으로 결정
            // - PointCost는 별도 컬럼 또는 기본값 1
            //TODO: Id             = int.Parse(cells[0]);
            //TODO: NameKey        = cells[1];
            //TODO: RuneType       = (RuneNodeTypet)System.Enum.Parse(typeof(RuneNodeTypet), cells[2]);
            //TODO: RequiredRuneId = int.Parse(cells[3]);
            //TODO: EffectType     = (RuneEffectTypet)System.Enum.Parse(typeof(RuneEffectTypet), cells[4]);
            //TODO: EffectValue    = float.Parse(cells[5]);
            //TODO: DescKey        = cells[6];
            //TODO: ClassId        = cells.Length > 7 ? (RuneClasst)System.Enum.Parse(typeof(RuneClasst), cells[7]) : RuneClasst.Dealer;
            //TODO: PointCost      = cells.Length > 8 ? int.Parse(cells[8]) : 1;
        }
    }

    /// <summary>룬 노드 타입.</summary>
    public enum RuneNodeTypet
    {
        /// <summary>메인 트리 노드(직업별 성장 트리의 한 칸).</summary>
        MainNode,

        /// <summary>보조 파편(추가 부착 가능한 소형 보너스).</summary>
        SubFragment,
    }

    /// <summary>룬 노드가 부여하는 효과 종류.</summary>
    public enum RuneEffectTypet
    {
        /// <summary>최대 HP 증가.</summary>
        AddMaxHP,

        /// <summary>최대 MP 증가.</summary>
        AddMaxMP,

        /// <summary>공격력 증가.</summary>
        AddATK,

        /// <summary>방어력 증가.</summary>
        AddDEF,

        /// <summary>공격속도 증가.</summary>
        AddSPD,

        /// <summary>
        /// 스킬 해금. EffectValue를 int로 캐스팅해 skillId로 사용.
        /// SkillTypet == Passive인 스킬이면 CharacterBaset.SyncPassivesFromRunes()가
        /// EntityBaset.PassiveSkills에 자동 등록한다.
        /// SkillTypet == Active인 스킬이면 별도 획득 처리(길드 Active 스킬 확장 등).
        /// </summary>
        UnlockSkill,

        /// <summary>데미지 배율 증가(EffectValue = %).</summary>
        DamageBoost,

        /// <summary>회복량 배율 증가(EffectValue = %).</summary>
        HealBoost,
    }

    /// <summary>직업 분류.</summary>
    public enum RuneClasst
    {
        /// <summary>아직 시작 직업 룬을 선택하지 않음.</summary>
        None, //Wave0write

        /// <summary>딜러 — 단일 대상 강화 + 공속.</summary>
        Dealer,

        /// <summary>탱커 — 방어 + HP.</summary>
        Tanker,

        /// <summary>마법딜러 — 범위 스킬 다수.</summary>
        MagicDealer,

        /// <summary>지원가 — 회복 + 보조.</summary>
        Supporter,
    }
}
