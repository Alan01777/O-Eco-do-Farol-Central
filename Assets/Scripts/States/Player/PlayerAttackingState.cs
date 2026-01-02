using Godot;

namespace EcoDoFarolCentral
{
    public class PlayerAttackingState : PlayerState
    {
        public override void Enter()
        {
            Player.IsAttacking = true;
            if (Player.IsOnFloor())
            {
                Player.CurrentStateEnum = Player.PlayerStates.Attacking;
            }
            else
            {
                Player.CurrentStateEnum = Player.PlayerStates.JumpAttack;
            }

            Player.UpdateHitBox();
        }

        public override void PhysicsUpdate(double delta)
        {
            if (Player.IsOnFloor())
            {
                Player.ApplyGravity(delta);
                Player.HandleMovement(0, 0.1f);

                // Lógica de encadeamento de combos
                if (Input.IsActionJustPressed("attack"))
                {
                    Player.QueueCombo();
                }
            }
            else
            {
                Player.ApplyGravity(delta);
                Player.HandleMovement();
            }
        }

        public override void OnAnimationFinished()
        {
            if (Player.ComboQueued)
            {
                // Verifica se o próximo nível do combo está desbloqueado
                int nextCombo = (Player.AttackCombo % 3) + 1;
                GD.Print($"[ATTACK STATE] Combo: AttackCombo={Player.AttackCombo}, NextCombo={nextCombo}, MaxComboLevel={Player.Abilities.MaxComboLevel}");

                if (nextCombo <= Player.Abilities.MaxComboLevel)
                {
                    Player.AdvanceCombo();
                    Player.UpdateHitBox();
                    // Adia a atualização da animação para o próximo frame para evitar recursão
                    Player.CallDeferred(nameof(Player.UpdateAnimations));
                }
                else
                {
                    // Não pode avançar combo - não desbloqueado ainda
                    GD.Print($"[ATTACK STATE] Combo blocked: Next level {nextCombo} > Max level {Player.Abilities.MaxComboLevel}");
                    Player.IsAttacking = false;
                    Player.ResetCombo();
                    Player.DisableAllHitBoxes();
                    StateMachine.ChangeState("Idle");
                }
            }
            else
            {
                Player.IsAttacking = false;
                Player.ResetCombo();
                Player.DisableAllHitBoxes();
                StateMachine.ChangeState("Idle");
            }
        }

        public override void Exit()
        {
            Player.IsAttacking = false;
            Player.DisableAllHitBoxes();
        }
    }
}
