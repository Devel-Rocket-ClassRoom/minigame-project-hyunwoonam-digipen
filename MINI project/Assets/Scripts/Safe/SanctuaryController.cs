namespace Tempt
{
    using UnityEngine;

    /// <summary>
    /// Safe2 성소 컨트롤러. 침식을 활성화하고 선택 단계 정화를 처리한다.
    /// </summary>
    public sealed class SanctuaryController : SafeZoneControllerBase
    {
        [SerializeField]
        private SanctuaryUI sanctuaryUI;

        protected override void Awake()
        {
            base.Awake();
            SafeIndex = 2;
        }

        protected override void SetupZoneFeatures()
        {
            SafeIndex = 2;
            if (GameSystemManager.Instance.CurrentRun?.IsClearedRun != true)
            {
                GameSystemManager.Instance.Erosion?.Activate();
            }
            sanctuaryUI?.Refresh();
        }

        public int GetPurifyCost(int stageIndex)
        {
            if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                return 0;
            }

            BalanceData balance = gsm.Data?.Balance;
            if (balance == null)
            {
                GameLog.LogError("[SanctuaryController] BalanceData 참조가 없습니다.");
                return 0;
            }

            // 설계 §3.6: 단계별 인플레이션 가중. PurifyCost(n) = Ceil(ErosionAltarCost * (1 + InflationCoef * (n - 1)))
            float cost = balance.ErosionAltarCost * (1f + balance.InflationCoef * (stageIndex - 1));
            return Mathf.Max(1, Mathf.CeilToInt(cost));
        }

        public bool CanPurify(int stageIndex)
        {
            if (
                !GameSystemManager.TryGetInstance(out GameSystemManager gsm)
                || gsm.CurrentRun == null
            )
            {
                return false;
            }

            ErosionSystem erosion = gsm.Erosion;
            if (erosion?.Model == null || !erosion.Model.IsActivated)
            {
                return false;
            }

            if (stageIndex < 1 || stageIndex > erosion.MaxStage)
            {
                return false;
            }

            if (erosion.IsStageFullyEroded(stageIndex) || erosion.Model.GetRate(stageIndex) <= 0f)
            {
                return false;
            }

            return gsm.CurrentRun.Gold >= GetPurifyCost(stageIndex);
        }

        public bool TryPurify(int stageIndex)
        {
            if (!CanPurify(stageIndex))
            {
                return false;
            }

            GameSystemManager gsm = GameSystemManager.Instance;
            int cost = GetPurifyCost(stageIndex);
            gsm.CurrentRun.Gold -= cost;
            gsm.Events?.RaiseGoldChanged(gsm.CurrentRun.Gold);
            gsm.Erosion.Reduce(stageIndex, gsm.Data.Balance.ErosionAltarReduction);
            gsm.Save?.SaveSnapshot();
            sanctuaryUI?.Refresh();
            return true;
        }
    }
}
