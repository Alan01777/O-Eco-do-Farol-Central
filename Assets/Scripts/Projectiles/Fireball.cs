using Godot;

namespace EcoDoFarolCentral
{
    public partial class Fireball : Area2D
    {
        [Export] public float Speed = 400f;
        [Export] public float Damage = 10f;
        [Export] public float LifeTime = 2f;

        private Vector2 _direction = Vector2.Zero;
        private AnimatedSprite2D _sprite;
        private bool _isExploding = false;
        private CollisionShape2D _collisionShape;

        public override void _Ready()
        {
            _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            _collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
            _collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

            // Conecta sinais
            BodyEntered += OnBodyEntered;
            AreaEntered += OnAreaEntered;

            // Conecta sinal de fim de animação para limpeza pós-explosão
            _sprite.AnimationFinished += OnAnimationFinished;

            // Configura notificador de visibilidade
            var visibilityNotifier = GetNodeOrNull<VisibleOnScreenNotifier2D>("VisibleOnScreenNotifier2D");
            if (visibilityNotifier != null)
            {
                visibilityNotifier.ScreenExited += OnScreenExited;
            }
            else
            {
                // Fallback de tempo de vida se não houver notificador
                GetTree().CreateTimer(LifeTime).Timeout += QueueFree;
            }

            // Garante animação de viagem inicial
            _sprite.Play("travel");
        }

        public void Setup(Vector2 direction)
        {
            _direction = direction.Normalized();

            _direction = direction.Normalized();

            // Espelha sprite baseado na direção
            // Verifica null caso Setup seja chamado antes do _Ready
            if (_sprite != null && _direction.X < 0)
            {
                _sprite.FlipH = true;
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_isExploding) return; // Para movimento se estiver explodindo

            // Aplica espelhamento aqui se necessário
            if (_sprite != null)
            {
                if (_direction.X < 0) _sprite.FlipH = true;
                else _sprite.FlipH = false;
            }

            GlobalPosition += _direction * Speed * (float)delta;
        }

        private void OnBodyEntered(Node2D body)
        {
            if (_isExploding) return;

            if (_isExploding) return;

            // Ignora colisão com o player
            if (body is Player) return;

            // Explode ao tocar paredes ou chão
            if (body is TileMap || body is TileMapLayer || body is StaticBody2D)
            {
                Explode();
            }
        }

        private void OnAreaEntered(Area2D area)
        {
            if (_isExploding) return;

            if (_isExploding) return;

            // Atingiu inimigo
            if (area.GetParent() is Actor enemy && enemy is not Player)
            {
                enemy.TakeDamage(Damage, GlobalPosition);
                Explode();
            }
        }

        private void OnScreenExited()
        {
            if (!_isExploding)
            {
                QueueFree();
            }
        }

        private void Explode()
        {
            _isExploding = true;
            _direction = Vector2.Zero;

            // Disable collision to prevent double hits
            if (_collisionShape != null)
            {
                _collisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
            }

            if (_sprite != null)
            {
                _sprite.Play("explosion");
            }
            if (_sprite != null)
            {
                _sprite.Play("explosion");
            }
            // Objeto será destruído em OnAnimationFinished
        }

        private void OnAnimationFinished()
        {
            if (_sprite.Animation == "explosion")
            {
                QueueFree();
            }
        }
    }
}
