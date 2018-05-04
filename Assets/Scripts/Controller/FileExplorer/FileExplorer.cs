using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System;

public class FileExplorer : Window
{
    public static FileExplorer Instance { get; private set; }
    public enum Mode
    {
        SelectFolder,
        SelectFile,
        InputFileName
    }
    private string[] _extensions;
    private DirectoryInfo _currentDirectory = null;
    private Mode _mode;
    private Callback _callback;
    [SerializeField] private InputField _directoryInputField;
    [SerializeField] private InputField _fileNameInputField;
    [SerializeField] private Text _selectedItemText;
    [SerializeField] private FileExplorerButton _buttonPrefab;
    [SerializeField] private ScrollRect _scrollView;
    [SerializeField] private Button _confirmButton;
    private string CurrentDirectory
    {
        get
        {
            string result = _currentDirectory.FullName;
            if (result.Last() != '\\') result += '\\';
            return result;
        }
    }
    private ObjectPool<FileExplorerButton> _buttonPool;
    private List<FileExplorerButton> _buttons = new List<FileExplorerButton>();
    public static string Result => Instance.NonStaticResult;
    private string NonStaticResult { get; set; }
    public new static void SetTagContent(params string[] texts)
    {
        (Instance as Window).SetTagContent(texts);
    }
    protected override void Open()
    {
        base.Open();
        tagContent.ForceUpdate();
        MoveToCenter();
    }
    public void OpenNonStatic(Mode mode, Callback callback, params string[] extensions)
    {
        Open();
        NonStaticResult = null;
        _extensions = extensions;
        _mode = mode;
        if (callback == null)
        {
            Close();
            Debug.LogError("Error: Expected callback when opening the file explorer");
        }
        _callback = callback;
        if (_currentDirectory == null) _currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
        if (_mode == Mode.InputFileName)
        {
            _fileNameInputField.gameObject.SetActive(true);
            _selectedItemText.gameObject.SetActive(false);
            _confirmButton.interactable = !string.IsNullOrEmpty(_fileNameInputField.text);
        }
        else if (_mode == Mode.SelectFile)
        {
            _fileNameInputField.gameObject.SetActive(false);
            _selectedItemText.gameObject.SetActive(true);
            _selectedItemText.text = "";
            _confirmButton.interactable = false;
        }
        else
        {
            _fileNameInputField.gameObject.SetActive(false);
            _selectedItemText.gameObject.SetActive(true);
            _selectedItemText.text = _currentDirectory.Name;
            _confirmButton.interactable = true;
        }
        Updates();
    }
    public static void Open(Mode mode, Callback callback, params string[] extensions)
    {
        Instance.OpenNonStatic(mode, callback, extensions);
    }
    public override void Close()
    {
        base.Close();
        _fileNameInputField.text = "";
    }
    public static void SetDefaultFileName(string name)
    {
        Instance._fileNameInputField.text = name;
    }
    private void Updates()
    {
        _directoryInputField.text = CurrentDirectory;
        for (int i = 0; i < _buttons.Count; i++) _buttonPool.ReturnObject(_buttons[i]);
        while (_buttons.Count > 0) _buttons.RemoveAt(0);
        if (_currentDirectory.Parent != null)
        {
            FileExplorerButton button = _buttonPool.GetObject();
            _buttons.Add(button);
            button.ButtonText = "..";
            button.callback = () => { GoToDirectory(_currentDirectory.Parent); };
        }
        DirectoryInfo[] directories = _currentDirectory.GetDirectories();
        for (int i = 0; i < directories.Length; i++)
        {
            FileExplorerButton button = _buttonPool.GetObject();
            _buttons.Add(button);
            button.ButtonText = directories[i].Name;
            int temp = i;
            button.callback = () => { GoToDirectory(directories[temp]); };
        }
        if (_mode != Mode.SelectFolder)
        {
            FileInfo[] files = _currentDirectory.GetFiles();
            for (int i = 0; i < files.Length; i++)
                if (Array.Exists(_extensions, extension => extension.ToLower() == files[i].Extension.ToLower()))
                {
                    FileExplorerButton button = _buttonPool.GetObject();
                    _buttons.Add(button);
                    button.ButtonText = files[i].Name;
                    int temp = i;
                    if (_mode == Mode.SelectFile)
                        button.callback = () =>
                        {
                            _selectedItemText.text = files[temp].Name;
                            _confirmButton.interactable = true;
                        };
                    else
                        button.callback = () =>
                        {
                            _fileNameInputField.text = files[temp].Name;
                            FileNameInputCallback();
                        };
                }
        }
        _scrollView.verticalNormalizedPosition = 1.0f;
    }
    public void DirectoryInputCallback()
    {
        DirectoryInfo directory = new DirectoryInfo(_directoryInputField.text);
        if (directory.Exists)
            GoToDirectory(directory);
        else
            Updates();
    }
    public void FileNameInputCallback()
    {
        if (string.IsNullOrWhiteSpace(_fileNameInputField.text))
        {
            _confirmButton.interactable = false;
            return;
        }
        if (!_fileNameInputField.text.EndsWith(_extensions[0]))
            _fileNameInputField.text += _extensions[0];
        string text = _fileNameInputField.text;
        bool invalid = false;
        invalid |= text.Contains('\\');
        invalid |= text.Contains('/');
        invalid |= text.Contains(':');
        invalid |= text.Contains('?');
        invalid |= text.Contains('\"');
        invalid |= text.Contains('<');
        invalid |= text.Contains('>');
        invalid |= text.Contains('|');
        invalid |= text.Contains('*');
        if (!invalid)
            _confirmButton.interactable = true;
        else
        {
            _fileNameInputField.text = "";
            _confirmButton.interactable = false;
            MessageBox.Activate(new[] { "Error", "错误" },
                new[] { "Please input a valid file name", "请输入合法的文件名" },
                new MessageBox.ButtonInfo { texts = new[] { "OK", "好的" } });
        }
    }
    private void GoToDirectory(DirectoryInfo directory)
    {
        _currentDirectory = directory;
        if (_mode == Mode.SelectFolder)
            _selectedItemText.text = directory.Name;
        else if (_mode == Mode.SelectFile)
        {
            _selectedItemText.text = "";
            _confirmButton.interactable = false;
        }
        Updates();
    }
    public void Confirm()
    {
        switch (_mode)
        {
            case Mode.SelectFolder:
                NonStaticResult = CurrentDirectory;
                Close();
                _callback();
                break;
            case Mode.SelectFile:
                NonStaticResult = CurrentDirectory + _selectedItemText.text;
                Close();
                _callback();
                break;
            case Mode.InputFileName:
                NonStaticResult = CurrentDirectory + _fileNameInputField.text;
                FileInfo file = new FileInfo(NonStaticResult);
                if (file.Exists)
                    MessageBox.Activate(new[] { "Overwriting existing file", "覆盖已存在文件" },
                        new[]
                        {
                            "A file with the file name already exists. Are you sure to overwrite it?",
                            "此文件夹下已有以该文件名为名的文件，是否确定要覆盖该文件？"
                        },
                        new MessageBox.ButtonInfo
                        {
                            callback = () => { Close(); _callback(); },
                            texts = new[] { "Yes", "是" }
                        },
                        new MessageBox.ButtonInfo
                        {
                            texts = new[] { "No", "否" }
                        });
                else
                {
                    Close();
                    _callback();
                }
                break;
        }
    }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instances of FileExplorer");
        }
        _buttonPool = new ObjectPool<FileExplorerButton>(_buttonPrefab, 10, _scrollView.content,
            item => { item.GetComponent<WindowClickResponse>().parentWindow = this; },
            item => { item.gameObject.SetActive(true); },
            item => { item.gameObject.SetActive(false); });
    }
}
