using NaughtyAttributes;
using System;
using System.Collections;
using UnityEngine;

public interface IPlayerInterface
{
    public event Action Jumped;
    public event Action<bool> Grounded;
    public event Action Dashed;
    public Vector2 PlayerVelocity { get; }
}
public class PlayerController : MonoBehaviour, IPlayerInterface
{
    [SerializeField, Expandable] ControllerStatsScriptable stats;

    Vector2 _inputVelocity;
    public Vector2 InputVelocity {  get { return _inputVelocity; } set { _inputVelocity = value; } }

    Vector2 _currentVelocity;
    float _deceleration;
    [SerializeField, ReadOnly] bool _grounded = true;
    bool _inCoyoteTime = true;
    [SerializeField, ReadOnly] bool _jumpEndEarly;

    Coroutine _jumpCoroutine = null;
    Coroutine _coyoteCoroutine = null;

    float _time = 0;
    float _jumpPressTime = 0;
    float _jumpReleaseTime = 0;
    [SerializeField, ReadOnly] int _jumpCount = 0;

    bool _dashing;
    [SerializeField, ReadOnly] bool _canDash;
    float _dashedTime;

    float _facingDirection = 1;

    Collider2D _col;
    Rigidbody2D _rb;

    public Vector2 PlayerVelocity => _inputVelocity;
    public event Action Jumped;
    public event Action<bool> Grounded;
    public event Action Dashed;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>() ? GetComponent<Rigidbody2D>() : gameObject.AddComponent<Rigidbody2D>();
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _col = GetComponent<Collider2D>() ? GetComponent<Collider2D>() : gameObject.AddComponent<Collider2D>();
    }
    private void OnDisable()
    {
        StopAllCoroutines();
    }
    private void FixedUpdate()
    {
        CheckGrounded();
        CheckCeiling();
        HorizontalVelocity();
        Gravity();

        DebugStats.Instance?.LogVelocity(_rb.velocity);
        DebugStats.Instance?.LogInput(_inputVelocity);

        _time += Time.deltaTime;
    }
    public void HorizontalVelocity()
    {
        if(_dashing) return;
        if (Mathf.Abs(_inputVelocity.x) < stats.deadZone)
        {
            _deceleration = _grounded ? stats.groundDeceleration : stats.airDeceleration;
            _currentVelocity.x = Mathf.MoveTowards(_currentVelocity.x, 0, _deceleration * Time.deltaTime);
        }
        else
        {
            _facingDirection = Mathf.CeilToInt(_inputVelocity.x);
            _currentVelocity.x = Mathf.MoveTowards(_currentVelocity.x, stats.maxSpeed * _inputVelocity.x, stats.acceleration * Time.deltaTime);
        }
        _rb.velocity = _currentVelocity;
    }
    public void HandleJump()
    {
        if (_jumpCount == 0)
        {
            if (_grounded && stats.jumpBuffer <= (_time - _jumpPressTime))
            {
                Jump();
            }
        }
        else if(_jumpCount > 0 && _jumpCount < stats.maxJumpCount)
        {
            Jump();
        }
    }
    void Jump()
    {
        Jumped?.Invoke();
        _jumpPressTime = _time;
        _jumpEndEarly = false;
        JumpEnd();
        _jumpCoroutine = StartCoroutine(JumpRoutine());
        _jumpCount++;
    }
    public void JumpReleased()
    {
        JumpEnd();
        _jumpReleaseTime = _time;
    }
    void JumpEnd()
    {
        if (_jumpCoroutine != null)
        {
            StopCoroutine(_jumpCoroutine);
            _jumpCoroutine = null;
            if (_rb.velocity.y > 0 && (_jumpPressTime - _jumpReleaseTime) < 1.0f)
            {
                _jumpEndEarly = true;
            }
        }
    }
    IEnumerator JumpRoutine()
    {
        float t = 0f;
        while (t < 0.1f && !_dashing)
        {
            t += Time.deltaTime;
            _currentVelocity.y = stats.jumpPower;    
            yield return null;
        }
    }
    public void Dash()
    {
        if (!_dashing && _canDash && stats.dashBuffer <= (_time - _dashedTime))
        {
            Dashed?.Invoke();
            _dashedTime = _time;
            _dashing = true;
            _canDash = false;
            _jumpEndEarly = false;
            if (_inputVelocity.magnitude > 0)
            {
                Vector2 _dir = _inputVelocity.normalized;
                StartCoroutine(DashRoutine(_dir));
            }
            else
            {
                Vector2 _dir = new Vector2(_facingDirection, 0);
                StartCoroutine(DashRoutine(_dir));
            }
        }
    }
    IEnumerator DashRoutine(Vector2 _dir)
    {
        Vector2 initialVelocity = _rb.velocity;
        _rb.velocity = Vector2.zero;
        float t = 0;
        while (t < 0.05f && _dashing)
        {
            t += Time.deltaTime;
            _currentVelocity.x = stats.dashVelocity * _dir.x;
            _currentVelocity.y = stats.dashVelocity * _dir.y;
            _rb.velocity = _currentVelocity;
            yield return null;
        }
        _rb.velocity = initialVelocity / 2;
        _dashing = false;
        if(_grounded) _canDash = true;
    }
    void Gravity()
    {
        if(_dashing) return;
        if (_grounded && !_inCoyoteTime)
        {
            _currentVelocity.y = -stats.groundingAcceleration;
        }
        else
        {
            if (_jumpEndEarly)
            {
                _currentVelocity.y = Mathf.MoveTowards(_currentVelocity.y, -stats.maxFallSpeed, stats.fallAcceleration * Time.deltaTime * stats.jumpEndEarlyMultiplier);
            }
            else
            {
                _currentVelocity.y = Mathf.MoveTowards(_currentVelocity.y, -stats.maxFallSpeed, stats.fallAcceleration * Time.deltaTime);
            }
        }
    }
    void CheckGrounded()
    {
        if (Physics2D.CircleCast(_col.bounds.center, _col.bounds.size.x / 2, Vector2.down, _col.bounds.size.y / 2 - stats.groundCheckRayOffset, ~stats.playerLayer))
        {
            if (_coyoteCoroutine != null && !_grounded)
            {
                StopCoroutine(_coyoteCoroutine);
                _coyoteCoroutine = null;
                _grounded = true;
                _inCoyoteTime = false;
                _jumpEndEarly = false;
                _canDash = true;
                _jumpCount = 0;
                Grounded?.Invoke(true);
            }
        }
        else
        {
            if (_dashing)
            {
                Grounded?.Invoke(false);
                _grounded = false;
            }
            else if(_coyoteCoroutine == null)
            {
                _coyoteCoroutine = StartCoroutine(CoyoteTime());
                Grounded?.Invoke(false);
            }
        }
    }
    void CheckCeiling()
    {
        if (Physics2D.CircleCast(_col.bounds.center, _col.bounds.size.x / 2, Vector2.up, _col.bounds.size.y / 2 - stats.groundCheckRayOffset, ~stats.playerLayer))
        {
            if (_rb.velocity.y >= 0)
            {
                _currentVelocity.y = Mathf.MoveTowards(_currentVelocity.y, 0, stats.fallAcceleration * Time.deltaTime);
                JumpEnd();
                _jumpEndEarly = true;
                _dashing = false;
            }
        }
    }
    IEnumerator CoyoteTime()
    {
        _inCoyoteTime = true;
        yield return new WaitForSeconds(stats.coyoteTime);
        _grounded = false;
        _inCoyoteTime = false;
    }
}
