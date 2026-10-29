namespace MauiLesson_8_SurvivalGame;

public partial class InventoryPage : ContentPage
{
    public InventoryPage()
    {
        InitializeComponent();
        ItemsCollectionView.ItemsSource = GameState.Instance.Items;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        HudView.RefreshHud();
        UpdateSlotsUI();
    }

    private void UpdateSlotsUI()
    {
        var state = GameState.Instance;
        SlotsCountLabel.Text = $"Зайнято: {state.TotalUniqueStacks} / {state.MaxSlots} слотів";
    }

    private void OnUseItemClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is string itemId)
        {
            var state = GameState.Instance;
            var item = state.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return;

            switch (item.Type)
            {
                case ItemType.Weapon:
                    state.EquippedWeaponName = item.RawName;
                    state.EquippedWeaponDamage = item.RestoreValue;
                    state.NotifyStateChanged();
                    InventoryStatusLabel.Text = $"⚔️ Ви екіпірували «{item.RawName}»! Загальна сила атаки: {state.TotalAttackDamage}.";
                    break;

                case ItemType.Food:
                    state.ChangeStats(hungerDelta: item.RestoreValue, thirstDelta: 5);
                    state.RemoveItem(item.Id, 1);
                    InventoryStatusLabel.Text = $"🍽️ Ви з'їли «{item.RawName}» (+{item.RestoreValue} ситості).";
                    break;

                case ItemType.Drink:
                    state.ChangeStats(thirstDelta: item.RestoreValue, energyDelta: 15);
                    state.RemoveItem(item.Id, 1);
                    InventoryStatusLabel.Text = $"💧 Ви випили «{item.RawName}» (+{item.RestoreValue} спраги, +15 енергії).";
                    break;

                case ItemType.Medical:
                    state.ChangeStats(hpDelta: item.RestoreValue);
                    state.RemoveItem(item.Id, 1);
                    InventoryStatusLabel.Text = $"🩹 Використано «{item.RawName}» (+{item.RestoreValue} здоров'я).";
                    break;

                case ItemType.Resource:
                    InventoryStatusLabel.Text = $"🛠️ «{item.RawName}» — це матеріал. Використайте його у вкладці «Покращення»!";
                    break;
            }

            UpdateSlotsUI();
        }
    }

    private void OnDropItemClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is string itemId)
        {
            var state = GameState.Instance;
            var item = state.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return;

            string rawName = item.RawName;
            state.RemoveItem(itemId, 1);
            InventoryStatusLabel.Text = $"🗑️ Викинуто 1 од. предмета «{rawName}».";
            UpdateSlotsUI();
        }
    }
}