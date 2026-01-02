using Godot;

namespace EcoDoFarolCentral
{
    /// <summary>
    /// Estado de Dash do jogador
    /// </summary>
    public class PlayerDashingState : PlayerState
    {
        private float _dashTimer;

        public override void Enter()
        {
            Player.CurrentStateEnum = Player.PlayerStates.Dashing;
            Player.StartDash();
            _dashTimer = Player.DashDuration;
        }

        public override void PhysicsUpdate(double delta)
        {
            _dashTimer -= (float)delta;

            if (_dashTimer <= 0)
            {
                Player.EndDash();
                StateMachine.ChangeState("Idle");
            }
        }

        public override void Exit()
        {
            Player.EndDash();
        }
    }
}
