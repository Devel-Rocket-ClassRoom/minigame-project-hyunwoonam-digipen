namespace Tempt
{
    /// <summary>
    /// 행동의 실행 시간을 합산하는 유틸. 기본 0.1초 + 애니메이션 + 이펙트 + 데이터 명시 시간.
    /// </summary>
    public static class ActionTimingt
    {
        /// <summary>최소 기본 시간(초).</summary>
        public const float MinBaseSec = 0.1f;

        /// <summary>
        /// CombatActiont에 사용할 총 실행 시간 계산.
        /// </summary>
        public static float Compute(CombatActiont action)
        {
            // 동작 요약:
            // - total = MinBaseSec.
            // - action.Type별:
            //   * Attack → +기본 공격 애니메이션 길이(0.3 등).
            //   * Skill → + Skill.Data.ActionDuration(설정값) 또는 애니/이펙트 길이.
            //   * Defend → +0.1(즉시).
            //   * Item → +0.2 정도(애니 짧음).
            // - 합산 후 반환. 데이터에 명시값 있으면 우선.
            //TODO: float total = MinBaseSec;
            //TODO: switch (action.Type)
            //TODO: {
            //TODO:     case CombatActionTypet.Attack:
            //TODO:         total += 0.3f;
            //TODO:         break;
            //TODO:     case CombatActionTypet.Skill:
            //TODO:         float dataDuration = (action.Skill != null && action.Skill.Data != null)
            //TODO:             ? action.Skill.Data.ActionDuration
            //TODO:             : 0f;
            //TODO:         total += dataDuration > 0f ? dataDuration : 0.5f;
            //TODO:         break;
            //TODO:     case CombatActionTypet.Defend:
            //TODO:         total += 0.1f;
            //TODO:         break;
            //TODO:     case CombatActionTypet.Item:
            //TODO:         total += 0.2f;
            //TODO:         break;
            //TODO: }
            //TODO: return total;
            if (action == null) //Wave0write
            { //Wave0write
                return MinBaseSec; //Wave0write
            } //Wave0write

            float total = MinBaseSec; //Wave0write
            switch (action.Type) //Wave0write
            { //Wave0write
                case CombatActionTypet.Attack: //Wave0write
                    total += 0.3f; //Wave0write
                    break; //Wave0write
                case CombatActionTypet.Skill: //Wave0write
                    total += action.Skill?.Data != null && action.Skill.Data.ActionDuration > 0f ? action.Skill.Data.ActionDuration : 0.5f; //Wave0write
                    break; //Wave0write
                case CombatActionTypet.Defend: //Wave0write
                    total += 0.1f; //Wave0write
                    break; //Wave0write
                case CombatActionTypet.Item: //Wave0write
                    total += 0.2f; //Wave0write
                    break; //Wave0write
            } //Wave0write

            return total; //Wave0write
        }
    }
}
