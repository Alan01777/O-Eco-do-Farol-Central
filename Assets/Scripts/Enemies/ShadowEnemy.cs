using Godot;

namespace EcoDoFarolCentral
{
    public partial class ShadowEnemy : Actor
    {
        public enum EnemyStates { Sleeping, Awakening, Idle, Chasing, GoingToSleep, Dead, Hurt, Attacking }
        public EnemyStates CurrentStateEnum { get; set; } = EnemyStates.Sleeping;

        public StateMachine StateMachineInstance { get; private set; }
        public AnimationController AnimControllerInstance { get; private set; }
        public AnimatedSprite2D SpriteInstance => _sprite;

        private float _attackCooldownTimer = 0f;
        [Export] public float HurtDuration = 0.4f;
        [Export] public Vector2 KnockbackIntensity = new Vector2(150f, -50f);
        [Export] public float DetectionRange = 250f;

        private AnimatedSprite2D _sprite;
        private Timer _sleepTimer;
        public Player TargetPlayer { get; private set; }

        public float NextAttackRange => GD.Randf() > 0.5f ? Attack1Range : Attack2Range; // Escolhe aleatoriamente entre Attack1Range e Attack2Range

        [Export] public int ContactDamage = 10;
        [Export] public float AttackCooldown = 1.5f;

        // Cada ataque tem um range diferente
        [Export] public float Attack1Damage = 15f;
        [Export] public float Attack1Range = 60f;
        [Export] public float Attack2Damage = 25f;
        [Export] public float Attack2Range = 80f;

        private CombatAttackData _attack1;
        private CombatAttackData _attack2;
        private CombatAttackData _currentAttack;

        private Area2D _attackArea;
        private CollisionPolygon2D _attackShape1;
        private CollisionPolygon2D _attackShape2;

        public override void _Ready()
        {
            _attack1 = new CombatAttackData("attack_1", Attack1Damage, Attack1Range, "AttackArea");
            _attack2 = new CombatAttackData("attack_2", Attack2Damage, Attack2Range, "AttackArea");
            _currentAttack = _attack1;

            _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            AnimControllerInstance = new AnimationController();
            AnimControllerInstance.Initialize(_sprite);
            _sprite.AnimationFinished += OnAnimationFinished;

            _sleepTimer = GetNode<Timer>("Timer");
            _sleepTimer.Timeout += OnTimerTimeout;

            var contactHitBox = GetNodeOrNull<Area2D>("HitBox") ?? GetNodeOrNull<Area2D>("Hitbox") ?? GetNodeOrNull<Area2D>("hitbox");

            // Fallback: Busca nos filhos
            if (contactHitBox == null)
            {
                foreach (var child in GetChildren())
                {
                    if (child is Area2D area && (area.Name.ToString().ToLower().Contains("hitbox") || area.Name.ToString().ToLower().Contains("hit_box")))
                    {
                        contactHitBox = area;
                        break;
                    }
                }
            }

            if (contactHitBox != null)
            {
                contactHitBox.BodyEntered += OnHitBoxBodyEntered;
            }

            // Referência ao player no grupo "player" 
            var players = GetTree().GetNodesInGroup("player");
            if (players.Count > 0) TargetPlayer = players[0] as Player;

            // Configura hitboxes dos ataques
            _attackArea = GetNodeOrNull<Area2D>("AttackArea") ?? GetNodeOrNull<Area2D>("Sword Hit Box");
            if (_attackArea != null)
            {
                _attackShape1 = _attackArea.GetNodeOrNull<CollisionPolygon2D>("attack_lvl1");
                _attackShape2 = _attackArea.GetNodeOrNull<CollisionPolygon2D>("attack_lvl2");
                _attackArea.AreaEntered += OnAttackAreaAreaEntered;
                DisableAllHitBoxes();
            }

            // Configura FSM
            StateMachineInstance = new StateMachine();

            var sleep = new EnemySleepState();
            var awaken = new EnemyAwakenState();
            var idle = new EnemyIdleState();
            var chase = new EnemyChaseState();
            var attack = new EnemyAttackState();
            var hurt = new EnemyHurtState();

            sleep.Initialize(this, StateMachineInstance);
            awaken.Initialize(this, StateMachineInstance);
            idle.Initialize(this, StateMachineInstance);
            chase.Initialize(this, StateMachineInstance);
            attack.Initialize(this, StateMachineInstance);
            hurt.Initialize(this, StateMachineInstance);

            StateMachineInstance.AddState("Sleep", sleep);
            StateMachineInstance.AddState("Awaken", awaken);
            StateMachineInstance.AddState("Idle", idle);
            StateMachineInstance.AddState("Chase", chase);
            StateMachineInstance.AddState("Attack", attack);
            StateMachineInstance.AddState("Hurt", hurt);

            StateMachineInstance.ChangeState("Sleep");
        }

        private void OnTimerTimeout()
        {
            if (CurrentStateEnum != EnemyStates.Sleeping && CurrentStateEnum != EnemyStates.GoingToSleep)
            {
                StateMachineInstance.ChangeState("Sleep");
            }
        }

        private void OnDamageAreaBodyEntered(Node2D body)
        {
            OnHitBoxBodyEntered(body);
        }

        private void OnHitBoxBodyEntered(Node2D body)
        {
            if (body is Player player && CurrentStateEnum != EnemyStates.Sleeping)
            {
                player.TakeDamage(ContactDamage, GlobalPosition);
            }
        }

