using System.Collections.Generic;

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
        if (weights == null || weights.Count == 0)
        {
            return -1;
        }

        int total = 0;
        foreach (int weight in weights)
        {
            total += System.Math.Max(0, weight);
        }

        if (total <= 0)
        {
            return 0;
        }

        int roll = UnityEngine.Random.Range(0, total);
        int cumulative = 0;
        for (int i = 0; i < weights.Count; i++)
        {
            cumulative += System.Math.Max(0, weights[i]);
            if (roll < cumulative)
            {
                return i;
            }
        }

        return weights.Count - 1;
    }

    /// <summary>
    /// 후보 객체와 가중치를 받아 객체 1개 선택.
    /// </summary>
    public static T Pick<T>(IList<T> items, IList<int> weights)
    {
        // 동작 요약: PickIndex(weights) → items[index] 반환.
        int index = PickIndex(weights);
        return items != null && index >= 0 && index < items.Count ? items[index] : default;
    }

    /// <summary>
    /// SPD 점수 보정용. ±range 범위 정수 1개 샘플.
    /// </summary>
    public static int Sample(int minInclusive, int maxInclusive)
    {
        // 동작 요약: UnityEngine.Random.Range(minInclusive, maxInclusive+1) 반환.
        if (maxInclusive < minInclusive)
        {
            int tmp = minInclusive;
            minInclusive = maxInclusive;
            maxInclusive = tmp;
        }

        return UnityEngine.Random.Range(minInclusive, maxInclusive + 1);
    }
}

