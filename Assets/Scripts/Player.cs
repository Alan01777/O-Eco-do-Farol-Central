using Godot;

namespace EcoDoFarolCentral
{
    public partial class Player : Actor
    {
        public enum PlayerStates { Idle, Running, Jumping, DoubleJump, Attacking, JumpAttack, Dashing, Hurt, Dead, Cast }

        private PlayerStates _lastState;
        public PlayerStates CurrentStateEnum { get; set; } = PlayerStates.Idle;

        public StateMachine StateMachineInstance { get; private set; }
        public AnimationController AnimControllerInstance { get; private set; }
        public AnimatedSprite2D SpriteInstance => _sprite;
        protected AnimatedSprite2D _sprite;
        private CombatAttackData[] _comboAttacks;
        private CombatAttackData _jumpAttack;

        public bool IsAttacking { get; set; } = false;
        public int AttackCombo { get; set; } = 1;
        public bool ComboQueued { get; set; } = false;

        public bool CanDoubleJump { get; set; } = false;

        // Propriedades de dash
        public bool IsDashing { get; private set; } = false;
        public float DashTimer { get; private set; } = 0f;
        public float DashCooldownTimer { get; private set; } = 0f;
        public float DashDirection { get; private set; } = 1f;

        // Variáveis de dano (recebido)
        [Export] public float HurtDuration = 0.3f; // Duração Iframes
        [Export] public Vector2 KnockbackIntensity = new Vector2(300, -200);
        public float HurtTimer { get; private set; } = 0f;
        public float DamageSourceDirection { get; private set; } = 0f; // Direção que o jogador olha quando toma dano

        // Variáveis de combate
        [Export] public float Combo1Damage = 20f;
        [Export] public float Combo2Damage = 30f;
        [Export] public float Combo3Damage = 50f;
        [Export] public float JumpAttackDamage = 25f;

        // Fireball
        [Export] public PackedScene FireballScene { get; set; }
        public Marker2D CastPoint { get; private set; }

        // Sistema de habilidades
        public PlayerAbilities Abilities { get; private set; }

        // Sistema de Safe Ground (última posição segura)
        private Vector2 _lastSafePosition;
        private double _safePosTimer = 0;
        private const double SAFE_POS_INTERVAL = 0.2; // Atualiza a cada 0.2s
        private Area2D _swordHitBox;
        private CollisionPolygon2D _hitBoxShape1;
        private CollisionPolygon2D _hitBoxShape2;
        private CollisionPolygon2D _hitBoxShape3;
        private CollisionPolygon2D _hitBoxShapeJump;

