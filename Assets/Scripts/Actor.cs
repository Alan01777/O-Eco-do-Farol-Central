using Godot;

namespace EcoDoFarolCentral
{
    /// <summary>
    /// Classe base para personagens (física, vida, etc)
    /// </summary>
    public partial class Actor : CharacterBody2D
    {
        [Export] public float Speed = 300.0f;
        [Export] public float JumpVelocity = -400.0f;
        [Export] public float DoubleJumpVelocity = -350.0f;

        [Export] public float DashSpeed = 800.0f;
        [Export] public float DashDuration = 0.2f;
        [Export] public float DashCooldown = 0.5f;
        [Export] public float MaxHealth = 100;
        [Export] public float CurrentHealth = 100;
        protected bool _isDead = false;

        [Signal]
        public delegate void HealthChangedEventHandler(float current, float max);
        [Signal]
        public delegate void DiedEventHandler();

        public float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

        protected Timer _invincibilityTimer;

        public override void _Ready()
        {
            CurrentHealth = MaxHealth;
            _invincibilityTimer = GetNodeOrNull<Timer>("Timer to take damage");
            if (_invincibilityTimer == null)
                _invincibilityTimer = GetNodeOrNull<Timer>("InvincibilityTimer");
        }

        public virtual void TakeDamage(float amount, Vector2? sourcePosition = null)
        {
            if (_isDead) return;

            // Verifica se o timer de invencibilidade está rodando
            if (_invincibilityTimer != null && !_invincibilityTimer.IsStopped())
            {
                return;
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
            GD.Print($"{Name} took {amount} damage. Health: {CurrentHealth}/{MaxHealth}");

            // Inicia timer de invencibilidade após receber dano
            if (_invincibilityTimer != null)
            {
                _invincibilityTimer.Start();
            }

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        public virtual void Die()
        {
            if (_isDead) return;
            _isDead = true;
            EmitSignal(SignalName.Died);
            GD.Print($"{Name} has died.");
        }

        public void ApplyGravity(double delta)
        {
            if (!IsOnFloor())
            {
                Vector2 v = Velocity;
                v.Y += Gravity * (float)delta;
                Velocity = v;
            }
        }
    }
}