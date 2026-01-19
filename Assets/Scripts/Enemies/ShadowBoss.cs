using Godot;
using System;
using EcoDoFarolCentral;

namespace EcoDoFarolCentral
{
    public partial class ShadowBoss : Actor
    {
        public enum BossState { Idle, Wander, Chase, Attack, Hurt, Dead }

        [ExportGroup("Movement")]
        [Export] public float WanderSpeed = 80.0f;
        [Export] public float ChaseSpeed = 150.0f;
        [Export] public float WanderTime = 2.0f;       // Tempo andando em uma direção
        [Export] public float IdleTime = 1.5f;         // Tempo parado entre wanders

        [ExportGroup("Combat")]
        [Export] public float ContactDamage = 15.0f;   // Dano ao encostar no player
        [Export] public float AttackCooldown = 1.5f;   // Cooldown por tipo de ataque (em segundos)
        [Export] public float AttackRange = 80.0f;     // Distância para ataques corpo a corpo
        [Export] public float Attack3Range = 200.0f;   // Distância máxima para usar attack3 (modo sonic)
        [Export] public float Attack1Damage = 25.0f;   // Dano do ataque 1/2
        [Export] public float Attack3Damage = 30.0f;   // Dano do ataque 3 (modo sonic)
        [Export] public float Attack3Speed = 400.0f;   // Velocidade do dash no attack3
        [Export] public float BossAttackCooldown = 2.0f; // Cooldown entre ataques do boss

        private BossState _currentState = BossState.Idle;
        private AnimatedSprite2D _sprite;
        private AnimationPlayer _animPlayer;
        private RayCast2D _groundCheck;
        private Area2D _hurtbox;
        private Area2D _hitbox;
        private CollisionShape2D _collisionShape;
        private Player _player;
        private float _stateTimer = 0f;
        private float _moveDirection = 1f;            // 1 = direita, -1 = esquerda
        private float _turnCooldown = 0f;             // Cooldown para evitar viradas repetidas
        private float _bossAttackCooldown = 0f;       // Cooldown do ataque do boss
        private bool _isAttacking = false;
        private bool _isInSonicMode = false;          // Flag para o attack3 (modo sonic)
        private bool _hasHitPlayerThisAttack = false; // Evita múltiplos hits no mesmo ataque
        private const float TURN_COOLDOWN_TIME = 0.3f;

        // Dicionário para rastrear cooldown de cada tipo de ataque
        private System.Collections.Generic.Dictionary<string, double> _attackCooldowns = new();

        public override void _Ready()
        {
            AddToGroup("enemies");

            _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            _groundCheck = GetNode<RayCast2D>("RayCast2D");
            _hurtbox = GetNode<Area2D>("hurtbox");
            _hitbox = GetNode<Area2D>("hitbox");
            _collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

            // Configurações iniciais
            _stateTimer = IdleTime;
            _currentState = BossState.Idle;

            // Configura o RayCast para apontar para baixo (detectar chão)
            _groundCheck.TargetPosition = new Vector2(0, 30);
            _groundCheck.Enabled = true;

            // Conecta o sinal de contato da hurtbox
            if (_hurtbox != null)
            {
                _hurtbox.BodyEntered += OnBodyEntered;
            }

            // Conecta sinal da hitbox para aplicar dano dos ataques
            if (_hitbox != null)
            {
                _hitbox.AreaEntered += OnHitboxAreaEntered;
            }

            // Conecta sinal de animação terminada
            _animPlayer.AnimationFinished += OnAnimationFinished;

        }

