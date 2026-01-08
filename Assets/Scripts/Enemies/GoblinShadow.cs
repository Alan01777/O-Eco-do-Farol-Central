using Godot;
using EcoDoFarolCentral.States.Goblin;

namespace EcoDoFarolCentral
{
    public partial class GoblinShadow : Actor
    {
        public enum GoblinStates { Idle, Fleeing, Jumping, Hurt, Dead }
        public GoblinStates CurrentStateEnum { get; set; } = GoblinStates.Idle;

        public StateMachine StateMachineInstance { get; private set; }
        public AnimationController AnimControllerInstance { get; private set; }
        public AnimatedSprite2D SpriteInstance => _sprite;

        [ExportGroup("AI")]
        [Export] public float DetectionRange = 150f;
        [Export] public float SafeDistance = 250f;
        [Export] public float FleeSpeed = 150f;

        [ExportGroup("General")]
        [Export] public float HurtDuration = 0.4f;
        [Export] public Vector2 KnockbackIntensity = new Vector2(100f, -100f);

        private AnimatedSprite2D _sprite;
        public Player TargetPlayer { get; private set; }

        // Raycasts for environment detection
        public RayCast2D WallCheck { get; private set; }
        public RayCast2D FloorCheck { get; private set; }

        [Export] public float JumpForce = -350f;

        // Sistema de áudio
        private AudioStreamPlayer _audioSFX;
        private const string AUDIO_PATH = "res://Assets/Audio/goblin/";
        private readonly string[] HIT_SOUNDS = { "hit.wav", "hit1.wav" };
        private readonly string[] ATTACK_SOUNDS = { "attack1.wav", "attack2.wav", "attack3.wav" };

        public override void _Ready()
        {
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

            // Get Raycasts from scene
            WallCheck = GetNode<RayCast2D>("WallCheck");
            FloorCheck = GetNode<RayCast2D>("FloorCheck");

            // Force collision masks to only detect terrain (layer 1)
            WallCheck.CollisionMask = 1;
            FloorCheck.CollisionMask = 1;

            GD.Print($"[GOBLIN INIT] WallCheck.CollisionMask = {WallCheck.CollisionMask}, FloorCheck.CollisionMask = {FloorCheck.CollisionMask}");

            var players = GetTree().GetNodesInGroup("player");
            if (players.Count > 0) TargetPlayer = players[0] as Player;

            InitializeStateMachine();
        }

        private void InitializeStateMachine()
        {
            StateMachineInstance = new StateMachine();

            var idle = new GoblinIdleState();
            var flee = new GoblinFleeState();
            var jump = new GoblinJumpState();
            var hurt = new GoblinHurtState();


            idle.Initialize(this, StateMachineInstance);
            flee.Initialize(this, StateMachineInstance);
            jump.Initialize(this, StateMachineInstance);
            hurt.Initialize(this, StateMachineInstance);

            StateMachineInstance.AddState("Idle", idle);
            StateMachineInstance.AddState("Flee", flee);
            StateMachineInstance.AddState("Jump", jump);
            StateMachineInstance.AddState("Hurt", hurt);

            StateMachineInstance.ChangeState("Idle");
        }

        private void OnAnimationFinished()
        {
            StateMachineInstance.OnAnimationFinished();
        }

        public override void _PhysicsProcess(double delta)
        {
            if (CurrentStateEnum == GoblinStates.Dead) return;

            if (TargetPlayer == null)
            {
                var players = GetTree().GetNodesInGroup("player");
                if (players.Count > 0) TargetPlayer = players[0] as Player;
            }

            StateMachineInstance.PhysicsUpdate(delta);
            MoveAndSlide();

            // Atualiza direção dos raycasts baseado no movimento
            UpdateRaycasts();
        }

        private void UpdateRaycasts()
        {
            // Determina direção baseada na velocidade
            float facing = 1; // Default: direita

            if (Mathf.Abs(Velocity.X) > 1)
            {
                facing = Mathf.Sign(Velocity.X);
            }
            else if (_sprite != null)
            {
                // Se não está se movendo, usa o FlipH do sprite
                facing = _sprite.FlipH ? -1 : 1;
            }

            // Atualiza WallCheck - horizontal na direção do movimento
            WallCheck.TargetPosition = new Vector2(20 * facing, 0);

            // Atualiza FloorCheck - vertical mas a partir da posição à frente
            FloorCheck.Position = new Vector2(50 * facing, 0);
            // Target position continua vertical (0, 50)
        }

        public bool IsPlayerInRange()
        {
            if (TargetPlayer == null) return false;
            return GlobalPosition.DistanceTo(TargetPlayer.GlobalPosition) < DetectionRange;
        }

        public bool IsPlayerSafe()
        {
            if (TargetPlayer == null) return true;
            return GlobalPosition.DistanceTo(TargetPlayer.GlobalPosition) > SafeDistance;
        }

        public bool IsCliff()
        {
            if (!FloorCheck.IsColliding())
            {
                // Cria um raycast mais longo para medir profundidade
                var spaceState = GetWorld2D().DirectSpaceState;
                var query = PhysicsRayQueryParameters2D.Create(
                    GlobalPosition,
                    GlobalPosition + new Vector2(0, 300) // 300 pixels para baixo
                );
                query.CollisionMask = 1; // Layer do mundo

                var result = spaceState.IntersectRay(query);

                if (result.Count == 0)
                {
                    // Não encontrou chão = precipício profundo
                    return true;
                }

                // Calcula distância até o chão
                Vector2 hitPoint = (Vector2)result["position"];
                float depth = hitPoint.Y - GlobalPosition.Y;

                // Se profundidade > 100, é precipício
                return depth > 100f;
            }

            return false;
        }

        public override void TakeDamage(float amount, Vector2? sourcePosition = null)
        {
            if (_isDead) return;

            base.TakeDamage(amount, sourcePosition);

            // Toca som de hit aleatório
            PlayRandomHitSound();

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

        public override void Die()
        {
            if (CurrentStateEnum == GoblinStates.Dead) return;
            base.Die();
            CurrentStateEnum = GoblinStates.Dead;
            AnimControllerInstance.Play("death");
            Velocity = Vector2.Zero;
            CollisionLayer = 0;

            // Toca som de morte
            AnimControllerInstance.PlaySFX(AUDIO_PATH + "death.wav", 0.9f, 1.1f);

            // Disable hitboxes if any
            var hitBox = GetNodeOrNull<Area2D>("HitBox");
            if (hitBox != null) hitBox.CollisionLayer = 0;
        }

        // Métodos de áudio
        public void PlayRandomHitSound()
        {
            string hitSound = HIT_SOUNDS[GD.RandRange(0, HIT_SOUNDS.Length - 1)];
            AnimControllerInstance.PlaySFX(AUDIO_PATH + hitSound, 0.9f, 1.1f);
        }

        public void PlayAlertSound()
        {
            AnimControllerInstance.PlaySFX(AUDIO_PATH + "alert.wav", 0.95f, 1.05f);
        }

        public void PlayRandomAttackSound()
        {
            string attackSound = ATTACK_SOUNDS[GD.RandRange(0, ATTACK_SOUNDS.Length - 1)];
            AnimControllerInstance.PlaySFX(AUDIO_PATH + attackSound, 0.9f, 1.1f);
        }
    }
}
