namespace Tempt
{
    /// <summary>
    /// 성소. 단계 침식률을 마석으로 차감. 100%까지 차감은 불가.
    /// </summary>
    public sealed class ErosionAltart
    {
        /// <summary>
        /// 성소 UI 열기.
        /// </summary>
        public void Open()
        {
            // 동작 요약:
            // - 단계 침식률 1~6 표시.
            // - 각 단계 차감 비용(BalanceDatat.ErosionAltarCost), 차감량 표시.
            //TODO: var erosion = GameSystemManagert.Instance.CurrentRun.Erosion;
            //TODO: AltarUI.Show();
            //TODO: for (int i = 1; i <= 6; i++)
            //TODO: {
            //TODO:     float rate = erosion.Model.GetRate(i);
            //TODO:     bool canReduce = rate < 100f;
            //TODO:     int cost = BalanceDatat.ErosionAltarCost[i - 1];
            //TODO:     float reduction = BalanceDatat.ErosionAltarReduction;
            //TODO:     AltarUI.SetStageRow(i, rate, cost, reduction, canReduce, stageIdx: i);
            //TODO: }
            //TODO: AltarUI.OnReduceClicked = TryReduceErosion;
        }

        /// <summary>
        /// 단계 침식률 차감.
        /// </summary>
        public bool TryReduceErosion(int stageIndex)
        {
            // 동작 요약:
            // - 마석 >= cost 검사.
            // - 현재 침식률 < 100 검사.
            // - 차감 후 ErosionSystemt.Reduce(stageIndex, BalanceDatat.ErosionAltarReduction).
            // - 차감 후 0 미만 클램프.
            //TODO: var run = GameSystemManagert.Instance.CurrentRun;
            //TODO: int cost = BalanceDatat.ErosionAltarCost[stageIndex - 1];
            //TODO: if (run.ManaStone < cost) return false;
            //TODO: if (run.Erosion.Model.GetRate(stageIndex) >= 100f) return false;
            //TODO: run.ManaStone -= cost;
            //TODO: run.Erosion.Reduce(stageIndex, BalanceDatat.ErosionAltarReduction);
            //TODO: return true;
            return false;
        }
    }
}