        public override void _PhysicsProcess(double delta)
        {
            // Aplica gravidade
            ApplyGravity(delta);

            // Atualiza cooldown de virada
            if (_turnCooldown > 0)
                _turnCooldown -= (float)delta;

            // Atualiza cooldown de ataque do boss
            if (_bossAttackCooldown > 0)
                _bossAttackCooldown -= (float)delta;

            // Busca referência do player se não tiver
            if (_player == null)
            {
                _player = GetTree().GetFirstNodeInGroup("player") as Player;
            }

            // Processa estado atual
            switch (_currentState)
            {
                case BossState.Idle:
                    ProcessIdle(delta);
                    break;
                case BossState.Wander:
                    ProcessWander(delta);
                    break;
                case BossState.Attack:
                    ProcessAttack(delta);
                    break;
                case BossState.Dead:
                    // Não faz nada
                    break;
            }

            UpdateAnimations();
            MoveAndSlide();
        }

        private void ProcessIdle(double delta)
        {
            // Para movimento
            Velocity = new Vector2(0, Velocity.Y);

            // Verifica se pode atacar o player
            if (IsPlayerInAttackRange() && _bossAttackCooldown <= 0)
            {
                StartAttack();
                return;
            }

            _stateTimer -= (float)delta;

            if (_stateTimer <= 0)
            {
                // Escolhe direção aleatória e começa a andar
                _moveDirection = GD.Randf() > 0.5f ? 1f : -1f;
                _stateTimer = WanderTime;
                _currentState = BossState.Wander;
                UpdateGroundCheckPosition();
            }
        }

        private void ProcessWander(double delta)
        {
            // Atualiza posição do RayCast baseado na direção
            UpdateGroundCheckPosition();

            // Força o RayCast a atualizar neste frame
            _groundCheck.ForceRaycastUpdate();

            // Só verifica virada se o cooldown permitir
            if (_turnCooldown <= 0)
            {
                bool shouldTurn = false;

                // Verifica se há chão à frente
                if (!_groundCheck.IsColliding())
                {
                    shouldTurn = true;
                }
                // Verifica se bateu em parede
                else if (IsOnWall())
                {
                    shouldTurn = true;
                }

                if (shouldTurn)
                {
                    _moveDirection *= -1;
                    _turnCooldown = TURN_COOLDOWN_TIME;
                    UpdateGroundCheckPosition();
                }
            }

            // Move na direção atual
            Velocity = new Vector2(_moveDirection * WanderSpeed, Velocity.Y);

            // Atualiza flip do sprite
            _sprite.FlipH = _moveDirection < 0;

            _stateTimer -= (float)delta;

            if (_stateTimer <= 0)
            {
                // Volta para idle
                _stateTimer = IdleTime;
                _currentState = BossState.Idle;
            }
        }

        /// <summary>
        /// Posiciona o RayCast à frente do boss para detectar abismos
        /// </summary>
        private void UpdateGroundCheckPosition()
        {
            // Move o raycast para a frente do boss baseado na direção
            float xOffset = 30f * _moveDirection;
            _groundCheck.Position = new Vector2(xOffset, 10);
            // Aponta para baixo para detectar o chão
            _groundCheck.TargetPosition = new Vector2(0, 40);
        }

        /// <summary>
        /// Verifica se o player está no range de ataque (qualquer ataque)
        /// </summary>
        private bool IsPlayerInAttackRange()
        {
            if (_player == null) return false;
            float distance = GlobalPosition.DistanceTo(_player.GlobalPosition);
            // Attack3Range é o range máximo (modo sonic), AttackRange é para ataques normais
            return distance <= Attack3Range;
        }
        /// <summary>
        /// Verifica se o player está atrás do boss
        /// </summary>
        private bool IsPlayerBehind()
        {
            if (_player == null) return false;

            // Se o boss está olhando para a direita (_moveDirection > 0), player atrás = player.X < boss.X
            // Se o boss está olhando para a esquerda (_moveDirection < 0), player atrás = player.X > boss.X
            bool playerOnLeft = _player.GlobalPosition.X < GlobalPosition.X;
            bool bossFacingRight = _moveDirection > 0;

            return (bossFacingRight && playerOnLeft) || (!bossFacingRight && !playerOnLeft);
        }

