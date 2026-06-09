using NUnit.Framework;

public sealed class BackgroundMusicRouteTests
{
    [Test]
    public void SceneRoute_UsesMainMenuTrackForMainMenuAndFloorMap()
    {
        Assert.AreEqual("mainmenu_floormap", BackgroundMusicRoute.ForScene(SceneId.MainMenu, 0.1f));
        Assert.AreEqual("mainmenu_floormap", BackgroundMusicRoute.ForScene(SceneId.FloorMap, 0.9f));
    }

    [Test]
    public void SceneRoute_UsesSafeTracksByRollForAllSafeScenes()
    {
        Assert.AreEqual("safe", BackgroundMusicRoute.ForScene(SceneId.Safe0, 0f));
        Assert.AreEqual("safe", BackgroundMusicRoute.ForScene(SceneId.Safe3, 0.49f));
        Assert.AreEqual("safe2", BackgroundMusicRoute.ForScene(SceneId.Safe2, 0.5f));
        Assert.AreEqual("safe2", BackgroundMusicRoute.ForScene(SceneId.Safe5, 0.99f));
    }

    [Test]
    public void CombatRoute_UsesErosionOnlyWhenStageIsFullyEroded()
    {
        Assert.AreEqual("erosion", BackgroundMusicRoute.ForCombat(2, false, true));
        Assert.AreEqual("stage2", BackgroundMusicRoute.ForCombat(2, false, false));
    }

    [Test]
    public void CombatRoute_UsesStageAndBossTracksForStageOneToFive()
    {
        Assert.AreEqual("stage1", BackgroundMusicRoute.ForCombat(1, false, false));
        Assert.AreEqual("stage3boss", BackgroundMusicRoute.ForCombat(3, true, false));
        Assert.AreEqual("stage5", BackgroundMusicRoute.ForCombat(5, false, false));
        Assert.AreEqual("stage5boss", BackgroundMusicRoute.ForCombat(5, true, false));
    }

    [Test]
    public void CombatRoute_ClampsOutOfRangeStagesToAvailableStageTracks()
    {
        Assert.AreEqual("stage1", BackgroundMusicRoute.ForCombat(0, false, false));
        Assert.AreEqual("stage5boss", BackgroundMusicRoute.ForCombat(6, true, false));
    }
}
