/*
using UnityEngine;

public class PlayerCrouchState : PlayerBaseState
{
    // variables to safe the original height and Center of the CapsuleCollider
    private float normalHeight;
    private Vector3 normalCenter;

    public PlayerCrouchState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {
        //please never change the value after that
        normalHeight = _ctx._collider.height;
        normalCenter = _ctx._collider.center;
    }
    
    public override void EnterState() {
        //lower the height of the hitbox and change the position of its center
        _ctx._collider.height *= _ctx._crouchingHeightFactor;
        Vector3 center = _ctx._collider.center;
        _ctx._collider.center = new Vector3(0,center.y * _ctx._crouchingHeightFactor,0);
        

    }

    public override void FixedUpdateState() {
        Move();
        Animate();
    }

    public override void UpdateState() {
        CheckSwitchStates();
    }

    public override void ExitState() {
        //restore the original height and Center of the Collider
        _ctx._collider.height = normalHeight;
        _ctx._collider.center = normalCenter;
    }

    public override void CheckSwitchStates() {
        // Check whether there is a ceiling above the character that prevents him from exiting the crouching state.
        bool cantStandUp = Physics.Raycast(_ctx.transform.position, Vector3.up, normalHeight);
        if(!_ctx.IsCrouchPressed && !cantStandUp) {
            SwitchState(_factory.Idle());
        }

        if (!_ctx.Grounded) {
            SwitchState(_factory.Fall());
        }
    }

    void Move() {

        _ctx._rb.AddForce(_ctx.MoveDirection.normalized * _ctx.CrouchSpeed * 10f, ForceMode.Force);

        //Limit Velocity to movespeed
        if(_ctx.Velocity.magnitude > _ctx.CrouchSpeed) {
            _ctx._rb.velocity = new Vector3(_ctx.Velocity.normalized.x * _ctx.CrouchSpeed, _ctx._rb.velocity.y, _ctx.Velocity.normalized.z * _ctx.CrouchSpeed);
        }
    }
}

*/
