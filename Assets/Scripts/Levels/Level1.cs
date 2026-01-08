using EcoDoFarolCentral;
using Godot;
using System;

namespace EcoDoFarolCentral
{
    public partial class Level1 : Node2D
    {
        public AnimationPlayer transitionScene;
        public Player player;
        [Export] public PackedScene caveArea;
        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            transitionScene = GetNodeOrNull<AnimationPlayer>("TransitionScene/AnimationPlayer");
            GameManager.Instance.SaveGame();
        }

        public override void _Process(double delta)
        {
        }

        public void OnCaveAreaEntered(Node2D body)
        {
            if (body is Player)
            {
                // Usa CallDeferred para adiar a troca de cena, evitando erro durante callback de física
                CallDeferred(nameof(ChangeToNextLevel));
            }
        }

        private async void ChangeToNextLevel()
        {
            // Salva dados do player antes de trocar de cena
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SavePlayerDataForTransition();
            }

            if (transitionScene != null)
            {
                transitionScene.Play("Transition");

                // Aguarda a animação terminar usando async/await (mais seguro que eventos)
                await ToSignal(transitionScene, AnimationPlayer.SignalName.AnimationFinished);
            }

            GetTree().ChangeSceneToPacked(caveArea);
        }

    }
}