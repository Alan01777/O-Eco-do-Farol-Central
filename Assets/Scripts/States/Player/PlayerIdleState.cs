using Godot;

namespace EcoDoFarolCentral
{
    public class PlayerIdleState : PlayerState
    {
        public override void PhysicsUpdate(double delta)
        {
            Player.CurrentStateEnum = Player.PlayerStates.Idle;

            // tá feio mas funciona
            if (Input.GetAxis("ui_left", "ui_right") != 0)
            {
                StateMachine.ChangeState("Running");
                return;
            }

            // Caiu da plataforma
            if (!Player.IsOnFloor() && Player.Velocity.Y > 0)
            {
                StateMachine.ChangeState("Falling");
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

            if (Input.IsActionJustPressed("dash") &&
                Player.CanDash() &&
                Player.Abilities.CanDash)
            {
                StateMachine.ChangeState("Dashing");
                return;
            }

            Player.ApplyGravity(delta);
            Player.HandleMovement(0);
        }
    }
}
