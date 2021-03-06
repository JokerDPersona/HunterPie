﻿using System;
using System.Windows;
using System.Windows.Media;
using HunterPie.Core;
using HunterPie.Core.Events;
using FertilizerControl = HunterPie.GUI.Widgets.Harvest_Box.Parts.FertilizerControl;

namespace HunterPie.GUI.Widgets
{

    public partial class HarvestBox : Widget
    {

        public new WidgetType Type => WidgetType.HarvestWidget;

        Player PlayerContext;
        Core.HarvestBox Context => PlayerContext?.Harvest;

        public HarvestBox(Player Context)
        {
            InitializeComponent();
            BaseWidth = Width;
            BaseHeight = Height;
            SetWindowFlags();
            SetContext(Context);
            CreateFertilizerControls();
            ApplySettings();
        }

        public override void EnterWidgetDesignMode()
        {
            base.EnterWidgetDesignMode();
            RemoveWindowTransparencyFlag();
        }

        public override void LeaveWidgetDesignMode()
        {
            base.LeaveWidgetDesignMode();
            ApplyWindowTransparencyFlag();
            SaveSettings();
        }

        private void SaveSettings()
        {
            UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Position[0] = (int)Left - UserSettings.PlayerConfig.Overlay.Position[0];
            UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Position[1] = (int)Top - UserSettings.PlayerConfig.Overlay.Position[1];
            UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Scale = DefaultScaleX;
        }

        public override void ApplySettings(bool FocusTrigger = false)
        {
            bool alwaysShow = UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.AlwaysShow;
            bool inHarvest = PlayerContext?.InHarvestZone ?? false;
            bool shouldShow = alwaysShow || inHarvest;

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                if (!FocusTrigger)
                {
                    Top = UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Position[1] + UserSettings.PlayerConfig.Overlay.Position[1];
                    Left = UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Position[0] + UserSettings.PlayerConfig.Overlay.Position[0];
                    WidgetActive = UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Enabled;
                    WidgetHasContent = shouldShow;
                    ScaleWidget(UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Scale, UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Scale);
                    SteamTracker.Visibility = UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.ShowSteamTracker ? Visibility.Visible : Visibility.Collapsed;
                    ArgosyTracker.Visibility = UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.ShowArgosyTracker ? Visibility.Visible : Visibility.Collapsed;
                    TailraidersTracker.Visibility = UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.ShowTailraidersTracker ? Visibility.Visible : Visibility.Collapsed;
                    Opacity = UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Opacity;
                    HarvestBoxContainer.Opacity = UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.BackgroundOpacity;
                    SetMode(UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.CompactMode);
                }
                base.ApplySettings();
            }));
        }

        private void SetMode(bool IsCompact)
        {
            int h = IsCompact ? 130 : 260;
            BaseHeight = Height = h;
            foreach (FertilizerControl fC in HarvestBoxFertilizerHolder.Children)
            {
                fC.SetMode(IsCompact);
            }
        }
        
        public override void ScaleWidget(double NewScaleX, double NewScaleY)
        {
            if (NewScaleX <= 0.2) return;
            Width = BaseWidth * NewScaleX;
            //Height = BaseHeight * NewScaleY;
            HarvestBoxComponent.LayoutTransform = new ScaleTransform(NewScaleX, NewScaleY);
            DefaultScaleX = NewScaleX;
            DefaultScaleY = NewScaleY;
        }

        public void SetContext(Player ctx)
        {
            PlayerContext = ctx;
            HookEvents();
        }

        private void Dispatch(Action function) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, function);

        private void HookEvents()
        {
            PlayerContext.OnZoneChange += ChangeHarvestBoxState;
            Context.OnCounterChange += OnCounterChange;

            PlayerContext.Activity.OnNaturalSteamChange += OnNaturalSteamFuelChange;
            PlayerContext.Activity.OnStoredSteamChange += OnStoredSteamFuelChange;
            PlayerContext.Activity.OnTailraidersDaysChange += OnTailraidersQuestChange;
            PlayerContext.Activity.OnArgosyDaysChange += OnArgosyDaysChange;
        }

        public void UnhookEvents()
        {
            DestroyFertilizerControls();
            PlayerContext.OnZoneChange -= ChangeHarvestBoxState;
            Context.OnCounterChange -= OnCounterChange;

            PlayerContext.Activity.OnNaturalSteamChange -= OnNaturalSteamFuelChange;
            PlayerContext.Activity.OnStoredSteamChange -= OnStoredSteamFuelChange;
            PlayerContext.Activity.OnArgosyDaysChange -= OnArgosyDaysChange;
            PlayerContext.Activity.OnTailraidersDaysChange -= OnTailraidersQuestChange;
            PlayerContext = null;
        }

        private void CreateFertilizerControls()
        {
            for (int i = 0; i < 4; i++)
            {
                FertilizerControl fC = new FertilizerControl();
                fC.SetContext(Context.Box[i]);
                HarvestBoxFertilizerHolder.Children.Add(fC);
            }
        }

        private void DestroyFertilizerControls()
        {
            foreach (FertilizerControl control in HarvestBoxFertilizerHolder.Children)
            {
                control.UnhookEvents();
            }
            HarvestBoxFertilizerHolder.Children.Clear();
        }

        private void OnTailraidersQuestChange(object source, DaysLeftEventArgs args) => Dispatch(() =>
        {
            TailraidersWarnIcon.Visibility = (args.Modifier && args.Days == 0) ? Visibility.Visible : Visibility.Hidden;
            TailraidersDaysText.Text = args.Days.ToString();
        });

        private void OnArgosyDaysChange(object source, DaysLeftEventArgs args) => Dispatch(() =>
        {
            ArgosyWarnIcon.Visibility = args.Modifier ? Visibility.Visible : Visibility.Hidden;
            ArgosyDaysText.Text = args.Days.ToString();
        });

        private void OnStoredSteamFuelChange(object source, SteamFuelEventArgs args) => Dispatch(() =>
        {
            StoredFuelText.Text = FormatToK(args.Available);
        });

        private void OnNaturalSteamFuelChange(object source, SteamFuelEventArgs args) => Dispatch(() =>
        {
            SteamFuelWarnIcon.Visibility = args.Available >= args.Max ? Visibility.Visible : Visibility.Hidden;
            NaturalFuelText.Text = FormatToK(args.Available);
        });

        private void ChangeHarvestBoxState(object source, EventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
        {
            bool alwaysShow = UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.AlwaysShow;
            bool inHarverst = PlayerContext?.InHarvestZone ?? false;
            WidgetHasContent = alwaysShow || inHarverst;
            ChangeVisibility();
        }));

        private void OnCounterChange(object source, HarvestBoxEventArgs args) => Dispatch(() =>
        {
            HarvestBoxItemsCounter.Content = $"{args.Counter}/{args.Max}";
        });

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UnhookEvents();
            PlayerContext = null;
            IsClosed = true;
        }

        private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                MoveWidget();
                SaveSettings();
            }
        }

        private void OnMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            double delta = e.Delta > 0 ? 0.05 : -0.05;
            ScaleWidget(DefaultScaleX + delta, DefaultScaleY + delta);
        }

        // Helper
        private string FormatToK(int value)
        {
            if (value >= 1000000)
                return $"{(float)value / 1000000:0.0}M";
            if (value >= 1000)
                return $"{(float)value / 1000:0.0}K";
            return value.ToString();
        }
    }
}
