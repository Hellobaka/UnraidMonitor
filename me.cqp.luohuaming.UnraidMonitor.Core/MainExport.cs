using System.Collections.Generic;
using System.Linq;
using System.Threading;
using me.cqp.luohuaming.UnraidMonitor.Code;
using me.cqp.luohuaming.UnraidMonitor.Sdk.Cqp.EventArgs;
using me.cqp.luohuaming.UnraidMonitor.Sdk.Cqp.Interface;
using me.cqp.luohuaming.UnraidMonitor.PublicInfos;

namespace me.cqp.luohuaming.UnraidMonitor.Core
{
    public class MainExport : IGroupMessage, IPrivateMessage
    {
        public void GroupMessage(object sender, CQGroupMessageEventArgs e)
        {
            FunctionResult result = Event_GroupMessage.GroupMessage(e);
            if (result.SendFlag)
            {
                if (result.SendObject == null || result.SendObject.Count == 0)
                {
                    e.Handler = false;
                }
                foreach (var item in result.SendObject)
                {
                    foreach (var sendMsg in item.MsgToSend)
                    {
                        e.CQApi.SendGroupMessage(item.SendID, sendMsg);
                    }
                }
            }
            e.Handler = result.Result;
        }

        public void PrivateMessage(object sender, CQPrivateMessageEventArgs e)
        {
            FunctionResult result = Event_PrivateMessage.PrivateMessage(e);
            if (result.SendFlag)
            {
                if (result.SendObject == null || result.SendObject.Count == 0)
                {
                    e.Handler = false;
                }
                foreach (var item in result.SendObject)
                {
                    foreach (var sendMsg in item.MsgToSend)
                    {
                        e.CQApi.SendPrivateMessage(item.SendID, sendMsg);
                    }
                }
            }
            e.Handler = result.Result;
        }
    }
}