        public override void _Ready()
        {
            AddToGroup("player");
            _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            _invincibilityTimer = GetNodeOrNull<Timer>("Timer to take damage");

            AnimControllerInstance = new AnimationController();
            AnimControllerInstance = new AnimationController();
            AnimControllerInstance.Initialize(_sprite);
            _sprite.AnimationFinished += OnAnimationFinished;

            // Configura CastPoint (Ponto de Disparo) da fireball
            CastPoint = GetNodeOrNull<Marker2D>("CastPoint");
            if (CastPoint == null)
            {
                // Cria se não existir
                CastPoint = new Marker2D();
                CastPoint.Name = "CastPoint";
                CastPoint.Position = new Vector2(20, -10); // Offset padrão
                AddChild(CastPoint);
            }

            // Configura dados de ataque
            _comboAttacks = new CombatAttackData[]
            {
                new CombatAttackData("attack_lvl1", Combo1Damage, 0, "attack_lvl1"),
                new CombatAttackData("attack_lvl2", Combo2Damage, 0, "attack_lvl2"),
                new CombatAttackData("attack_lvl3", Combo3Damage, 0, "attack_lvl3")
            };
            _jumpAttack = new CombatAttackData("jump_attack", JumpAttackDamage, 0, "jump_attack");

            // Configura hitboxes da espada
            _swordHitBox = GetNodeOrNull<Area2D>("Sword HitBox");
            if (_swordHitBox != null)
            {
                _hitBoxShape1 = _swordHitBox.GetNodeOrNull<CollisionPolygon2D>("attack_lvl1");
                _hitBoxShape2 = _swordHitBox.GetNodeOrNull<CollisionPolygon2D>("attack_lvl2");
                _hitBoxShape3 = _swordHitBox.GetNodeOrNull<CollisionPolygon2D>("attack_lvl3");
                _hitBoxShapeJump = _swordHitBox.GetNodeOrNull<CollisionPolygon2D>("jump_attack");
                _swordHitBox.AreaEntered += OnSwordHitBoxAreaEntered;
            }

            // Configura hurtbox do jogador para receber dano
            var hurtBox = GetNodeOrNull<Area2D>("HurtBox");
            if (hurtBox != null)
            {
                hurtBox.AreaEntered += OnHurtBoxAreaEntered;
            }
            else
            {
                GD.PrintErr("[PLAYER] HurtBox not found! Player won't be able to take damage!");
            }

            // Inicializa sistema de habilidades
            Abilities = new PlayerAbilities();
            Abilities.Name = "Abilities";
            AddChild(Abilities);
            GD.Print("[PLAYER] Abilities system initialized");


            // Configura Máquina de Estados (FSM)
            // TODO: Deixar isso mais elegante (tendencia é crescer como um monstro)
            StateMachineInstance = new StateMachine();

            var idle = new PlayerIdleState();
            var run = new PlayerRunningState();
            var jump = new PlayerJumpingState();
            var attack = new PlayerAttackingState();
            var dash = new PlayerDashingState();
            var hurt = new PlayerHurtState();
            var cast = new PlayerCastState();

            idle.Initialize(this, StateMachineInstance);
            run.Initialize(this, StateMachineInstance);
            jump.Initialize(this, StateMachineInstance);
            attack.Initialize(this, StateMachineInstance);
            dash.Initialize(this, StateMachineInstance);
            hurt.Initialize(this, StateMachineInstance);
            cast.Initialize(this, StateMachineInstance);

            StateMachineInstance.AddState("Idle", idle);
            StateMachineInstance.AddState("Running", run);
            StateMachineInstance.AddState("Jumping", jump);
            StateMachineInstance.AddState("Attacking", attack);
            StateMachineInstance.AddState("Dashing", dash);
            StateMachineInstance.AddState("Hurt", hurt);
            StateMachineInstance.AddState("Cast", cast);

            StateMachineInstance.ChangeState("Idle");

            // Define última posição segura
            _lastSafePosition = GlobalPosition;
        }

        public override void _PhysicsProcess(double delta)
        {
            UpdateSafePosition(delta);

            if (CurrentStateEnum == PlayerStates.Dead)
            {
                ApplyGravity(delta);
                UpdateAnimations();
                MoveAndSlide();
                return;
            }

            // Cooldowns
            if (DashCooldownTimer > 0)
                DashCooldownTimer -= (float)delta;

            if (HurtTimer > 0)
            {
                HurtTimer -= (float)delta;
                if (HurtTimer <= 0)
                {
                    if (CurrentStateEnum == PlayerStates.Hurt)
                        StateMachineInstance.ChangeState("Idle");
                }
            }

            StateMachineInstance.PhysicsUpdate(delta);

            UpdateAnimations();
            MoveAndSlide();
        }

        public override void _Input(InputEvent @event)
        {
            if (CurrentStateEnum == PlayerStates.Dead) return;
            StateMachineInstance.HandleInput(@event);
        }

        public void HandleMovement(float direction = 0, float speedMultiplier = 1)
        {
            if (direction == 0)
                direction = Input.GetAxis("ui_left", "ui_right");

            Vector2 velocity = Velocity;

            if (direction != 0)
            {
                velocity.X = direction * Speed * speedMultiplier;
                _sprite.FlipH = direction < 0;

                // Espelha hitboxes quando virado (flip)
                if (_swordHitBox != null)
                {
                    var s = _swordHitBox.Scale;
                    s.X = Mathf.Abs(s.X) * (direction < 0 ? -1 : 1);
                    _swordHitBox.Scale = s;
                }
            }
            else
                velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);

            Velocity = velocity;
        }

