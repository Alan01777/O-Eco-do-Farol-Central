using Godot;
using System;

namespace EcoDoFarolCentral
{
    /// <summary>
    /// Area que dá dano e teleporta o player pra um terreno seguro
    /// </summary>
    public partial class DangerZone : Area2D
    {
        [Export] public float Damage { get; set; } = 10f;
        [Export] public bool InstantKill { get; set; } = false;
        [Export] public float RespawnDelay { get; set; } = 0.5f;

        public override void _Ready()
        {
            BodyEntered += OnBodyEntered;
        }

        private void OnBodyEntered(Node2D body)
        {
            if (body is Player player)
            {
                if (InstantKill)
                {
                    player.TakeDamage(1000f);
                }
                else
                {
                    player.TakeDamage(Damage, GlobalPosition);

                    if (player.CurrentHealth > 0)
                    {
                        var timer = GetTree().CreateTimer(RespawnDelay);
                        timer.Timeout += () => RespawnPlayer(player);
                    }
                }
            }
        }

        private void RespawnPlayer(Player player)
        {
            player.RespawnAtSafePosition();
        }
    }
}
