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
            return null;
        }

        /// <summary>
        /// 동료 모집 시 사용할 시드 생성(런 시드 + 동료 ID).
        /// </summary>
        public static int MakeSeed(int runSeed, int companionId)
        {
            // 동작 요약: 둘을 해시 결합.
            return 0;
        }
    }
}
