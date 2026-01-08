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

        public override void _Ready()
        {
            // Verifica se já foi coletado
            if (!string.IsNullOrEmpty(ID) && GameManager.Instance != null)
            {
                if (GameManager.Instance.IsItemCollected(ID))
                {
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

            // Conecta sinal de colisão
            _area.BodyEntered += OnBodyEntered;

            // Aplica icone customizado se existir
            if (CustomIcon != null)
            {
                _sprite.Texture = CustomIcon;
            }

            // Inicia animação de flutuar
            _animPlayer.Play("default");
        }

        private void OnBodyEntered(Node2D body)
        {
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
