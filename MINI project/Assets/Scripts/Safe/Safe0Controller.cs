namespace Tempt
{
    /// <summary>
    /// 안전지대 0: 시작 안식처. 호수/모래밭, 비석/묘비, 최초 룬 선택.
    /// </summary>
    public sealed class Safe0Controller : SafeZoneControllerBase
    {
        [UnityEngine.SerializeField] private SafeZone0SanctuaryMockupUI mockupUI;

        /// <summary>
        /// 최초 시작 룬 선택 UI 준비.
        /// </summary>
        protected override void SetupZoneFeatures()
        {
            if (mockupUI != null)
            {
                mockupUI.InitializeMockup();
                return;
            }

            UnityEngine.Debug.LogError("[Safe0Controller] SafeZone0SanctuaryMockupUI 참조가 씬에 직접 할당되어 있지 않습니다.");
        }

        /// <summary>
        /// 시작 룬 선택 결과를 현재 런의 PlayerState 에 적용한다.
        /// </summary>
        public void ApplyStartingRuneClass(RuneClass runeClass)
        {
            GameRunState run = GameSystemManager.Instance.CurrentRun;
            if (run?.Player == null)
            {
                return;
            }

            if (run.Player.StartingClass == RuneClass.None)
            {
                run.Player.StartingClass = runeClass;
                run.Player.Rune = new PlayerRuneState
                {
                    ClassId = runeClass,
                    RunePoints = 0,
                    UnlockedIds = new System.Collections.Generic.HashSet<int>(),
                    Tree = RuneTree.BuildFromData(runeClass, GameSystemManager.Instance.Data.Runes.Values),
                };
                run.Player.Rune.UnlockStarter();
            }
        }
    }
}

