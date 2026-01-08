using Godot;

namespace EcoDoFarolCentral.States.Goblin
{
    public class GoblinJumpState : GoblinShadowState
    {
        private bool _hasJumped;
        private float _jumpTimer;
        private float _horizontalDirection;
        private const float MIN_JUMP_TIME = 0.3f; // NOVO: Tempo mínimo no estado de pulo
        private const float MAX_JUMP_TIME = 2f; // NOVO: Tempo máximo (fallback)

        public override void Enter()
        {
            GD.Print($"[JUMP STATE] ENTER - IsOnFloor: {Goblin.IsOnFloor()}, VelocityY: {Goblin.Velocity.Y}");

            Goblin.CurrentStateEnum = GoblinShadow.GoblinStates.Jumping;
            _hasJumped = false;
            _jumpTimer = 0;

            // Atualiza label de estado, se existir
            var stateLabel = Goblin.GetNodeOrNull<Label>("StateLabel");
            if (stateLabel != null)
            {
                stateLabel.Text = "JUMP";
            }

            // Define direção UMA VEZ
            if (Goblin.TargetPlayer != null)
            {
                float directionToPlayer = Mathf.Sign(Goblin.TargetPlayer.GlobalPosition.X - Goblin.GlobalPosition.X);
                _horizontalDirection = -directionToPlayer;
            }
            else
            {
                _horizontalDirection = Mathf.Sign(Goblin.Velocity.X);
                if (_horizontalDirection == 0) _horizontalDirection = 1;
            }

            // Define flip e animação
            Goblin.AnimControllerInstance.UpdateFlip(_horizontalDirection);
            Goblin.AnimControllerInstance.Play("jump");
        }

        public override void PhysicsUpdate(double delta)
        {
            Goblin.ApplyGravity(delta);

            _jumpTimer += (float)delta;

            // CORREÇÃO 1: Só pula se REALMENTE está no chão e não pulou ainda
            if (!_hasJumped)
            {
                if (Goblin.IsOnFloor() && Mathf.Abs(Goblin.Velocity.Y) < 50f)
                {
                    float jumpForce = Goblin.JumpForce;

                    if (Goblin.TargetPlayer != null && Goblin.IsPlayerInRange())
                    {
                        jumpForce *= 1.2f;
                    }

                    Goblin.Velocity = new Vector2(
                        _horizontalDirection * Goblin.FleeSpeed * 0.8f,
                        jumpForce
                    );
                    _hasJumped = true;
                    GD.Print("[JUMP STATE] JUMPED!");
                }
                else
                {
                    // Se não está no chão ao entrar, considera que já pulou
                    _hasJumped = true;
                    GD.Print("[JUMP STATE] Not on floor, setting _hasJumped = true");
                }
            }
            else
            {
                // CORREÇÃO 2: Mantém momento horizontal no ar (mais forte)
                Goblin.Velocity = new Vector2(
                    _horizontalDirection * Goblin.FleeSpeed * 0.7f, // Aumentado de 0.6 para 0.7
                    Goblin.Velocity.Y
                );
            }

            // CORREÇÃO 3: Só sai do estado de pulo se:
            // 1. Passou tempo mínimo (evita sair antes de subir)
            // 2. Está no chão
            // 3. Velocidade vertical é baixa (realmente pousou)
            bool canExit = _jumpTimer > MIN_JUMP_TIME &&
                          Goblin.IsOnFloor() &&
                          Mathf.Abs(Goblin.Velocity.Y) < 50f;

            // CORREÇÃO 4: Fallback - se passou muito tempo, força saída
            bool forceExit = _jumpTimer > MAX_JUMP_TIME;

            if (canExit || forceExit)
            {
                GD.Print($"[JUMP STATE] EXIT - Timer: {_jumpTimer:F2}, CanExit: {canExit}, ForceExit: {forceExit}");

                if (Goblin.TargetPlayer != null && Goblin.IsPlayerInRange())
                {
                    StateMachine.ChangeState("Flee");
                }
                else
                {
                    StateMachine.ChangeState("Idle");
                }
            }
        }

        public override void Exit()
        {
            GD.Print($"[JUMP STATE] EXIT METHOD - Timer: {_jumpTimer:F2}, HasJumped: {_hasJumped}");
        }
    }
}