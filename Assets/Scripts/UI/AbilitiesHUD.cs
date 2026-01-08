using Godot;
using System.Collections.Generic;

namespace EcoDoFarolCentral
{
    /// <summary>
    /// HUD que exibe ícones das habilidades desbloqueadas do player.
    /// </summary>
    public partial class AbilitiesHUD : HBoxContainer
    {
        // Dicionário de ícones por habilidade
        private Dictionary<string, TextureRect> _abilityIcons = new();

        // Lista de habilidades suportadas
        private readonly string[] _abilities =
        {
            "DoubleJump",
            "Dash",
            "Fireball",
            "ComboLevel2",
            "ComboLevel3"
        };

        [Export] public bool ShowLockedAbilities { get; set; } = false;
        [Export] public float IconSize { get; set; } = 32f;
        [Export] public float LockedAlpha { get; set; } = 0.3f;

        private Player _player;
        private PlayerAbilities _abilities_ref;

        public override void _Ready()
        {
            // Configura o container
            AddThemeConstantOverride("separation", 8);

            // Busca o player
            CallDeferred(nameof(FindPlayer));
        }

        private void FindPlayer()
        {
            var players = GetTree().GetNodesInGroup("player");
            if (players.Count > 0)
            {
                _player = players[0] as Player;
                if (_player != null)
                {
                    _abilities_ref = _player.GetNodeOrNull<PlayerAbilities>("Abilities");
                    if (_abilities_ref != null)
                    {
                        // Conecta ao sinal de habilidade desbloqueada
                        _abilities_ref.AbilityUnlocked += OnAbilityUnlocked;

                        // Atualiza os ícones iniciais
                        UpdateAllIcons();
                    }
                }
            }
            else
            {
                // Tenta novamente em alguns frames
                GetTree().CreateTimer(0.5).Timeout += FindPlayer;
            }
        }

        private void UpdateAllIcons()
        {
            if (_abilities_ref == null) return;

            var unlockedAbilities = _abilities_ref.GetUnlockedAbilities();

            foreach (var abilityName in _abilities)
            {
                bool isUnlocked = false;

                if (unlockedAbilities.TryGetValue(abilityName, out bool value))
                {
                    isUnlocked = value;
                }
                else if (abilityName.StartsWith("ComboLevel"))
                {
                    int level = int.Parse(abilityName.Replace("ComboLevel", ""));
                    isUnlocked = _abilities_ref.MaxComboLevel >= level;
                }

                UpdateAbilityIcon(abilityName, isUnlocked);
            }
        }

        private void OnAbilityUnlocked(string abilityName)
        {
            UpdateAbilityIcon(abilityName, true);

            // Animação de destaque
            if (_abilityIcons.TryGetValue(abilityName, out var icon))
            {
                PlayUnlockAnimation(icon);
            }
        }

        private void UpdateAbilityIcon(string abilityName, bool isUnlocked)
        {
            if (!_abilityIcons.ContainsKey(abilityName))
            {
                // Cria o ícone se não existir
                CreateAbilityIcon(abilityName);
            }

            if (_abilityIcons.TryGetValue(abilityName, out var icon))
            {
                if (isUnlocked)
                {
                    icon.Modulate = Colors.White;
                    icon.Visible = true;
                }
                else if (ShowLockedAbilities)
                {
                    icon.Modulate = new Color(1, 1, 1, LockedAlpha);
                    icon.Visible = true;
                }
                else
                {
                    icon.Visible = false;
                }
            }
        }

        private void CreateAbilityIcon(string abilityName)
        {
            var icon = new TextureRect();
            icon.Name = abilityName + "Icon";
            icon.CustomMinimumSize = new Vector2(IconSize, IconSize);
            icon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;

            Texture2D texture = null;
            string tooltipText = FormatAbilityName(abilityName);

            // Tenta usar o ícone do PowerUpData (coletado)
            if (_abilities_ref != null)
            {
                var powerUpData = _abilities_ref.GetPowerUpData(abilityName);
                if (powerUpData != null)
                {
                    texture = powerUpData.Icon;
                    tooltipText = powerUpData.ItemName;
                }
            }

            // Fallback: tenta carregar ícone de arquivo
            if (texture == null)
            {
                string iconPath = $"res://Assets/Sprites/UI/Icons/{abilityName.ToLower()}.png";
                texture = GD.Load<Texture2D>(iconPath);
            }

            // Fallback: placeholder colorido
            if (texture == null)
            {
                texture = CreatePlaceholderTexture(abilityName);
            }

            icon.Texture = texture;
            icon.TooltipText = tooltipText;

            AddChild(icon);
            _abilityIcons[abilityName] = icon;

            // Começa escondido
            icon.Visible = false;
        }

        private Texture2D CreatePlaceholderTexture(string abilityName)
        {
            // Cria uma imagem placeholder com cor baseada no nome
            var image = Image.CreateEmpty(32, 32, false, Image.Format.Rgba8);

            // Cor baseada no hash do nome
            int hash = abilityName.GetHashCode();
            Color color = new Color(
                ((hash >> 16) & 0xFF) / 255f,
                ((hash >> 8) & 0xFF) / 255f,
                (hash & 0xFF) / 255f,
                1f
            );

            image.Fill(color);

            return ImageTexture.CreateFromImage(image);
        }

        private string FormatAbilityName(string abilityName)
        {
            return abilityName switch
            {
                "DoubleJump" => "Double Jump",
                "Dash" => "Dash",
                "Fireball" => "Fireball",
                "ComboLevel2" => "Combo Lv.2",
                "ComboLevel3" => "Combo Lv.3",
                _ => abilityName
            };
        }

        private void PlayUnlockAnimation(TextureRect icon)
        {
            // Animação de escala e brilho
            icon.Scale = Vector2.One * 0.5f;
            icon.Modulate = new Color(2, 2, 2, 1); // Brilho extra

            var tween = CreateTween();
            tween.TweenProperty(icon, "scale", Vector2.One, 0.3f)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.Out);
            tween.Parallel().TweenProperty(icon, "modulate", Colors.White, 0.5f);
        }
    }
}
