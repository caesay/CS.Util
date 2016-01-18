using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;

namespace CS.Util.Input
{
    public class HotKey : IDisposable
    {
        private static Dictionary<int, HotKey> _dictHotKeyToCalBackProc;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, UInt32 fsModifiers, UInt32 vlc);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int WmHotKey = 0x0312;
        private bool _disposed = false;

        public Key Key { get; private set; }
        public ModifierKeys KeyModifiers { get; private set; }
        public Action<HotKey> Action { get; private set; }
        public int Id { get; set; }

        public HotKey(KeyGesture gesture, Action<HotKey> action)
            :this(gesture.Key, gesture.Modifiers, action)
        {
        }
        public HotKey(Key k, ModifierKeys keyModifiers, Action<HotKey> action)
        {
            Key = k;
            KeyModifiers = keyModifiers;
            Action = action;
        }

        public bool Register()
        {
            int virtualKeyCode = KeyInterop.VirtualKeyFromKey(Key);
            Id = virtualKeyCode + ((int)KeyModifiers * 0x10000);
            bool result = RegisterHotKey(IntPtr.Zero, Id, (UInt32)KeyModifiers, (UInt32)virtualKeyCode);

            if (_dictHotKeyToCalBackProc == null)
            {
                _dictHotKeyToCalBackProc = new Dictionary<int, HotKey>();
                ComponentDispatcher.ThreadFilterMessage += new ThreadMessageEventHandler(ComponentDispatcherThreadFilterMessage);
            }

            _dictHotKeyToCalBackProc.Add(Id, this);

            return result;
        }
        public void Unregister()
        {
            HotKey hotKey;
            if (_dictHotKeyToCalBackProc.TryGetValue(Id, out hotKey))
            {
                UnregisterHotKey(IntPtr.Zero, Id);
            }
        }

        private static void ComponentDispatcherThreadFilterMessage(ref MSG msg, ref bool handled)
        {
            if (!handled)
            {
                if (msg.message == WmHotKey)
                {
                    HotKey hotKey;

                    if (_dictHotKeyToCalBackProc.TryGetValue((int)msg.wParam, out hotKey))
                    {
                        if (hotKey.Action != null)
                        {
                            hotKey.Action.Invoke(hotKey);
                        }
                        handled = true;
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    Unregister();
                }
                _disposed = true;
            }
        }
    }
}