        /// <summary>
        /// Inicia um ataque
        /// </summary>
        private void StartAttack()
        {
            _currentState = BossState.Attack;
            _isAttacking = true;
            _hasHitPlayerThisAttack = false;
            _isInSonicMode = false;
            Velocity = new Vector2(0, Velocity.Y);

            if (_player == null) return;

            float distanceToPlayer = GlobalPosition.DistanceTo(_player.GlobalPosition);

            // Attack3 - DESABILITADO
            /*
            if (distanceToPlayer > AttackRange && distanceToPlayer <= Attack3Range && HasGroundAhead())
            {
                _isInSonicMode = true;
                _moveDirection = _player.GlobalPosition.X > GlobalPosition.X ? 1f : -1f;
                _sprite.FlipH = _moveDirection < 0;
                UpdateHitboxDirection();
                _animPlayer.Play("attack3");
            }
            else
            */
            // Attack2 - se o player está atrás do boss (próximo)
            if (IsPlayerBehind())
            {
                // Attack2 ataca para trás, então a hitbox deve estar na direção oposta
                _animPlayer.Play("attack2");
            }
            // Attack1 - ataque padrão para frente
            else
            {
                _moveDirection = _player.GlobalPosition.X > GlobalPosition.X ? 1f : -1f;
                _sprite.FlipH = _moveDirection < 0;
                UpdateHitboxDirection();
                _animPlayer.Play("attack1");
            }
        }

        /// <summary>
        /// Espelha a hitbox na direção que o boss está olhando
        /// </summary>
        private void UpdateHitboxDirection()
        {
            if (_hitbox == null) return;

            // Espelha a posição X da hitbox baseado na direção
            float xPos = Mathf.Abs(_hitbox.Position.X);
            _hitbox.Position = new Vector2(xPos * _moveDirection, _hitbox.Position.Y);

            // Também espelha a escala para inverter o CollisionPolygon
            _hitbox.Scale = new Vector2(_moveDirection, 1);
        }

        /// <summary>
        /// Verifica se há chão à frente na direção do player
        /// </summary>
        private bool HasGroundAhead()
        {
            if (_player == null) return true;

            // Posiciona raycast na direção do player
            float checkDirection = _player.GlobalPosition.X > GlobalPosition.X ? 1f : -1f;
            _groundCheck.Position = new Vector2(60f * checkDirection, 10);
            _groundCheck.TargetPosition = new Vector2(0, 50);
            _groundCheck.ForceRaycastUpdate();

            return _groundCheck.IsColliding();
        }

        /// <summary>
        /// Processa o estado de ataque
        /// </summary>
        private void ProcessAttack(double delta)
        {
            // Se está no modo sonic (attack3), move rapidamente na direção do player
            if (_isInSonicMode)
            {
                // Verifica se há chão à frente antes de continuar o dash
                UpdateGroundCheckPosition();
                _groundCheck.ForceRaycastUpdate();

                if (!_groundCheck.IsColliding() || IsOnWall())
                {
                    // Abismo ou parede detectada! Para o sonic mode
                    _isInSonicMode = false;
                    Velocity = new Vector2(0, Velocity.Y);
                }
                else
                {
                    Velocity = new Vector2(_moveDirection * Attack3Speed, Velocity.Y);
                }
            }
            else
            {
                // Para durante ataques normais
                Velocity = new Vector2(0, Velocity.Y);
            }

            // O ataque termina quando a animação terminar (callback OnAnimationFinished)
        }

        /// <summary>
        /// Callback quando uma animação termina
        /// </summary>
        private void OnAnimationFinished(StringName animName)
        {
            if (animName == "attack1" || animName == "attack2" || animName == "attack3")
            {
                _isAttacking = false;
                _isInSonicMode = false;
                _hasHitPlayerThisAttack = false;
                _bossAttackCooldown = BossAttackCooldown;
                _currentState = BossState.Idle;
                _stateTimer = 0.5f; // Pequena pausa após atacar
                Velocity = new Vector2(0, Velocity.Y); // Para o movimento do sonic mode
            }
        }

