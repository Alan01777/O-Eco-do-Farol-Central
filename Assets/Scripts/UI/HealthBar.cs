using Godot;

namespace EcoDoFarolCentral;

public partial class HealthBar : ProgressBar
{
    private Actor _owner;
    //[Export] public Label FpsCounter; //debug

    public override void _Ready()
    {
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
            GD.PrintErr($"HealthBar ({Name}) could not find an Actor owner!");
            // Tenta novamente em alguns frames
            GetTree().CreateTimer(0.5).Timeout += FindAndConnectToPlayer;
            return;
        }

        MaxValue = _owner.MaxHealth;
        Value = _owner.CurrentHealth;

        // Conecta ao sinal de mudança de vida
        if (!_owner.IsConnected(Actor.SignalName.HealthChanged, Callable.From<float, float>(OnHealthChanged)))
        {
            _owner.HealthChanged += OnHealthChanged;
            GD.Print($"[HEALTHBAR] Connected to {_owner.Name}'s HealthChanged signal");
        }
    }
    
    public void OnHealthChanged(float current, float max)
    {
        Value = current;
        MaxValue = max;
    }
}
