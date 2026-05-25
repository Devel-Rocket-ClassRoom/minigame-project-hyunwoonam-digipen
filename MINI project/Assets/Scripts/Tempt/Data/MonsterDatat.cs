using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 몬스터 정적 데이터. CSV 1행.
    /// </summary>
    public sealed class MonsterDatat : DataTablet
    {
        /// <summary>난이도(노드 풀 분류).</summary>
        public int Difficulty;

        /// <summary>최대 HP.</summary>
        public int MaxHP;

        /// <summary>최대 MP.</summary>
        public int MaxMP;

        /// <summary>공격력.</summary>
        public int ATK;

        /// <summary>방어력.</summary>
        public int DEF;

        /// <summary>공격속도.</summary>
        public int SPD;

        /// <summary>처치 시 지급 EXP.</summary>
        public int ExpReward;

        /// <summary>드랍 골드 평균.</summary>
        public int GoldDropAvg;

        /// <summary>보유 스킬 ID 목록.</summary>
        public List<int> SkillIds;

        /// <summary>행동 가중치(공격/스킬/방어 표).</summary>
        public ActionWeightTablet ActionWeights;

        /// <summary>프리팹 키.</summary>
        public string PrefabKey;

        /// <summary>침식 변이 셰이더 키.</summary>
        public string ErosionShaderKey;

        /// <inheritdoc/>
        public override void Parse(string[] cells)
        {
            // 동작 요약:
            // - Id, NameKey, DescKey 채우기.
            // - 수치 필드 정수 파싱.
            // - SkillIds는 ';' 구분.
            // - ActionWeights는 별도 컬럼에서 파싱.
        }
    }

    /// <summary>행동 가중치 표.</summary>
    public sealed class ActionWeightTablet
    {
        /// <summary>공격 가중치(기본 70 권장).</summary>
        public int Attack;

        /// <summary>스킬 가중치(기본 20).</summary>
        public int Skill;

        /// <summary>방어 가중치(기본 10).</summary>
        public int Defend;
    }
}
