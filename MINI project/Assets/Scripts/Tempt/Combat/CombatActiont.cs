using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 전투 행동 1건. 행위자, 타입, 타겟, 사용 스킬, 행동 시간 등 1회 실행 단위.
    /// </summary>
    public sealed class CombatActiont
    {
        /// <summary>행위자.</summary>
        public EntityBaset Actor;

        /// <summary>행동 타입.</summary>
        public CombatActionTypet Type;

        /// <summary>사용 스킬(Type=Skill일 때).</summary>
        public Skillt Skill;

        /// <summary>소모 아이템 ID(Type=Item일 때).</summary>
        public int ConsumableSlotIndex;

        /// <summary>실제 대상 목록(타겟팅 후 확정).</summary>
        public List<EntityBaset> Targets;

        /// <summary>이 행동의 누적 실행 시간(초). ActionTimingt가 계산.</summary>
        public float DurationSec;

        /// <summary>턴을 소비하는가. Item은 false, 그 외는 true.</summary>
        public bool ConsumesTurn;
    }

    /// <summary>전투 행동 타입.</summary>
    public enum CombatActionTypet
    {
        /// <summary>공격.</summary>
        Attack,

        /// <summary>스킬.</summary>
        Skill,

        /// <summary>방어(라운드 끝까지 유지).</summary>
        Defend,

        /// <summary>소모 아이템 사용(턴 미소모).</summary>
        Item,
    }
}