        public void DoJump(bool doubleJump = false)
        {
            Vector2 velocity = Velocity;
            velocity.Y = doubleJump ? DoubleJumpVelocity : JumpVelocity;
            Velocity = velocity;
            if (!doubleJump) CanDoubleJump = true;
            else CanDoubleJump = false;
        }

        public void StartDash()
        {
            IsDashing = true;
            DashTimer = DashDuration;
            DashCooldownTimer = DashCooldown;
            DashDirection = _sprite.FlipH ? -1f : 1f;

            Vector2 velocity = Velocity;
            velocity.X = DashDirection * DashSpeed;
            velocity.Y = 0;
            Velocity = velocity;
        }

        public void EndDash()
        {
            IsDashing = false;
            var v = Velocity;
            v.X = 0;
            Velocity = v;
        }

        public bool CanDash() => !IsDashing && DashCooldownTimer <= 0;

        public void QueueCombo()
        {
            ComboQueued = true;
        }

        public void AdvanceCombo()
        {
            ComboQueued = false;
            AttackCombo = AttackCombo % 3 + 1;
        }

        public void ResetCombo()
        {
            ComboQueued = false;
            AttackCombo = 1;
        }


        private void OnAnimationFinished()
        {
            StateMachineInstance.OnAnimationFinished();
        }

        public override void Die()
        {
            base.Die();
            CurrentStateEnum = PlayerStates.Dead;
            IsDashing = false;
            IsAttacking = false;
            HurtTimer = 0;
            DisableAllHitBoxes();
        }

        public override void TakeDamage(float amount, Vector2? sourcePosition = null)
        {
            base.TakeDamage(amount, sourcePosition);

            if (CurrentHealth > 0 && sourcePosition != null)
            {
                HurtTimer = HurtDuration;
                IsDashing = false;
                IsAttacking = false;
                DisableAllHitBoxes();

                // Calcula a direção do knockback
                float knockbackDir = Mathf.Sign(GlobalPosition.X - sourcePosition.Value.X);

                // Guarda a direção do dano (opposite of knockback)
                DamageSourceDirection = -knockbackDir;

                // knockback horizontal com minimal vertical
                Velocity = new Vector2(knockbackDir * KnockbackIntensity.X, -50f); // pulinho pra cima pra dar efeito visual

                // Transição para o estado de dano (hurt) via FSM
                StateMachineInstance.ChangeState("Hurt");
            }
        }

        private void OnSwordHitBoxAreaEntered(Area2D area)
        {
            // Previne que o jogador tome dano da hitbox da propria espada
            if (area.GetParent() == this) return;

            // Qualquer area detectada aqui deve ser uma HurtBox devido às camadas de colisão definidas
            if (area.GetParent() is Actor enemy)
            {
                CombatAttackData currentAttack = CurrentStateEnum == PlayerStates.JumpAttack ? _jumpAttack : _comboAttacks[AttackCombo - 1];
                float damage = currentAttack.Damage;

                enemy.TakeDamage(damage, GlobalPosition);
            }
        }

        private void OnHurtBoxAreaEntered(Area2D area)
        {
            // Previne que o jogador tome dano da hitbox da propria espada
            if (area.GetParent() == this) return;

            // Verifica se a area detectada é uma HurtBox
            Node parent = area.GetParent();

            if (parent is Actor enemy)
            {
                // Verifica se é dano de contato
                if (area.Name.ToString().Contains("HitBox"))
                {
                    float damage = 10f; // Dano de contato padrão
                    TakeDamage(damage, enemy.GlobalPosition);
                }
            }
        }

        public void DisableAllHitBoxes()
        {
            if (_hitBoxShape1 != null) _hitBoxShape1.SetDeferred(CollisionPolygon2D.PropertyName.Disabled, true);
            if (_hitBoxShape2 != null) _hitBoxShape2.SetDeferred(CollisionPolygon2D.PropertyName.Disabled, true);
            if (_hitBoxShape3 != null) _hitBoxShape3.SetDeferred(CollisionPolygon2D.PropertyName.Disabled, true);
            if (_hitBoxShapeJump != null) _hitBoxShapeJump.SetDeferred(CollisionPolygon2D.PropertyName.Disabled, true);
        }

