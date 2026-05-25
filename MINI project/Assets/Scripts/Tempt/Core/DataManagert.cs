using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 정적 데이터(CSV/JSON) 일괄 로드 및 조회. 런타임에 데이터를 수정하지 않는다.
    /// </summary>
    public sealed class DataManagert
    {
        /// <summary>몬스터 테이블.</summary>
        public IReadOnlyDictionary<int, MonsterDatat> Monsters => monsters;

        /// <summary>스킬 테이블.</summary>
        public IReadOnlyDictionary<int, SkillDatat> Skills => skills;

        /// <summary>아이템 테이블.</summary>
        public IReadOnlyDictionary<int, ItemDatat> Items => items;

        /// <summary>룬 테이블.</summary>
        public IReadOnlyDictionary<int, RuneDatat> Runes => runes;

        /// <summary>월드/노드/단계 설정.</summary>
        public WorldDatat World => world;

        /// <summary>동료 모집/기본 데이터.</summary>
        public IReadOnlyDictionary<int, CompanionDatat> Companions => companions;

        /// <summary>밸런스 곡선(EXP, 침식, 가격 인플레이션).</summary>
        public BalanceDatat Balance => balance;

        /// <summary>언어 리소스.</summary>
        public LanguageDatat Language => language;

        private Dictionary<int, MonsterDatat> monsters;
        private Dictionary<int, SkillDatat> skills;
        private Dictionary<int, ItemDatat> items;
        private Dictionary<int, RuneDatat> runes;
        private WorldDatat world;
        private Dictionary<int, CompanionDatat> companions;
        private BalanceDatat balance;
        private LanguageDatat language;

        /// <summary>
        /// 모든 데이터 파일을 일괄 로드.
        /// </summary>
        public void LoadAll()
        {
            // 동작 요약:
            // - LoadCsv("Monsters.csv") → monsters.
            // - LoadCsv("Skills.csv") → skills.
            // - LoadCsv("Items.csv") → items.
            // - LoadCsv("Runes.csv") → runes.
            // - LoadJson("World.json") → world.
            // - LoadCsv("Companions.csv") → companions.
            // - LoadJson("Balance.json") → balance.
            // - LoadCsv("Language.csv") → language.
            // - 필수 필드 검증, 실패 시 로그 + 예외.
        }

        /// <summary>
        /// 특정 난이도 등급의 몬스터 풀에서 1~3마리 무작위 선택.
        /// </summary>
        /// <param name="difficulty">노드 난이도.</param>
        /// <param name="count">선택할 마리 수(1~3).</param>
        /// <returns>선택된 몬스터 ID 리스트.</returns>
        public IList<int> PickMonsterGroup(int difficulty, int count)
        {
            // 동작 요약:
            // - monsters에서 difficulty 일치 항목 필터링.
            // - 가중치(WorldDatat.MonsterPoolWeights) 기반으로 count개 선택.
            return null;
        }

        /// <summary>
        /// 단계의 가격 인플레이션 배수 계산.
        /// </summary>
        /// <param name="stageIndex">단계(1~6).</param>
        /// <param name="erosionRate">해당 단계 침식률(0~100).</param>
        public float ComputeInflation(int stageIndex, float erosionRate)
        {
            // 동작 요약:
            // - balance.InflationCoef 사용해 price = base * (1 + erosionRate * coef).
            return 1f;
        }
    }
}
