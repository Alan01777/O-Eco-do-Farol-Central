using Godot;

namespace EcoDoFarolCentral.States.Goblin
{
    public class GoblinFleeState : GoblinShadowState
    {
        private float _stuckTimer;
        private Vector2 _lastPosition;
        private float _checkStuckInterval = 0.2f;
        private float _changeDirectionTimer;
        private float _changeDirectionInterval = 1.5f;
        private int _consecutiveObstacles;
        private float _panicMultiplier = 1f;
        private float _currentFleeDirection;
        private float _jumpCooldown; // NOVO: Cooldown para evitar pulos consecutivos
        private const float JUMP_COOLDOWN_TIME = 0.5f; // Meio segundo entre pulos
        private float _directionCommitTimer; // NOVO: Timer para manter direção
        private const float DIRECTION_COMMIT_TIME = 0.5f; // Tempo mínimo na mesma direção
        private bool _isFakeCharging; // NOVO: Está fingindo um ataque?
        private float _fakeChargeTimer; // Timer para fake charge
        private const float FAKE_CHARGE_DURATION = 0.3f; // Duração do fake charge

        public override void Enter()
        {
            Goblin.CurrentStateEnum = GoblinShadow.GoblinStates.Fleeing;
            Goblin.AnimControllerInstance.Play("run");

            // Toca som de alerta aleatoriamente
            if (GD.Randf() < 0.4f)
            {
                Goblin.PlayAlertSound();
            }

            _stuckTimer = 0;
            _changeDirectionTimer = 0;
            _lastPosition = Goblin.GlobalPosition;
            _consecutiveObstacles = 0;
            _panicMultiplier = 1f;
            _jumpCooldown = 0; // Reseta cooldown
            _directionCommitTimer = 0; // Reseta timer de commit
            _isFakeCharging = false;
            _fakeChargeTimer = 0;

            // 20% de chance de fazer um fake charge
            if (GD.Randf() < 0.2f)
            {
                _isFakeCharging = true;
                _fakeChargeTimer = FAKE_CHARGE_DURATION;
                Goblin.PlayRandomAttackSound(); // Toca som de ataque durante fake charge
                GD.Print("[FLEE] FAKE CHARGE! Goblin will briefly charge at player");
            }

            // Atualiza label de estado, se existir
            var stateLabel = Goblin.GetNodeOrNull<Label>("StateLabel");
            if (stateLabel != null)
            {
                stateLabel.Text = "FLEE";
            }

            if (Goblin.TargetPlayer != null)
            {
                float directionToPlayer = Mathf.Sign(Goblin.TargetPlayer.GlobalPosition.X - Goblin.GlobalPosition.X);

                // Se está fazendo fake charge, vai em direção ao player
                if (_isFakeCharging)
                {
                    _currentFleeDirection = directionToPlayer;
                }
                else
                {
                    _currentFleeDirection = -directionToPlayer;
                }

                GD.Print($"[FLEE ENTER] Initial flee direction: {_currentFleeDirection} (away from player at {directionToPlayer}, fakeCharge={_isFakeCharging})");
            }
            else
            {
                _currentFleeDirection = Mathf.Sign(Goblin.Velocity.X);
                if (_currentFleeDirection == 0) _currentFleeDirection = 1;
            }
        }

        public override void PhysicsUpdate(double delta)
        {
            Goblin.ApplyGravity(delta);

            // CORREÇÃO: Atualiza cooldown do pulo
            if (_jumpCooldown > 0)
            {
                _jumpCooldown -= (float)delta;
            }

            // NOVO: Atualiza timer de commit de direção
            if (_directionCommitTimer > 0)
            {
                _directionCommitTimer -= (float)delta;
            }

            // NOVO: Atualiza fake charge timer
            if (_isFakeCharging)
            {
                _fakeChargeTimer -= (float)delta;
                if (_fakeChargeTimer <= 0)
                {
                    _isFakeCharging = false;
                    GD.Print("[FLEE] Fake charge ended, returning to normal flee");
                }
            }

            if (Goblin.IsPlayerSafe())
            {
                StateMachine.ChangeState("Idle");
                return;
            }

            if (Goblin.TargetPlayer != null)
            {
                float newFleeDirection = CalculateFleeDirection();

                // CORREÇÃO: Só muda direção se o timer de commit expirou
                if (Mathf.Abs(newFleeDirection - _currentFleeDirection) > 0.1f)
                {
                    if (_directionCommitTimer <= 0)
                    {
                        GD.Print($"[FLEE] Direction change: {_currentFleeDirection} -> {newFleeDirection}");
                        _currentFleeDirection = newFleeDirection;
                        _directionCommitTimer = DIRECTION_COMMIT_TIME; // Reseta o timer
                    }
                    // Removido log excessivo de "blocked by commit timer"
                }

                float currentSpeed = Goblin.FleeSpeed * _panicMultiplier;
                Goblin.Velocity = new Vector2(_currentFleeDirection * currentSpeed, Goblin.Velocity.Y);

                Goblin.AnimControllerInstance.UpdateFlip(_currentFleeDirection);

                // CORREÇÃO: Só tenta pular se o cooldown acabou
                if (_jumpCooldown <= 0)
                {
                    if (HandleObstacles())
                    {
                        _jumpCooldown = JUMP_COOLDOWN_TIME; // Inicia cooldown
                        return;
                    }

                    if (HandleStuckDetection(delta))
                    {
                        _jumpCooldown = JUMP_COOLDOWN_TIME; // Inicia cooldown
                        return;
                    }
                }

                _changeDirectionTimer += (float)delta;
            }
        }

