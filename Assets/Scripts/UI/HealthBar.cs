using Godot;

namespace EcoDoFarolCentral;

public partial class HealthBar : ProgressBar
{
    private Actor _owner;
    private StyleBoxFlat _fillStyle;

    // Colors for health gradient
    private static readonly Color HealthColorFull = new Color(0.2f, 0.85f, 0.3f);    // Green
    private static readonly Color HealthColorMid = new Color(0.95f, 0.8f, 0.2f);     // Yellow
    private static readonly Color HealthColorLow = new Color(0.9f, 0.2f, 0.2f);      // Red

    public override void _Ready()
    {
        // Get the fill style for dynamic color changes
        var fillStyleVariant = GetThemeStylebox("fill");
        if (fillStyleVariant is StyleBoxFlat styleBox)
        {
            // Create a unique copy so we don't modify the shared resource
            _fillStyle = (StyleBoxFlat)styleBox.Duplicate();
            AddThemeStyleboxOverride("fill", _fillStyle);
        }

        // Usa CallDeferred para garantir que o player já foi carregado após troca de cena
        CallDeferred(nameof(FindAndConnectToPlayer));
    }

    private void FindAndConnectToPlayer()
    {
        // 1. Tenta encontrar o Actor como pai pessoal (para inimigos/barras locais)
        _owner = GetParentOrNull<Actor>();

        // 2. Se não encontrar (como em um HUD), tenta encontrar o player no grupo "player" ou "Player"
        if (_owner == null)
        {
            var playerNodes = GetTree().GetNodesInGroup("player");
            if (playerNodes.Count == 0)
                playerNodes = GetTree().GetNodesInGroup("Player");

            if (playerNodes.Count > 0)
            {
                _owner = playerNodes[0] as Actor;
            }
        }

        if (_owner == null)
        {
            // Tenta novamente em alguns frames
            GetTree().CreateTimer(0.5).Timeout += FindAndConnectToPlayer;
            return;
        }

        MaxValue = _owner.MaxHealth;
        Value = _owner.CurrentHealth;
        UpdateHealthColor();

        // Conecta ao sinal de mudança de vida
        if (!_owner.IsConnected(Actor.SignalName.HealthChanged, Callable.From<float, float>(OnHealthChanged)))
        {
            _owner.HealthChanged += OnHealthChanged;
        }
    }

    public void OnHealthChanged(float current, float max)
    {
        Value = current;
        MaxValue = max;
        UpdateHealthColor();
    }

    /// <summary>
    /// Updates the health bar color based on current health percentage.
    /// Red (0-30%) → Yellow (30-60%) → Green (60-100%)
    /// </summary>
    private void UpdateHealthColor()
    {
        if (_fillStyle == null) return;

        float healthPercent = MaxValue > 0 ? (float)(Value / MaxValue) : 1.0f;
        Color newColor;

        if (healthPercent <= 0.3f)
        {
            // Low health: Red to Yellow
            float t = healthPercent / 0.3f;
            newColor = HealthColorLow.Lerp(HealthColorMid, t);
        }
        else if (healthPercent <= 0.6f)
        {
            // Mid health: Yellow to Green
            float t = (healthPercent - 0.3f) / 0.3f;
            newColor = HealthColorMid.Lerp(HealthColorFull, t);
        }
        else
        {
            // Full health: Green
            newColor = HealthColorFull;
        }

        _fillStyle.BgColor = newColor;
    }
}

