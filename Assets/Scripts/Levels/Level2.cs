using Godot;
using System;

namespace EcoDoFarolCentral
{
    public partial class Level2 : Node2D
    {
        Player player;
        public AnimationPlayer TransitionScene;
        [Export] public string villageAreaPath = "res://Scenes/Levels/Level_1.tscn";
        [Export] public string LightHouseAreaPath = "res://Scenes/Levels/Level_3.tscn";


        // Coordenadas de spawn do player na vila
        [Export] public Vector2 villageSpawnPosition = new Vector2(3900, 380);

        public override void _Ready()
        {
            TransitionScene = GetNodeOrNull<AnimationPlayer>("TransitionScene/AnimationPlayer");
            if (TransitionScene != null) PlayTransition();
            GameManager.Instance.SaveGame();
        }

        public override void _Process(double delta)
        {
        }

        private async void PlayTransition()
        {
            TransitionScene.PlayBackwards("Transition");
            await ToSignal(TransitionScene, AnimationPlayer.SignalName.AnimationFinished);
        }

        public void OnLightHouseAreaEntered(Node2D body)
        {

            // Verifica se é o Player diretamente ou se o Owner é o Player
            if (body is Player || body.Owner is Player)
            {
                CallDeferred(nameof(ChangeToNextLevel));
            }
        }

        public void OnVillageAreaEntered(Node2D body)
        {

            // Verifica se é o Player diretamente ou se o Owner é o Player
            if (body is Player || body.Owner is Player)
            {
                CallDeferred(nameof(ChangeToPreviousLevel));
            }
        }

        private async void ChangeToNextLevel()
        {
            // Salva dados do player antes de trocar de cena
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SavePlayerDataForTransition();
            }

            if (TransitionScene != null)
            {
                TransitionScene.Play("Transition");

                // Aguarda a animação terminar usando async/await (mais seguro que eventos)
                await ToSignal(TransitionScene, AnimationPlayer.SignalName.AnimationFinished);
            }

            // Carrega a cena dinamicamente para evitar referência circular
            var LightHouseArea = GD.Load<PackedScene>(LightHouseAreaPath);
            GetTree().ChangeSceneToPacked(LightHouseArea);
        }

        private async void ChangeToPreviousLevel()
        {
            // Salva dados do player antes de trocar de cena
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SavePlayerDataForTransition();
                // Define onde o player vai aparecer na vila (perto da entrada da caverna)
                GameManager.Instance.TransitionSpawnPosition = villageSpawnPosition;
            }

            if (TransitionScene != null)
            {
                TransitionScene.Play("Transition");

                // Aguarda a animação terminar usando async/await (mais seguro que eventos)
                await ToSignal(TransitionScene, AnimationPlayer.SignalName.AnimationFinished);
            }

            // Carrega a cena dinamicamente para evitar referência circular
            var villageArea = GD.Load<PackedScene>(villageAreaPath);
            GetTree().ChangeSceneToPacked(villageArea);
        }
    }
}