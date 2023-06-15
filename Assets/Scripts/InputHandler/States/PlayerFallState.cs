using UnityEngine;

public class PlayerFallState : PlayerBaseState
{
    public PlayerFallState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {}

    public override void EnterState() {
        _ctx._rb.drag = _ctx.AirDrag;
    }

    public override void FixedUpdateState() {
        Move();
        Animate();
    }

    public override void UpdateState() {
        CheckSwitchStates();
    }

    public override void ExitState() {
        _ctx._rb.drag = _ctx.GroundDrag;
    }

    public override void CheckSwitchStates() {

        if (_ctx.Grounded) {
            SwitchState(_factory.Idle());
        }
    }

    void Move() {

        _ctx._rb.AddForce(_ctx.MoveDirection.normalized * _ctx.AirMultiplier * _ctx.WalkSpeed * 10f, ForceMode.Force);

        //Limit Velocity to movespeed
        if(_ctx.Velocity.magnitude > _ctx.WalkSpeed) {
            _ctx._rb.velocity = new Vector3(_ctx.Velocity.normalized.x * _ctx.AirMultiplier * _ctx.WalkSpeed, _ctx._rb.velocity.y, _ctx.Velocity.normalized.z * _ctx.AirMultiplier * _ctx.WalkSpeed);
        }
    }
}
