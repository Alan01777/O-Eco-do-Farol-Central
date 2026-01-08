using Godot;

namespace EcoDoFarolCentral
{
    public partial class ShadowEnemy2 : Actor
    {
        public enum EnemyStates { Idle, Chasing, Dead, Hurt, Attacking }
        public EnemyStates CurrentStateEnum { get; set; } = EnemyStates.Idle;

        public StateMachine StateMachineInstance { get; private set; }
        public AnimationController AnimControllerInstance { get; private set; }
        public AnimatedSprite2D SpriteInstance => _sprite;

        private float _attackCooldownTimer = 0f;
        [ExportGroup("General")]
        [Export] public float HurtDuration = 0.4f;
        [Export] public Vector2 KnockbackIntensity = new Vector2(150f, -50f);
        [Export] public float DetectionRange = 250f;

        private AnimatedSprite2D _sprite;
        public Player TargetPlayer { get; private set; }

        [ExportGroup("Combat")]
        [Export] public int ContactDamage = 10;
        [Export] public float AttackCooldown = 1.5f;

        // Ataque único
        [Export] public float AttackDamage = 20f;
        [Export] public float AttackRange = 40f;

        private CombatAttackData _attackData;

        private Area2D _attackArea;
        private CollisionPolygon2D _attackShape;

        // Sistema de áudio
        private AudioStreamPlayer _audioSFX;
        private const string AUDIO_PATH = "res://Assets/Audio/shadows/";
        private readonly string[] HIT_SOUNDS = { "hit 1.wav", "hit 2.wav", "hit 3.wav" };

        public override void _Ready()
        {
            _attackData = new CombatAttackData("attack", AttackDamage, AttackRange, "AttackArea");

            _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

            // Configura áudio - cria AudioStreamPlayer se não existir
            _audioSFX = GetNodeOrNull<AudioStreamPlayer>("AudioSFX");
            if (_audioSFX == null)
            {
                _audioSFX = new AudioStreamPlayer();
                _audioSFX.Name = "AudioSFX";
                AddChild(_audioSFX);
            }

            AnimControllerInstance = new AnimationController();
            AnimControllerInstance.Initialize(_sprite, _audioSFX, null, null);
            _sprite.AnimationFinished += OnAnimationFinished;

            var contactHitBox = GetNodeOrNull<Area2D>("HitBox") ?? GetNodeOrNull<Area2D>("Hitbox") ?? GetNodeOrNull<Area2D>("hitbox");

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

            var players = GetTree().GetNodesInGroup("player");
            if (players.Count > 0) TargetPlayer = players[0] as Player;

            _attackArea = GetNodeOrNull<Area2D>("AttackArea");
            if (_attackArea != null)
            {
                // Tenta pegar o CollisionPolygon2D filho direto se não tiver nomes específicos
                _attackShape = _attackArea.GetNodeOrNull<CollisionPolygon2D>("CollisionPolygon2D");
                _attackArea.AreaEntered += OnAttackAreaAreaEntered;
                DisableAllHitBoxes();
            }

            // Configura FSM com estados do ShadowEnemy2
            StateMachineInstance = new StateMachine();

            var idle = new ShadowEnemy2IdleState();
            var chase = new ShadowEnemy2ChaseState();
            var attack = new ShadowEnemy2AttackState();
            var hurt = new ShadowEnemy2HurtState();

            idle.Initialize(this, StateMachineInstance);
            chase.Initialize(this, StateMachineInstance);
            attack.Initialize(this, StateMachineInstance);
            hurt.Initialize(this, StateMachineInstance);

            StateMachineInstance.AddState("Idle", idle);
            StateMachineInstance.AddState("Chase", chase);
            StateMachineInstance.AddState("Attack", attack);
            StateMachineInstance.AddState("Hurt", hurt);

            StateMachineInstance.ChangeState("Idle");
        }

        private void OnHitBoxBodyEntered(Node2D body)
        {
            if (body is Player player && CurrentStateEnum != EnemyStates.Dead)
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
            GD.Print($"[ENEMY 2] {Name} took {amount} damage! Current health: {CurrentHealth}");

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

            if (_attackArea != null)
            {
                var s = _attackArea.Scale;
                s.X = Mathf.Abs(s.X) * (direction < 0 ? -1 : 1);
                _attackArea.Scale = s;
            }
        }

        public bool CanAttack() => _attackCooldownTimer <= 0;

        public void ExecuteAttack()
        {
            AnimControllerInstance.Play("attack");

            // Toca som de hit aleatório
            PlayRandomHitSound();

            UpdateHitBox();
        }

        public void FinishAttack()
        {
            _attackCooldownTimer = AttackCooldown;
            DisableAllHitBoxes();
        }

        public void PlayAnimation(string anim, bool force = false) => AnimControllerInstance.Play(anim, force);

        public override void Die()
        {
            if (CurrentStateEnum == EnemyStates.Dead) return;
            base.Die();
            CurrentStateEnum = EnemyStates.Dead;
            AnimControllerInstance.Play("death");
            Velocity = Vector2.Zero;

            // Toca som de morte
            AnimControllerInstance.PlaySFX(AUDIO_PATH + "monster dying.wav", 0.9f, 1.1f);

            CollisionLayer = 0;
            DisableAllHitBoxes();

            var hurtBox = GetNodeOrNull<Area2D>("HurtBox");
            if (hurtBox != null) hurtBox.CollisionLayer = 0;

            var contactHitBox = GetNodeOrNull<Area2D>("HitBox");
            if (contactHitBox != null) contactHitBox.CollisionLayer = 0;

            GD.Print($"[ENEMY 2] {Name} has died.");
        }

        private void OnAttackAreaAreaEntered(Area2D area)
        {
            if (area.GetParent() == this) return;

            if (area.GetParent() is Player player)
            {
                float damage = _attackData.Damage;
                player.TakeDamage(damage, GlobalPosition);
                GD.Print($"[COMBAT] Enemy 2 hit player for {damage} damage!");
            }
        }

        public void DisableAllHitBoxes()
        {
            if (_attackShape != null) _attackShape.SetDeferred(CollisionPolygon2D.PropertyName.Disabled, true);
        }

        public void UpdateHitBox()
        {
            DisableAllHitBoxes();

            // Ativa hitbox (ajuste fino conforme animação seria ideal, mas aqui ativamos direto)
            if (_attackShape != null) _attackShape.SetDeferred(CollisionPolygon2D.PropertyName.Disabled, false);
        }

        // Efeito visual de dano (flash vermelho)
        public async void FlashDamageEffect()
        {
            _sprite.Modulate = Colors.Red;
            await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
            _sprite.Modulate = Colors.White;
        }

        // Métodos de áudio
        public void PlayRandomHitSound()
        {
            string hitSound = HIT_SOUNDS[GD.RandRange(0, HIT_SOUNDS.Length - 1)];
            AnimControllerInstance.PlaySFX(AUDIO_PATH + hitSound, 0.9f, 1.1f);
        }

        public void PlayChaseSound()
        {
            AnimControllerInstance.PlaySFX(AUDIO_PATH + "chase.wav", 0.95f, 1.05f, -3f);
        }
    }
}
