using Godot;

namespace EcoDoFarolCentral.States.Goblin
{
    // MELHORIA: Estado Idle mais inteligente com patrulha e atenção
    public class GoblinIdleState : GoblinShadowState
    {
        private float _idleTimer;
        private float _patrolTimer;
        private float _patrolChangeInterval = 3f;
        private float _patrolDirection = 0f; // 0 = parado, -1 ou 1 = patrulhando
        private bool _isPatrolling;
        private float _awarenessRadius; // Raio de atenção aumentado

        public override void Enter()
        {
            Goblin.CurrentStateEnum = GoblinShadow.GoblinStates.Idle;
            Goblin.AnimControllerInstance.Play("idle");
            _idleTimer = 0;
            _patrolTimer = 0;
            _patrolDirection = (float)GD.RandRange(-1, 1);
            if (_patrolDirection == 0) _patrolDirection = 1;
            _awarenessRadius = Goblin.DetectionRange * 1.2f; // 20% maior que detecção normal

            // Atualiza label de estado, se existir
            var stateLabel = Goblin.GetNodeOrNull<Label>("StateLabel");
            if (stateLabel != null)
            {
                stateLabel.Text = "IDLE";
            }

            // Decide aleatoriamente se vai patrulhar ou ficar parado
            if (GD.Randf() > 0.5f)
            {
                StartPatrol();
            }
            else
            {
                StopPatrol();
            }
        }

        public override void PhysicsUpdate(double delta)
        {
            Goblin.ApplyGravity(delta);

            // MELHORIA 1: Detecção antecipada - Goblin fica alerta antes de fugir
            if (Goblin.TargetPlayer != null)
            {
                float distanceToPlayer = Goblin.GlobalPosition.DistanceTo(Goblin.TargetPlayer.GlobalPosition);

                // Zona de alerta (antes da zona de fuga)
                if (distanceToPlayer < _awarenessRadius && distanceToPlayer > Goblin.DetectionRange)
                {
                    // Olha na direção do jogador (fica alerta)
                    float directionToPlayer = Mathf.Sign(Goblin.TargetPlayer.GlobalPosition.X - Goblin.GlobalPosition.X);
                    Goblin.AnimControllerInstance.UpdateFlip(directionToPlayer);

                    // Para de patrulhar quando detecta algo
                    if (_isPatrolling)
                    {
                        StopPatrol();
                    }
                }

                // Zona de fuga
                if (Goblin.IsPlayerInRange())
                {
                    StateMachine.ChangeState("Flee");
                    return;
                }
            }

            // MELHORIA 2: Comportamento de patrulha
            if (_isPatrolling)
            {
                HandlePatrol(delta);
            }
            else
            {
                Goblin.Velocity = new Vector2(0, Goblin.Velocity.Y);

                _idleTimer += (float)delta;

                // Após um tempo parado, começa a patrulhar
                if (_idleTimer > 2f && GD.Randf() > 0.3f)
                {
                    StartPatrol();
                    _idleTimer = 0;
                }
            }
        }

        private void HandlePatrol(double delta)
        {
            // Movimento de patrulha lento
            float patrolSpeed = Goblin.FleeSpeed * 0.3f; // 30% da velocidade de fuga
            Goblin.Velocity = new Vector2(_patrolDirection * patrolSpeed, Goblin.Velocity.Y);

            // Verifica obstáculos
            bool wallAhead = Goblin.WallCheck.IsColliding();
            bool gapAhead = !Goblin.FloorCheck.IsColliding();

            if (wallAhead || gapAhead)
            {
                // Vira na direção oposta
                _patrolDirection *= -1;
                Goblin.AnimControllerInstance.UpdateFlip(_patrolDirection);
            }

            _patrolTimer += (float)delta;

            // Muda de direção ou para periodicamente
            if (_patrolTimer >= _patrolChangeInterval)
            {
                _patrolTimer = 0;

                if (GD.Randf() > 0.6f)
                {
                    // 40% de chance de parar
                    StopPatrol();
                }
                else
                {
                    // 60% de chance de mudar direção
                    _patrolDirection *= -1;
                    Goblin.AnimControllerInstance.UpdateFlip(_patrolDirection);
                }
            }
        }

        private void StartPatrol()
        {
            _isPatrolling = true;
            _patrolDirection = GD.Randf() > 0.5f ? 1f : -1f;
            Goblin.AnimControllerInstance.Play("run");
            Goblin.AnimControllerInstance.UpdateFlip(_patrolDirection);
        }

        private void StopPatrol()
        {
            _isPatrolling = false;
            _patrolDirection = 0f;
            Goblin.AnimControllerInstance.Play("idle");
        }
    }
}