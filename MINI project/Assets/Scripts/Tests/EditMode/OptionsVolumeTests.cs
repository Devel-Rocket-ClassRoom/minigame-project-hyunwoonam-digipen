using NUnit.Framework;
using UnityEngine;

public sealed class OptionsVolumeTests
{
    [Test]
    public void OptionsService_DefaultMasterVolume_IsReducedFromFullVolume()
    {
        Assert.AreEqual(0.7f, OptionsService.DefaultMasterVolume);
    }

    [Test]
    public void OptionsService_PreviewMasterVolume_ChangesAudioListenerWithoutSavingCurrent()
    {
        float previousVolume = AudioListener.volume;
        try
        {
            var service = new OptionsService();
            service.PreviewMasterVolume(0.35f);

            Assert.AreEqual(0.35f, AudioListener.volume);
            Assert.IsNull(service.Current);
        }
        finally
        {
            AudioListener.volume = previousVolume;
        }
    }

    [Test]
    public void OptionsService_PreviewMasterVolume_ClampsToValidRange()
    {
        float previousVolume = AudioListener.volume;
        try
        {
            var service = new OptionsService();
            service.PreviewMasterVolume(1.25f);

            Assert.AreEqual(1f, AudioListener.volume);

            service.PreviewMasterVolume(-0.5f);

            Assert.AreEqual(0f, AudioListener.volume);
        }
        finally
        {
            AudioListener.volume = previousVolume;
        }
    }
}
