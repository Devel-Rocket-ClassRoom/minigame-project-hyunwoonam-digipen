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
            //TODO: var result = new NodeRewardSummary();
            //TODO: if (contributions == null) return result;
            //TODO: foreach (var c in contributions)
            //TODO: {
            //TODO:     result.TotalExp  += c.Exp;
            //TODO:     result.TotalGold += c.Gold;
            //TODO:     if (c.DroppedItemIds != null) result.DroppedItemIds.AddRange(c.DroppedItemIds);
            //TODO: }
            //TODO: return result;
            var result = new NodeRewardSummary(); //Wave0write
            if (contributions == null) //Wave0write
            { //Wave0write
                return result; //Wave0write
            } //Wave0write

            foreach (NodeRewardContribution contribution in contributions) //Wave0write
            { //Wave0write
                if (contribution == null) //Wave0write
                { //Wave0write
                    continue; //Wave0write
                } //Wave0write

                result.TotalExp += contribution.Exp; //Wave0write
                result.TotalGold += contribution.Gold; //Wave0write
                if (contribution.DroppedItemIds != null) //Wave0write
                { //Wave0write
                    result.DroppedItemIds.AddRange(contribution.DroppedItemIds); //Wave0write
                } //Wave0write
            } //Wave0write

            return result; //Wave0write
        }
    }
}

