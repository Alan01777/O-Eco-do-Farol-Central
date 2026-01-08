using Godot;
using System.Collections.Generic;

namespace EcoDoFarolCentral
{
    /// <summary>
    /// Gerencia habilidades do player
    /// </summary>
    public partial class PlayerAbilities : Node
    {
        // false    = travado
        // true     = desbloqueado
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

        // Dados dos power-ups coletados (para reutilizar icones/descrições na UI)
        private Dictionary<string, PowerUpData> _collectedPowerUps = new();

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

        /// <summary>
        /// Retorna um dicionário com o estado atual das habilidades para salvamento
        /// </summary>
        public System.Collections.Generic.Dictionary<string, bool> GetUnlockedAbilities()
        {
            return new System.Collections.Generic.Dictionary<string, bool>
            {
                { "DoubleJump", CanDoubleJump },
                { "Dash", CanDash },
                { "WallJump", CanWallJump },
                { "JumpAttack", CanJumpAttack },
                { "Fireball", CanCastFireball }
            };
        }

        /// <summary>
        /// Carrega habilidades a partir de dados salvos
        /// </summary>
        public void LoadAbilities(System.Collections.Generic.Dictionary<string, bool> abilities, int maxComboLevel)
        {
            if (abilities == null) return;

            if (abilities.TryGetValue("DoubleJump", out bool doubleJump) && doubleJump) UnlockDoubleJump();
            if (abilities.TryGetValue("Dash", out bool dash) && dash) UnlockDash();
            if (abilities.TryGetValue("WallJump", out bool wallJump) && wallJump) UnlockWallJump();
            if (abilities.TryGetValue("JumpAttack", out bool jumpAttack) && jumpAttack) UnlockJumpAttack();
            if (abilities.TryGetValue("Fireball", out bool fireball) && fireball) UnlockFireball();

            if (maxComboLevel > 1) UnlockComboLevel(maxComboLevel);

            GD.Print($"[ABILITIES] Abilities loaded. MaxCombo: {MaxComboLevel}");
        }

        /// <summary>
        /// Registra os dados de um power-up coletado para uso na UI
        /// </summary>
        public void RegisterPowerUpData(PowerUpData data)
        {
            if (data == null) return;
            _collectedPowerUps[data.AbilityName] = data;
            GD.Print($"[ABILITIES] Registered power-up data: {data.AbilityName} with icon: {data.Icon != null}");
        }

        /// <summary>
        /// Obtém os dados de um power-up coletado pelo nome da habilidade
        /// </summary>
        public PowerUpData GetPowerUpData(string abilityName)
        {
            return _collectedPowerUps.TryGetValue(abilityName, out var data) ? data : null;
        }

        /// <summary>
        /// Obtém todos os dados de power-ups coletados
        /// </summary>
        public Dictionary<string, PowerUpData> GetAllPowerUpData()
        {
            return _collectedPowerUps;
        }
    }
}
