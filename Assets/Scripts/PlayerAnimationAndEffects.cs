using NaughtyAttributes;
using System.Collections;
using UnityEngine;

public class PlayerAnimationAndEffects : MonoBehaviour
{
    [SerializeField] SpriteRenderer _sprite;
    [SerializeField] Transform _camTarget;
    [SerializeField, Tooltip("How much time to wait before the camera target start switching")]
    float _camTargetSwitchTime;
    [SerializeField, Tooltip("Time it takes for the camera target to lerp")]
    float _camTargetSmoothTime;
    [SerializeField] Animator _animator;
    [SerializeField] CameraShakeStats dashShake;

    [AnimatorParam("_animator")]
    public int animatorSpeedkey;
    [AnimatorParam("_animator")]
    public int jumpkey;
    [AnimatorParam("_animator")]
    public int groundedkey;

    float _goingBackTime;
    Vector3 _camTargetPos;
    Coroutine _cameraLerpRoutine;
    Vector3 _ref = Vector3.zero;

    IPlayerInterface _player;


    private void Awake()
    {
        if (_sprite == null)
        {
            if (!TryGetComponent<SpriteRenderer>(out _sprite))
            {
                Debug.LogError("Sprite renderer not found");
            }
        }
        if (_animator == null)
        {
            if (!TryGetComponent<Animator>(out _animator))
            {
                Debug.LogError("Animator not found");
            }
        }
        _player = GetComponentInParent<IPlayerInterface>();
        if (_player == null)
        {
            Debug.LogError("Player script not found in parent");
        }
        _camTargetPos = _camTarget.localPosition;
    }

    private void OnEnable()
    {
        if (_player != null)
        {
            _player.Jumped += Jumped;
            _player.Grounded += GroundedChanged;
            _player.Dashed += Dashed;
        }
    }
    private void OnDisable()
    {
        if (_player != null)
        {
            _player.Jumped -= Jumped;
            _player.Grounded -= GroundedChanged;
            _player.Dashed -= Dashed;
        }
    }
    private void LateUpdate()
    {
        if (_player == null) return;

        HandleSpriteFlip();
        HandleMoveAnimation();
    }
    void HandleMoveAnimation()
    {
        float absVelocity = Mathf.Abs(_player.PlayerVelocity.x);
        _animator.SetFloat(animatorSpeedkey, Mathf.Lerp(0, 1, absVelocity));
    }
    void HandleSpriteFlip()
    {
        if (_player.PlayerVelocity.x != 0)
        {
            _sprite.flipX = _player.PlayerVelocity.x < 0;
            HandleCameraTarget();
        }
    }
    void HandleCameraTarget()
    {
        _goingBackTime += Mathf.Sign(_player.PlayerVelocity.x) * Time.deltaTime;
        _goingBackTime = Mathf.Clamp(_goingBackTime, -_camTargetSwitchTime, _camTargetSwitchTime);
        if (Mathf.Abs(_goingBackTime) >= _camTargetSwitchTime)
        {
            _cameraLerpRoutine ??= StartCoroutine(LerpCameraTarget());
        }
    }
    IEnumerator LerpCameraTarget()
    {
        Vector3 _ref = Vector3.zero;
        Vector3 targetPos = _camTargetPos * Mathf.Sign(_player.PlayerVelocity.x);
        if (targetPos != _camTarget.localPosition)
        {
            float t = _camTargetSmoothTime;
            while (targetPos != _camTarget.localPosition)
            {
                Vector3 tempPos = Vector3.SmoothDamp(_camTarget.localPosition, targetPos, ref _ref, t);
                _camTarget.localPosition = tempPos;
                t -= Time.deltaTime;
                yield return null;
            }
            _camTarget.localPosition = targetPos;
        }
        _cameraLerpRoutine = null;
    }
    void Jumped()
    {
        _animator.SetTrigger(jumpkey);
        _animator.ResetTrigger(groundedkey);
        AudioManager.instance.PlayOneShot(FMODEvents.instance.jumpSFX, transform.position);
    }
    void GroundedChanged(bool grounded)
    {
        if(grounded)
            _animator.SetTrigger(groundedkey);
    }
    void Dashed()
    {
        CameraShake.instance.ShakeDirectional(_player.PlayerVelocity, dashShake);
        AudioManager.instance.PlayOneShot(FMODEvents.instance.dashSFX, transform.position);
    }
}