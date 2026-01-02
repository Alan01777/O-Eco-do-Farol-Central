using Godot;

namespace EcoDoFarolCentral
{
    public class PlayerHurtState : PlayerState
    {
        public override void Enter()
        {
            Player.CurrentStateEnum = Player.PlayerStates.Hurt;

            // Encara a fonte do dano
            Player.SpriteInstance.FlipH = Player.DamageSourceDirection < 0;
        }

        public override void PhysicsUpdate(double delta)
        {
            Player.ApplyGravity(delta);

            // Permite leve movimento horizontal durante o dano para o efeito de knockback
            // Mas não permite input do jogador

            if (Player.HurtTimer <= 0)
            {
                if (Player.IsOnFloor())
                {
                    StateMachine.ChangeState("Idle");
                }
            }
        }

        public override void Exit()
        {
            // Limpa velocidade para prevenir que o sprite vire baseado no momentum do knockback
            Player.Velocity = Vector2.Zero;
        }
    }
}
