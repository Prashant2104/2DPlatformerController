using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public PlayerController player;
    CharacterInputs _controller;

    private void OnEnable()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
            Debug.LogError("Player not assigned, finding and adding player from scene");
        }
        if (_controller == null && player != null)
        {
            _controller = new CharacterInputs();
            _controller.Character.Walk.performed += i => player.InputVelocity = i.ReadValue<Vector2>();
            _controller.Character.Jump.performed += i => { player.HandleJump(); };
            _controller.Character.Jump.canceled += i => { player.JumpReleased(); };
            _controller.Character.Dash.performed += i => { player.Dash(); };
        }
        _controller.Enable();
    }
    private void OnDisable()
    {
        _controller.Disable();
    }
}