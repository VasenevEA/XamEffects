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

namespace XamEffects.iOS
{
    public class CommandsPlatform : PlatformEffect
    {
        public UIView View => Control ?? Container;

        DateTime _tapTime;
        ICommand _tapCommand;
        ICommand _longCommand;
        ICommand _longPressCommand;
        ICommand _cancelTapCommand;
        object _tapParameter;
        object _longParameter;
        object _longPressParameter;
        object _cancelTapParameter;

        protected override void OnAttached()
        {
            View.UserInteractionEnabled = true;

            UpdateTap();
            UpdateTapParameter();
            UpdateLongTap();
            UpdateLongTapParameter();
            UpdateLongPress();
            UpdateLongPressParameter();
            UpdateCancelTap();
            UpdateCancelTapParameter();

            TouchGestureCollector.Add(View, OnTouch);
        }

        protected override void OnDetached()
        {
            TouchGestureCollector.Delete(View, OnTouch);
        }

        CancellationTokenSource disableLongPress;
        bool isLongPressActive = false;

        void StartLongPressWaiter()
        {
            isLongPressActive = false;
            disableLongPress?.Cancel();
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
        }

        void StopLongPressWaiter()
        {
            disableLongPress?.Cancel();
        }

        void OnTouch(TouchGestureRecognizer.TouchArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.State);
            switch (e.State)
            {
                case TouchGestureRecognizer.TouchState.Started:
                    _tapTime = DateTime.Now;
                    StartLongPressWaiter();
                    break;

                case TouchGestureRecognizer.TouchState.Ended:
                    StopLongPressWaiter();
                    if (e.Inside)
                    {
                        var range = (DateTime.Now - _tapTime).TotalMilliseconds;
                        if (range > Commands.GetLongTapDelay(Element))
                            LongClickHandler();
                        else if (!isLongPressActive)
                            ClickHandler();
                    }
                    CancelClickHandler();
                    break;
                default:
                    StopLongPressWaiter();
                    CancelClickHandler();
                    break;

            }
        }

        void ClickHandler()
        {
            if (_tapCommand?.CanExecute(_tapParameter) ?? false)
                _tapCommand.Execute(_tapParameter);
        }

        void LongClickHandler()
        {
            if (_longCommand == null)
                ClickHandler();
            else if (_longCommand.CanExecute(_longParameter))
                _longCommand.Execute(_longParameter);
        }

        void LongPressHandler()
        {
            if (_longPressCommand == null)
                ClickHandler();
            else if (_longPressCommand.CanExecute(_longPressParameter))
                _longPressCommand.Execute(_longPressParameter);
        }

        void CancelClickHandler()
        {
            var cmd = Commands.GetCancelTap(Element);
            var param = Commands.GetCancelTapParameter(Element);
            if (cmd?.CanExecute(param) ?? false)
                cmd.Execute(param);
        }

        protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
        {
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
            else if (args.PropertyName == Commands.CancelTapProperty.PropertyName)
                UpdateCancelTap();
            else if (args.PropertyName == Commands.CancelTapParameterProperty.PropertyName)
                UpdateCancelTapParameter();
        }

        void UpdateTap()
        {
            _tapCommand = Commands.GetTap(Element);
        }

        void UpdateTapParameter()
        {
            _tapParameter = Commands.GetTapParameter(Element);
        }

        void UpdateLongTap()
        {
            _longCommand = Commands.GetLongTap(Element);
        }

        void UpdateLongTapParameter()
        {
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

        void UpdateCancelTap()
        {
            _cancelTapCommand = Commands.GetCancelTap(Element);
        }

        void UpdateCancelTapParameter()
        {
            _cancelTapParameter = Commands.GetCancelTapParameter(Element);
        }
        public static void Init()
        {
        }
    }
}