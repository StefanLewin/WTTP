using System.Collections.Generic;

public class PlayerStateFactory
{
    enum PlayerStates
    {
        idle,
        walk,
        run,
        jump,
        longjump,
        crouch,
        slide,
        fall
    }

    PlayerStateMachine _context;
    Dictionary<PlayerStates, PlayerBaseState> _states = new Dictionary<PlayerStates, PlayerBaseState>();
     
    public PlayerStateFactory(PlayerStateMachine currentContext) {
        _context = currentContext;
        _states[PlayerStates.idle]      = new PlayerIdleState(_context, this);
        _states[PlayerStates.walk]      = new PlayerWalkState(_context, this);
        _states[PlayerStates.jump]      = new PlayerJumpState(_context, this);
        _states[PlayerStates.longjump]  = new PlayerLongJumpState(_context, this);
        _states[PlayerStates.fall]      = new PlayerFallState(_context, this);


        /*
        _states[PlayerStates.run ] = new PlayerRunState(_context, this);        
        _states[PlayerStates.crouch] = new PlayerCrouchState(_context, this);
        _states[PlayerStates.slide] = new PlayerSlideState(_context, this);
        */        
    }

    public PlayerBaseState Idle() {
        return _states[PlayerStates.idle];
    }

    public PlayerBaseState Walk() {
        return _states[PlayerStates.walk];
    }

    public PlayerBaseState Run() {
        return _states[PlayerStates.run];
    }

    public PlayerBaseState Jump() {
        return _states[PlayerStates.jump];
    }

    public PlayerBaseState LongJump() {
        return _states[PlayerStates.longjump];
    }

    public PlayerBaseState Crouch() {
        return _states[PlayerStates.crouch];
    }

    public PlayerBaseState Slide() {
        return _states[PlayerStates.slide];
    }

    public PlayerBaseState Fall() {
        return _states[PlayerStates.fall];
    }
}
