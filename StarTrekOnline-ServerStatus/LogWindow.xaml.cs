﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using StarTrekOnline_ServerStatus.Utils.API;

namespace StarTrekOnline_ServerStatus
{
    public partial class LogWindow : Window
    {
        private static LogWindow instance;
        private RichTextBox logRichTextBox;

        public static LogWindow Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LogWindow();
                    instance.InitializeComponent();
                    instance.logRichTextBox = instance.LogRichTextBox;
                    Logger.SetLogTarget(instance.logRichTextBox);
                }

                return instance;
            }
        }
        
        protected override void OnClosing(CancelEventArgs ev)
        {
            ev.Cancel = true;
            this.Hide();
        }

        private LogWindow()
        {
            InitializeComponent();
            logRichTextBox = LogRichTextBox;
        }
    }

}