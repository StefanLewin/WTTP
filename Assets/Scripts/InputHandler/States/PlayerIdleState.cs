using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {}

    public override void EnterState() { }

    public override void FixedUpdateState() { }

    public override void UpdateState() {
        CheckSwitchStates();
        Animate();
    }

    public override void ExitState() {}

    public override void CheckSwitchStates() {
        
        if (_ctx.IsMovementPressed) {
            SwitchState(_factory.Walk());
        }

        if (_ctx.IsJumpPressed) {
            SwitchState(_factory.Jump());
        }

        if (_ctx.IsCrouchPressed) {
            SwitchState(_factory.Crouch());
        }

        if (!_ctx.Grounded) {
            SwitchState(_factory.Fall());
        }
    }
}
