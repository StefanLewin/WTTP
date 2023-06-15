using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWalkState : PlayerBaseState
{

    public PlayerWalkState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {}

    public override void EnterState() {}

    public override void FixedUpdateState() {
        Move();
    }

    public override void UpdateState() {
        CheckSwitchStates();
        Animate();
        SpeedControll();
    }

    public override void ExitState() {}

    public override void CheckSwitchStates() {

        /*
        if (_ctx.Velocity.magnitude >= (_ctx.WalkSpeed + (_ctx.RunSpeed - _ctx.WalkSpeed) * _ctx.AccelerationCurve.Evaluate(1f))) {
            SwitchState(_factory.Run());
        }

        if (!_ctx.IsMovementPressed) {
            SwitchState(_factory.Idle());
        }

        if (!_ctx.Grounded) {
            SwitchState(_factory.Fall());
        }

        if (_ctx.IsJumpPressed) {
            SwitchState(_factory.Jump());
        }

        if (_ctx.IsCrouchPressed) {
            SwitchState(_factory.Crouch());
        }
        */
    }

    void Move() {

        _ctx._rb.AddForce(_ctx.MoveDirection.normalized * _ctx.WalkSpeed * 10f, ForceMode.Force);
    }

    void SpeedControll() {
        
        if (_ctx.IsRunPressed)
            _ctx.AccelerationCounter += Time.deltaTime;
        else if (_ctx.AccelerationCounter != 0f)
            _ctx.AccelerationCounter = 0f;
        
        float speed = _ctx.WalkSpeed + (_ctx.RunSpeed - _ctx.WalkSpeed) * _ctx.AccelerationCurve.Evaluate(_ctx.AccelerationCounter / _ctx.AccelerationTime);
        if(_ctx.Velocity.magnitude > speed) {
            _ctx._rb.velocity = new Vector3(_ctx.Velocity.normalized.x * speed, _ctx._rb.velocity.y, _ctx.Velocity.normalized.z * speed);
        }
    }

    protected override void OnMovementInput(InputAction.CallbackContext obj) {
        
    }

    protected override void OnJumpInput(InputAction.CallbackContext obj) {
        SwitchState(_factory.Jump());
    }

    private void OnEnable()
    {
        _action.Player.Jump.started += OnJumpInput;
        _action.Player.Jump.canceled += OnJumpInput;

        _action.Player.Move.started += OnMovementInput;
        _action.Player.Move.canceled += OnJumpInput;
    }

    private void OnDisable()
    {
        
    }

}