        /// <summary>
        /// Callback quando a hitbox do boss acerta algo
        /// </summary>
        private void OnHitboxAreaEntered(Area2D area)
        {
            if (_currentState == BossState.Dead) return;
            if (!_isAttacking) return; // Só causa dano durante ataque
            if (_hasHitPlayerThisAttack) return; // Já acertou neste ataque

            // Verifica se é a hurtbox do player
            if (area.GetParent() is Player player)
            {
                float damage = _isInSonicMode ? Attack3Damage : Attack1Damage;
                player.TakeDamage(damage, GlobalPosition);
                _hasHitPlayerThisAttack = true; // Marca que já acertou
            }
        }

        private void UpdateAnimations()
        {
            // Não atualiza animações se estiver morto e a animação de morte já terminou
            if (_currentState == BossState.Dead && !_animPlayer.IsPlaying())
            {
                return; // Mantém no último frame da animação de morte
            }

            // Durante ataque, a animação é controlada pelo StartAttack
            if (_currentState == BossState.Attack)
            {
                return;
            }

            string animName = _currentState switch
            {
                BossState.Idle => "idle",
                BossState.Wander => "walk",
                BossState.Hurt => "hurt",
                BossState.Dead => "death",
                _ => "idle"
            };

            if (_animPlayer.CurrentAnimation != animName)
            {
                _animPlayer.Play(animName);
            }
        }

        public override void TakeDamage(float amount, Vector2? sourcePosition = null)
        {
            // Chamada genérica sem identificador de ataque - usa o método base
            TakeDamageFromAttack(amount, "generic", sourcePosition);
        }

        /// <summary>
        /// Aplica dano de um tipo específico de ataque, respeitando o cooldown por ataque.
        /// </summary>
        /// <param name="amount">Quantidade de dano</param>
        /// <param name="attackId">Identificador único do ataque (ex: "attack_lvl1")</param>
        /// <param name="sourcePosition">Posição da fonte do dano</param>
        public void TakeDamageFromAttack(float amount, string attackId, Vector2? sourcePosition = null)
        {
            if (_currentState == BossState.Dead) return;

            // Verifica se este ataque específico está em cooldown
            double currentTime = Time.GetTicksMsec() / 1000.0; // Tempo atual em segundos
            if (_attackCooldowns.TryGetValue(attackId, out double lastHitTime))
            {
                if (currentTime - lastHitTime < AttackCooldown)
                {
                    return; // Este ataque ainda está em cooldown
                }
            }

            // Registra o timestamp deste ataque
            _attackCooldowns[attackId] = currentTime;

            base.TakeDamage(amount, sourcePosition);


            if (CurrentHealth <= 0)
            {
                Die();
            }
            else
            {
                // Entra no estado de hurt
                _currentState = BossState.Hurt;
                Velocity = new Vector2(0, Velocity.Y); // Para movimento

                // Volta ao idle após a animação de hurt (0.3s)
                GetTree().CreateTimer(0.3).Timeout += () =>
                {
                    if (_currentState == BossState.Hurt) // Só muda se ainda estiver em hurt
                    {
                        _currentState = BossState.Idle;
                        _stateTimer = IdleTime;
                    }
                };
            }
        }

        public override void Die()
        {
            base.Die();
            _currentState = BossState.Dead;
            Velocity = Vector2.Zero;

            // Muda o collision layer para que o player possa atravessar o corpo
            // Mantém colisão com terreno (layer 1) mas remove da camada que o player detecta
            CollisionLayer = 0; // Remove de todas as layers de colisão

            // Desabilita a hurtbox para não detectar mais contato
            if (_hurtbox != null)
                _hurtbox.SetDeferred(Area2D.PropertyName.Monitoring, false);

        }

        /// <summary>
        /// Chamado quando um corpo (CharacterBody2D) entra na hurtbox do boss
        /// </summary>
        private void OnBodyEntered(Node2D body)
        {
            if (_currentState == BossState.Dead) return;

            // Verifica se o corpo é o player
            if (body is Player player)
            {
                player.TakeDamage(ContactDamage, GlobalPosition);
            }
        }
    }
}
