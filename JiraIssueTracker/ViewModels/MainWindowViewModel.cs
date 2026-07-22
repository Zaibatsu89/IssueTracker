using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace JiraIssueTracker.ViewModels;

public sealed class MainWindowViewModel : BindableBase
{
    private readonly Models.Issue issue;
    private readonly RelayCommand nextCheckCommand;
    private readonly RelayCommand saveResultCommand;
    private readonly RelayCommand backToQueueCommand;
    private string activityMessage = string.Empty;
    private CheckItemViewModel? selectedCheck;

    public MainWindowViewModel()
    {
        issue = Models.IssueFactory.CreateSample();

        Checks = new ObservableCollection<CheckItemViewModel>(
            issue.Step.Parts.SelectMany(part =>
                part.Actions.SelectMany(action =>
                    action.Checks.Select(check => new CheckItemViewModel(issue.Step, part, action, check)))));

        foreach (var check in Checks)
        {
            check.PropertyChanged += OnCheckPropertyChanged;
        }

        nextCheckCommand = new RelayCommand(SelectNextCheck, CanSelectNextCheck);
        saveResultCommand = new RelayCommand(SaveResult, CanSaveResult);
        backToQueueCommand = new RelayCommand(BackToQueue);

        SelectedCheck = Checks.FirstOrDefault();
        UpdateDerivedState();
    }

    public string SystemStatus => "System Online";

    public string IssueHeading => $"{issue.Project}-{issue.Code}:\n{issue.Title}";

    public string AreaTag => issue.AreaTag;

    public string PriorityTag => issue.PriorityTag;

    public string EstimatedEffort => issue.Step.Effort;

    public string Elapsed => issue.Step.Elapsed;

    public string AssignedTo => issue.AssignedTo;

    public string EnvironmentName => issue.EnvironmentName;

    public ObservableCollection<CheckItemViewModel> Checks { get; }

    public CheckItemViewModel? SelectedCheck
    {
        get => selectedCheck;
        set
        {
            if (!SetProperty(ref selectedCheck, value))
            {
                return;
            }

            UpdateDerivedState();
        }
    }

    public string ProgressText
    {
        get
        {
            var completed = Checks.Count(check => check.Result != Models.Results.Unknown);
            return $"{completed} / {Checks.Count} checked";
        }
    }

    public string ActivityMessage
    {
        get => activityMessage;
        private set => SetProperty(ref activityMessage, value);
    }

    public ICommand NextCheckCommand => nextCheckCommand;

    public ICommand SaveResultCommand => saveResultCommand;
    public ICommand BackToQueueCommand => backToQueueCommand;

    private void OnCheckPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not (nameof(CheckItemViewModel.Result) or nameof(CheckItemViewModel.Remarks)))
        {
            return;
        }

        OnPropertyChanged(nameof(ProgressText));

        if (ReferenceEquals(sender, SelectedCheck))
        {
            UpdateDerivedState();
            return;
        }

        nextCheckCommand.RaiseCanExecuteChanged();
        saveResultCommand.RaiseCanExecuteChanged();
    }

    private void UpdateDerivedState()
    {
        ActivityMessage = SelectedCheck is null
            ? "No check selected."
            : SelectedCheck.SaveHint;

        nextCheckCommand.RaiseCanExecuteChanged();
        saveResultCommand.RaiseCanExecuteChanged();
    }

    private bool CanSelectNextCheck()
    {
        return SelectedCheck is not null &&
               Checks.IndexOf(SelectedCheck) < Checks.Count - 1;
    }

    private void SelectNextCheck()
    {
        if (SelectedCheck is null)
        {
            return;
        }

        var currentIndex = Checks.IndexOf(SelectedCheck);
        if (currentIndex < 0 || currentIndex >= Checks.Count - 1)
        {
            return;
        }

        SelectedCheck = Checks[currentIndex + 1];
    }

    private bool CanSaveResult()
    {
        return SelectedCheck?.IsReadyToSave == true;
    }

    private void SaveResult()
    {
        if (SelectedCheck is null)
        {
            return;
        }

        SelectedCheck.LastSavedAt = DateTime.Now;
        ActivityMessage =
            $"{SelectedCheck.ResultLabel} saved for '{SelectedCheck.Name}' in {SelectedCheck.ActionName} at {SelectedCheck.LastSavedAt:HH:mm}.";
        saveResultCommand.RaiseCanExecuteChanged();
    }

    private void BackToQueue()
    {
        ActivityMessage = "Navigating to issue queue.";
    }
}

