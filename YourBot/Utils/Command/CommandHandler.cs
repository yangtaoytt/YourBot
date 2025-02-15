namespace YourBot.Utils.Command;

public class CommandHandler {
    private readonly List<string> _commandList;
    private int _commandIndex;
    public CommandHandler(List<string> commandList) {
        if (commandList.Count == 0) {
            throw new InvalidCommandException();
        }
        _commandList = commandList;
        _commandIndex = 0;
    }
    public string Command => _commandList[_commandIndex];
    public CommandHandler? TryNext() {
        return _commandIndex + 1 < _commandList.Count ? Next() : null;
    }
    public CommandHandler? TryBack() {
        return _commandIndex > 0 ? Back() : null;
    }
    public CommandHandler Next() {
        if (_commandIndex  + 1>= _commandList.Count) {
            throw new InvalidCommandException();
        }

        ++_commandIndex;
        return this;
    }
    public CommandHandler Back() {
        if (_commandIndex <= 0) {
            throw new InvalidCommandException();
        }

        --_commandIndex;
        return this;
    }
    
    
    
    
    
    public void ResetCommandIndex() {
        _commandIndex = 0;
    }
    public void ResetCommandIndex(int index) {
        if (index < 0 || index >= _commandList.Count) {
            throw new Exception("Invalid index");
        }
        _commandIndex = index;
    }
    public void ResetCommandIndex(string command) {
        var index = _commandList.IndexOf(command);
        if (index == -1) {
            throw new Exception("Command not found");
        }
        _commandIndex = index;
    }
    
}