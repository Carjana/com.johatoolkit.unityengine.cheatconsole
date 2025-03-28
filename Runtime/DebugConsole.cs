using System;
using System.Text;
using JohaToolkit.UnityEngine.DataStructures.Lists.CircularLinkedList;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JoHaToolkit.UnityEngine.CheatConsole
{
    public class DebugConsole : MonoBehaviour
    {
        public struct LogMessage
        {
            public string Message;
            public Color Color;
        }

        private class ChangeGUIColor : IDisposable
        {
            private readonly Color _oldColor;

            public ChangeGUIColor(Color color)
            {
                _oldColor = GUI.color;
                GUI.color = color;
            }

            public void Dispose()
            {
                GUI.color = _oldColor;
            }
        }

        public static DebugConsole Instance { get; private set; }
        
        [SerializeField] private InputActionReference toggleConsoleInputAction;
        [SerializeField] private InputActionReference executeCommandInputAction;

        [SerializeField] private bool catchConsoleLogs;
        [SerializeField] private int maxLogs;

        private const int TextInputFieldSpacing = 20;
        private const int TopSpacing = 30;
        private const int TextInputFieldHeight = 30;
        private const int LogHeight = 50;
        private const int SuggestionHeight = 20;
        private Rect _consoleRect;
        private Rect _inputRect;
        private Rect _suggestionsAreaRect;

        private Rect _logsRect;
        private Rect _logsContentRect;

        private Rect _suggestionsRect;
        private Rect _suggestionsContentRect;

        private Rect _toggleLogButtonRect;
        private Rect _clearLogButtonRect;
        private Rect _toggleSuggestionsButtonRect;

        private string _userInput = "";
        private bool _isConsoleShown;
        private bool _showLog = true;
        private bool _showSuggestions = true;

        private Vector2 _scrollPositionLogs;
        private Vector2 _scrollPositionSuggestions;

        private CircularLinkedList<LogMessage> _logs;
        private BaseCheatCommand[] _possibleCommands;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                DontDestroyOnLoad(this);
                Instance = this;
            }
            
            _logs = new CircularLinkedList<LogMessage>(maxLogs);
            _possibleCommands = Array.Empty<BaseCheatCommand>();
            RecalculateRects();

            toggleConsoleInputAction.action.performed += _ => ToggleConsole();
            executeCommandInputAction.action.performed += _ => HandleInput();

            Application.logMessageReceived += HandleLog;

            _scrollPositionLogs.y = _logsContentRect.height - _logsRect.height;
        }

        private void RecalculateRects()
        {
            _consoleRect = new Rect(Screen.width/2, Screen.height - GetConsoleRectHeight(), Screen.width/2, GetConsoleRectHeight());
            _suggestionsAreaRect = new Rect(0,Screen.height - GetSuggestionsHeight(),Screen.width/2,GetSuggestionsHeight());
            _inputRect = new Rect(0, Screen.height - TextInputFieldHeight, Screen.width, TextInputFieldHeight);

            _logsRect = new Rect(_consoleRect.x, Screen.height - _consoleRect.height + TopSpacing, _consoleRect.width, _consoleRect.height - TopSpacing - TextInputFieldHeight - TextInputFieldSpacing);
            _logsContentRect = new Rect(0, 0, _logsRect.width, _logs.Count * LogHeight);

            _clearLogButtonRect = new Rect(Screen.width - 100 - 120, Screen.height - _consoleRect.height, 100, TopSpacing);
            _toggleLogButtonRect = new Rect(Screen.width - 100, Screen.height - _consoleRect.height, 100, TopSpacing);
            _toggleSuggestionsButtonRect = new Rect(0, Screen.height - _suggestionsAreaRect.height, 150, TopSpacing);
            
            _suggestionsRect = new Rect(0, Screen.height - _suggestionsAreaRect.height + TopSpacing, _suggestionsAreaRect.width, _suggestionsAreaRect.height - TopSpacing - TextInputFieldHeight - TextInputFieldSpacing);
            _suggestionsContentRect = new Rect(0, 0, _suggestionsRect.width, _possibleCommands.Length * SuggestionHeight);
        }

        private int GetConsoleRectHeight() => Mathf.Min(TextInputFieldSpacing + TextInputFieldHeight + TopSpacing + (_showLog ? (_logs.Count * LogHeight) : 0), Screen.height / 2);

        private int GetSuggestionsHeight() => Mathf.Min(TextInputFieldSpacing + TextInputFieldHeight + TopSpacing + (_showSuggestions ? _possibleCommands.Length * SuggestionHeight : 0), Screen.height / 2);

        private void HandleLog(string condition, string stacktrace, LogType type)
        {
            if(!catchConsoleLogs)
                return;
            
            AddLog(new LogMessage
            {
                Message = $"[{DateTime.Now:HH:mm:ss}] {type} \n{condition}",
                Color = type switch
                {
                    LogType.Error => Color.red,
                    LogType.Warning => Color.yellow,
                    _ => Color.white
                }
            });
        }

        public void AddLog(LogMessage message)
        {
            _logs.Add(message);
            RecalculateRects();
            _scrollPositionLogs.y = _logsContentRect.height - _logsRect.height;
        }

        public void AddLog(string message, Color color)
        {
            AddLog(new LogMessage() {
                Message = message,
                Color = color
            });
        }

        private void ToggleConsole() => _isConsoleShown = !_isConsoleShown;

        private void OnGUI()
        {
            if (!_isConsoleShown)
                return;

            _possibleCommands = CheatCommandExecutor.GetPossibleCommands(_userInput);

            RecalculateRects();
            
            DrawConsoleLogArea();
            DrawSuggestionsArea();
            DrawConsoleInput();
            if (_showLog)
                DrawConsoleLogLogs();
            if(_showSuggestions)
                DrawSuggestions();
        }

        private void DrawConsoleInput()
        {
            bool validCommand = CheatCommandExecutor.IsValidCommand(_userInput);
            using (new ChangeGUIColor(validCommand ? Color.white : Color.red))
            {
                _userInput = GUI.TextField(_inputRect, _userInput);
            }
        }

        private void DrawConsoleLogArea()
        {
            GUI.Box(_consoleRect, "Console Log");
            DrawConsoleLogButtons();
        }

        private void DrawConsoleLogLogs()
        {
            _scrollPositionLogs = GUI.BeginScrollView(_logsRect, _scrollPositionLogs, _logsContentRect, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);

            for (int i = 0; i < _logs.Count; i++)
            {
                LogMessage log = _logs.Get(_logs.Count - 1 - i);
                using (new ChangeGUIColor(log.Color))
                {
                    GUI.TextArea(new Rect(0, _logsContentRect.height - LogHeight * (i + 1), _consoleRect.width, LogHeight),
                        log.Message);
                }
            }

            GUI.EndScrollView();
        }

        private void DrawConsoleLogButtons()
        {
            if (GUI.Button(_toggleLogButtonRect, "Toggle Log"))
                _showLog = !_showLog;

            if (GUI.Button(_clearLogButtonRect, "Clear Log"))
                _logs.Clear();
        }

        private void HandleInput() => CheatCommandExecutor.Execute(_userInput);

        private void DrawSuggestionsArea()
        {
            GUI.Box(_suggestionsAreaRect, "Suggestions");
            DrawSuggestionsButton();
        }

        private void DrawSuggestionsButton()
        {
            if(GUI.Button(_toggleSuggestionsButtonRect, "Toggle Suggestions"))
                _showSuggestions = !_showSuggestions;
        }

        private void DrawSuggestions()
        {
            _scrollPositionSuggestions = GUI.BeginScrollView(_suggestionsRect, _scrollPositionSuggestions, _suggestionsContentRect, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);

            for (int i = 0; i < _possibleCommands.Length; i++)
            {
                BaseCheatCommand baseCheatCommand = _possibleCommands[i];
                StringBuilder stringBuilder = new();
                stringBuilder.Append(baseCheatCommand.CommandName + " ");

                if (baseCheatCommand.ParameterTypes != null && baseCheatCommand.ParameterTypes.Length > 0)
                    for (int index = 0; index < baseCheatCommand.ParameterTypes.Length; index++)
                    {
                        stringBuilder.Append(
                            $"<{(typeof(float) == baseCheatCommand.ParameterTypes[index] ? "float" : baseCheatCommand.ParameterTypes[index].Name)}>{baseCheatCommand.ParameterNames[index]}");
                        if (index != baseCheatCommand.ParameterTypes.Length - 1)
                            stringBuilder.Append(", ");
                    }

                GUI.TextArea(new Rect(0, _suggestionsContentRect.height - SuggestionHeight * (i + 1), _suggestionsContentRect.width, SuggestionHeight), stringBuilder.ToString());
            }

            GUI.EndScrollView();
        }
    }

}