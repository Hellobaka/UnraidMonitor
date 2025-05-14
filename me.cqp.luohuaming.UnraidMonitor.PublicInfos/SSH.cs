using Renci.SshNet;
using Renci.SshNet.Common;
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

        private string Host { get; set; }
       
        private int Port { get; set; }
       
        private string User { get; set; }
       
        private string Pwd { get; set; }

        public SshCommandQueue(string host, int port, string user, string pwd)
        {
            Host = host;
            Port = port;
            User = user;
            Pwd = pwd;

            SSHClient = new SshClient(Host, Port, User, Pwd);
            SSHClient.Connect();
            MainSave.CQLog?.Info("SSH连接", $"连接到 {AppConfig.SSHHost}:{AppConfig.SSHPort} 成功");
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
                        if (SSHClient == null || !SSHClient.IsConnected)
                        {
                            MainSave.CQLog?.Info("SSH连接", $"连接到 {AppConfig.SSHHost}:{AppConfig.SSHPort} 成功");
                            SSHClient = new SshClient(Host, Port, User, Pwd);
                            SSHClient.Connect();
                        }
                        var command = SSHClient.CreateCommand(item.cmd);
                        Console.WriteLine($"Executing {item.cmd}");
                        command.CommandTimeout = TimeSpan.FromSeconds(AppConfig.SSHCommandTimeout);
                        command.Execute();
                        item.task.SetResult((command.Error, command.Result));
                    }
                    catch (SshOperationTimeoutException ex)
                    {
                        item.task.SetResult(("Timeout", null));
                        SSHClient.Dispose();
                        SSHClient = null;
                        MainSave.CQLog?.Error("SSH连接", $"指令 {item.cmd} 执行超时：{ex}");
                    }
                    catch (Exception ex)
                    {
                        item.task.SetResult((ex.Message, null));
                        SSHClient.Dispose();
                        SSHClient = null;
                        MainSave.CQLog?.Error("SSH连接", $"连接发生异常：{ex}");
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
