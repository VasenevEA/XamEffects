using System;
using System.Collections.Generic;

using Xamarin.Forms;
using XamEffects;

namespace XamExample {
    public partial class MyPage : ContentPage {
        public MyPage() {
            InitializeComponent();

            var taps = 0;
            var longPresses = 0;
            var longTaps = 0;


            Commands.SetLongPressDelay(touch, 500);
            Commands.SetLongTapDelay(touch, 2000);

            Commands.SetTap(touch, new Command(() => {
                taps++;
                text.Text = $"{taps} Tap";
            }));
            Commands.SetLongPress(touch, new Command(() =>
            {
                longPresses++;
                text.Text = $"{longPresses} Long Press";
            }));

            Commands.SetLongTap(touch, new Command(() =>
            {
                longTaps++;
                text.Text = $"{longTaps} Long Tap";
            }));
        }
    }
}
