using EcoDoFarolCentral;
using Godot;
using System;
using System.Collections.Generic;

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

        // Configuração do Alçapão
        [ExportGroup("Trapdoor")]
        [Export] public AnimationPlayer TrapdoorAnimationPlayer;
        [Export] public string TrapdoorOpenAnimation = "open";
        [Export] public Node LanternsContainer; // Node que contém as lamparinas

        // Configuração do Final do Jogo
        [ExportGroup("Ending")]
        [Export] public float EndingDelaySeconds = 5.0f;
        [Export] public string CreditsScenePath = "res://Scenes/UI/Credits.tscn";
        [Export] public Node EnemiesContainer; // Node que contém os inimigos

        private List<LightBulbActionable> _lanterns = new();
        private bool _trapdoorOpened = false;
        private bool _endingTriggered = false;
        
        public override void _Ready()
        {
            transitionScene = GetNodeOrNull<AnimationPlayer>("TransitionScene/AnimationPlayer");
            player = GetNodeOrNull<Player>("Player");
            GameManager.Instance.SaveGame();

            // Encontra e registra todas as lamparinas
            FindAndConnectLanterns();
        }

        private void FindAndConnectLanterns()
        {
            _lanterns.Clear();

            // Se tiver um container específico, busca nele
            Node searchRoot = LanternsContainer ?? this;

            // Busca recursivamente por todas as lamparinas
            FindLanternsRecursive(searchRoot);


            // Conecta o sinal de cada lamparina
            foreach (var lantern in _lanterns)
            {
                lantern.LightActivated += OnLanternActivated;
            }
        }

        private void FindLanternsRecursive(Node node)
        {
            foreach (Node child in node.GetChildren())
            {

                // Verifica se é um LightBulb instanciado
                var lightBulb = child.GetNodeOrNull<LightBulbActionable>("Area2D");
                if (lightBulb != null)
                {
                    _lanterns.Add(lightBulb);
                }
                else if (child is LightBulbActionable lantern)
                {
                    _lanterns.Add(lantern);
                }
                else
                {
                    // Continua buscando nos filhos
                    FindLanternsRecursive(child);
                }
            }
        }

        private void OnLanternActivated()
        {
            if (_trapdoorOpened) return;

            // Conta quantas lamparinas estão ativadas
            int activatedCount = 0;
            foreach (var lantern in _lanterns)
            {
                if (lantern.IsActivated)
                {
                    activatedCount++;
                }
            }


            // Se todas estão ativadas, abre o alçapão
            if (activatedCount >= _lanterns.Count && _lanterns.Count > 0)
            {
                OpenTrapdoor();
            }
        }

        private void OpenTrapdoor()
        {
            if (_trapdoorOpened) return;
            _trapdoorOpened = true;


            if (TrapdoorAnimationPlayer != null)
            {
                TrapdoorAnimationPlayer.Play(TrapdoorOpenAnimation);
            }
            else
            {
            }
        }

        /// <summary>
        /// Chamado quando o farol é ativado (via LighthouseActionable ou manualmente)
        /// </summary>
        public void ActivateLighthouse()
        {
            if (_lighthouseActivated) return;


            _lighthouseActivated = true;
            CanUseTheLight = false;

            // Toca animação do farol
            if (FarolAnimationPlayer != null)
            {
                FarolAnimationPlayer.Play(LightOnAnimation);
            }

            // Inicia sequência de final do jogo
            TriggerEnding();
        }

        private async void TriggerEnding()
        {
            if (_endingTriggered) return;
            _endingTriggered = true;


            // Desativa input do player
            DisablePlayerInput();

            // Desativa todos os inimigos
            DisableAllEnemies();


            // Aguarda o timer
            await ToSignal(GetTree().CreateTimer(EndingDelaySeconds), "timeout");

            // Verifica se ainda é válido
            if (!IsInstanceValid(this)) return;


            // Toca transição se disponível
            if (transitionScene != null)
            {
                transitionScene.Play("Transition");
                await ToSignal(transitionScene, AnimationPlayer.SignalName.AnimationFinished);
            }

            // Vai para a cena de créditos
            GetTree().ChangeSceneToFile(CreditsScenePath);
        }

        private void DisablePlayerInput()
        {
            if (player == null)
            {
                player = GetNodeOrNull<Player>("Player");
            }

            if (player != null)
            {
                player.SetPhysicsProcess(false);
                player.SetProcess(false);
                player.Velocity = Vector2.Zero;

                // Força estado idle
                player.CurrentStateEnum = Player.PlayerStates.Idle;
                player.UpdateAnimations();

            }
        }

        private void DisableAllEnemies()
        {
            Node enemyRoot = EnemiesContainer ?? GetNodeOrNull<Node>("Enemies") ?? this;
            DisableEnemiesRecursive(enemyRoot);
        }

        private void DisableEnemiesRecursive(Node node)
        {
            foreach (Node child in node.GetChildren())
            {
                // Verifica se é um inimigo (ShadowEnemy, ShadowEnemy2, ShadowBoss)
                if (child is CharacterBody2D enemy &&
                    (child.GetType().Name.Contains("Shadow") || child.GetType().Name.Contains("Enemy") || child.GetType().Name.Contains("Boss")))
                {
                    enemy.SetPhysicsProcess(false);
                    enemy.SetProcess(false);
                    enemy.Velocity = Vector2.Zero;
                }
                else
                {
                    DisableEnemiesRecursive(child);
                }
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