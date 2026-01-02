using Godot;

namespace EcoDoFarolCentral
{
    /// <summary>
    /// Gerencia habilidades do player
    /// </summary>
    public partial class PlayerAbilities : Node
    {
        // Movimento (todas bloqueadas por padrão)
        // Jogador precisa coletar os itens para desbloquear (trabalha vagabundo)
        // false = travado
        // true = desbloqueado
        [Export] public bool CanDoubleJump { get; private set; } = false;
        [Export] public bool CanDash { get; private set; } = false;
        [Export] public bool CanWallJump { get; private set; } = false;

        // Combate (apenas ataque básico)
        [Export] public int MaxComboLevel { get; private set; } = 1;  // só ataque 1 no início
        [Export] public bool CanJumpAttack { get; private set; } = true;
        [Export] public bool CanCastFireball { get; private set; } = false;

        // Sinal emitido quando uma habilidade é desbloqueada
        [Signal]
        public delegate void AbilityUnlockedEventHandler(string abilityName);

        /// <summary>
        /// Desbloqueia a habilidade de pulo duplo
        /// </summary>
        public void UnlockDoubleJump()
        {
            if (!CanDoubleJump)
            {
                CanDoubleJump = true;
                EmitSignal(SignalName.AbilityUnlocked, "DoubleJump");
                GD.Print("[ABILITIES] Double Jump unlocked!");
            }
        }

        /// <summary>
        /// Desbloqueia a habilidade de dash
        /// </summary>
        public void UnlockDash()
        {
            if (!CanDash)
            {
                CanDash = true;
                EmitSignal(SignalName.AbilityUnlocked, "Dash");
                GD.Print("[ABILITIES] Dash unlocked!");
            }
        }

        /// <summary>
        /// Desbloqueia a habilidade de wall jump (TODO: implementar wall jump)
        /// </summary>
        public void UnlockWallJump()
        {
            if (!CanWallJump)
            {
                CanWallJump = true;
                EmitSignal(SignalName.AbilityUnlocked, "WallJump");
                GD.Print("[ABILITIES] Wall Jump unlocked!");
            }
        }

        /// <summary>
        /// Desbloqueia a habilidade de ataque de pulo
        /// </summary>
        public void UnlockJumpAttack()
        {
            if (!CanJumpAttack)
            {
                CanJumpAttack = true;
                EmitSignal(SignalName.AbilityUnlocked, "JumpAttack");
                GD.Print("[ABILITIES] Jump Attack unlocked!");
            }
        }

        /// <summary>
        /// Desbloqueia a habilidade de fireball
        /// </summary>
        public void UnlockFireball()
        {
            if (!CanCastFireball)
            {
                CanCastFireball = true;
                EmitSignal(SignalName.AbilityUnlocked, "Fireball");
                GD.Print("[ABILITIES] Fireball unlocked!");
            }
        }

        /// <summary>
        /// Desbloqueia o nível de combo (2 ou 3)
        /// </summary>
        public void UnlockComboLevel(int level)
        {
            if (level > MaxComboLevel)
            {
                MaxComboLevel = level;
                EmitSignal(SignalName.AbilityUnlocked, $"ComboLevel{level}");
                GD.Print($"[ABILITIES] Combo Level {level} unlocked!");
            }
        }

        /// <summary>
        /// Aumenta a saúde máxima do jogador
        /// </summary>
        public void UpgradeHealth(Actor actor, float amount)
        {
            actor.MaxHealth += amount;
            actor.CurrentHealth = actor.MaxHealth; // cura tudo
            EmitSignal(SignalName.AbilityUnlocked, "HealthUpgrade");
            GD.Print($"[ABILITIES] Health upgraded by {amount}!");
        }
    }
}
