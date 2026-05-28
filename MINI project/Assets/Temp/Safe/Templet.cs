namespace Tempt
{
    /// <summary>
    /// 신전. 룬 변경/초기화. 비용은 골드(또는 마석).
    /// </summary>
    public sealed class Templet
    {
        /// <summary>
        /// 신전 UI 열기.
        /// </summary>
        public void Open()
        {
            // 동작 요약:
            // - 룬 트리 UI 표시(시작 룬 + 해금된 노드).
            // - 해금 가능한 노드(이웃) 강조.
            // - 초기화 버튼.
            //TODO: var player = GameSystemManagert.Instance.CurrentRun.Player;
            //TODO: RuneTreeUI.Render(player.Rune, onUnlock: (nodeId) => TryUnlockNode(nodeId, BalanceDatat.RuneUnlockGoldCost));
            //TODO: ResetButton.onClick.RemoveAllListeners();
            //TODO: ResetButton.onClick.AddListener(() => TryResetRune(BalanceDatat.RuneResetGoldCost));
        }

        /// <summary>
        /// 노드 해금 비용 지불 후 PlayerRuneStatet.TryUnlock 호출.
        /// </summary>
        public bool TryUnlockNode(int nodeId, int goldPrice)
        {
            // 동작 요약:
            // - 골드 차감 → Player.Rune.TryUnlock(nodeId).
            //TODO: var player = GameSystemManagert.Instance.CurrentRun.Player;
            //TODO: if (player.Gold < goldPrice) return false;
            //TODO: if (!player.Rune.TryUnlock(nodeId)) return false;
            //TODO: player.Gold -= goldPrice;
            //TODO: player.SyncPassivesFromRunes();
            //TODO: RuneTreeUI.Refresh(player.Rune);
            //TODO: return true;
            return false;
        }

        /// <summary>
        /// 룬 초기화. 골드(또는 마석) 비용 + 환급.
        /// </summary>
        public bool TryResetRune(int goldPrice)
        {
            // 동작 요약:
            // - 골드 차감 → Player.Rune.ResetTree().
            //TODO: var player = GameSystemManagert.Instance.CurrentRun.Player;
            //TODO: if (player.Gold < goldPrice) return false;
            //TODO: player.Gold -= goldPrice;
            //TODO: player.Rune.ResetTree(); // 포인트 환급 포함
            //TODO: player.SyncPassivesFromRunes();
            //TODO: RuneTreeUI.Refresh(player.Rune);
            //TODO: return true;
            return false;
        }
    }
}
