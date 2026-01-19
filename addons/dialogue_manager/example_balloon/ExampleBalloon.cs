using Godot;
using Godot.Collections;

namespace DialogueManagerRuntime
{
    public partial class ExampleBalloon : CanvasLayer
    {
        [Export] public Resource DialogueResource;
        [Export] public string StartFromTitle = "";
        [Export] public bool AutoStart = false;
        [Export] public string NextAction = "ui_accept";
        [Export] public string SkipAction = "ui_cancel";


        Control balloon;
        RichTextLabel characterLabel;
        RichTextLabel dialogueLabel;
        VBoxContainer responsesMenu;
        Polygon2D progress;
        TextureRect portrait;

        Array<Variant> temporaryGameStates = new Array<Variant>();
        bool isWaitingForInput = false;
        bool willHideBalloon = false;

        DialogueLine dialogueLine;
        DialogueLine DialogueLine
        {
            get => dialogueLine;
            set
            {
                // Dialogue has finished so close the balloon
                if (value == null)
                {
                    if (Owner == null)
                    {
                        QueueFree();
                    }
                    else
                    {
                        Hide();
                    }
                    return;
                }

                dialogueLine = value;
                ApplyDialogueLine();
            }
        }

        Timer MutationCooldown = new Timer();


        public override void _Ready()
        {
            balloon = GetNode<Control>("%Balloon");
            characterLabel = GetNode<RichTextLabel>("%CharacterLabel");
            dialogueLabel = GetNode<RichTextLabel>("%DialogueLabel");
            responsesMenu = GetNode<VBoxContainer>("%ResponsesMenu");
            progress = GetNode<Polygon2D>("%Progress");
            portrait = GetNode<TextureRect>("%Portrait");

            balloon.Hide();

            balloon.GuiInput += (@event) =>
            {
                if ((bool)dialogueLabel.Get("is_typing"))
                {
                    bool mouseWasClicked = @event is InputEventMouseButton && (@event as InputEventMouseButton).ButtonIndex == MouseButton.Left && @event.IsPressed();
                    bool skipButtonWasPressed = @event.IsActionPressed(SkipAction);
                    if (mouseWasClicked || skipButtonWasPressed)
                    {
                        GetViewport().SetInputAsHandled();
                        dialogueLabel.Call("skip_typing");
                        return;
                    }
                }

                if (!isWaitingForInput) return;
                if (dialogueLine.Responses.Count > 0) return;

                GetViewport().SetInputAsHandled();

                if (@event is InputEventMouseButton && @event.IsPressed() && (@event as InputEventMouseButton).ButtonIndex == MouseButton.Left)
                {
                    Next(dialogueLine.NextId);
                }
                else if (@event.IsActionPressed(NextAction) && GetViewport().GuiGetFocusOwner() == balloon)
                {
                    Next(dialogueLine.NextId);
                }
            };

            if (string.IsNullOrEmpty((string)responsesMenu.Get("next_action")))
            {
                responsesMenu.Set("next_action", NextAction);
            }
            responsesMenu.Connect("response_selected", Callable.From((DialogueResponse response) =>
            {
                Next(response.NextId);
            }));


            // Hide the balloon when a mutation is running
            MutationCooldown.Timeout += () =>
            {
                if (willHideBalloon)
                {
                    willHideBalloon = false;
                    balloon.Hide();
                }
            };
            AddChild(MutationCooldown);

            DialogueManager.Mutated += OnMutated;

            if (AutoStart)
            {
                if (!IsInstanceValid(DialogueResource))
                {
                    throw new System.Exception(DialogueManager.GetErrorMessage(143));
                }
                Start();
            }
        }


        public override void _ExitTree()
        {
            DialogueManager.Mutated -= OnMutated;
        }


        public override void _UnhandledInput(InputEvent @event)
        {
            // Only the balloon is allowed to handle input while it's showing
            GetViewport().SetInputAsHandled();
        }


        public override async void _Notification(int what)
        {
            // Detect a change of locale and update the current dialogue line to show the new language
            if (what == NotificationTranslationChanged && IsInstanceValid(dialogueLabel))
            {
                float visibleRatio = dialogueLabel.VisibleRatio;
                DialogueLine = await DialogueManager.GetNextDialogueLine(DialogueResource, DialogueLine.Id, temporaryGameStates);
                if (visibleRatio < 1.0f)
                {
                    dialogueLabel.Call("skip_typing");
                }
            }
        }


        public override void _Process(double delta)
        {
            base._Process(delta);

            if (IsInstanceValid(dialogueLine))
            {
                progress.Visible = !(bool)dialogueLabel.Get("is_typing") && dialogueLine.Responses.Count == 0 && !dialogueLine.HasTag("voice");
            }
        }


        public async void Start(Resource dialogueResource = null, string title = "", Array<Variant> extraGameStates = null)
        {
            temporaryGameStates = new Array<Variant> { this } + (extraGameStates ?? new Array<Variant>());
            isWaitingForInput = false;

            if (IsInstanceValid(dialogueResource))
            {
                DialogueResource = dialogueResource;
            }
            if (title != "")
            {
                StartFromTitle = title;
            }

            DialogueLine = await DialogueManager.GetNextDialogueLine(DialogueResource, StartFromTitle, temporaryGameStates);
            Show();
        }


