using Godot;

namespace EcoDoFarolCentral;

public partial class HealthBar : ProgressBar
{
    private Actor _owner;

    public override void _Ready()
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
            GD.PrintErr($"HealthBar ({Name}) could not find an Actor owner!");
            return;
        }

        // Inicializa valores
        MaxValue = _owner.MaxHealth;
        Value = _owner.CurrentHealth;
    }

    public void OnHealthChanged(float current, float max)
    {
        Value = current;
        MaxValue = max;
    }
}
