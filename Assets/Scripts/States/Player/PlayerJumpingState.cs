using Godot;

namespace EcoDoFarolCentral
{
    public class PlayerJumpingState : PlayerState

    {
        private float _timeInState = 0f;

        public override void Enter()
        {
            _timeInState = 0f;
            Player.CurrentStateEnum = Player.PlayerStates.Jumping;
            Player.AnimControllerInstance.PlayVoice(Player._playerAudioPath + "jump.wav");
            Player.DoJump();
        }

        public override void PhysicsUpdate(double delta)
        {
            _timeInState += (float)delta;

            if (_timeInState > 0.1f && Player.IsOnFloor() && Player.Velocity.Y >= 0)
            {
                StateMachine.ChangeState("Idle");
                return;
            }

            // Começou a cair (Falling)
            if (Player.Velocity.Y > 0)
            {
                StateMachine.ChangeState("Falling");
                return;
            }

            // Double Jump
            if (Input.IsActionJustPressed("jump"))
            {
                GD.Print($"[JUMP STATE] Double Jump: CanDoubleJump={Player.CanDoubleJump}, Abilities.CanDoubleJump={Player.Abilities.CanDoubleJump}");
                if (Player.CanDoubleJump && Player.Abilities.CanDoubleJump)
                {
                    Player.CurrentStateEnum = Player.PlayerStates.DoubleJump;
                    Player.DoJump(true);
                }
            }

            // Jump Attack
            if (Input.IsActionJustPressed("attack"))
            {
                GD.Print($"[JUMP STATE] Jump Attack: Abilities.CanJumpAttack={Player.Abilities.CanJumpAttack}");
                if (Player.Abilities.CanJumpAttack)
                {
                    StateMachine.ChangeState("Attacking");
                    return;
                }
            }

            // Disparar Fireball (permitido no ar)
            // TODO: Rever o funcionamento da fireball no ar
            if (Input.IsActionJustPressed("special_1") && Player.Abilities.CanCastFireball)
            {
                StateMachine.ChangeState("Cast");
                return;
            }

            // Dash no ar
            if (Input.IsActionJustPressed("dash") &&
                Player.CanDash() &&
                Player.Abilities.CanDash)
            {
                StateMachine.ChangeState("Dashing");
                return;
            }

            Player.ApplyGravity(delta);
            Player.HandleMovement();
        }
    }
}