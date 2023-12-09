using System.Numerics;
using Android.Views;
using REngine.Core.IO;

namespace REngine.Android.Windows;

internal class WindowTouchListener(WindowImpl window) : Java.Lang.Object, View.IOnTouchListener
{
    public bool OnTouch(View? v, MotionEvent? e)
    {
        if (e is null)
            return false;
        var action = e.Action;
        var evtPointerIdx = ((int)action & (int)MotionEventActions.PointerIndexMask) >> (int)MotionEventActions.PointerIndexShift;
        action &= MotionEventActions.Mask;
        
        switch (e.Action)
        {
            case MotionEventActions.Down:
            case MotionEventActions.Up:
            {
                var toolType = e.GetToolType(evtPointerIdx);
                if (toolType is MotionEventToolType.Finger or MotionEventToolType.Unknown)
                {
                    window.ForwardMouseMove(new Vector2(e.GetX(evtPointerIdx), e.GetY(evtPointerIdx)));
                    if (e.Action == MotionEventActions.Down)
                        window.ForwardMouseDown(MouseKey.Left);
                    else
                        window.ForwardMouseUp(MouseKey.Left);
                }
            }
                break;
            case MotionEventActions.ButtonPress:
            case MotionEventActions.ButtonRelease:
            {
                // var buttonState = e.ButtonState;
                // if((buttonState & MotionEventButtonState.Primary) != 0)
                //     
            }
                break;
            case MotionEventActions.HoverMove:
            case MotionEventActions.Move:
                window.ForwardMouseMove(new Vector2(e.GetX(evtPointerIdx), e.GetY(evtPointerIdx)));
                break;
        }

        return true;
    }
}