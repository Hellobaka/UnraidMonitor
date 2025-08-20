using me.cqp.luohuaming.UnraidMonitor.PublicInfos;
using me.cqp.luohuaming.UnraidMonitor.Sdk.Cqp.EventArgs;
using me.cqp.luohuaming.UnraidMonitor.Sdk.Cqp.Interface;
using System;
using System.Threading;
using System.Windows;

namespace me.cqp.luohuaming.UnraidMonitor.UI
{
    public class Event_MenuCall : IMenuCall
    {
        private App App { get; set; }

        public void MenuCall(object sender, CQMenuCallEventArgs e)
        {
            try
            {
                if (App == null)
                {
                    Thread thread = new Thread(() =>
                    {
                        App = new();
                        App.ShutdownMode = ShutdownMode.OnMainWindowClose;
                        App.InitializeComponent();
                        App.Run();
                    });
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                }
                else
                {
                    MainWindow.Instance.Dispatcher.BeginInvoke(new Action(MainWindow.Instance.Show));
                }
            }
            catch (Exception exc)
            {
                MainSave.CQLog.Info("Error", exc.Message, exc.StackTrace);
            }
        }

        ///<summary>
        ///窗体关闭时触发
        ///</summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow.Instance.Dispatcher.BeginInvoke(new Action(MainWindow.Instance.Hide));
        }
    }
}