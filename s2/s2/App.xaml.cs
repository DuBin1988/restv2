using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Com.Aote.ObjectTools;
using Com.Aote.Pages;

namespace s2
{
    public partial class App : Application
    {
        public App()
        {
            this.Startup += (o, e) => 
            { 
                this.RootVisual = new Com.Aote.Pages.Frame(); 
            };
            this.Exit += this.Application_Exit;
            this.UnhandledException += this.Application_UnhandledException;
            
            InitializeComponent();
        }

        private void Application_Exit(object sender, EventArgs e)
        {
        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
            //errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");
            e.Handled = false;
            MessageBox.Show(errorMsg);
        }

        private void ReportErrorToDOM(ApplicationUnhandledExceptionEventArgs e)
        {
            try
            {
                string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
                errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");

                System.Windows.Browser.HtmlPage.Window.Eval("throw new Error(\"Unhandled Error in Silverlight Application " + errorMsg + "\");");
            }
            catch (Exception)
            {
            }
        }
    }
}
