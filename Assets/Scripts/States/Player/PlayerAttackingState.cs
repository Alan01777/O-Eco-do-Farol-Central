using Godot;

namespace EcoDoFarolCentral
{
    public class PlayerAttackingState : PlayerState
    {
        // Safety timeout to prevent getting stuck
        private const float MAX_ATTACK_DURATION = 2.0f;
        private float _attackTimer = 0f;

        public override void Enter()
        {
            Player.IsAttacking = true;
            _attackTimer = 0f;

            if (Player.IsOnFloor())
            {
                Player.CurrentStateEnum = Player.PlayerStates.Attacking;
                // Toca o áudio do ataque atual
                var attackData = Player.GetComboAttackData(Player.AttackCombo - 1);
                if (!string.IsNullOrEmpty(attackData.Audio))
                {
                    Player.AnimControllerInstance.PlayVoice(attackData.Audio, 0.9f, 1.1f);
                    Player.AnimControllerInstance.PlaySFX(Player._playerAudioPath + "swoosh.wav", 0.7f, 1.3f, -16f);
                }
            }
            else
            {
                Player.CurrentStateEnum = Player.PlayerStates.JumpAttack;
                // Toca o áudio do jump attack
                var jumpAttackData = Player.GetJumpAttackData();
                if (!string.IsNullOrEmpty(jumpAttackData.Audio))
                {
                    Player.AnimControllerInstance.PlayVoice(jumpAttackData.Audio, 0.9f, 1.1f);
                    Player.AnimControllerInstance.PlaySFX(Player._playerAudioPath + "swoosh.wav", 0.9f, 1.1f, -16f);
                }
            }

            Player.UpdateHitBox();
        }

        public override void PhysicsUpdate(double delta)
        {
            // Safety timeout - exit if stuck for too long
            _attackTimer += (float)delta;
            if (_attackTimer >= MAX_ATTACK_DURATION)
            {
                ForceExitAttack();
                return;
            }

            if (Player.IsOnFloor())
            {
                Player.ApplyGravity(delta);
                Player.HandleMovement(0, 0.0f);

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

                if (nextCombo <= Player.Abilities.MaxComboLevel)
                {
                    Player.AdvanceCombo();
                    Player.UpdateHitBox();

                    // Toca o áudio do próximo ataque no combo
                    var nextAttackData = Player.GetComboAttackData(Player.AttackCombo - 1);
                    if (!string.IsNullOrEmpty(nextAttackData.Audio))
                    {
                        Player.AnimControllerInstance.PlayVoice(nextAttackData.Audio, 0.9f, 1.1f);
                        Player.AnimControllerInstance.PlaySFX(Player._playerAudioPath + "swoosh.wav", 0.7f, 1.3f, -16f);
                    }

                    // Adia a atualização da animação para o próximo frame para evitar recursão
                    Player.CallDeferred(nameof(Player.UpdateAnimations));
                }
                else
                {
                    // Não pode avançar combo - não desbloqueado ainda
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

        /// <summary>
        /// Forces exit from attack state (used by safety timeout)
        /// </summary>
        private void ForceExitAttack()
        {
            Player.IsAttacking = false;
            Player.ResetCombo();
            Player.DisableAllHitBoxes();
            StateMachine.ChangeState("Idle");
        }

        public override void Exit()
        {
            Player.IsAttacking = false;
            Player.DisableAllHitBoxes();
        }
    }
}
