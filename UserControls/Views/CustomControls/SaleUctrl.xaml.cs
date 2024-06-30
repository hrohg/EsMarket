using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UserControls.ViewModels.Invoices;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;


namespace UserControls.Views.CustomControls
{
    /// <summary>
    /// Interaction logic for SaleUserControl.xaml
    /// </summary>
    public partial class SaleUctrl : UserControl
    {
        private SaleInvoiceViewModel _vm;
        public SaleUctrl()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            KeyDown += OnKeyDown;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _vm = DataContext as SaleInvoiceViewModel;
        }
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // process barcode
            if (e.Key == Key.Enter)
            {
                //string barcode = new string(_barcode.ToArray());
                //if (_vm == null) return;
                //if (_barcode.Count == 16)
                //{
                //    _vm.SetPartnerCardNumber(barcode);
                //}
                //else if (_barcode.Count > 5)
                //{
                //    _vm.SetInvoiceItem(barcode);
                //}
                //_barcode.Clear();
                //_barcodeText.Clear();
                //return;
            }

            // check timing (keystrokes within 100 ms)
            TimeSpan elapsed = (DateTime.Now - _lastKeystroke);
            if (elapsed.TotalMilliseconds > 200)
            {
                _barcode.Clear();
                _barcodeText.Clear();
            }

            // record keystroke & timestamp
            var key = KeyboardHelper.KeyToChar(e.Key == Key.System ? e.SystemKey : e.Key);
            if (e.Key == Key.System)
            {
                if (Keyboard.IsKeyDown(Key.NumPad0)) key = '0';
                if (Keyboard.IsKeyDown(Key.NumPad1)) key = '1';
                if (Keyboard.IsKeyDown(Key.NumPad2)) key = '2';
                if (Keyboard.IsKeyDown(Key.NumPad3)) key = '3';
                if (Keyboard.IsKeyDown(Key.NumPad4)) key = '4';
                if (Keyboard.IsKeyDown(Key.NumPad5)) key = '5';
                if (Keyboard.IsKeyDown(Key.NumPad6)) key = '6';
                if (Keyboard.IsKeyDown(Key.NumPad7)) key = '7';
                if (Keyboard.IsKeyDown(Key.NumPad8)) key = '8';
                if (Keyboard.IsKeyDown(Key.NumPad9)) key = '9';
            }


            if (key == '\x00')
            {
                //if (Keyboard.IsKeyDown(Key.NumPad1)) { MessageManager.OnMessage("1" + " " + (int)e.Key); } else { MessageManager.OnMessage((int)e.SystemKey + " " + (int)e.Key); }
                return;
            }
            _barcode.Add(key);
            _barcodeText.Append(key);
            _lastKeystroke = DateTime.Now;

        }

        private void CtrlInvoice_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
            {
                switch (e.Key)
                {
                    case Key.D1:

                        break;
                    case Key.Enter:
                        TxtPaid.Focus();
                        TxtPaid.SelectAll();
                        break;
                    case Key.F:
                    case Key.D:
                        //new Thread(() =>
                        //{
                        //    Thread.Sleep(5000);
                        //    //Keys key;
                        //    //Enum.TryParse("Enter", out key);

                        //    //var str = "012345678910111213141516171819202122bla - bla";
                        //    //var hex = str.Select(c => ((int)c).ToString("X")).Aggregate(String.Empty, (x, y) => x + y);
                        //    Dispatcher.Invoke(() => {
                        //        //    //foreach (var key in text)uint.Parse(key, System.Globalization.NumberStyles.HexNumber)
                        //        //    { PostMessage(Process.GetCurrentProcess().Handle, hex, 0, 0); }
                        //        _vm.SetExternalText(new ES.Common.Helpers.ExternalTextImputEventArgs("012345678910111213141516171819202122bla-bla"));
                        //    });
                        //    //keybd_event(Convert.ToUInt32("012345678910111213141516171819202122bla-bla"), 0, 0, 0);
                           
                        //}).Start();
                        break;
                    case Key.N:
                        break;
                    //todo
                    //case Key.S:
                    //    _viewModel.OnSaveInvoice(null);
                    //    break;
                    case Key.Q:

                        break;
                    case Key.X:

                        break;
                }
            }
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void keybd_event(uint bVk, uint bScan, uint dwFlags, uint dwExtraInfo);

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        private DateTime _lastKeystroke = new DateTime(0);
        private List<char> _barcode = new List<char>(10);
        private System.Text.StringBuilder _barcodeText = new System.Text.StringBuilder();
        //protected override void OnPreviewKeyDown(KeyEventArgs e)
        //{
        //    base.OnPreviewKeyDown(e);
        //    // process barcode
        //    if (e.Key == Key.Enter)
        //    {
        //        string barcode = new string(_barcode.ToArray());
        //        if (_vm == null) return;
        //        if (_barcode.Count == 16)
        //        {
        //            _vm.SetPartnerCardNumber(barcode);
        //        }
        //        else if (_barcode.Count > 5)
        //        {
        //            _vm.SetInvoiceItem(barcode);
        //        }
        //        MessageManager.OnMessage(_barcode.Count + " " + _barcodeText);
        //        _barcode.Clear();
        //        return;
        //    }

        //    // check timing (keystrokes within 100 ms)
        //    TimeSpan elapsed = (DateTime.Now - _lastKeystroke);
        //    if (elapsed.TotalMilliseconds > 1000)
        //    {
        //        _barcode.Clear();
        //        //_barcodeText.Clear();
        //    }

        //    // record keystroke & timestamp
        //    var key = KeyboardHelper.KeyToChar(e.Key);
        //    if (key == '\x00' || key == '\x01')
        //    {
        //        MessageManager.OnMessage(key + " " + (int)e.Key);
        //        _barcodeText.Append((int)e.Key);
        //        return;
        //    }
        //    _barcode.Add(key);

        //    _lastKeystroke = DateTime.Now;

        //}
    }
}
