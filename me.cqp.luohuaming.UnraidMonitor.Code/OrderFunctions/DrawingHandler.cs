using me.cqp.luohuaming.UnraidMonitor.PublicInfos;
using me.cqp.luohuaming.UnraidMonitor.Sdk.Cqp;
using me.cqp.luohuaming.UnraidMonitor.Sdk.Cqp.EventArgs;
using System;
using System.IO;
using System.Linq;

namespace me.cqp.luohuaming.UnraidMonitor.Code.OrderFunctions
{
    public class DrawingHandler : IOrderModel
    {
        public bool ImplementFlag { get; set; } = true;

        public int Priority { get; set; } = 10;
        
        public string GetCommand() => "";

        public bool CanExecute(string command) => true;

        public FunctionResult Execute(CQGroupMessageEventArgs e)//群聊处理
        {
            FunctionResult result = new();
            SendText sendText = new()
            {
                SendID = e.FromGroup,
            };
            var d = GetDrawing(e.Message.Text);
            if (d != null)
            {
                result.Result = true;
                result.SendFlag = true;
                result.SendObject.Add(sendText);
                sendText.MsgToSend.Add(d);
            }
            return result;
        }

        public FunctionResult Execute(CQPrivateMessageEventArgs e)//私聊处理
        {
            FunctionResult result = new();
            SendText sendText = new()
            {
                SendID = e.FromQQ,
            };
            var d = GetDrawing(e.Message.Text);
            if (d != null)
            {
                result.Result = true;
                result.SendFlag = true;
                result.SendObject.Add(sendText);
                sendText.MsgToSend.Add(d);
            }
            return result;
        }

        public string? GetDrawing(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return null;
            }
            var command = MainSave.Commands.FirstOrDefault(x => x.Command == message.Trim());
            if (command.DrawingStyle == null)
            {
                MainSave.CQLog?.Error("绘制图像", $"指令 {command.Command} 的绘图对象为空，无法绘制");
                return null;
            }
            var path = Path.Combine(MainSave.UnraidMonitorImageSavePath, $"{DateTime.Now:yyyyMMddHHmmss}.png");
            command.DrawingStyle.Draw(command.DrawingStyle.Width).Save(path);

            return CQApi.CQCode_Image(Path.Combine("UnraidMonitor", Path.GetFileName(path))).ToSendString();
        }
    }
}
