namespace MauiLesson_8_SurvivalGame;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    private void OnVolumeChanged(object sender, ValueChangedEventArgs e)
    {
        if (VolumeValueLabel != null)
        {
            VolumeValueLabel.Text = $"{(int)e.NewValue}%";
        }
    }

    private void OnSaveSettingsClicked(object sender, EventArgs e)
    {
        string graphics = GraphicsPicker.SelectedItem?.ToString() ?? "Середня (Збалансовано)";
        int volume = (int)VolumeSlider.Value;
        SettingsStatusLabel.Text = $"[{DateTime.Now:HH:mm:ss}] Налаштування збережено (Гучність: {volume}%, Графіка: {graphics}).";
    }

    private async void OnResetProgressClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Підтвердження", "Ви впевнені, що хочете видалити збереження та почати нову гру?", "Так, скинути", "Скасувати");
        if (confirm)
        {
            GameState.Instance.ResetToDefaults();
            SettingsStatusLabel.Text = "🔄 Прогрес повністю скинуто до початкового стану!";
            await DisplayAlert("Готово", "Гру скинуто до початкових параметрів.", "OK");
        }
    }
}