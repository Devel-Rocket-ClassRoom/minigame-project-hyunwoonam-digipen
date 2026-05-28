using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 가중치 무작위 선택 유틸. 정수/실수 가중치 모두 지원.
    /// </summary>
    public static class WeightedRandom
    {
        /// <summary>
        /// 가중치 배열에서 인덱스 1개 선택.
        /// </summary>
        public static int PickIndex(IList<int> weights)
        {
            // 동작 요약:
            // - 합산 후 [0, sum) 무작위.
            // - 누적합 비교로 인덱스 결정.
            //TODO: int total = 0;
            //TODO: foreach (int w in weights) total += w;
            //TODO: int roll = UnityEngine.Random.Range(0, total);
            //TODO: int cumulative = 0;
            //TODO: for (int i = 0; i < weights.Count; i++)
            //TODO: {
            //TODO:     cumulative += weights[i];
            //TODO:     if (roll < cumulative) return i;
            //TODO: }
            //TODO: return weights.Count - 1; // fallback
            if (weights == null || weights.Count == 0) //Wave0write
            { //Wave0write
                return -1; //Wave0write
            } //Wave0write

            int total = 0; //Wave0write
            foreach (int weight in weights) //Wave0write
            { //Wave0write
                total += System.Math.Max(0, weight); //Wave0write
            } //Wave0write

            if (total <= 0) //Wave0write
            { //Wave0write
                return 0; //Wave0write
            } //Wave0write

            int roll = UnityEngine.Random.Range(0, total); //Wave0write
            int cumulative = 0; //Wave0write
            for (int i = 0; i < weights.Count; i++) //Wave0write
            { //Wave0write
                cumulative += System.Math.Max(0, weights[i]); //Wave0write
                if (roll < cumulative) //Wave0write
                { //Wave0write
                    return i; //Wave0write
                } //Wave0write
            } //Wave0write

            return weights.Count - 1; //Wave0write
        }

        /// <summary>
        /// 후보 객체와 가중치를 받아 객체 1개 선택.
        /// </summary>
        public static T Pick<T>(IList<T> items, IList<int> weights)
        {
            // 동작 요약: PickIndex(weights) → items[index] 반환.
            //TODO: return items[PickIndex(weights)];
            int index = PickIndex(weights); //Wave0write
            return items != null && index >= 0 && index < items.Count ? items[index] : default; //Wave0write
        }

        /// <summary>
        /// SPD 점수 보정용. ±range 범위 정수 1개 샘플.
        /// </summary>
        public static int Sample(int minInclusive, int maxInclusive)
        {
            // 동작 요약: UnityEngine.Random.Range(minInclusive, maxInclusive+1) 반환.
            //TODO: return UnityEngine.Random.Range(minInclusive, maxInclusive + 1);
            if (maxInclusive < minInclusive) //Wave0write
            { //Wave0write
                int tmp = minInclusive; //Wave0write
                minInclusive = maxInclusive; //Wave0write
                maxInclusive = tmp; //Wave0write
            } //Wave0write

            return UnityEngine.Random.Range(minInclusive, maxInclusive + 1); //Wave0write
        }
    }
}

