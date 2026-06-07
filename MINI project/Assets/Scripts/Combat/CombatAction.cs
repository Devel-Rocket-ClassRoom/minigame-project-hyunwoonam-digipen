using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 전투 행동 1건. 행위자, 타입, 타겟, 사용 스킬, 행동 시간 등 1회 실행 단위.
    /// </summary>
    public sealed class CombatAction
    {
        /// <summary>행위자.</summary>
        public EntityBase Actor;

        /// <summary>행동 타입.</summary>
        public CombatActionType Type;

        /// <summary>사용 스킬(Type=Skill일 때).</summary>
        public Skill Skill;

        /// <summary>룬 보정이 적용된 스킬 데이터. null이면 Skill.Data 사용.</summary>
        public SkillData EffectiveSkillData;

        /// <summary>현재 행동에 사용할 스킬 데이터.</summary>
        public SkillData ResolvedSkillData => EffectiveSkillData ?? Skill?.Data;

        /// <summary>전투 UI 소모품 슬롯 인덱스. 플레이어 아이템 사용 경로에서만 사용한다.</summary>
        public int ConsumableSlotIndex;

        /// <summary>실제 대상 목록(타겟팅 후 확정).</summary>
        public List<EntityBase> Targets;

        /// <summary>이 행동의 누적 실행 시간(초). ActionTimingt가 계산.</summary>
        public float DurationSec;

        /// <summary>턴을 소비하는가.</summary>
        public bool ConsumesTurn;
    }

    /// <summary>전투 행동 타입.</summary>
    public enum CombatActionType
    {
        /// <summary>공격.</summary>
        Attack,

        /// <summary>스킬.</summary>
        Skill,

        /// <summary>방어(라운드 끝까지 유지).</summary>
        Defend,
    }
}

