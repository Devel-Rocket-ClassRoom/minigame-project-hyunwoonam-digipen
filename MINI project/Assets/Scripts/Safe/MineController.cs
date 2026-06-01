namespace Tempt
{
    public sealed class MineController : SafeZoneControllerBase
    {
        protected override void Awake()
        {
            base.Awake();
            SafeIndex = ResolveSafeIndexFromScene();
        }

        protected override void SetupZoneFeatures()
        {
            if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm) || gsm.CurrentRun == null)
            {
                return;
            }

            int gainIndex = SafeIndex - 3;
            if (gsm.Data?.Balance?.MineDailyGain == null || gainIndex < 0 || gainIndex >= gsm.Data.Balance.MineDailyGain.Count)
            {
                return;
            }

            int gain = System.Math.Max(0, gsm.Data.Balance.MineDailyGain[gainIndex]);
            gsm.CurrentRun.ManaStone += gain;
            gsm.Save?.SaveSnapshot();
        }

        private static int ResolveSafeIndexFromScene()
        {
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
