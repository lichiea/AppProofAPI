using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace AppProofAPI.ViewModels;

public partial class KeyValueItem : ObservableObject
{
    [ObservableProperty]
    private string? _key;

    [ObservableProperty]
    private string? _value;

    public IRelayCommand RemoveCommand { get; }

    public KeyValueItem(Action<KeyValueItem> onRemove)
    {
        RemoveCommand = new RelayCommand(() => onRemove(this));
    }
}