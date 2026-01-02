using Godot;

namespace EcoDoFarolCentral
{
    public class PlayerRunningState : PlayerState
    {
        public override void PhysicsUpdate(double delta)
        {
            Player.CurrentStateEnum = Player.PlayerStates.Running;

            float direction = Input.GetAxis("ui_left", "ui_right");
            if (direction == 0)
            {
                StateMachine.ChangeState("Idle");
                return;
            }

            if (Input.IsActionJustPressed("jump"))
            {
                StateMachine.ChangeState("Jumping");
                return;
            }

            if (Input.IsActionJustPressed("attack"))
            {
                StateMachine.ChangeState("Attacking");
                return;
            }

            // Fireball
            if (Input.IsActionJustPressed("special_1") && Player.Abilities.CanCastFireball)
            {
                StateMachine.ChangeState("Cast");
                return;
            }

            // Dash
            if (Input.IsActionJustPressed("dash") &&
                Player.CanDash() &&
                Player.Abilities.CanDash)  // Checa habilidade
            {
                StateMachine.ChangeState("Dashing");
                return;
            }

            Player.ApplyGravity(delta);
            Player.HandleMovement(direction);
        }
    }
}
