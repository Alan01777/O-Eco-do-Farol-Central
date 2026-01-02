using Godot;

namespace EcoDoFarolCentral
{
    public partial class AnimationController
    {
        private AnimatedSprite2D _sprite;
        private string _currentAnim = "";

        public void Initialize(AnimatedSprite2D sprite) => _sprite = sprite;

        /// <summary>
        /// Toca uma animação se ela já não estiver tocando.
        /// </summary>
        public void Play(string animName, bool forceRestart = false)
        {
            if (_sprite == null) return;

            // Reinicia se forçado, mesmo que seja a mesma animação
            if (_currentAnim == animName && !forceRestart) return;

            if (forceRestart)
            {
                // Reseta o frame e força o play
                // Usar stop() tava causando conflito com sinais \_(o.o)_/
                _sprite.Frame = 0;
            }
            _sprite.Play(animName);
            _currentAnim = animName;
        }

        /// <summary>
        /// Atualiza o espelhamento do sprite baseado na direção horizontal.
        /// </summary>
        public void UpdateFlip(float direction)
        {
            if (direction == 0) return;
            _sprite.FlipH = direction < 0;
        }
    }
}