        public void UpdateAnimations()
        {
            // Não inverte automaticamente durante o estado de dano - o jogador deve se virar para o atacante
            // TODO: Melhorar isso aqui
            if (CurrentStateEnum != PlayerStates.Hurt)
            {
                AnimControllerInstance.UpdateFlip(Velocity.X);
            }

            CombatAttackData currentAttack = CurrentStateEnum switch
            {
                PlayerStates.JumpAttack => _jumpAttack,
                PlayerStates.Attacking => _comboAttacks[AttackCombo - 1],
                _ => default
            };

            string animName = CurrentStateEnum switch
            {
                PlayerStates.JumpAttack => currentAttack.AnimationName,
                PlayerStates.Attacking => currentAttack.AnimationName,
                PlayerStates.Jumping => "jump",
                PlayerStates.DoubleJump => "jump",
                PlayerStates.Running => "run",
                PlayerStates.Dashing => "dash",
                PlayerStates.Dead => "death",
                PlayerStates.Hurt => "hurt",
                PlayerStates.Cast => "special_1",
                _ => "idle"
            };

            // Animações que devem forçar o restart
            bool forceRestart = (CurrentStateEnum == PlayerStates.DoubleJump && _lastState != PlayerStates.DoubleJump) ||
                                (CurrentStateEnum == PlayerStates.Attacking && _lastCombo != AttackCombo);

            AnimControllerInstance.Play(animName, forceRestart);

            _lastState = CurrentStateEnum;
            _lastCombo = AttackCombo;
        }

        private int _lastCombo = 1;

        public void UpdateHitBox()
        {
            DisableAllHitBoxes();

            if (!IsAttacking) return;

            if (CurrentStateEnum == PlayerStates.JumpAttack)
            {
                if (_hitBoxShapeJump != null) _hitBoxShapeJump.SetDeferred(CollisionPolygon2D.PropertyName.Disabled, false);
            }
            else if (CurrentStateEnum == PlayerStates.Attacking)
            {
                // Ativa a hitbox dos ataques baseada no combo
                switch (AttackCombo)
                {
                    case 1: if (_hitBoxShape1 != null) _hitBoxShape1.SetDeferred(CollisionPolygon2D.PropertyName.Disabled, false); break;
                    case 2: if (_hitBoxShape2 != null) _hitBoxShape2.SetDeferred(CollisionPolygon2D.PropertyName.Disabled, false); break;
                    case 3: if (_hitBoxShape3 != null) _hitBoxShape3.SetDeferred(CollisionPolygon2D.PropertyName.Disabled, false); break;
                }
            }
        }

        private void UpdateSafePosition(double delta)
        {
            if (IsOnFloor())
            {
                _safePosTimer += delta;
                if (_safePosTimer >= SAFE_POS_INTERVAL)
                {
                    _lastSafePosition = GlobalPosition;
                    _safePosTimer = 0;
                }
            }
        }

        public void RespawnAtSafePosition()
        {
            GlobalPosition = _lastSafePosition;
            Velocity = Vector2.Zero;
            // Reseta o estado para Idle para evitar ficar preso em estados
            if (StateMachineInstance != null)
            {
                StateMachineInstance.ChangeState("Idle");
            }
            GD.Print($"[PLAYER] Respawned at safe position: {_lastSafePosition}");
        }

        public void CastFireball()
        {
            if (FireballScene == null)
            {
                GD.PrintErr("[PLAYER] FireballScene is not set!");
                return;
            }

            var fireball = FireballScene.Instantiate<Fireball>();
            GetTree().Root.AddChild(fireball); // Adiciona à root para ser independente

            // Determina a direção baseada na flip do sprite
            float direction = _sprite.FlipH ? -1f : 1f;

            // Define a posição um pouco à frente
            fireball.GlobalPosition = CastPoint.GlobalPosition;

            fireball.Setup(new Vector2(direction, 0));
        }

        // Helper para inverter o cast point se necessário
        public override void _Process(double delta)
        {
            if (CastPoint != null)
            {
                float xPos = Mathf.Abs(CastPoint.Position.X);
                CastPoint.Position = new Vector2(_sprite.FlipH ? -xPos : xPos, CastPoint.Position.Y);
            }
        }
    }
}