using System;
using System.ComponentModel;
using System.Windows.Input;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using XamEffects;
using XamEffects.iOS;
using XamEffects.iOS.GestureCollectors;
using XamEffects.iOS.GestureRecognizers;
using System.Threading;
using System.Threading.Tasks;

[assembly: ExportEffect(typeof(CommandsPlatform), nameof(Commands))]

namespace XamEffects.iOS {
    public class CommandsPlatform : PlatformEffect {
        public UIView View => Control ?? Container;

        DateTime _tapTime;
        ICommand _tapCommand;
        ICommand _longCommand;
        ICommand _longPressCommand;
        object _tapParameter;
        object _longParameter;
        object _longPressParameter;

        protected override void OnAttached() {
            View.UserInteractionEnabled = true;

            UpdateTap();
            UpdateTapParameter();
            UpdateLongTap();
            UpdateLongTapParameter();
            UpdateLongPress();
            UpdateLongPressParameter();

            TouchGestureCollector.Add(View, OnTouch);
        }

        protected override void OnDetached() {
            TouchGestureCollector.Delete(View, OnTouch);
        }

        CancellationTokenSource disableLongPress;
        bool isLongPressActive = false;
        void OnTouch(TouchGestureRecognizer.TouchArgs e) {
            switch (e.State) {
                case TouchGestureRecognizer.TouchState.Started:
                    _tapTime = DateTime.Now;
                    disableLongPress?.Cancel();
                    isLongPressActive = false;
                    disableLongPress = new CancellationTokenSource();
                    Task.Run(async () =>
                    {
                        try
                        {
                            await Task.Delay(Commands.GetLongPressDelay(Element), disableLongPress.Token);
                            Device.BeginInvokeOnMainThread(() => LongPressHandler());
                            isLongPressActive = true;
                        }
                        catch (Exception)
                        {
                        }
                    });
                    break;

                case TouchGestureRecognizer.TouchState.Ended:
                    disableLongPress?.Cancel();
                    if (e.Inside) {
                        var range = (DateTime.Now - _tapTime).TotalMilliseconds;
                        if (range > Commands.GetLongTapDelay(Element))
                            LongClickHandler();
                        else if(!isLongPressActive)
                            ClickHandler();
                    }
                    break;

                case TouchGestureRecognizer.TouchState.Cancelled:
                    break;
            }
        }

        void ClickHandler() {
            if (_tapCommand?.CanExecute(_tapParameter) ?? false)
                _tapCommand.Execute(_tapParameter);
        }

        void LongClickHandler() {
            if (_longCommand == null)
                ClickHandler();
            else if (_longCommand.CanExecute(_longParameter))
                _longCommand.Execute(_longParameter);
        }

        void LongPressHandler()
        {
            if (_longPressCommand == null)
                ClickHandler();
            else if (_longPressCommand.CanExecute(_longPressCommand))
                _longPressCommand.Execute(_longPressCommand);
        }

        protected override void OnElementPropertyChanged(PropertyChangedEventArgs args) {
            base.OnElementPropertyChanged(args);

            if (args.PropertyName == Commands.TapProperty.PropertyName)
                UpdateTap();
            else if (args.PropertyName == Commands.TapParameterProperty.PropertyName)
                UpdateTapParameter();
            else if (args.PropertyName == Commands.LongTapProperty.PropertyName)
                UpdateLongTap();
            else if (args.PropertyName == Commands.LongTapParameterProperty.PropertyName)
                UpdateLongTapParameter();
            else if (args.PropertyName == Commands.LongPressProperty.PropertyName)
                UpdateLongPress();
            else if (args.PropertyName == Commands.LongPressParameterProperty.PropertyName)
                UpdateLongPressParameter();
        }

        void UpdateTap() {
            _tapCommand = Commands.GetTap(Element);
        }

        void UpdateTapParameter() {
            _tapParameter = Commands.GetTapParameter(Element);
        }

        void UpdateLongTap() {
            _longCommand = Commands.GetLongTap(Element);
        }

        void UpdateLongTapParameter() {
            _longParameter = Commands.GetLongTapParameter(Element);
        }

        void UpdateLongPress()
        {
            _longPressCommand = Commands.GetLongPress(Element);
        }

        void UpdateLongPressParameter()
        {
            _longPressParameter = Commands.GetLongPressParameter(Element);
        }

        public static void Init() {
        }
    }
}