using Godot;

namespace EcoDoFarolCentral
{
    /// <summary>
    /// Script pra registrar o player no GameManager quando a cena do jogo carregar
    /// </summary>
    public partial class GameSceneController : Node
    {
        public override void _Ready()
        {
            CallDeferred(nameof(RegisterPlayer));
        }

        private void RegisterPlayer()
        {
            // Procura o player na cena
            var player = GetTree().Root.FindChild("Player", true, false) as Player;

            if (player == null)
            {
                GD.PrintErr("[GAME SCENE] Player not found in scene!");
                return;
            }

            // Registra o player no GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CurrentPlayer = player;
                GD.Print("[GAME SCENE] Player registered with GameManager");

                // Se carregamos um save, aplicamos agora
                if (SaveSystem.SaveFileExists())
                {
                    GameManager.Instance.ApplySaveDataIfLoaded();
                }
            }
            else
            {
                GD.PrintErr("[GAME SCENE] GameManager instance not found!");
            }
        }
    }
}
