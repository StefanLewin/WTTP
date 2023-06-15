using UnityEngine;

public class PlayerLongJumpState : PlayerBaseState
{
    float _jumpCooldown;
    
    public PlayerLongJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {}

    public override void EnterState() {
        Jump();
        _jumpCooldown = _ctx.JumpCooldown;
        _ctx._rb.drag = _ctx.AirDrag;
    }

    public override void FixedUpdateState() {
        Move();
        Animate();
    }

    public override void UpdateState() {
        CheckSwitchStates();
        _jumpCooldown -= Time.deltaTime;
    }

    public override void ExitState() {
        _ctx._rb.drag = _ctx.GroundDrag;
    }

    public override void CheckSwitchStates() {

        if (_ctx.Grounded && _jumpCooldown <= 0) {
            if (!_ctx.IsMovementPressed) {
                SwitchState(_factory.Idle());
            }
            else {
                SwitchState(_factory.Walk());
            }
        }
    }

    void Move() {

        _ctx._rb.AddForce(_ctx.MoveDirection.normalized * _ctx.AirMultiplier * _ctx.RunSpeed * 10f, ForceMode.Force);

        //Limit Velocity to movespeed
        if(_ctx.Velocity.magnitude > _ctx.RunSpeed) {
            _ctx._rb.velocity = new Vector3(_ctx.Velocity.normalized.x * _ctx.RunSpeed, _ctx._rb.velocity.y, _ctx.Velocity.normalized.z * _ctx.RunSpeed);
        }
    }

    void Jump() {
        _ctx._rb.AddForce(Vector3.up * _ctx.LongJumpForce, ForceMode.Impulse);
    }
}
