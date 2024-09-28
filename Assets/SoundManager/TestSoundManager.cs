using NaughtyAttributes;
using UnityEngine;

public class TestSoundManager : MonoBehaviour
{
    [Button]
    void PlayTest1_d()
    {
        SoundManager.instance.PlaySfx(SFX.Test1);
    }
    [Button]
    void PlayTest2_d()
    {
        SoundManager.instance.PlaySfx(SFX.Test2);
    }
    [Button]
    void PlayTest3_d()
    {
        SoundManager.instance.PlaySfx(SFX.Test3);
    }
    [Button]
    void PlayBGM1()
    {
        SoundManager.instance.PlayMusic(BGM.Bgm1);
    }
    [Button]
    void PlayBGM2()
    {
        SoundManager.instance.PlayMusic(BGM.Bgm2);
    }
    [Button]
    void PlayBGM3()
    {
        SoundManager.instance.PlayMusic(BGM.Bgm3);
    }
}
