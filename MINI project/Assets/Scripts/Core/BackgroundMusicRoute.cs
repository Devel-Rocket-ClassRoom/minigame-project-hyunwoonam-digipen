/// <summary>
/// 현재 씬/전투 컨텍스트를 배경음악 키로 변환한다.
/// 실제 AudioClip 경로는 Resources/Sound/Background/{key}.
/// </summary>
public static class BackgroundMusicRoute
{
    public const string ResourceRoot = "Sound/Background/";

    public static string ForScene(SceneId sceneId, float safeRoll)
    {
        switch (sceneId)
        {
            case SceneId.MainMenu:
            case SceneId.FloorMap:
                return "mainmenu_floormap";
            case SceneId.Safe0:
            case SceneId.Safe1:
            case SceneId.Safe2:
            case SceneId.Safe3:
            case SceneId.Safe4:
            case SceneId.Safe5:
                return safeRoll < 0.5f ? "safe" : "safe2";
            default:
                return string.Empty;
        }
    }

    public static string ForCombat(FloorNode node, ErosionStateModel erosion)
    {
        int stageIndex = node != null ? node.StageIndex : 1;
        bool isBoss = node != null && node.IsBoss;
        bool isFullyEroded =
            erosion != null && erosion.IsStageFullyEroded(stageIndex);
        return ForCombat(stageIndex, isBoss, isFullyEroded);
    }

    public static string ForCombat(int stageIndex, bool isBoss, bool isStageFullyEroded)
    {
        if (isStageFullyEroded)
        {
            return "erosion";
        }

        int stage = System.Math.Max(1, System.Math.Min(5, stageIndex));
        return "stage" + stage + (isBoss ? "boss" : string.Empty);
    }
}
