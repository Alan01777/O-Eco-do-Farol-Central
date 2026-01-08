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
            Player.AnimControllerInstance.PlaySFX(Player._playerAudioPath + "dash.wav", 0.9f, 1.1f);
            Player.StartDash();
            _dashTimer = Player.DashDuration;
        }

        public override void PhysicsUpdate(double delta)
        {
            _dashTimer -= (float)delta;

            if (_dashTimer <= 0)
            {
                Player.EndDash();

                if (Player.IsOnFloor())
                    StateMachine.ChangeState("Idle");
                else
                    StateMachine.ChangeState("Falling");
            }
        }

        public override void Exit()
        {
            Player.EndDash();
        }
    }
}
