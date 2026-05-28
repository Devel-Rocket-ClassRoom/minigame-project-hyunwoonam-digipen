namespace Tempt
{
    /// <summary>
    /// 안전지대 0: 시작 안식처. 호수/모래밭, 비석/묘비, 최초 룬 선택.
    /// </summary>
    public sealed class Safe0Controllert : SafeZoneControllerBaset
    {
        /// <summary>
        /// 최초 시작 룬 선택 UI 준비.
        /// </summary>
        protected override void SetupZoneFeatures()
        {
            // 동작 요약:
            // - 비석/묘비 데이터 렌더링(RecordBookt 사용, 글자 크기 누적에 따라 작아짐).
            // - 플레이어가 룬을 한 번도 선택하지 않은 상태이면 룬 선택 UI를 강제 표시.
            // - 룬 선택 완료 콜백 → Playert.ApplyStartingClass.
            // - 출발 버튼 활성(룬 선택 후).
            //TODO: var records = GameSystemManagert.Instance.Save.Records;
            //TODO: GraveStoneUI.Render(records); // 누적 기록 묘비 표시
            //TODO: var player = GameSystemManagert.Instance.CurrentRun.Player;
            //TODO: bool needClassSelect = player.StartingClass == RuneClasst.None;
            //TODO: if (needClassSelect)
            //TODO: {
            //TODO:     ClassSelectUI.Show(onSelected: (runeClass) =>
            //TODO:     {
            //TODO:         player.ApplyStartingClass(runeClass, GameSystemManagert.Instance.Data.Runes);
            //TODO:         DepartButton.interactable = true;
            //TODO:     });
            //TODO: }
            //TODO: else DepartButton.interactable = true;
            GameRunStatet run = GameSystemManagert.Instance.CurrentRun; //Wave0write
            if (run?.Player == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            if (run.Player.StartingClass == RuneClasst.None) //Wave0write
            { //Wave0write
                run.Player.StartingClass = RuneClasst.Dealer; //Wave0write
                run.Player.Rune = new PlayerRuneStatet //Wave0write
                { //Wave0write
                    ClassId = RuneClasst.Dealer, //Wave0write
                    RunePoints = 0, //Wave0write
                    UnlockedIds = new System.Collections.Generic.HashSet<int>(), //Wave0write
                    Tree = RuneTreet.BuildFromData(RuneClasst.Dealer, GameSystemManagert.Instance.Data.Runes.Values), //Wave0write
                }; //Wave0write
                run.Player.Rune.UnlockStarter(); //Wave0write
            } //Wave0write
        }
    }
}
