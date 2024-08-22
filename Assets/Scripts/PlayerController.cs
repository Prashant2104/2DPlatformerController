using NaughtyAttributes;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField, Expandable] ControllerStatsScriptable stats;

    Vector2 _inputVelocity;
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

    bool _dashing;
    [SerializeField, ReadOnly] bool _canDash;
    float _dashedTime;

    float _facingDirection = 1;

    Collider2D _col;
    Rigidbody2D _rb;
    CharacterInputs _controller;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>() ? GetComponent<Rigidbody2D>() : gameObject.AddComponent<Rigidbody2D>();
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _col = GetComponent<Collider2D>() ? GetComponent<Collider2D>() : gameObject.AddComponent<Collider2D>();
    }

    private void OnEnable()
    {
        if (_controller == null)
        {
            _controller = new();
            _controller.Character.Walk.performed += i => _inputVelocity = i.ReadValue<Vector2>();
            _controller.Character.Jump.performed += i => { Jump(); };
            _controller.Character.Jump.canceled += i =>  { JumpEnd(); _jumpReleaseTime = _time; };
            _controller.Character.Dash.performed += i => { Dash(); };
        }
        _controller.Enable();
    }
    private void OnDisable()
    {
        _controller.Disable();
        StopAllCoroutines();
    }
    private void FixedUpdate()
    {
        CheckGrounded();
        CheckCeiling();
        HorizontalVelocity();
        Gravity();

        _time += Time.deltaTime;
    }
    void HorizontalVelocity()
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
    void Jump()
    {
        if (_grounded && stats.jumpBuffer <= (_time - _jumpPressTime))
        {
            _jumpPressTime = _time;
            _jumpEndEarly = false;
            JumpEnd();
            _jumpCoroutine = StartCoroutine(JumpRoutine());
        }
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
    void Dash()
    {
        if (!_dashing && _canDash && stats.dashBuffer <= (_time - _dashedTime))
        {
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
        if (Physics2D.CircleCast(_col.bounds.center, _col.bounds.size.x / 2, Vector2.down, _col.bounds.size.y / 2 - 0.15f, ~stats.playerLayer))
        { 
            if (_coyoteCoroutine != null && !_grounded)
            {
                StopCoroutine(_coyoteCoroutine);
                _coyoteCoroutine = null;
                _grounded = true;
                _inCoyoteTime = false;
                _jumpEndEarly = false;
                _canDash = true;
            }
        }
        else
        {
            if(_dashing) _grounded = false;
            else _coyoteCoroutine ??= StartCoroutine(CoyoteTime());           
        }
    }
    void CheckCeiling()
    {
        if (Physics2D.CircleCast(_col.bounds.center, _col.bounds.size.x / 2, Vector2.up, _col.bounds.size.y / 2 - 0.15f, ~stats.playerLayer))
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