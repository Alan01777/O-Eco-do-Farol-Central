using Godot;
using System;

namespace EcoDoFarolCentral
{
    public partial class Level2 : Node2D
    {
        public AnimationPlayer TransitionScene;
        
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
    }
}