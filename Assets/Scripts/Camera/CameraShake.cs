using Godot;

namespace EcoDoFarolCentral
{
    /// <summary>
    /// Script para adicionar efeito de screen shake à câmera.
    /// Anexar ao node Camera2D do Player.
    /// </summary>
    public partial class CameraShake : Camera2D
    {
        private float _shakeIntensity = 0f;

        private float _shakeDuration = 0f;

        // Offset original da câmera
        private Vector2 _originalOffset;

        // Random para gerar variação
        private RandomNumberGenerator _rng = new RandomNumberGenerator();

        public override void _Ready()
        {
            _originalOffset = Offset;
            _rng.Randomize();
        }

        public override void _Process(double delta)
        {
            if (_shakeDuration > 0)
            {
                _shakeDuration -= (float)delta;

                // Calcula o offset aleatório baseado na intensidade
                float offsetX = _rng.RandfRange(-_shakeIntensity, _shakeIntensity);
                float offsetY = _rng.RandfRange(-_shakeIntensity, _shakeIntensity);

                Offset = _originalOffset + new Vector2(offsetX, offsetY);

                // Diminui a intensidade gradualmente
                _shakeIntensity = Mathf.Lerp(_shakeIntensity, 0, (float)delta * 5f);
            }
            else if (Offset != _originalOffset)
            {
                // Reseta para posição original quando termina
                Offset = _originalOffset;
            }
        }

        /// <summary>
        /// Inicia o efeito de screen shake.
        /// </summary>
        /// <param name="intensity">Intensidade do shake em pixels</param>
        /// <param name="duration">Duração em segundos</param>
        public void Shake(float intensity = 5f, float duration = 0.2f)
        {
            _shakeIntensity = intensity;
            _shakeDuration = duration;
        }
    }
}
