using UnityEngine;

namespace Tempt
{
    public sealed partial class GameSystemManager
    {
        private void AttachErosionToCurrentRun()
        {
            if (CurrentRun?.Erosion == null)
            {
                Erosion = null;
                return;
            }

            CurrentRun.Erosion.EnsureStageCount(ErosionSystem.GetMaxStage(Data?.World));
            CurrentRun.SafeUnlocks?.EnsureCapacity(GetSafeZoneCount());
            Erosion = new ErosionSystem(CurrentRun.Erosion, Events, Data?.Balance, Data?.World);

            SubscribeErosionEvents();
        }

        public void RequestQuitConfirm()
        {
            if (GlobalOverlayController.TryGetInstance(out GlobalOverlayController overlay))
            {
                overlay.ShowQuitConfirm(QuitGame);
                return;
            }

            GameLog.LogError(
                "[GameSystemManager] GlobalOverlayController 를 찾을 수 없어 종료 확인 팝업을 표시할 수 없습니다."
            );
        }

        public void RequestTogglePage(HotkeyPageId pageId)
        {
            if (GlobalOverlayController.TryGetInstance(out GlobalOverlayController overlay))
            {
                overlay.HandleTogglePage(pageId);
                return;
            }

            GameLog.LogError(
                "[GameSystemManager] GlobalOverlayController 를 찾을 수 없어 단축키 페이지를 열 수 없습니다."
            );
        }

        private void SubscribeErosionEvents()
        {
            if (Events == null)
            {
                return;
            }

            Events.OnStageFullyEroded -= HandleStageFullyEroded;
            Events.OnAllStagesEroded -= HandleAllStagesEroded;
            Events.OnStageFullyEroded += HandleStageFullyEroded;
            Events.OnAllStagesEroded += HandleAllStagesEroded;
        }

        private void UnsubscribeErosionEvents()
        {
            if (Events == null)
            {
                return;
            }

            Events.OnStageFullyEroded -= HandleStageFullyEroded;
            Events.OnAllStagesEroded -= HandleAllStagesEroded;
        }

        private void HandleStageFullyEroded(int stage)
        {
            if (CurrentRun?.SafeUnlocks == null)
            {
                return;
            }

            int safeIndex = StageIndexResolver.SafeIndexForStage(stage, Data?.World);
            CurrentRun.FloorMap?.ResetStageProgression(stage);
            CurrentRun.SafeUnlocks.Lock(safeIndex);
            Events?.RaiseSafeZoneLockChanged(safeIndex, true);
            Save?.SaveSnapshot();
        }

        private int GetSafeZoneCount()
        {
            return Data?.World?.SafeZones != null && Data.World.SafeZones.Count > 0
                ? Data.World.SafeZones.Count
                : 6;
        }

        private void HandleAllStagesEroded()
        {
            if (CurrentRun != null)
            {
                Save?.AppendGrave(
                    CurrentRun.Player != null ? CurrentRun.Player.Name : "Player",
                    System.DateTime.Now
                );
            }

            Save?.ClearContinue();
            CurrentRun = null;
            CombatContext = null;
            ShowAllStagesErodedOverlay();
            Scenes.LoadMainMenu();
        }

        private static void ShowAllStagesErodedOverlay()
        {
            if (GlobalOverlayController.TryGetInstance(out GlobalOverlayController overlay))
            {
                overlay.ShowAllStagesEroded();
            }
            else
            {
                GameLog.LogError(
                    "[GameSystemManager] GlobalOverlayController 를 찾을 수 없어 전체 침식 게임오버 패널을 표시할 수 없습니다."
                );
            }
        }
    }
}
