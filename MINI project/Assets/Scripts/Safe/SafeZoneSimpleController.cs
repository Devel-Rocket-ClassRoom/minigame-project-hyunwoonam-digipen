/// <summary>
/// 임시 Safe2~5 씬을 실제 씬 전환/공통 HUD 루프에 연결하는 최소 컨트롤러.
/// </summary>
public sealed class SafeZoneSimpleController : SafeZoneControllerBase
{
    protected override void SetupZoneFeatures()
    {
        if (SafeIndex == 2 && GameSystemManager.Instance.CurrentRun?.IsClearedRun != true)
        {
            GameSystemManager.Instance.Erosion?.Activate();
        }
    }
}
