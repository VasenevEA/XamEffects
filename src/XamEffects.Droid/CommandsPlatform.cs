﻿using System;
using Android.Graphics;
using Android.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XamEffects;
using XamEffects.Droid;
using View = Android.Views.View;
using System.Threading;
using XamEffects.Droid.GestureCollectors;
using System.Threading.Tasks;

[assembly: ExportEffect(typeof(CommandsPlatform), nameof(Commands))]

namespace XamEffects.Droid
{
    public class CommandsPlatform : PlatformEffect
    {
        public View View => Control ?? Container;
        public bool IsDisposed => (Container as IVisualElementRenderer)?.Element == null;

        DateTime _tapTime;
        readonly Rect _rect = new Rect();
        readonly int[] _location = new int[2];

        public static void Init()
        {
        }

        protected override void OnAttached()
        {
            View.Clickable = true;
            View.LongClickable = true;
            View.SoundEffectsEnabled = true;
            TouchCollector.Add(View, OnTouch);
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

        void OnTouch(View.TouchEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine(args.Event.Action);
            switch (args.Event.Action)
            {
                case MotionEventActions.Down:
                    _tapTime = DateTime.Now;
                    StartLongPressWaiter();
                    break;

                case MotionEventActions.Up:
                    StopLongPressWaiter();
                    if (IsViewInBounds((int)args.Event.RawX, (int)args.Event.RawY))
                    {
                        var range = (DateTime.Now - _tapTime).TotalMilliseconds;
                        if (range > Commands.GetLongTapDelay(Element))
                            LongClickHandler();
                        else if (!isLongPressActive)
                            ClickHandler();
                    }
                    break;
                case MotionEventActions.Move:
                    break;
                default:
                    StopLongPressWaiter();
                    break;
            }
        }

        bool IsViewInBounds(int x, int y)
        {
            View.GetDrawingRect(_rect);
            View.GetLocationOnScreen(_location);
            _rect.Offset(_location[0], _location[1]);
            return _rect.Contains(x, y);
        }

        void ClickHandler()
        {
            var cmd = Commands.GetTap(Element);
            var param = Commands.GetTapParameter(Element);
            if (cmd?.CanExecute(param) ?? false)
                cmd.Execute(param);
        }

        void LongClickHandler()
        {
            var cmd = Commands.GetLongTap(Element);

            if (cmd == null)
            {
                ClickHandler();
                return;
            }

            var param = Commands.GetLongTapParameter(Element);
            if (cmd.CanExecute(param))
                cmd.Execute(param);
        }

        void LongPressHandler()
        {
            var cmd = Commands.GetLongPress(Element);

            if (cmd == null)
            {
                ClickHandler();
                return;
            }
            var param = Commands.GetLongPressParameter(Element);
            if (cmd.CanExecute(param))
                cmd.Execute(param);
        }

        protected override void OnDetached()
        {
            if (IsDisposed) return;
            TouchCollector.Delete(View, OnTouch);
        }
    }
}