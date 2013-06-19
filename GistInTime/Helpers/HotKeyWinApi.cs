﻿using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Input;

internal class HotKeyWinApi
{
    public const int WmHotKey = 0x0312;

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifiers, Keys vk);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}