using Godot;

namespace EcoDoFarolCentral
{
    public class PlayerFallingState : PlayerState
    {
        public override void Enter()
        {
            // Reutiliza o estado/animação de Jumping para cair
            Player.CurrentStateEnum = Player.PlayerStates.Falling;
        }

        public override void PhysicsUpdate(double delta)
        {
            // Se tocou o chão, vai para Idle
            if (Player.IsOnFloor())
            {
                StateMachine.ChangeState("Idle");
                return;
            }

            // Double Jump
            if (Input.IsActionJustPressed("jump"))
            {
                if (Player.CanDoubleJump && Player.Abilities.CanDoubleJump)
                {
                    Player.CurrentStateEnum = Player.PlayerStates.DoubleJump;
                    Player.AnimControllerInstance.PlayVoice(Player._playerAudioPath + "jump.wav", 0.9f, 1.1f);
                    Player.DoJump(true);
                }
            }

            // Jump Attack
            if (Input.IsActionJustPressed("attack"))
            {
                if (Player.Abilities.CanJumpAttack)
                {
                    StateMachine.ChangeState("Attacking");
                    return;
                }
            }

            // Dash
            if (Input.IsActionJustPressed("dash") &&
                Player.CanDash() &&
                Player.Abilities.CanDash)
            {
                StateMachine.ChangeState("Dashing");
                return;
            }

            // Fireball
            if (Input.IsActionJustPressed("special_1") && Player.Abilities.CanCastFireball)
            {
                StateMachine.ChangeState("Cast");
                return;
            }

            Player.ApplyGravity(delta);
            Player.HandleMovement(0, .9f); // leve desaceleração na queda
        }
    }
}
