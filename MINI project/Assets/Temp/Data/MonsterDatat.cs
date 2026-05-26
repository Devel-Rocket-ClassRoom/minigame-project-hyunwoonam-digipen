using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 몬스터 정적 데이터. MonsterStatusTable.csv 1행.
    /// </summary>
    public sealed class MonsterDatat : DataTablet
    {
        /// <summary>보스 여부.</summary>
        public bool IsBoss;

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
        public int RewardExp;

        /// <summary>처치 시 지급 골드.</summary>
        public int RewardGold;

        /// <summary>
        /// 드랍 테이블 그룹 ID. DropTable.csv의 DropTableId와 매핑.
        /// 몬스터는 이 ID를 기준 드랍 구성으로 사용하며, 구체 몬스터 코드에서
        /// 필요하면 DropTableId 또는 ResolveDrops 이후 결과를 재정의/보정할 수 있다.
        /// 0이면 드랍 없음.
        /// </summary>
        public int DropTableId;

        /// <summary>보유 스킬 ID 목록(MonsterOnly 또는 Default 스킬).</summary>
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
            // 동작 요약: MonsterStatusTable.csv 열 순서대로 파싱.
            // - cells[0] = MonsterID → Id
            // - cells[1] = NameKey
            // - cells[2] = IsBoss (bool)
            // - cells[3] = Difficulty (int)
            // - cells[4] = BaseHP → MaxHP
            // - cells[5] = BaseMP → MaxMP
            // - cells[6] = BaseATK → ATK
            // - cells[7] = BaseDEF → DEF
            // - cells[8] = BaseSPD → SPD
            // - cells[9] = RewardExp (int)
            // - cells[10] = RewardGold (int)
            // - cells[11] = DropTableId (int, DropTable.csv 그룹 ID. 0이면 드랍 없음)
            // - cells[12] = SkillIds (';' 구분 int 목록)
            // - cells[13~15] = ActionWeights (Attack/Skill/Defend 가중치)
            // - cells[16] = PrefabKey
            // - cells[17] = ErosionShaderKey
            //TODO: Id         = int.Parse(cells[0]);
            //TODO: NameKey    = cells[1];
            //TODO: IsBoss     = bool.Parse(cells[2]);
            //TODO: Difficulty = int.Parse(cells[3]);
            //TODO: MaxHP      = int.Parse(cells[4]);
            //TODO: MaxMP      = int.Parse(cells[5]);
            //TODO: ATK        = int.Parse(cells[6]);
            //TODO: DEF        = int.Parse(cells[7]);
            //TODO: SPD        = int.Parse(cells[8]);
            //TODO: RewardExp  = int.Parse(cells[9]);
            //TODO: RewardGold = int.Parse(cells[10]);
            //TODO: DropTableId = int.Parse(cells[11]);
            //TODO: SkillIds = new List<int>();
            //TODO: if (!string.IsNullOrEmpty(cells[12]))
            //TODO:     foreach (var s in cells[12].Split(';')) SkillIds.Add(int.Parse(s.Trim()));
            //TODO: ActionWeights = new ActionWeightTablet { Attack = int.Parse(cells[13]), Skill = int.Parse(cells[14]), Defend = int.Parse(cells[15]) };
            //TODO: PrefabKey        = cells[16];
            //TODO: ErosionShaderKey = cells[17];
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