        private void OnAnimationFinished()
        {
            StateMachineInstance.OnAnimationFinished();
        }

        public override void _PhysicsProcess(double delta)
        {
            if (CurrentStateEnum == EnemyStates.Dead) return;

            if (_attackCooldownTimer > 0)
                _attackCooldownTimer -= (float)delta;

            // Encontra player se perdido
            if (TargetPlayer == null)
            {
                var players = GetTree().GetNodesInGroup("player");
                if (players.Count > 0) TargetPlayer = players[0] as Player;
            }

            StateMachineInstance.PhysicsUpdate(delta);
            MoveAndSlide();
        }

        public override void TakeDamage(float amount, Vector2? sourcePosition = null)
        {
            if (_isDead) return;

            base.TakeDamage(amount, sourcePosition);
            GD.Print($"[ENEMY] {Name} took {amount} damage! Current health: {CurrentHealth}");

            if (CurrentHealth > 0)
            {
                StateMachineInstance.ChangeState("Hurt");
                if (sourcePosition != null)
                {
                    float knockbackDir = Mathf.Sign(GlobalPosition.X - sourcePosition.Value.X);
                    Velocity = new Vector2(knockbackDir * KnockbackIntensity.X, KnockbackIntensity.Y);
                }
            }
        }

        public bool IsPlayerInRange()
        {
            if (TargetPlayer == null) return false;
            return GlobalPosition.DistanceTo(TargetPlayer.GlobalPosition) < DetectionRange;
        }

        public void MoveTowardsPlayer(float direction)
        {
            Velocity = new Vector2(direction * Speed, Velocity.Y);
            AnimControllerInstance.UpdateFlip(direction);
            AnimControllerInstance.Play("run");

            // Espelha área de ataque na direção do player
            if (_attackArea != null)
            {
                var s = _attackArea.Scale;
                s.X = Mathf.Abs(s.X) * (direction < 0 ? -1 : 1);
                _attackArea.Scale = s;
            }
        }

        public bool CanAttack() => _attackCooldownTimer <= 0;

        public void ExecuteCurrentAttack()
        {
            _currentAttack = GD.Randf() > 0.5f ? _attack1 : _attack2;
            AnimControllerInstance.Play(_currentAttack.AnimationName);
            UpdateHitBox();
        }

        public void FinishAttack()
        {
            _attackCooldownTimer = AttackCooldown;
            DisableAllHitBoxes();
        }

        public void PlayAnimation(string anim, bool force = false) => AnimControllerInstance.Play(anim, force);
        public void StartSleepTimer() => _sleepTimer.Start();
        public void StopSleepTimer() => _sleepTimer.Stop();

        public void ResetToSleep()
        {
            CurrentStateEnum = EnemyStates.Sleeping;
            AnimControllerInstance.Play("awakening");
            _sprite.Frame = 0;
            _sprite.Stop();
        }

        public override void Die()
        {
            if (CurrentStateEnum == EnemyStates.Dead) return;
            base.Die();
            CurrentStateEnum = EnemyStates.Dead;
            AnimControllerInstance.Play("death");
            Velocity = Vector2.Zero;

            // Limpeza de colisões, evita que o player tome dano de contato com o cadaver do mob
            CollisionLayer = 0;
            DisableAllHitBoxes();

            var hurtBox = GetNodeOrNull<Area2D>("HurtBox");
            if (hurtBox != null) hurtBox.CollisionLayer = 0;

            var contactHitBox = GetNodeOrNull<Area2D>("HitBox");
            if (contactHitBox != null) contactHitBox.CollisionLayer = 0;

            GD.Print($"[ENEMY] {Name} has died.");
        }

        private void OnAttackAreaAreaEntered(Area2D area)
        {
            // Se o ataque foi feito por si mesmo, não causa dano
            if (area.GetParent() == this) return;

            if (area.GetParent() is Player player)
            {
                float damage = _currentAttack.Damage;
                player.TakeDamage(damage, GlobalPosition);
                GD.Print($"[COMBAT] Enemy hit player for {damage} damage! (Attack: {_currentAttack.AnimationName})");
            }
        }

        public void DisableAllHitBoxes()
        {
            if (_attackShape1 != null) _attackShape1.SetDeferred(CollisionPolygon2D.PropertyName.Disabled, true);
            if (_attackShape2 != null) _attackShape2.SetDeferred(CollisionPolygon2D.PropertyName.Disabled, true);
        }

        public void UpdateHitBox()
        {
            DisableAllHitBoxes();

            var contactHitBox = GetNodeOrNull<Area2D>("HitBox");

            if (_currentAttack.AnimationName == "attack_1")
            {
                if (contactHitBox != null) contactHitBox.SetDeferred("monitoring", false);
                if (_attackShape1 != null) _attackShape1.SetDeferred(CollisionPolygon2D.PropertyName.Disabled, false);
            }
            else if (_currentAttack.AnimationName == "attack_2")
            {
                if (contactHitBox != null) contactHitBox.SetDeferred("monitoring", false);
                if (_attackShape2 != null) _attackShape2.SetDeferred(CollisionPolygon2D.PropertyName.Disabled, false);
            }
            else
            {
                if (contactHitBox != null && !_isDead) contactHitBox.SetDeferred("monitoring", true);
            }
        }
    }
}