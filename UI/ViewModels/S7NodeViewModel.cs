using CommunityToolkit.Mvvm.ComponentModel;

namespace UI.ViewModels;

public partial class S7NodeViewModel : ObservableObject
{
    [ObservableProperty] private int bitLength;

    [ObservableProperty] private int db;

    [ObservableProperty] private string description;

    [ObservableProperty] private bool enabled;

    [ObservableProperty] private string feedbackNodeKey;

    [ObservableProperty] private bool needFeedback;

    [ObservableProperty] private string offset;

    [ObservableProperty] private string title;

    [ObservableProperty] private string type;
}