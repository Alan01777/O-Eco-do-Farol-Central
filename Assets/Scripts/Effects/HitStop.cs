using Godot;

namespace EcoDoFarolCentral
{
    /// <summary>
    /// Sistema de HitStop (freeze frame) para dar impacto aos ataques.
    /// Singleton autoload - adicione como AutoLoad no projeto.
    /// </summary>
    public partial class HitStop : Node
    {
        public static HitStop Instance { get; private set; }

        public override void _Ready()
        {
            Instance = this;
        }

        /// <summary>
        /// Congela o jogo por um breve momento para dar impacto.
        /// </summary>
        /// <param name="duration">Duração do freeze em segundos (padrão: 0.06s)</param>
        public async void Freeze(float duration = 0.06f)
        {
            // Se já tem um freeze ativo, ignora
            if (Engine.TimeScale < 1.0f) return;

            Engine.TimeScale = 0.0f;

            // Usa Task.Delay do .NET que funciona independente do TimeScale
            await System.Threading.Tasks.Task.Delay((int)(duration * 1000));

            Engine.TimeScale = 1.0f;
        }

        /// <summary>
        /// Versão com slow motion gradual ao invés de freeze total.
        /// </summary>
        /// <param name="slowScale">Time scale durante o efeito (0.1 = 10% da velocidade)</param>
        /// <param name="duration">Duração do efeito</param>
        public async void SlowMotion(float slowScale = 0.1f, float duration = 0.1f)
        {
            if (Engine.TimeScale < 1.0f) return;

            Engine.TimeScale = slowScale;

            // Usa Task.Delay do .NET que funciona independente do TimeScale
            await System.Threading.Tasks.Task.Delay((int)(duration * 1000));

            Engine.TimeScale = 1.0f;
        }
    }
}
