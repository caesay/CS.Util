using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CS.Util.Input
{
    public class MouseHook : GlobalHook
    {
        public event EventHandler<MouseEventArgs> MouseDown;
        public event EventHandler<MouseEventArgs> MouseUp;
        public event EventHandler<MouseEventArgs> MouseMove;
        public event EventHandler<MouseEventArgs> MouseWheel;

        public event EventHandler Click;
        public event EventHandler DoubleClick;

        public MouseHook()
        {
            _hookType = WH_MOUSE_LL;
        }

        protected override int HookCallbackProcedure(int nCode, int wParam, IntPtr lParam)
        {
            if (nCode > -1 && (MouseDown != null || MouseUp != null || MouseMove != null))
            {
                MouseLLHookStruct mouseHookStruct =
                    (MouseLLHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseLLHookStruct));

                var button = GetButton(wParam);
                MouseEventType eventType = GetEventType(wParam);

                MouseEventArgs e = new MouseEventArgs(
                    button,
                    (eventType == MouseEventType.DoubleClick ? 2 : 1),
                    mouseHookStruct.pt.x,
                    mouseHookStruct.pt.y,
                    (eventType == MouseEventType.MouseWheel ? (short)((mouseHookStruct.mouseData >> 16) & 0xffff) : 0));

                // Prevent multiple Right Click events (this probably happens for popup menus)
                if (button == System.Windows.Input.MouseButton.Right && mouseHookStruct.flags != 0)
                {
                    eventType = MouseEventType.None;
                }

                switch (eventType)
                {
                    case MouseEventType.MouseDown:
                        MouseDown?.Invoke(this, e);
                        break;
                    case MouseEventType.MouseUp:
                        Click?.Invoke(this, new EventArgs());
                        MouseUp?.Invoke(this, e);
                        break;
                    case MouseEventType.DoubleClick:
                        DoubleClick?.Invoke(this, new EventArgs());
                        break;
                    case MouseEventType.MouseWheel:
                        MouseWheel?.Invoke(this, e);
                        break;
                    case MouseEventType.MouseMove:
                        MouseMove?.Invoke(this, e);
                        break;
                }

            }
            return CallNextHookEx(_handleToHook, nCode, wParam, lParam);
        }

        private System.Windows.Input.MouseButton GetButton(Int32 wParam)
        {
            switch (wParam)
            {
                case WM_LBUTTONDOWN:
                case WM_LBUTTONUP:
                case WM_LBUTTONDBLCLK:
                    return System.Windows.Input.MouseButton.Left;
                case WM_RBUTTONDOWN:
                case WM_RBUTTONUP:
                case WM_RBUTTONDBLCLK:
                    return System.Windows.Input.MouseButton.Right;
                case WM_MBUTTONDOWN:
                case WM_MBUTTONUP:
                case WM_MBUTTONDBLCLK:
                    return System.Windows.Input.MouseButton.Middle;
                default:
                    throw new ArgumentOutOfRangeException(nameof(wParam));
            }
        }
        private MouseEventType GetEventType(Int32 wParam)
        {

            switch (wParam)
            {

                case WM_LBUTTONDOWN:
                case WM_RBUTTONDOWN:
                case WM_MBUTTONDOWN:
                    return MouseEventType.MouseDown;
                case WM_LBUTTONUP:
                case WM_RBUTTONUP:
                case WM_MBUTTONUP:
                    return MouseEventType.MouseUp;
                case WM_LBUTTONDBLCLK:
                case WM_RBUTTONDBLCLK:
                case WM_MBUTTONDBLCLK:
                    return MouseEventType.DoubleClick;
                case WM_MOUSEWHEEL:
                    return MouseEventType.MouseWheel;
                case WM_MOUSEMOVE:
                    return MouseEventType.MouseMove;
                default:
                    return MouseEventType.None;

            }
        }

        private enum MouseEventType
        {
            None,
            MouseDown,
            MouseUp,
            DoubleClick,
            MouseWheel,
            MouseMove
        }
    }
    public class MouseEventArgs : EventArgs
    {
        public MouseEventArgs(System.Windows.Input.MouseButton button, int clicks, int x, int y, int delta)
        {
            this.Button = button;
            this.Clicks = clicks;
            this.X = x;
            this.Y = y;
            this.Delta = delta;
        }

        public System.Windows.Input.MouseButton Button { get; }
        public int Clicks { get; }
        public int X { get; }
        public int Y { get; }
        public int Delta { get; }
    }
}
