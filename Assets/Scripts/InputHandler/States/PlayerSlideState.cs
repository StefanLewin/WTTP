/*
using UnityEngine;

public class PlayerSlideState : PlayerBaseState
{
    public PlayerSlideState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {}

    public override void EnterState() {
        _ctx._rb.drag = _ctx.SlideDrag;
    }

    public override void FixedUpdateState() {
        
    }

    public override void UpdateState() {
        CheckSwitchStates();
        Animate();
    }

    public override void ExitState() {}

    public override void CheckSwitchStates() {

        if (!_ctx.Grounded) {
            SwitchState(_factory.Fall());
        }

        if (!_ctx.IsCrouchPressed) {
            SwitchState(_factory.Run());
        }
    }
}
*/