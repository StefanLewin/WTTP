
/*using UnityEngine;

public class PlayerRunState : PlayerBaseState
{
    public PlayerRunState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {}

    public override void EnterState() {
        _ctx.AccelerationCounter = _ctx.AccelerationTime;
    }

    public override void FixedUpdateState() {
        Move();
    }

    public override void UpdateState() {
        CheckSwitchStates();
        Animate();
    }

    public override void ExitState() {
        _ctx.AccelerationCounter = 0f;
    }

    public override void CheckSwitchStates() {
        
        if (!_ctx.IsMovementPressed) {
            SwitchState(_factory.Idle());
        }

        if (!_ctx.IsRunPressed && _ctx.IsMovementPressed) {
            SwitchState(_factory.Walk());
        }

        if (!_ctx.Grounded) {
            SwitchState(_factory.Fall());
        }

        if (_ctx.IsCrouchPressed) {
            SwitchState(_factory.Slide());
        }

        if (_ctx.IsJumpPressed) {
            SwitchState(_factory.LongJump());
        }
    }

    void Move() {
        _ctx.AccelerationCounter += Time.deltaTime;
        float speed = _ctx.WalkSpeed + (_ctx.RunSpeed - _ctx.WalkSpeed) * _ctx.AccelerationCurve.Evaluate(_ctx.AccelerationCounter / _ctx.AccelerationTime);
        _ctx._rb.AddForce(_ctx.MoveDirection.normalized * speed * 10f, ForceMode.Force);

        //Limit Velocity to movespeed
        if(_ctx.Velocity.magnitude > _ctx.RunSpeed) {
            _ctx._rb.velocity = new Vector3(_ctx.Velocity.normalized.x * speed, _ctx._rb.velocity.y, _ctx.Velocity.normalized.z * speed);
        }
    }
}
*/