using EcoDoFarolCentral;
using Godot;
using System;

namespace EcoDoFarolCentral
{
    public partial class Level3 : Node2D
    {
        public AnimationPlayer transitionScene;
        public Player player;
        public bool CanUseTheLight = false;
        private bool _lighthouseActivated = false;

        [Export] public string caveAreaPath = "res://Scenes/Levels/Level_2.tscn";

        // Configuração do Farol
        [ExportGroup("Lighthouse")]
        [Export] public AnimationPlayer FarolAnimationPlayer;
        [Export] public string LightOnAnimation = "ligar_farol";

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            transitionScene = GetNodeOrNull<AnimationPlayer>("TransitionScene/AnimationPlayer");
            GameManager.Instance.SaveGame();
        }

        private void ActivateLighthouse()
        {
            GD.Print("[LEVEL3] Tentando ativar o farol");
            if (FarolAnimationPlayer != null)
            {
                FarolAnimationPlayer.Play(LightOnAnimation);
                _lighthouseActivated = true;
                CanUseTheLight = false;
                GD.Print("[LEVEL3] Farol ativado!");
            }
            else
            {
                GD.PrintErr("[LEVEL3] FarolAnimationPlayer não configurado!");
            }
        }

        public void OnCaveAreaEntered(Node2D body)
        {
            if (body is Player)
            {
                // Usa CallDeferred para adiar a troca de cena, evitando erro durante callback de física
                CallDeferred(nameof(ChangeToPreviousLevel));
            }
        }

        private async void ChangeToPreviousLevel()
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

            // Carrega a cena dinamicamente para evitar referência circular
            var caveArea = GD.Load<PackedScene>(caveAreaPath);
            GetTree().ChangeSceneToPacked(caveArea);
        }
    }
}