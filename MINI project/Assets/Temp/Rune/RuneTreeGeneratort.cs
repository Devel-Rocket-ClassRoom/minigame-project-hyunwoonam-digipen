using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 동료의 고정 룬 트리 생성기. 동료의 RuneNodePool에서 시드 기반 무작위 순서 결정.
    /// </summary>
    public static class RuneTreeGeneratort
    {
        /// <summary>
        /// 데이터 + 시드로 고정 트리 순서를 만든다.
        /// </summary>
        public static CompanionRuneStatet GenerateFixedTree(CompanionDatat data, int seed)
        {
            // 동작 요약:
            // - new Random(seed).
            // - 데이터의 RuneNodePool을 셔플 후 RuneTreeLength만큼 잘라 FixedSequence 구성.
            // - 시작 룬은 FixedSequence[0]으로 강제.
            // - CompanionRuneStatet 생성 후 반환.
            //TODO: var rng = new System.Random(seed);
            //TODO: var pool = new List<int>(data.RuneNodePool);
            //TODO: // Fisher-Yates 셔플
            //TODO: for (int i = pool.Count - 1; i > 0; i--)
            //TODO: {
            //TODO:     int j = rng.Next(0, i + 1);
            //TODO:     int tmp = pool[i]; pool[i] = pool[j]; pool[j] = tmp;
            //TODO: }
            //TODO: // 길이 제한 + 시작 룬(Starter)을 0번 인덱스로 확보
            //TODO: int length = System.Math.Min(data.RuneTreeLength, pool.Count);
            //TODO: var sequence = pool.GetRange(0, length);
            //TODO: // 시작 룬(RequiredRuneId==0 노드 ID)을 앞으로 이동
            //TODO: var allRunes = GameSystemManagert.Instance.Data.Runes;
            //TODO: int starterIdx = sequence.FindIndex(id => allRunes[id].RequiredRuneId == 0);
            //TODO: if (starterIdx > 0) { var s = sequence[0]; sequence[0] = sequence[starterIdx]; sequence[starterIdx] = s; }
            //TODO: var runeTree = RuneTreet.BuildFromData(data.ClassId, allRunes.Values);
            //TODO: return new CompanionRuneStatet { ClassId = data.ClassId, Tree = runeTree, FixedSequence = sequence, UnlockedCount = 0 };
            return null;
        }

        /// <summary>
        /// 동료 모집 시 사용할 시드 생성(런 시드 + 동료 ID).
        /// </summary>
        public static int MakeSeed(int runSeed, int companionId)
        {
            // 동작 요약: 둘을 해시 결합.
            //TODO: return runSeed ^ (companionId * 397); // XOR + prime 결합
            return 0;
        }
    }
}
