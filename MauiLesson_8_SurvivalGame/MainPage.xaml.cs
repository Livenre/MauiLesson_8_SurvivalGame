namespace MauiLesson_8_SurvivalGame;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        GameState.Instance.StateChanged += RefreshUI;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefreshUI();
    }

    private void RefreshUI()
    {
        var state = GameState.Instance;

        DayLabel.Text = $"День виживання: {state.Day}";
        WeaponStatusLabel.Text = $"Зброя: {state.EquippedWeaponName} (Шкода: {state.TotalAttackDamage})";

        HpLabel.Text = $"{state.Hp} / {state.MaxHp}";
        HpBar.Progress = (double)state.Hp / state.MaxHp;

        HungerLabel.Text = $"{state.Hunger} / {state.MaxHunger}";
        HungerBar.Progress = (double)state.Hunger / state.MaxHunger;

        ThirstLabel.Text = $"{state.Thirst} / {state.MaxThirst}";
        ThirstBar.Progress = (double)state.Thirst / state.MaxThirst;

        EnergyLabel.Text = $"{state.Energy} / {state.MaxEnergy}";
        EnergyBar.Progress = (double)state.Energy / state.MaxEnergy;
    }

    private void OnEatClicked(object sender, EventArgs e)
    {
        var state = GameState.Instance;
        var food = state.Items.FirstOrDefault(i => i.Type == ItemType.Food);
        if (food == null)
        {
            ActionLogLabel.Text = "⚠️ У рюкзаку немає їжі! Нафарміть ягоди або приготуйте м'ясо.";
            return;
        }

        state.ChangeStats(hungerDelta: food.RestoreValue, thirstDelta: 5);
        state.RemoveItem(food.Id, 1);
        ActionLogLabel.Text = $"🍖 Ви з'їли «{food.RawName}» (+{food.RestoreValue} ситості).";
    }

    private void OnDrinkClicked(object sender, EventArgs e)
    {
        var state = GameState.Instance;
        var drink = state.Items.FirstOrDefault(i => i.Type == ItemType.Drink);
        if (drink == null)
        {
            ActionLogLabel.Text = "⚠️ У рюкзаку немає води! Пошукайте фляги під час розвідки.";
            return;
        }

        state.ChangeStats(thirstDelta: drink.RestoreValue, energyDelta: 15);
        state.RemoveItem(drink.Id, 1);
        ActionLogLabel.Text = $"💧 Ви випили «{drink.RawName}» (+{drink.RestoreValue} спраги, +15 енергії).";
    }

    private void OnRestClicked(object sender, EventArgs e)
    {
        var state = GameState.Instance;
        if (state.Hunger < 15 || state.Thirst < 15)
        {
            ActionLogLabel.Text = "⚠️ Занадто сильний голод або спрага, щоб заснути! Поїжте та попийте води.";
            return;
        }

        state.Day++;
        state.ChangeStats(hpDelta: 15, hungerDelta: -18, thirstDelta: -22, energyDelta: 60);
        ActionLogLabel.Text = $"💤 Ви поспали до ранку (День {state.Day}): +60⚡, +15❤️, -18🍖, -22💧.";
    }

    private void OnCampfireClicked(object sender, EventArgs e)
    {
        var state = GameState.Instance;
        if (!state.RemoveItem("wood", 2))
        {
            ActionLogLabel.Text = "⚠️ Потрібно 2 од. Деревини, щоб розпалити багаття!";
            return;
        }

        state.ChangeStats(hpDelta: 15, energyDelta: 25);
        ActionLogLabel.Text = "🔥 Ви розпалили вогнище: +15 здоров'я, +25 енергії (-2 Деревини).";
    }
}