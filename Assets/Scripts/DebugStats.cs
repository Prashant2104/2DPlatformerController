using NaughtyAttributes;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DebugStats : MonoBehaviour
{
    public PlayerController playerController;
    IPlayerInterface _player;
    [SerializeField] Text _debugTMP;
    string[] _debugText;
    [SerializeField, Expandable] ControllerStatsScriptable stats;

    public Slider s_maxVelocity, s_acceleration, s_deacceleration, s_jumpPower, s_fallPower, s_dashVelocity;
    public Text t_maxVelocity, t_acceleration, t_deacceleration, t_jumpPower, t_fallPower, t_dashVelocity;
    public float r_maxVelocity, r_acceleration, r_deacceleration, r_jumpPower, r_fallPower, r_dashVelocity;

    private void Awake()
    {
#if PLATFORM_ANDROID && !UNITY_EDITOR
        Application.targetFrameRate = 120;
#else
        Application.targetFrameRate = -1;
#endif
        _player = playerController.GetComponent<IPlayerInterface>();
        _debugText = new string[3];

        s_maxVelocity.value = stats.maxSpeed;
        s_acceleration.value = stats.acceleration;
        s_deacceleration.value = stats.groundDeceleration;
        s_jumpPower.value = stats.jumpPower;
        s_fallPower.value = stats.fallAcceleration;
        s_dashVelocity.value = stats.dashVelocity;

        t_maxVelocity.text = "Speed: " + stats.maxSpeed.ToString();
        t_acceleration.text = "Acc: " + stats.acceleration.ToString();
        t_deacceleration.text = "Deacc: " + stats.groundDeceleration.ToString();
        t_jumpPower.text = "Jump: " + stats.jumpPower.ToString();
        t_fallPower.text = "Fall: " + stats.fallAcceleration.ToString();
        t_dashVelocity.text = "Dash: " + stats.dashVelocity.ToString();

        s_maxVelocity.onValueChanged.AddListener((i) => { stats.maxSpeed = i; t_maxVelocity.text = "Speed: " + i.ToString(); });
        s_acceleration.onValueChanged.AddListener(i => { stats.acceleration = i; t_acceleration.text = "Acc: " + i.ToString(); });
        s_deacceleration.onValueChanged.AddListener(i => { stats.groundDeceleration = i; t_deacceleration.text = "Deacc: " + i.ToString(); });
        s_jumpPower.onValueChanged.AddListener(i => { stats.jumpPower = i; t_jumpPower.text = "Jump: " + i.ToString(); });
        s_fallPower.onValueChanged.AddListener(i => { stats.fallAcceleration = i; t_fallPower.text = "Fall: " + i.ToString(); });
        s_dashVelocity.onValueChanged.AddListener(i => { stats.dashVelocity = i; t_dashVelocity.text = "Dash: " + i.ToString(); });


        r_maxVelocity = stats.maxSpeed;
        r_acceleration = stats.acceleration;
        r_deacceleration = stats.groundDeceleration;
        r_jumpPower = stats.jumpPower;
        r_fallPower = stats.fallAcceleration;
        r_dashVelocity = stats.dashVelocity;

        StartCoroutine(FPS());
    }
    public void ResetValues()
    {
        stats.maxSpeed = r_maxVelocity;
        stats.acceleration = r_acceleration;
        stats.groundDeceleration = r_deacceleration;
        stats.jumpPower = r_jumpPower;
        stats.fallAcceleration = r_fallPower;
        stats.dashVelocity = r_dashVelocity;

        s_maxVelocity.value = stats.maxSpeed;
        s_acceleration.value = stats.acceleration;
        s_deacceleration.value = stats.groundDeceleration;
        s_jumpPower.value = stats.jumpPower;
        s_fallPower.value = stats.fallAcceleration;
        s_dashVelocity.value = stats.dashVelocity;
    }
    IEnumerator FPS()
    {
        while (true)
        {
            LogFPS(1 / Time.deltaTime);
            yield return new WaitForSeconds(0.25f);
        }
    }
    [SerializeField] Transform inputDirectionVisual;
    void DebugDirection()
    {
        inputDirectionVisual.transform.localPosition = new Vector3(_player.InputDirection.x, _player.InputDirection.y, 0);
    }
    void LogVelocity(Vector3 velocity)
    {
        _debugText[1] = "speed: " + velocity.ToString();
    }
    void LogInput(Vector2 input)
    {
        _debugText[2] = "input: " + input.ToString();
    }
    void LogFPS(float fps)
    {
        _debugText[0] = "FPS: " + fps.ToString("n2");
    }
    private void LateUpdate()
    {
        _debugTMP.text = "DEBUG \n" + string.Join("\n", _debugText);
    }
    private void FixedUpdate()
    {
        LogVelocity(_player.PlayerVelocity);
        LogInput(_player.InputDirection);
        DebugDirection();
    }
}
