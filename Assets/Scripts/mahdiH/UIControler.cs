using UnityEngine;
using UnityEngine.UI;

public class UIControler : MonoBehaviour
{
    public Slider musicSlider, sfxSlider;

    public void ToggleMusic()
    {
        AudioManager.instance.ToggleMusic();
    }

    public void ToggleSFX()
    {
        AudioManager.instance.ToggleSfx();
    }

    public void MusicVolume()
    {
        AudioManager.instance.MusicVolume(musicSlider.value);
    }

    public void SfxVolume()
    {
        AudioManager.instance.SfxVolume(sfxSlider.value);
    }
}
