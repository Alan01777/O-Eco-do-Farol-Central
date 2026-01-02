using Godot;
using System;

namespace EcoDoFarolCentral
{
    /// <summary>
    /// Area that deals damage and teleports player to last safe ground.
    /// Used for spikes, lava, bottomless pits, etc.
    /// </summary>
    public partial class DangerZone : Area2D
    {
        [Export] public float Damage { get; set; } = 10f;
        [Export] public bool InstantKill { get; set; } = false;
        [Export] public float RespawnDelay { get; set; } = 0.5f;

        public override void _Ready()
        {
            // Ensure we detect bodies
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
                    // Pass GlobalPosition so Player knows direction for knockback/hurt state
                    player.TakeDamage(Damage, GlobalPosition);

                    // Only respawn if player is still alive
                    if (player.CurrentHealth > 0)
                    {
                        // Slight delay before respawn for impact
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
