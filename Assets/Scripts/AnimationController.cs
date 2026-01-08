using Godot;

namespace EcoDoFarolCentral
{
    public partial class AnimationController
    {
        private AnimatedSprite2D _sprite;
        private AudioStreamPlayer _audioSFX;    // Para efeitos sonoros (hurt, jump, etc)
        private AudioStreamPlayer _audioSteps;  // Para sons de passos/corrida
        private AudioStreamPlayer _audioVoice;  // Para sons de voz/ataques
        private string _currentAnim = "";

        /// <summary>
        /// Inicializa o controller com o sprite e os três canais de áudio.
        /// </summary>
        public void Initialize(
            AnimatedSprite2D sprite,
            AudioStreamPlayer audioSFX,
            AudioStreamPlayer audioSteps,
            AudioStreamPlayer audioVoice)
        {
            _sprite = sprite;
            _audioSFX = audioSFX;
            _audioSteps = audioSteps;
            _audioVoice = audioVoice;
        }

        /// <summary>
        /// Toca uma animação se ela já não estiver tocando.
        /// </summary>
        public void Play(string animName, bool forceRestart = false, string audioPath = null)
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
        /// Toca um áudio no canal de SFX (efeitos sonoros gerais).
        /// </summary>
        /// <param name="volumeDb">Volume em decibéis. 0 dB = normal, -6 dB = metade, +6 dB = dobro</param>
        public void PlaySFX(string audioPath, float pitchMin = 1f, float pitchMax = 1f, float volumeDb = 0f)
        {
            PlayAudio(_audioSFX, audioPath, pitchMin, pitchMax, volumeDb);
        }

        /// <summary>
        /// Toca um áudio no canal de Steps (sons de passos/movimento).
        /// </summary>
        /// <param name="volumeDb">Volume em decibéis. 0 dB = normal, -6 dB = metade, +6 dB = dobro</param>
        public void PlaySteps(string audioPath, float pitchMin = 1f, float pitchMax = 1f, float volumeDb = 0f)
        {
            PlayAudio(_audioSteps, audioPath, pitchMin, pitchMax, volumeDb);
        }

        /// <summary>
        /// Toca um áudio no canal de Voice (sons de voz/ataques).
        /// </summary>
        /// <param name="volumeDb">Volume em decibéis. 0 dB = normal, -6 dB = metade, +6 dB = dobro</param>
        public void PlayVoice(string audioPath, float pitchMin = 1f, float pitchMax = 1f, float volumeDb = 0f)
        {
            PlayAudio(_audioVoice, audioPath, pitchMin, pitchMax, volumeDb);
        }

        /// <summary>
        /// Helper privado que carrega e toca um áudio em um AudioStreamPlayer específico.
        /// </summary>
        private void PlayAudio(AudioStreamPlayer player, string audioPath, float pitchMin = 1f, float pitchMax = 1f, float volumeDb = 0f)
        {
            if (player == null || string.IsNullOrEmpty(audioPath)) return;

            // Carrega o AudioStream do arquivo
            var audioStream = GD.Load<AudioStream>(audioPath);
            if (audioStream == null)
            {
                GD.PushWarning($"Não foi possível carregar o áudio: {audioPath}");
                return;
            }
            
            if (pitchMin != 1f || pitchMax != 1f)
            {
                player.PitchScale = (float)GD.RandRange(pitchMin, pitchMax);
            }

            player.VolumeDb = volumeDb;

            // Atribui o stream e toca
            player.Stream = audioStream;
            player.Play();
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
