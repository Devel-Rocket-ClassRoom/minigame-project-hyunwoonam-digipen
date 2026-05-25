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
        }

        /// <summary>
        /// 노드 해금 비용 지불 후 PlayerRuneStatet.TryUnlock 호출.
        /// </summary>
        public bool TryUnlockNode(int nodeId, int goldPrice)
        {
            // 동작 요약:
            // - 골드 차감 → Player.Rune.TryUnlock(nodeId).
            return false;
        }

        /// <summary>
        /// 룬 초기화. 골드(또는 마석) 비용 + 환급.
        /// </summary>
        public bool TryResetRune(int goldPrice)
        {
            // 동작 요약:
            // - 골드 차감 → Player.Rune.ResetTree().
            return false;
        }
    }
}
