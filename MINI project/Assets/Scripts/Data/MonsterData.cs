using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 몬스터 정적 데이터. MonsterStatusTable.csv 1행.
    /// </summary>
    public sealed class MonsterData : DataTable
    {
        /// <summary>주 출현 단계. 0이면 난이도 기반 전역 풀.</summary>
        public int StageIndex;

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
        public ActionWeightTable ActionWeights;

        /// <summary>프리팹 키.</summary>
        public string PrefabKey;

        /// <summary>침식 변이 셰이더 키.</summary>
        public string ErosionShaderKey;

        /// <summary>기본 공격 이펙트 키(Resources/Effects/{key}). 빈값이면 공용 "basicattack".</summary>
        public string AttackEffectKey;

        /// <summary>기본 공격 SPUM ATTACK 클립 인덱스. 빈값이면 0.</summary>
        public int AttackAnimIndex;

        /// <summary>기본 공격 효과음 키(Resources/Sfx/{key}). 빈값이면 무음.</summary>
        public string AttackSfxKey;

        /// <inheritdoc/>
        public override void Parse(string[] cells)
        {
            Id = cells.Length > 0 && CsvParser.TryParseInt(cells[0], out int id) ? id : 0;
            NameKey = cells.Length > 1 ? cells[1] : string.Empty;
            IsBoss = cells.Length > 2 && CsvParser.TryParseBool(cells[2], out bool isBoss) && isBoss;
            Difficulty = cells.Length > 3 && CsvParser.TryParseInt(cells[3], out int difficulty) ? difficulty : 0;
            MaxHP = cells.Length > 4 && CsvParser.TryParseInt(cells[4], out int hp) ? hp : 0;
            MaxMP = cells.Length > 5 && CsvParser.TryParseInt(cells[5], out int mp) ? mp : 0;
            ATK = cells.Length > 6 && CsvParser.TryParseInt(cells[6], out int atk) ? atk : 0;
            DEF = cells.Length > 7 && CsvParser.TryParseInt(cells[7], out int def) ? def : 0;
            SPD = cells.Length > 8 && CsvParser.TryParseInt(cells[8], out int spd) ? spd : 0;
        }

        public static MonsterData FromRow(IDictionary<string, string> row)
        {
            if (!CsvParser.HasColumns(row, nameof(MonsterData), "Id", "NameKey", "IsBoss", "Difficulty", "MaxHP", "MaxMP", "ATK", "DEF", "SPD", "RewardExp", "RewardGold", "DropTableId"))
            {
                return null;
            }

            return new MonsterData
            {
                Id = CsvParser.GetInt(row, "Id"),
                NameKey = CsvParser.GetString(row, "NameKey"),
                DescKey = CsvParser.GetString(row, "DescKey"),
                StageIndex = CsvParser.GetInt(row, "StageIndex"),
                IsBoss = CsvParser.GetBool(row, "IsBoss"),
                Difficulty = CsvParser.GetInt(row, "Difficulty"),
                MaxHP = CsvParser.GetInt(row, "MaxHP"),
                MaxMP = CsvParser.GetInt(row, "MaxMP"),
                ATK = CsvParser.GetInt(row, "ATK"),
                DEF = CsvParser.GetInt(row, "DEF"),
                SPD = CsvParser.GetInt(row, "SPD"),
                RewardExp = CsvParser.GetInt(row, "RewardExp"),
                RewardGold = CsvParser.GetInt(row, "RewardGold"),
                DropTableId = CsvParser.GetInt(row, "DropTableId"),
                SkillIds = CsvParser.GetIntList(row, "SkillIds"),
                ActionWeights = new ActionWeightTable
                {
                    Attack = CsvParser.GetInt(row, "ActionWeight_Attack", 80),
                    Skill = CsvParser.GetInt(row, "ActionWeight_Skill", 10),
                    Defend = CsvParser.GetInt(row, "ActionWeight_Defend", 10),
                },
                PrefabKey = CsvParser.GetString(row, "PrefabKey"),
                ErosionShaderKey = CsvParser.GetString(row, "ErosionShaderKey"),
                AttackEffectKey = CsvParser.GetString(row, "AttackEffectKey"),
                AttackAnimIndex = CsvParser.GetInt(row, "AttackAnimIndex"),
                AttackSfxKey = CsvParser.GetString(row, "AttackSfxKey"),
            };
        }
    }

    /// <summary>행동 가중치 표.</summary>
    public sealed class ActionWeightTable
    {
        /// <summary>공격 가중치(기본 70 권장).</summary>
        public int Attack;

        /// <summary>스킬 가중치(기본 20).</summary>
        public int Skill;

        /// <summary>방어 가중치(기본 10).</summary>
        public int Defend;

        /// <summary>데이터 누락 시 사용하는 기본 가중치. 호출마다 새 인스턴스 반환(공유 변이 방지).</summary>
        public static ActionWeightTable Default =>
            new ActionWeightTable { Attack = 80, Skill = 0, Defend = 20 };
    }
}

