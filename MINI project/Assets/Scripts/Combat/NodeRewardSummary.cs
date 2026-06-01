using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 노드 전투 종료 후 집계된 총 보상.
    /// 각 몬스터의 <see cref="NodeRewardContribution"/>를 <see cref="Aggregate"/>로 합산해 생성한다.
    /// </summary>
    public sealed class NodeRewardSummary
    {
        /// <summary>총 지급 EXP.</summary>
        public int TotalExp;

        /// <summary>총 드랍 골드.</summary>
        public int TotalGold;

        /// <summary>
        /// 드랍된 아이템 ID 목록(중복 허용, 수량 반영).
        /// 예: ItemId=5 짜리 2개 → [5, 5].
        /// </summary>
        public List<int> DroppedItemIds = new List<int>();

        /// <summary>
        /// 여러 몬스터의 보상 기여를 집계해 NodeRewardSummary 반환.
        /// CombatController.CollectNodeRewards()가 호출.
        /// </summary>
        public static NodeRewardSummary Aggregate(IEnumerable<NodeRewardContribution> contributions)
        {
            // 동작 요약:
            // - result = new NodeRewardSummary().
            // - contributions가 null이면 result 즉시 반환.
            // - 각 contribution에 대해:
            //     result.TotalExp  += c.Exp.
            //     result.TotalGold += c.Gold.
            //     c.DroppedItemIds != null → result.DroppedItemIds.AddRange(c.DroppedItemIds).
            // - result 반환.
            var result = new NodeRewardSummary();
            if (contributions == null)
            {
                return result;
            }

            foreach (NodeRewardContribution contribution in contributions)
            {
                if (contribution == null)
                {
                    continue;
                }

                result.TotalExp += contribution.Exp;
                result.TotalGold += contribution.Gold;
                if (contribution.DroppedItemIds != null)
                {
                    result.DroppedItemIds.AddRange(contribution.DroppedItemIds);
                }
            }

            return result;
        }
    }
}

