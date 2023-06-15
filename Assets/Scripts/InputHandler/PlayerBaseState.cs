using UnityEngine;
using UnityEngine.InputSystem;

public abstract class PlayerBaseState : MonoBehaviour
{
    protected PlayerStateMachine _ctx;
    protected PlayerStateFactory _factory;
    protected ThirdPersonActionAsset _action;

    public PlayerBaseState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) {
        _ctx = currentContext;
        _factory = playerStateFactory;
        _action= new ThirdPersonActionAsset();
    }

    /// <summary> Wird einmalig ausgeführt wenn zu diesem State gewechselt wird </summary>
    public abstract void EnterState();

    /// <summary> Wird solange als <see cref="PlayerStateMachine.FixedUpdate()"/> ausgeführt bis zu einem neuen State gewechselt wird </summary>
    public abstract void FixedUpdateState();

    /// <summary> Wird solange als <see cref="PlayerStateMachine.Update()"/> ausgeführt bis zu einem neuen State gewechselt wird </summary>
    public abstract void UpdateState();

    /// <summary> Wird einmalig ausgeführt wenn zu einem anderen State gewechselt wird </summary>
    public abstract void ExitState();

    /** <summary>
     * Enthält alle Verbindungen zu anderen States mit den entsprechenden Bedingungen (z.B. als if())
     * und des jeweiligen States in einer <see cref="SwitchState()"/> Methode.
     * </summary>
     */
    public abstract void CheckSwitchStates();

    /** <summary>
     * wechselt den aktuellen state der <see cref="PlayerStateMachine"/> zu einem neuen State (<paramref name="newState"/>) und führt die entsprechenden
     * <see cref="EnterState()"/> und <see cref="ExitState()"/> Methoden aus.
     * </summary>
     * <param name="newState"> Der State, durch den der Aktuelle ersetzt werden soll.</param>
     */
    protected void SwitchState(PlayerBaseState newState) {

        ExitState();
        newState.EnterState();
        _ctx.Currentstate = newState;
    }

    protected void Animate()
    {

    }

    protected abstract void OnMovementInput(InputAction.CallbackContext obj);
    protected abstract void OnJumpInput(InputAction.CallbackContext obj);
}