        public async void Next(string nextId)
        {
            DialogueLine = await DialogueManager.GetNextDialogueLine(DialogueResource, nextId, temporaryGameStates);
        }


        #region Helpers


        private async void ApplyDialogueLine()
        {
            MutationCooldown.Stop();

            isWaitingForInput = false;
            balloon.FocusMode = Control.FocusModeEnum.All;
            balloon.GrabFocus();

            // Set up the character name
            characterLabel.Visible = !string.IsNullOrEmpty(dialogueLine.Character);
            characterLabel.Text = Tr(dialogueLine.Character, "dialogue");

            // Set up the portrait
            UpdatePortrait(dialogueLine.Character);

            // Set up the dialogue
            dialogueLabel.Hide();
            dialogueLabel.Set("dialogue_line", dialogueLine);

            // Set up the responses
            responsesMenu.Hide();
            responsesMenu.Set("responses", dialogueLine.Responses);

            // Type out the text
            balloon.Show();
            willHideBalloon = false;
            dialogueLabel.Show();
            if (!string.IsNullOrEmpty(dialogueLine.Text))
            {
                dialogueLabel.Call("type_out");
                await ToSignal(dialogueLabel, "finished_typing");
            }

            // Wait for input
            if (dialogueLine.Responses.Count > 0)
            {
                balloon.FocusMode = Control.FocusModeEnum.None;
                responsesMenu.Show();
            }
            else if (!string.IsNullOrEmpty(dialogueLine.Time))
            {
                float time = 0f;
                if (!float.TryParse(dialogueLine.Time, out time))
                {
                    time = dialogueLine.Text.Length * 0.02f;
                }
                await ToSignal(GetTree().CreateTimer(time), "timeout");
                Next(dialogueLine.NextId);
            }
            else
            {
                isWaitingForInput = true;
                balloon.FocusMode = Control.FocusModeEnum.All;
                balloon.GrabFocus();
            }
        }


        /// <summary>
        /// Updates the portrait based on character name.
        /// Converts character name to snake_case and loads from Assets/portraits/{name}.png
        /// </summary>
        private void UpdatePortrait(string characterName)
        {
            if (string.IsNullOrEmpty(characterName))
            {
                portrait.Texture = null;
                portrait.Visible = false;
                return;
            }

            // Convert character name to snake_case for file path
            string fileName = ToSnakeCase(characterName);
            string portraitPath = $"res://Assets/portraits/{fileName}.png";

            if (ResourceLoader.Exists(portraitPath))
            {
                portrait.Texture = GD.Load<Texture2D>(portraitPath);
                portrait.Visible = true;
            }
            else
            {
                portrait.Texture = null;
                portrait.Visible = false;
                GD.Print($"[DialogueBalloon] Portrait not found: {portraitPath}");
            }
        }


        /// <summary>
        /// Converts a string to snake_case (e.g., "OldMan" -> "old_man", "Ancião da Vila" -> "anciao_da_vila")
        /// Handles accented Portuguese characters and spaces.
        /// </summary>
        private string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var result = new System.Text.StringBuilder();
            bool lastWasUnderscore = false;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                // Handle spaces - convert to underscore
                if (c == ' ')
                {
                    if (!lastWasUnderscore && result.Length > 0)
                    {
                        result.Append('_');
                        lastWasUnderscore = true;
                    }
                    continue;
                }

                // Remove accents from characters
                char normalized = RemoveAccent(c);

                if (char.IsUpper(normalized))
                {
                    if (i > 0 && result.Length > 0 && !lastWasUnderscore)
                    {
                        result.Append('_');
                    }
                    result.Append(char.ToLower(normalized));
                    lastWasUnderscore = false;
                }
                else if (char.IsLetterOrDigit(normalized) || normalized == '_')
                {
                    result.Append(char.ToLower(normalized));
                    lastWasUnderscore = (normalized == '_');
                }
            }
            return result.ToString();
        }


        /// <summary>
        /// Removes accents from Portuguese characters (ã→a, ç→c, é→e, etc.)
        /// </summary>
        private char RemoveAccent(char c)
        {
            return c switch
            {
                'á' or 'à' or 'ã' or 'â' or 'ä' => 'a',
                'Á' or 'À' or 'Ã' or 'Â' or 'Ä' => 'A',
                'é' or 'è' or 'ê' or 'ë' => 'e',
                'É' or 'È' or 'Ê' or 'Ë' => 'E',
                'í' or 'ì' or 'î' or 'ï' => 'i',
                'Í' or 'Ì' or 'Î' or 'Ï' => 'I',
                'ó' or 'ò' or 'õ' or 'ô' or 'ö' => 'o',
                'Ó' or 'Ò' or 'Õ' or 'Ô' or 'Ö' => 'O',
                'ú' or 'ù' or 'û' or 'ü' => 'u',
                'Ú' or 'Ù' or 'Û' or 'Ü' => 'U',
                'ç' => 'c',
                'Ç' => 'C',
                'ñ' => 'n',
                'Ñ' => 'N',
                _ => c
            };
        }


        #endregion


        #region signals


        private void OnMutated(Dictionary _mutation)
        {
            isWaitingForInput = false;
            willHideBalloon = true;
            MutationCooldown.Start(0.1f);
        }


        #endregion
    }
}
