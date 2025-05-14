using Renci.SshNet;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.UnraidMonitor.PublicInfos
{
    public class SshCommandQueue : IDisposable
    {
       
        private ConcurrentQueue<(string cmd, TaskCompletionSource<(string error, string output)> task)> CommandQueue { get; set; } = new();
       
        private AutoResetEvent QueueHandleSignal { get; set; } = new(false);
       
        private SshClient SSHClient { get; set; }
       
        private bool Running { get; set; } = true;
       
        private Thread Worker { get; set; }

        public SshCommandQueue(string host, int port, string user, string pwd)
        {
            SSHClient = new SshClient(host, port, user, pwd);
            SSHClient.Connect();
            MainSave.CQLog.Info("SSH连接", $"连接到 {AppConfig.SSHHost}:{AppConfig.SSHPort} 成功");
            Worker = new Thread(ProcessQueue) { IsBackground = true };
            Worker.Start();
        }

        public Task<(string error, string output)> EnqueueCommand(string cmd)
        {
            var tcs = new TaskCompletionSource<(string error, string output)>(TaskCreationOptions.RunContinuationsAsynchronously);
            CommandQueue.Enqueue((cmd, tcs));
            QueueHandleSignal.Set();
            return tcs.Task;
        }

        private void ProcessQueue()
        {
            while (Running)
            {
                while (CommandQueue.TryDequeue(out var item))
                {
                    try
                    {
                        if (!SSHClient.IsConnected)
                        {
                            MainSave.CQLog.Info("SSH连接", $"连接到 {AppConfig.SSHHost}:{AppConfig.SSHPort} 成功");
                            SSHClient.Connect();
                        }
                        var command = SSHClient.CreateCommand(item.cmd);
                        command.CommandTimeout = TimeSpan.FromSeconds(AppConfig.SSHCommandTimeout);
                        command.Execute();
                        item.task.SetResult((command.Error, command.Result));
                    }
                    catch (Exception ex)
                    {
                        item.task.SetResult((ex.Message, null));
                        SSHClient.Dispose();
                        SSHClient = null;
                        MainSave.CQLog.Error("SSH连接", $"连接发生异常：{ex}");
                    }
                }
                QueueHandleSignal.WaitOne();
            }
        }

        public void Dispose()
        {
            Running = false;
            QueueHandleSignal.Set();
            Worker.Join();
            SSHClient?.Dispose();
        }
    }
}
