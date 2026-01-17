using Godot;

namespace EcoDoFarolCentral
{
    /// <summary>
    /// Coletável (Power Up) que desbloqueia habilidades
    /// </summary>
    public partial class PowerUp : Node2D
    {
        public enum PowerUpType
        {
            DoubleJump,
            Dash,
            WallJump,
            ComboLevel2,
            ComboLevel3,
            JumpAttack,
            Fireball,
            HealthUpgrade
        }

        [ExportGroup("Power-up Configuration")]
        [Export] public PowerUpType Type { get; set; } = PowerUpType.DoubleJump;
        [Export] public string ItemName { get; set; } = "Double Jump";
        [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "Allows you to jump in mid-air!";
        [Export] public float HealthUpgradeAmount { get; set; } = 25f;

        [ExportGroup("Persistence")]
        [Export] public string ID { get; set; } = "";

        [ExportGroup("Visual")]
        [Export] public Texture2D CustomIcon { get; set; }

        private Area2D _area;
        private Sprite2D _sprite;
        private AnimationPlayer _animPlayer;
        private bool _collected = false;
        private float _debugTimer = 0f;

        public override void _PhysicsProcess(double delta)
        {
            if (_collected) return;

            // Fallback: verifica distância do player a cada frame
            var player = GetTree().GetFirstNodeInGroup("Player") as Player;
            if (player != null)
            {
                float distance = GlobalPosition.DistanceTo(player.GlobalPosition);

                // Se o player estiver muito perto, força a coleta (fallback para problemas de física)
                if (distance < 50)
                {
                    GD.Print($"[POWER-UP] {ItemName} - Player within 50px, triggering collection via proximity fallback!");
                    TriggerCollection(player);
                    return;
                }
            }

            // Debug periódico (a cada 1 segundo)
            _debugTimer += (float)delta;
            if (_debugTimer >= 1.0f)
            {
                _debugTimer = 0;
                var bodies = _area.GetOverlappingBodies();
                if (bodies.Count > 0)
                {
                    GD.Print($"[POWER-UP] {ItemName} - Currently overlapping: {bodies.Count} bodies");
                }

                if (player != null)
                {
                    float distance = GlobalPosition.DistanceTo(player.GlobalPosition);
                    if (distance < 100)
                    {
                        GD.Print($"[POWER-UP] {ItemName} - Player is NEAR! Distance: {distance:F1}px");
                    }
                }
            }
        }

        private void TriggerCollection(Player player)
        {
            if (_collected) return;

            _collected = true;
            UnlockAbility(player);
            PlayCollectionEffect();

            // Registra coleta se tiver ID
            if (!string.IsNullOrEmpty(ID) && GameManager.Instance != null)
            {
                GameManager.Instance.RegisterCollectedItem(ID);
            }

            // Remove o item
            QueueFree();
        }

        public override void _Ready()
        {
            // Verifica se já foi coletado
            if (!string.IsNullOrEmpty(ID) && GameManager.Instance != null)
            {
                bool isCollected = GameManager.Instance.IsItemCollected(ID);
                GD.Print($"[POWER-UP] {ItemName} (ID:{ID}) - Already collected? {isCollected}");

                if (isCollected)
                {
                    GD.Print($"[POWER-UP] {ItemName} - Removing because already collected");
                    QueueFree();
                    return;
                }
            }
            else if (string.IsNullOrEmpty(ID))
            {
                // Aviso para o dev não esquecer de setar ID
                GD.PushWarning($"[POWER-UP] Item '{ItemName}' at {GlobalPosition} has no ID! It will respawn after save/load.");
            }

            _area = GetNode<Area2D>("Area2D");
            _sprite = GetNode<Sprite2D>("Sprite2D");
            _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

            // Garante que a collision shape tenha um raio adequado
            var collisionShape = _area.GetNode<CollisionShape2D>("CollisionShape2D");
            if (collisionShape.Shape is CircleShape2D circle)
            {
                if (circle.Radius < 20)
                {
                    circle.Radius = 25; // Raio adequado para detecção
                    GD.Print($"[POWER-UP] Adjusted collision radius to 25 for {ItemName}");
                }
            }

            // Garante configuração correta da Area2D para detectar Player (Layer 2)
            _area.CollisionMask = 2; // Detecta Layer 2 (Player)
            _area.CollisionLayer = 0; // Não precisa ser detectado por outros
            _area.Monitoring = true;
            _area.Monitorable = true;

            // Debug: mostra configuração de colisão
            GD.Print($"[POWER-UP] {ItemName} at {GlobalPosition} - Area2D mask: {_area.CollisionMask}, monitorable: {_area.Monitorable}, monitoring: {_area.Monitoring}");

            // Conecta sinal de colisão
            _area.BodyEntered += OnBodyEntered;

            // Verifica se a CollisionShape está desabilitada
            var collisionShapeDebug = _area.GetNode<CollisionShape2D>("CollisionShape2D");
            if (collisionShapeDebug.Disabled)
            {
                GD.PrintErr($"[POWER-UP] WARNING: CollisionShape2D is DISABLED for {ItemName}! Enabling it now.");
                collisionShapeDebug.Disabled = false;
            }

            // Debug: verifica bodies já dentro da área
            CallDeferred("CheckOverlappingBodies");

            // Aplica icone customizado se existir
            if (CustomIcon != null)
            {
                _sprite.Texture = CustomIcon;
            }

            // Inicia animação de flutuar
            _animPlayer.Play("default");
        }

        private void CheckOverlappingBodies()
        {
            var bodies = _area.GetOverlappingBodies();
            GD.Print($"[POWER-UP] {ItemName} - Overlapping bodies count: {bodies.Count}");
            foreach (var body in bodies)
            {
                GD.Print($"[POWER-UP] {ItemName} - Overlapping body: {body.Name}");
            }
        }

        private void OnBodyEntered(Node2D body)
        {
            GD.Print($"[POWER-UP] {ItemName} detected body: {body.Name} (type: {body.GetType().Name})");

            if (_collected) return;

            if (body is Player player)
            {
                _collected = true;
                _collected = true;
                UnlockAbility(player);
                PlayCollectionEffect();

                // Registra coleta se tiver ID
                if (!string.IsNullOrEmpty(ID) && GameManager.Instance != null)
                {
                    GameManager.Instance.RegisterCollectedItem(ID);
                }

                // Remove do mundo após pequeno delay
                GetTree().CreateTimer(0.3).Timeout += QueueFree;
            }
        }

        private void UnlockAbility(Player player)
        {
            // Obtém ou cria PlayerAbilities
            var abilities = player.GetNodeOrNull<PlayerAbilities>("Abilities");
            if (abilities == null)
            {
                abilities = new PlayerAbilities();
                abilities.Name = "Abilities";
                player.AddChild(abilities);
            }

            // Obtém o ícone visual do power-up (sprite ou customIcon)
            Texture2D iconTexture = CustomIcon;
            if (iconTexture == null && _sprite != null)
            {
                iconTexture = _sprite.Texture;
            }

            // Registra os dados do power-up para uso na UI
            var powerUpData = new PowerUpData(Type, ItemName, Description, iconTexture);
            abilities.RegisterPowerUpData(powerUpData);

            // Desbloqueia baseado no tipo
            switch (Type)
            {
                case PowerUpType.DoubleJump:
                    abilities.UnlockDoubleJump();
                    ShowMessage($"Unlocked: {ItemName}!\n{Description}");
                    break;

                case PowerUpType.Dash:
                    abilities.UnlockDash();
                    ShowMessage($"Unlocked: {ItemName}!\n{Description}");
                    break;

                case PowerUpType.WallJump:
                    abilities.UnlockWallJump();
                    ShowMessage($"Unlocked: {ItemName}!\n{Description}");
                    break;

                case PowerUpType.ComboLevel2:
                    abilities.UnlockComboLevel(2);
                    ShowMessage($"Unlocked: {ItemName}!\n{Description}");
                    break;

                case PowerUpType.ComboLevel3:
                    abilities.UnlockComboLevel(3);
                    ShowMessage($"Unlocked: {ItemName}!\n{Description}");
                    break;

                case PowerUpType.JumpAttack:
                    abilities.UnlockJumpAttack();
                    ShowMessage($"Unlocked: {ItemName}!\n{Description}");
                    break;

                case PowerUpType.Fireball:
                    abilities.UnlockFireball();
                    ShowMessage($"Unlocked: {ItemName}!\n{Description}");
                    break;

                case PowerUpType.HealthUpgrade:
                    abilities.UpgradeHealth(player, HealthUpgradeAmount);
                    ShowMessage($"Health increased by {HealthUpgradeAmount}!");
                    break;
            }

            GD.Print($"[POWER-UP] {player.Name} collected {ItemName}");
        }

        private void PlayCollectionEffect()
        {
            // Feedback visual
            _sprite.Modulate = new Color(1, 1, 1, 0.5f);

            // Animação de escala
            var tween = CreateTween();
            tween.TweenProperty(_sprite, "scale", Vector2.One * 1.5f, 0.2);
            tween.Parallel().TweenProperty(_sprite, "modulate:a", 0f, 0.2);

            // TODO: Adicionar efeito de particula
            // TODO: Adicionar efeito sonoro
        }

        private void ShowMessage(string message)
        {
            // Instancia o modal de power-up
            var modalScene = GD.Load<PackedScene>("res://Scenes/UI/PowerUpModal.tscn");
            if (modalScene == null)
            {
                GD.PrintErr("[POWER-UP] PowerUpModal.tscn not found!");
                GD.Print($"[POWER-UP MESSAGE] {message}");
                return;
            }

            var modal = modalScene.Instantiate<PowerUpModal>();
            GetTree().Root.AddChild(modal);

            // Configura o modal com os dados do power-up
            modal.Setup(CustomIcon, ItemName, Description);
        }
    }
}
