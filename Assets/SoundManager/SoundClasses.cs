using UnityEngine;

public enum SFX
{
    Test1, Test2, Test3
}
public enum BGM
{
    Bgm1, Bgm2, Bgm3
}
[System.Serializable]
public class Sounds
{
    public SFX sfx;
    public AudioClip[] clips;
    public Vector2 pitchRange = Vector2.one;
    [Range(0f, 1f)]
    public float volumeMultiplier = 1f;
}
[System.Serializable]
public class Music
{
    public BGM bgm;
    public AudioClip clip;
}