        private float CalculateFleeDirection()
        {
            float directionToPlayer = Mathf.Sign(Goblin.TargetPlayer.GlobalPosition.X - Goblin.GlobalPosition.X);
            float fleeDirection = -directionToPlayer;

            float distanceToPlayer = Goblin.GlobalPosition.DistanceTo(Goblin.TargetPlayer.GlobalPosition);

            if (distanceToPlayer < 80f && Mathf.Abs(Goblin.Velocity.X) > 10f)
            {
                return _currentFleeDirection;
            }

            bool wallInFleeDirection = CheckWallInDirection(fleeDirection);
            bool gapInFleeDirection = CheckGapInDirection(fleeDirection);

            if (wallInFleeDirection || gapInFleeDirection)
            {
                float alternativeDirection = -fleeDirection;
                bool wallInAlternative = CheckWallInDirection(alternativeDirection);
                bool gapInAlternative = CheckGapInDirection(alternativeDirection);

                if (!wallInAlternative && !gapInAlternative)
                {
                    fleeDirection = alternativeDirection;
                }
            }

            return fleeDirection;
        }

        private bool HandleObstacles()
        {
            bool wallAhead = Goblin.WallCheck.IsColliding();
            bool gapAhead = !Goblin.FloorCheck.IsColliding();

            // DEBUG: Log what WallCheck is hitting
            if (wallAhead)
            {
                var collider = Goblin.WallCheck.GetCollider();
                if (collider is Node node)
                {
                    GD.Print($"[WALLCHECK] Colliding with: {node.Name} (Type: {node.GetType().Name})");

                    // Check collision layer
                    if (collider is CollisionObject2D collisionObj)
                    {
                        GD.Print($"[WALLCHECK] Raycast mask={Goblin.WallCheck.CollisionMask}, Object layer={collisionObj.CollisionLayer}");
                    }
                }
            }

            if (wallAhead || gapAhead)
            {
                // NOVO: Se é precipício, vira ao invés de pular
                if (gapAhead)
                {
                    bool isCliff = Goblin.IsCliff();

                    if (isCliff)
                    {
                        GD.Print($"[FLEE] CLIFF DETECTED! Reversing direction from {_currentFleeDirection} to {-_currentFleeDirection}");
                        // Inverte direção de fuga
                        _currentFleeDirection *= -1;
                        _consecutiveObstacles = 0; // Reseta pânico ao evitar precipício
                        _directionCommitTimer = DIRECTION_COMMIT_TIME * 2; // DOBRA o tempo de commit após evitar cliff
                        return false; // NÃO pula
                    }

                }

                // Se é buraco pequeno, pula normalmente
                _consecutiveObstacles++;
                _panicMultiplier = Mathf.Min(1.5f, 1f + (_consecutiveObstacles * 0.1f));

                // CORREÇÃO: Verifica se REALMENTE está no chão
                bool isOnFloor = Goblin.IsOnFloor();
                bool velocityOk = Mathf.Abs(Goblin.Velocity.Y) < 10f;

                if (isOnFloor && velocityOk)
                {
                    StateMachine.ChangeState("Jump");
                    return true;
                }
            }
            else
            {
                // Sem obstáculos à frente
                if (_consecutiveObstacles > 0)
                {
                    _consecutiveObstacles = Mathf.Max(0, _consecutiveObstacles - 1);
                    _panicMultiplier = 1f + (_consecutiveObstacles * 0.1f);
                }
            }

            return false;
        }

        private bool HandleStuckDetection(double delta)
        {
            _stuckTimer += (float)delta;

            if (_stuckTimer >= _checkStuckInterval)
            {
                float distanceMoved = Goblin.GlobalPosition.DistanceTo(_lastPosition);
                float minimumMovement = 10f;

                // CORREÇÃO: Verifica velocidade Y também para confirmar que está no chão
                if (distanceMoved < minimumMovement && Goblin.IsOnFloor() && Mathf.Abs(Goblin.Velocity.Y) < 10f)
                {
                    StateMachine.ChangeState("Jump");
                    _consecutiveObstacles++;
                    _stuckTimer = 0;
                    return true;
                }

                if (Mathf.Abs(Goblin.Velocity.X) < 20f && Goblin.IsOnFloor() && Mathf.Abs(Goblin.Velocity.Y) < 10f)
                {
                    StateMachine.ChangeState("Jump");
                    _stuckTimer = 0;
                    return true;
                }

                _lastPosition = Goblin.GlobalPosition;
                _stuckTimer = 0;
            }

            return false;
        }

        private bool CheckWallInDirection(float direction)
        {
            Vector2 originalTarget = Goblin.WallCheck.TargetPosition;

            Goblin.WallCheck.TargetPosition = new Vector2(20 * direction, 0);
            Goblin.WallCheck.ForceRaycastUpdate();
            bool hasWall = Goblin.WallCheck.IsColliding();

            Goblin.WallCheck.TargetPosition = originalTarget;

            return hasWall;
        }

        private bool CheckGapInDirection(float direction)
        {
            Vector2 originalTarget = Goblin.FloorCheck.TargetPosition;

            Goblin.FloorCheck.TargetPosition = new Vector2(25 * direction, 30);
            Goblin.FloorCheck.ForceRaycastUpdate();
            bool hasGap = !Goblin.FloorCheck.IsColliding();

            Goblin.FloorCheck.TargetPosition = originalTarget;

            return hasGap;
        }

        public override void Exit()
        {
            _panicMultiplier = 1f;
            _consecutiveObstacles = 0;
        }
    }
}