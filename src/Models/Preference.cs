namespace JsonToCsharpPoco.Models;
public class Preferences
{
    public bool IsSettingsSaved { get; set; }
    public bool IsEditorContentSaved { get; set; }
    public string CurrentTheme { get; set; } = "light";
}