public sealed class CheckItemViewModel : BindableBase
{
    private static readonly SolidColorBrush UnknownBrush = new(ColorHelper.FromArgb(255, 158, 158, 158));
    private static readonly SolidColorBrush UnknownBackgroundBrush = new(ColorHelper.FromArgb(255, 32, 32, 32));
    private static readonly SolidColorBrush OkBrush = new(ColorHelper.FromArgb(255, 0, 230, 118));
    private static readonly SolidColorBrush OkBackgroundBrush = new(ColorHelper.FromArgb(255, 17, 48, 32));
    private static readonly SolidColorBrush NokBrush = new(ColorHelper.FromArgb(255, 255, 82, 82));
    private static readonly SolidColorBrush NokBackgroundBrush = new(ColorHelper.FromArgb(255, 42, 21, 21));

    private readonly Models.TestAction action;
    private readonly Models.Check check;
    private readonly Models.Part part;
    private readonly Models.Step step;
    private DateTime? lastSavedAt;

    public CheckItemViewModel(Models.Step step, Models.Part part, Models.TestAction action, Models.Check check)
    {
        this.step = step;
        this.part = part;
        this.action = action;
        this.check = check;
    }

    public string StepLabel => $"{step.Type.ToString().ToUpperInvariant()} / {part.Name.ToUpperInvariant()}";

    public string Name => check.Name;

    public string ContextLine => $"Part: {part.Name} / Action: {action.Name}";

    public string PartName => part.Name;

    public string ActionName => action.Name;

    public string StepTypeName => step.Type.ToString();

    public Models.Results Result
    {
        get => check.Result;
        set
        {
            if (check.Result == value)
            {
                return;
            }

            check.Result = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedResultIndex));
            OnPropertyChanged(nameof(ResultLabel));
            OnPropertyChanged(nameof(StatusBrush));
            OnPropertyChanged(nameof(StateBackgroundBrush));
            OnPropertyChanged(nameof(StateText));
            OnPropertyChanged(nameof(RemarkVisibility));
            OnPropertyChanged(nameof(RemarkValidationVisibility));
            OnPropertyChanged(nameof(RemarkValidationMessage));
            OnPropertyChanged(nameof(IsReadyToSave));
            OnPropertyChanged(nameof(SaveHint));
        }
    }

    public int SelectedResultIndex
    {
        get => Result switch
        {
            Models.Results.Ok => 0,
            Models.Results.Nok => 1,
            _ => 2
        };
        set => Result = value switch
        {
            0 => Models.Results.Ok,
            1 => Models.Results.Nok,
            _ => Models.Results.Unknown
        };
    }

    public string Remarks
    {
        get => check.Remarks;
        set
        {
            if (check.Remarks == value)
            {
                return;
            }

            check.Remarks = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(RemarkValidationVisibility));
            OnPropertyChanged(nameof(IsReadyToSave));
            OnPropertyChanged(nameof(SaveHint));
            OnPropertyChanged(nameof(StateText));
        }
    }

    public DateTime? LastSavedAt
    {
        get => lastSavedAt;
        set => SetProperty(ref lastSavedAt, value);
    }

    public string ResultLabel => Result switch
    {
        Models.Results.Ok => "OK",
        Models.Results.Nok => "NOK",
        _ => "OPEN"
    };

    public SolidColorBrush StatusBrush => Result switch
    {
        Models.Results.Ok => OkBrush,
        Models.Results.Nok => NokBrush,
        _ => UnknownBrush
    };

    public SolidColorBrush StateBackgroundBrush => Result switch
    {
        Models.Results.Ok => OkBackgroundBrush,
        Models.Results.Nok => NokBackgroundBrush,
        _ => UnknownBackgroundBrush
    };

    public string StateText => Result switch
    {
        Models.Results.Ok => "OK / READY TO SAVE",
        Models.Results.Nok when string.IsNullOrWhiteSpace(Remarks) => "NOK / REMARK REQUIRED",
        Models.Results.Nok => "NOK / REMARK PRESENT",
        _ => "OPEN / WAITING FOR RESULT"
    };

    public Visibility RemarkVisibility => Result == Models.Results.Nok
        ? Visibility.Visible
        : Visibility.Collapsed;

    public Visibility RemarkValidationVisibility => Result == Models.Results.Nok &&
                                                    string.IsNullOrWhiteSpace(Remarks)
        ? Visibility.Visible
        : Visibility.Collapsed;

    public string RemarkValidationMessage => "REMARK IS REQUIRED WHEN RESULT = NOK";

    public bool IsReadyToSave => Result != Models.Results.Unknown &&
                                 (Result != Models.Results.Nok || !string.IsNullOrWhiteSpace(Remarks));

    public string SaveHint => Result switch
    {
        Models.Results.Unknown => "Select a result before saving.",
        Models.Results.Nok when string.IsNullOrWhiteSpace(Remarks) => "Add a remark before saving NOK.",
        _ => $"Current check in {ActionName} is ready to save."
    };
}

public abstract class BindableBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
        {
            return false;
        }

        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public sealed class RelayCommand : ICommand
{
    private readonly Func<bool>? canExecute;
    private readonly Action execute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        this.execute = execute;
        this.canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        return canExecute?.Invoke() ?? true;
    }

    public void Execute(object? parameter)
    {
        execute();
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
