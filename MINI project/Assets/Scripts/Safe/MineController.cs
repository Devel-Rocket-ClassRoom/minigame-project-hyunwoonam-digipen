namespace Tempt
{
    using UnityEngine;

    /// <summary>
    /// Safe3~5 광산 컨트롤러. 골드 기반 활성화, 일일 적립, 수령을 담당한다.
    /// </summary>
    public sealed class MineController : SafeZoneControllerBase
    {
        [SerializeField]
        private MineUI mineUI;

        protected override void Awake()
        {
            base.Awake();
            SafeIndex = ResolveSafeIndexFromScene();
        }

        protected override void SetupZoneFeatures()
        {
            if (
                !GameSystemManager.TryGetInstance(out GameSystemManager gsm)
                || gsm.CurrentRun == null
            )
            {
                return;
            }

            EnsureMineState(gsm.CurrentRun);
            AccumulateDailyGain(gsm);
            mineUI?.Refresh();
        }

        public int MineIndex => SafeIndex - 3;

        public bool IsActivated
        {
            get
            {
                if (!TryGetRun(out GameRunState run))
                {
                    return false;
                }

                int index = MineIndex;
                return IsValidMineIndex(index) && run.MineActivated[index];
            }
        }

        public int GetStoredGold()
        {
            if (!TryGetRun(out GameRunState run))
            {
                return 0;
            }

            int index = MineIndex;
            return IsValidMineIndex(index) ? run.MineStored[index] : 0;
        }

        public int GetDailyGain()
        {
            if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                return 0;
            }

            int index = MineIndex;
            if (
                !IsValidMineIndex(index)
                || gsm.Data?.Balance?.MineDailyGain == null
                || index >= gsm.Data.Balance.MineDailyGain.Count
            )
            {
                return 0;
            }

            return System.Math.Max(0, gsm.Data.Balance.MineDailyGain[index]);
        }

        public int GetActivationCost()
        {
            if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                return 0;
            }

            BalanceData balance = gsm.Data?.Balance;
            if (balance == null)
            {
                Debug.LogError("[MineController] BalanceData 참조가 없습니다.");
                return 0;
            }

            // 설계 §5.4: 광산 인덱스별 인플레이션 가중. Cost(i) = Ceil(MineActivationCost * (1 + InflationCoef * i))
            int index = Mathf.Max(0, MineIndex);
            float cost = balance.MineActivationCost * (1f + balance.InflationCoef * index);
            return Mathf.Max(1, Mathf.CeilToInt(cost));
        }

        public bool CanActivate()
        {
            if (!TryGetRun(out GameRunState run))
            {
                return false;
            }

            int index = MineIndex;
            return IsValidMineIndex(index)
                && !run.MineActivated[index]
                && run.Gold >= GetActivationCost();
        }

        public bool TryActivateMine()
        {
            if (!CanActivate())
            {
                return false;
            }

            GameSystemManager gsm = GameSystemManager.Instance;
            int index = MineIndex;
            int cost = GetActivationCost();
            gsm.CurrentRun.Gold -= cost;
            gsm.CurrentRun.MineActivated[index] = true;
            gsm.Events?.RaiseGoldChanged(gsm.CurrentRun.Gold);
            AccumulateDailyGain(gsm);
            gsm.Save?.SaveSnapshot();
            mineUI?.Refresh();
            return true;
        }

        public bool TryCollectStoredGold()
        {
            if (!TryGetRun(out GameRunState run))
            {
                return false;
            }

            int index = MineIndex;
            if (!IsValidMineIndex(index) || run.MineStored[index] <= 0)
            {
                return false;
            }

            int amount = run.MineStored[index];
            run.Gold += amount;
            run.MineStored[index] = 0;
            GameSystemManager gsm = GameSystemManager.Instance;
            gsm.Events?.RaiseGoldChanged(run.Gold);
            gsm.Save?.SaveSnapshot();
            mineUI?.Refresh();
            return true;
        }

        private void AccumulateDailyGain(GameSystemManager gsm)
        {
            if (gsm?.CurrentRun == null)
            {
                return;
            }

            GameRunState run = gsm.CurrentRun;
            int index = MineIndex;
            if (!IsValidMineIndex(index))
            {
                return;
            }

            EnsureMineState(run);
            if (!run.MineActivated[index] || run.LastMineGainDay[index] >= run.CurrentDay)
            {
                return;
            }

            run.MineStored[index] += GetDailyGain();
            run.LastMineGainDay[index] = run.CurrentDay;
            gsm.Save?.SaveSnapshot();
        }

        private static void EnsureMineState(GameRunState run)
        {
            run?.EnsureMineState();
        }

        private static bool IsValidMineIndex(int index)
        {
            return index >= 0 && index < 3;
        }

        private static bool TryGetRun(out GameRunState run)
        {
            run = null;
            if (
                !GameSystemManager.TryGetInstance(out GameSystemManager gsm)
                || gsm.CurrentRun == null
            )
            {
                return false;
            }

            run = gsm.CurrentRun;
            run.EnsureMineState();
            return true;
        }

        private int ResolveSafeIndexFromScene()
        {
            switch (gameObject.scene.name)
            {
                case "Safe4":
                    return 4;
                case "Safe5":
                    return 5;
                case "Safe3":
                    return 3;
            }

            if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm) || gsm.Scenes == null)
            {
                return 3;
            }

            switch (gsm.Scenes.CurrentSceneId)
            {
                case SceneId.Safe4:
                    return 4;
                case SceneId.Safe5:
                    return 5;
                default:
                    return 3;
            }
        }
    }
}
