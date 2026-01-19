using Godot;

namespace EcoDoFarolCentral
{
    public partial class Intro : CanvasLayer
    {
        private Button _continueButton;
        private AnimationPlayer _transitionPlayer;

        public override void _Ready()
        {

            // Obtém referências
            _continueButton = GetNode<Button>("ColorRect/CenterContainer/VBoxContainer/Button");
            _transitionPlayer = GetNode<AnimationPlayer>("TransitionScene/AnimationPlayer");

            // Desabilita mouse no RichTextLabel para não bloquear cliques no botão
            var richText = GetNode<RichTextLabel>("ColorRect/CenterContainer/VBoxContainer/RichTextLabel");
            richText.MouseFilter = Control.MouseFilterEnum.Ignore;

            // Desabilita mouse no ColorRect da TransitionScene (ele bloqueia mesmo sendo transparente!)
            var transitionColorRect = GetNode<ColorRect>("TransitionScene/ColorRect");
            transitionColorRect.MouseFilter = Control.MouseFilterEnum.Ignore;

            // Conecta o botão
            _continueButton.Pressed += OnContinuePressed;

        }

        public async void OnContinuePressed()
        {

            // Toca animação de fade out
            _transitionPlayer.Play("Transition");

            // Aguarda a animação terminar
            await ToSignal(_transitionPlayer, AnimationPlayer.SignalName.AnimationFinished);

            // Muda para o Level 1
            GetTree().ChangeSceneToFile("res://Scenes/Levels/Level_1.tscn");
        }
    }
}
