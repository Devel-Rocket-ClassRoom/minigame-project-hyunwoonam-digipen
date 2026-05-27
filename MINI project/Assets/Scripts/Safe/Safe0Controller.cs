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
            GameRunState run = GameSystemManager.Instance.CurrentRun; //Wave0write
            if (run?.Player == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            if (run.Player.StartingClass == RuneClass.None) //Wave0write
            { //Wave0write
                run.Player.StartingClass = runeClass; //Wave0write
                run.Player.Rune = new PlayerRuneState //Wave0write
                { //Wave0write
                    ClassId = runeClass, //Wave0write
                    RunePoints = 0, //Wave0write
                    UnlockedIds = new System.Collections.Generic.HashSet<int>(), //Wave0write
                    Tree = RuneTree.BuildFromData(runeClass, GameSystemManager.Instance.Data.Runes.Values), //Wave0write
                }; //Wave0write
                run.Player.Rune.UnlockStarter(); //Wave0write
            } //Wave0write
        }
    }
}

