using System.ComponentModel;
using System.Runtime.CompilerServices;
using JsonToCsharpPoco.Models.Enums;

public class ConversionSettings : INotifyPropertyChanged
{
  private bool _useRecords;
  private bool _usePrimaryConstructor;
  private PropertyAccess _propertyAccess;
  private ArrayType _arrayType;
  private bool _addAttribute = true;
  private bool _isNullable;
  private bool _isRequired;
  private bool _isDefaultInitialized;
  private string _namespace = "JsonToCsharp";
  private string _rootTypeName = "Root";

  public event PropertyChangedEventHandler? PropertyChanged;

  public bool UseRecords
  {
    get => _useRecords;
    set => SetProperty(ref _useRecords, value);
  }

  public bool UsePrimaryConstructor
  {
    get => _usePrimaryConstructor;
    set => SetProperty(ref _usePrimaryConstructor, value);
  }

  public PropertyAccess PropertyAccess
  {
    get => _propertyAccess;
    set => SetProperty(ref _propertyAccess, value);
  }

  public ArrayType ArrayType
  {
    get => _arrayType;
    set => SetProperty(ref _arrayType, value);
  }

  public bool AddAttribute
  {
    get => _addAttribute;
    set => SetProperty(ref _addAttribute, value);
  }

  public bool IsNullable
  {
    get => _isNullable;
    set => SetProperty(ref _isNullable, value);
  }

  public bool IsRequired
  {
    get => _isRequired;
    set => SetProperty(ref _isRequired, value);
  }

  public bool IsDefaultInitialized
  {
    get => _isDefaultInitialized;
    set => SetProperty(ref _isDefaultInitialized, value);
  }

  public string Namespace
  {
    get => _namespace;
    set => SetProperty(ref _namespace, value);
  }

  public string RootTypeName
  {
    get => _rootTypeName;
    set => SetProperty(ref _rootTypeName, value);
  }


  protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
  {
    if (EqualityComparer<T>.Default.Equals(field, value))
      return false;

    field = value;
    OnPropertyChanged(propertyName);
    return true;
  }

  // Will use this event to trigger saving the values in local storage
  protected virtual void OnPropertyChanged(string propertyName)
  {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }
}
