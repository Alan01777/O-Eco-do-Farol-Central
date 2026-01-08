using Godot;
using System;
using DialogueManagerRuntime;

namespace EcoDoFarolCentral
{
    public partial class OldMan : Actor
    {
        private enum WanderState { Idle, Walking }
        private WanderState _currentState = WanderState.Idle;

        // Configurações de andar só (Wander)
        [ExportGroup("Wander")]
        [Export] public float WanderSpeed = 50f;
        [Export] public float MinWalkTime = 1.5f;
        [Export] public float MaxWalkTime = 3f;
        [Export] public float MinIdleTime = 2f;
        [Export] public float MaxIdleTime = 5f;

        private float _stateTimer = 0f;
        private float _direction = 1f;
        private AnimatedSprite2D _sprite;
        private bool _isInDialogue = false;

        public override void _Ready()
        {
            base._Ready();

            _sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");

            // Começa em idle
            _currentState = WanderState.Idle;
            _stateTimer = (float)GD.RandRange(MinIdleTime, MaxIdleTime);

            // Conecta aos sinais do DialogueManager
            var dialogueMgr = DialogueManager.Instance;
            if (dialogueMgr != null)
            {
                dialogueMgr.Connect("dialogue_started", Callable.From((Resource res) => _isInDialogue = true));
                dialogueMgr.Connect("dialogue_ended", Callable.From((Resource res) => _isInDialogue = false));
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            ApplyGravity(delta);

            // Para de andar durante diálogos
            if (_isInDialogue)
            {
                Velocity = new Vector2(0, Velocity.Y);
                UpdateAnimation();
                MoveAndSlide();
                return;
            }

            _stateTimer -= (float)delta;

            switch (_currentState)
            {
                case WanderState.Idle:
                    HandleIdleState();
                    break;
                case WanderState.Walking:
                    HandleWalkingState();
                    break;
            }

            UpdateAnimation();
            MoveAndSlide();
        }

        private void HandleIdleState()
        {
            Velocity = new Vector2(0, Velocity.Y);

            if (_stateTimer <= 0)
            {
                // Transição para Walking
                _currentState = WanderState.Walking;
                _stateTimer = (float)GD.RandRange(MinWalkTime, MaxWalkTime);

                // Escolhe uma direção aleatória
                _direction = GD.Randf() > 0.5f ? 1f : -1f;
            }
        }

        private void HandleWalkingState()
        {
            Velocity = new Vector2(_direction * WanderSpeed, Velocity.Y);

            if (_stateTimer <= 0)
            {
                // Transição para Idle
                _currentState = WanderState.Idle;
                _stateTimer = (float)GD.RandRange(MinIdleTime, MaxIdleTime);
            }
        }

        private void UpdateAnimation()
        {
            if (_sprite == null) return;

            // Flip sprite baseado na direção
            if (_currentState == WanderState.Walking)
            {
                _sprite.FlipH = _direction < 0;
                _sprite.Play("walk");
            }
            else
            {
                _sprite.Play("idle");
            }
        }
    }
}