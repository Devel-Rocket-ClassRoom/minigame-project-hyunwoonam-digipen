namespace Tempt
{
    /// <summary>
    /// 주점. 숙소(돈을 내면 활성), 보관함 구매, 동료 모집(층수에 따라 해금).
    /// </summary>
    public sealed class Innt
    {
        /// <summary>
        /// 주점 UI 열기.
        /// </summary>
        public void Open()
        {
            // 동작 요약:
            // - UIManagert.OpenPage(InnPage).
            // - 보관함 잠금 여부에 따라 구매 버튼 표시.
            // - 동료 후보 = CompanionDatat 중 RequiredFloor ≤ HighestFloor.
            // - 모집 버튼은 RecruitPrice 만큼 골드 차감으로 동작.
            //TODO: GameSystemManagert.Instance.UI.OpenPage(InnPage);
            //TODO: var run = GameSystemManagert.Instance.CurrentRun;
            //TODO: BuyLockerButton.SetActive(!run.Player.Locker.Unlocked);
            //TODO: var candidates = new System.Collections.Generic.List<CompanionDatat>();
            //TODO: foreach (var c in GameSystemManagert.Instance.Data.Companions.Values)
            //TODO:     if (c.RequiredFloor <= run.HighestFloor) candidates.Add(c);
            //TODO: RecruitListUI.Bind(candidates, onRecruit: TryRecruit);
        }

        /// <summary>
        /// 숙소 사용(휴식). 골드 차감 후 HP/MP 완전 회복.
        /// </summary>
        public bool TryRest(int price)
        {
            // 동작 요약:
            // - 골드 >= price 검사 후 차감.
            // - Player + 동료 HP/MP 완전 회복.
            //TODO: var run = GameSystemManagert.Instance.CurrentRun;
            //TODO: if (run.Player.Gold < price) return false;
            //TODO: run.Player.Gold -= price;
            //TODO: run.Player.Stats.RestoreToFull();
            //TODO: foreach (var companion in run.Roster.Active) companion.Stats.RestoreToFull();
            //TODO: return true;
            return false;
        }

        /// <summary>
        /// 보관함 구매.
        /// </summary>
        public bool TryBuyLocker(int price)
        {
            // 동작 요약:
            // - 보관함 이미 구매 시 false.
            // - 골드 차감.
            // - Player.Locker.Unlock().
            //TODO: var player = GameSystemManagert.Instance.CurrentRun.Player;
            //TODO: if (player.Locker.Unlocked) return false;
            //TODO: if (player.Gold < price) return false;
            //TODO: player.Gold -= price;
            //TODO: player.Locker.Unlock();
            //TODO: BuyLockerButton.SetActive(false);
            //TODO: return true;
            return false;
        }

        /// <summary>
        /// 동료 모집.
        /// </summary>
        public bool TryRecruit(int companionId)
        {
            // 동작 요약:
            // - CompanionDatat 조회.
            // - RecruitPrice 차감.
            // - Roster에 추가(Bench로). 길드에서 파티 구성 가능.
            //TODO: var data = GameSystemManagert.Instance.Data.Companions[companionId];
            //TODO: var run = GameSystemManagert.Instance.CurrentRun;
            //TODO: if (run.Player.Gold < data.RecruitPrice) return false;
            //TODO: // 이미 모집한 동료 중복 방지
            //TODO: if (run.Roster.Bench.Exists(c => c.Data.Id == companionId) ||
            //TODO:     run.Roster.Active.Exists(c => c.Data.Id == companionId)) return false;
            //TODO: run.Player.Gold -= data.RecruitPrice;
            //TODO: var companion = new TeamBaset();
            //TODO: companion.Initialize(data, run.Seed);
            //TODO: run.Roster.Bench.Add(companion);
            //TODO: return true;
            return false;
        }
    }
}
