using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using StarTrekOnline_ServerStatus.Utils.API;

namespace StarTrekOnline_ServerStatus;

public partial class SetWindow : Window
{
    public bool isInitialized { get; set; } = false;
    public static string Language { get; private set; } = "en-US";
    
    private static readonly Lazy<SetWindow> lazyInstance = new Lazy<SetWindow>(() => new SetWindow());
    
    public string selectedFileName { get; set; }
    
    public bool Debug_Mode { get; set; }

    public bool PlayAudio = false;
    
    public static SetWindow Instance => lazyInstance.Value;
    private SetWindow()
    {
        InitializeComponent();

        CheckBox_Audio.Checked += CheckBox_Audio_Checked;
        CheckBox_Audio.Unchecked += CheckBox_Audio_Unchecked;
        Debug.Checked += DebugChecked;
        Debug.Unchecked += DebugUnchecked;
    }
    
    private void OnMusicClick(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Audio Files|*.mp3;*.wav;*.wma|All Files|*.*";
        if (openFileDialog.ShowDialog() == true)
        {
            selectedFileName = openFileDialog.FileName;
            
            Logger.Log($"Selected music file: {selectedFileName}");
        }
    }

    private void DebugChecked(object sender, RoutedEventArgs ev)
    {
        Debug_Mode = true;
    }
    
    private void DebugUnchecked(object sender, RoutedEventArgs ev)
    {
        Debug_Mode = false;
    }
    
    private void LanguageChanged(object sender, SelectionChangedEventArgs e)
    {
        ComboBoxItem selectedItem = (ComboBoxItem)LanguageComboBox.SelectedItem;
        string selectedLanguage = selectedItem.Content.ToString();

        try
        {
            if (isInitialized)
            {
                switch (selectedLanguage)
                {
                    case "English":
                        Language = "en-US";
                        LanguageManager.SetLanguage(Language);
                        Utils.API.Language.SwitchLanguage(Language);
                        break;
                    case "Chinese":
                        Language = "zh-CN";
                        LanguageManager.SetLanguage(Language);
                        Utils.API.Language.SwitchLanguage(Language);
                        break;
                    default:
                        break;
                }
            }
        }
        catch (Exception exception)
        {
            var error = $"Error while changing language. {exception}";
            Logger.Error(error);
            throw;
        }
        finally
        {
            Logger.Log($"Language has been changed to {selectedLanguage}.");
        }
    }

    protected override void OnClosing(CancelEventArgs ev)
    {
        ev.Cancel = true;
        this.Hide();
    }
    
    private void CheckBox_Audio_Checked(object sender, RoutedEventArgs e)
    {
        PlayAudio = true;
        Logger.Log("Checkbox is checked!");
    }
    
    private void CheckBox_Audio_Unchecked(object sender, RoutedEventArgs e)
    {
        PlayAudio = false;
        Logger.Log("Checkbox is unchecked!");
    }
}