using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;
using XamEffects;

namespace XamExample {
    public partial class MyPage : ContentPage {
        public MyPage() {
            InitializeComponent();

            var list = new List<Item>();

            for (int i = 0; i < 50; i++)
            {
                list.Add(new Item(i));
            }
            listView.ItemsSource = list;
        }
    }
    class Item: INotifyPropertyChanged
    {
        int taps = 0;
        int longPresses = 0;
        int longTaps = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Number { get; set; }
        public string Text { get; set; } = "0";
        public ICommand LongTapCommand { get; set; }
        public ICommand LongPressCommand { get; set; }
        public ICommand TapCommand { get; set; }

        public ICommand CancelTapCommand { get; set; }

        public Item(int number)
        {
            Number = number.ToString();

            TapCommand = new Command(() =>
            {
                taps++;
                Text = $"{taps} Tap";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            });

            LongTapCommand = new Command(() =>
            {
                longTaps++;
                Text = $"{longTaps} Long Tap";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            });

            LongPressCommand = new Command(() =>
            {
                longPresses++;
                Text = $"{longPresses} Long Press";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            });

            CancelTapCommand = new Command(() =>
            {
                longPresses++;
                Text = $"{longPresses} Cancel";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            });
        }
    }
}
