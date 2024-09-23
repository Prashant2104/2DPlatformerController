using UnityEngine;
using UnityEngine.UI;

public class DebugStats : MonoBehaviour
{
    [SerializeField] Text _debugTMP;
    string[] _debugText;

    public static DebugStats Instance;
    private void Awake()
    {
        _debugText = new string[2];
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LogVelocity(Vector3 velocity)
    {
        _debugText[0] = "speed: " + velocity.ToString();
    }
    public void LogInput(Vector2 input)
    {
        _debugText[1] = "input: " + input.ToString();
    }
    private void LateUpdate()
    {
        _debugTMP.text = "DEBUG \n" + string.Join("\n", _debugText);
    }
}
