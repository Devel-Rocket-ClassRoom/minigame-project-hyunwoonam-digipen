using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 월드 전체 구조 정의. 단계 / 안전지대 / 노드 생성 규칙.
    /// </summary>
    public sealed class WorldData
    {
        /// <summary>단계 정의(1~6).</summary>
        public List<StageDef> Stages;

        /// <summary>안전지대 정의(0~5).</summary>
        public List<SafeZoneDef> SafeZones;

        /// <summary>플로어 맵 생성 규칙.</summary>
        public FloorGenRule FloorGen;

        /// <summary>몬스터 풀 가중치 옵션.</summary>
        public List<int> MonsterPoolWeights;
    }

    /// <summary>한 단계 정의.</summary>
    public sealed class StageDef
    {
        /// <summary>단계 번호(1~6).</summary>
        public int StageIndex;

        /// <summary>이 단계에 속한 일반 층 범위(start~end).</summary>
        public int FloorStart;

        /// <summary>일반 층 끝.</summary>
        public int FloorEnd;

        /// <summary>단계 보스 노드가 위치한 층(설계상 마지막 일반 층의 마지막).</summary>
        public int BossFloor;

        /// <summary>이 단계 클리어 시 해금되는 안전지대 인덱스(설계: 안전지대 N).</summary>
        public int UnlocksSafeZoneIndex;

        /// <summary>이 단계의 난이도 범위(min, max).</summary>
        public int DifficultyMin;

        /// <summary>난이도 max.</summary>
        public int DifficultyMax;
    }

    /// <summary>안전지대 정의.</summary>
    public sealed class SafeZoneDef
    {
        /// <summary>인덱스(0~5).</summary>
        public int Index;

        /// <summary>층 번호(4, 12, 20, 30, 40 등 사용자 확정).</summary>
        public int FloorNumber;

        /// <summary>이 안전지대가 제공하는 기능 키들(Inn/Shop/Guild/Forge/Temple/ErosionAltar/Mine/Locker 등).</summary>
        public List<string> FeatureKeys;

        /// <summary>BGM 키.</summary>
        public string BgmKey;

        /// <summary>배경 색조 셰이더 키.</summary>
        public string TintShaderKey;
    }

    /// <summary>플로어 맵 생성 규칙.</summary>
    public sealed class FloorGenRule
    {
        /// <summary>최대 층(설계 49층).</summary>
        public int MaxFloor;

        /// <summary>각 층 최소 노드 수.</summary>
        public int MinNodesPerFloor;

        /// <summary>각 층 최대 노드 수.</summary>
        public int MaxNodesPerFloor;

        /// <summary>튜토리얼 단계의 노드 수 고정 규칙(1층=1, 2층=2, 3층=1보스).</summary>
        public List<int> TutorialNodeCounts;

        /// <summary>한 노드 몬스터 수 범위.</summary>
        public int MonstersMin;

        /// <summary>한 노드 몬스터 수 최대.</summary>
        public int MonstersMax;
    }
}

