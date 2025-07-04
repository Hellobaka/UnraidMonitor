using me.cqp.luohuaming.UnraidMonitor.Sdk.Cqp.Enum;
using me.cqp.luohuaming.UnraidMonitor.Sdk.Cqp.Expand;
using me.cqp.luohuaming.UnraidMonitor.Sdk.Cqp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.UnraidMonitor.Sdk.Cqp.EventArgs
{
	/// <summary>
	/// 提供用于描述酷Q群文件上传事件参数的类
	/// <para/>
	/// Type: 11
	/// </summary>
	public sealed class CQGroupUploadEventArgs : CQEventEventArgs
	{
		#region --属性--
		/// <summary>
		/// 获取当前事件的消息子类型
		/// </summary>
		public CQGroupFileUploadType SubType { get; private set; }

		/// <summary>
		/// 获取当前事件的发送时间
		/// </summary>
		public DateTime SendTime { get; private set; }

		/// <summary>
		/// 获取当前事件的来源群
		/// </summary>
		public Group FromGroup { get; private set; }

		/// <summary>
		/// 获取当前事件的来源QQ
		/// </summary>
		public QQ FromQQ { get; private set; }

		/// <summary>
		/// 获取当前事件的文件信息
		/// </summary>
		public GroupFileInfo FileInfo { get; private set; }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 初始化 <see cref="CQGroupUploadEventArgs"/> 类的新实例
		/// </summary>
		/// <param name="api">酷Q的接口实例</param>
		/// <param name="log">酷Q的日志实例</param>
		/// <param name="id">事件Id</param>
		/// <param name="type">事件类型</param>
		/// <param name="name">事件名称</param>
		/// <param name="function">函数名称</param>
		/// <param name="priority">默认优先级</param>
		/// <param name="subType">子类型</param>
		/// <param name="sendTime">发送时间</param>
		/// <param name="fromGroup">来源群</param>
		/// <param name="fromQQ">来源QQ</param>
		/// <param name="file">文件信息</param>
		public CQGroupUploadEventArgs (CQApi api, CQLog log, int id, int type, string name, string function, uint priority, int subType, int sendTime, long fromGroup, long fromQQ, string file)
			: base (api, log, id, type, name, function, priority)
		{
			this.SubType = (CQGroupFileUploadType)subType;
			this.SendTime = sendTime.ToDateTime ();
			this.FromGroup = new Group (api, fromGroup);
			this.FromQQ = new QQ (api, fromQQ);
			this.FileInfo = new GroupFileInfo (api, file);
		}
		#endregion

		#region --公开函数--
		/// <summary>
		/// 返回表示当前对象的字符串
		/// </summary>
		/// <returns>表示当前对象的字符串</returns>
		public override string ToString ()
		{
			StringBuilder builder = new StringBuilder ();
			builder.AppendLine (string.Format ("ID: {0}", this.Id));
			builder.AppendLine (string.Format ("类型: {0}({1})", this.Type, (int)this.Type));
			builder.AppendLine (string.Format ("名称: {0}", this.Name));
			builder.AppendLine (string.Format ("函数: {0}", this.Function));
			builder.AppendLine (string.Format ("优先级: {0}", this.Priority));
			builder.AppendLine (string.Format ("子类型: {0}({1})", this.SubType, (int)this.SubType));
			builder.AppendLine (string.Format ("发送时间: {0}", this.SendTime));
			builder.AppendLine (string.Format ("群号: {0}", this.FromGroup != null ? this.FromGroup.Id.ToString () : string.Empty));
			builder.AppendLine (string.Format ("账号: {0}", this.FromQQ != null ? this.FromQQ.Id.ToString () : string.Empty));
			builder.Append (this.FileInfo != null ? this.FileInfo.ToString () : "文件信息: ");
			return builder.ToString ();
		}
		#endregion
	}
}
