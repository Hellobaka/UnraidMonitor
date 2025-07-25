using me.cqp.luohuaming.UnraidMonitor.Sdk.Cqp.Enum;
using me.cqp.luohuaming.UnraidMonitor.Sdk.Cqp.Expand;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.UnraidMonitor.Sdk.Cqp.Model
{
	/// <summary>
	/// 描述 QQ陌生人信息 的类
	/// </summary>
	public class StrangerInfo : BasisStreamModel, IEquatable<StrangerInfo>
	{
		#region --属性--
		/// <summary>
		/// 获取一个值, 指示当前的账号的 <see cref="Cqp.Model.QQ"/> 实例
		/// </summary>
		public QQ QQ { get; private set; }
		/// <summary>
		/// 获取一个值, 指示当前的QQ昵称
		/// </summary>
		public string Nick { get; private set; }
		/// <summary>
		/// 获取一个值, 指示当前QQ的性别
		/// </summary>
		public QQSex Sex { get; private set; }
		/// <summary>
		/// 获取一个值, 指示当前的年龄
		/// </summary>
		public int Age { get; private set; }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 使用指定的密文初始化 <see cref="StrangerInfo"/> 类的新实例
		/// </summary>
		/// <param name="api">模型使用的 <see cref="Cqp.CQApi"/></param>
		/// <param name="cipherText">模型使用的解密密文字符串</param>
		public StrangerInfo (CQApi api, string cipherText)
			: base (api, cipherText)
		{
		}
		#endregion

		#region --公开方法--
		/// <summary>
		/// 指示当前对象是否等于同一类型的另一个对象
		/// </summary>
		/// <param name="other">一个与此对象进行比较的对象</param>
		/// <returns>如果当前对象等于 other 参数，则为 <see langword="true"/>；否则为 <see langword="false"/></returns>
		public bool Equals (StrangerInfo other)
		{
			if (other == null)
			{
				return false;
			}

			return this.QQ.Equals (other.QQ) && this.Nick.Equals (other.Nick) && this.Sex == other.Sex && this.Age == other.Age;
		}
		/// <summary>
		/// 指示当前对象是否等于同一类型的另一个对象
		/// </summary>
		/// <param name="obj">一个与此对象进行比较的对象</param>
		/// <returns>如果当前对象等于 other 参数，则为 <see langword="true"/>；否则为 <see langword="false"/></returns>
		public override bool Equals (object obj)
		{
			return this.Equals (obj as StrangerInfo);
		}
		/// <summary>
		/// 返回此实例的哈希代码
		/// </summary>
		/// <returns>32 位有符号整数哈希代码</returns>
		public override int GetHashCode ()
		{
			return this.QQ.GetHashCode () & this.Nick.GetHashCode () & this.Sex.GetHashCode () & this.Age.GetHashCode ();
		}
		/// <summary>
		/// 返回表示当前对象的字符串
		/// </summary>
		/// <returns>表示当前对象的字符串</returns>
		public override string ToString ()
		{
			StringBuilder builder = new StringBuilder ();
			builder.AppendFormat ("QQ: {0}{1}", this.QQ.ToSendString (), Environment.NewLine);
			builder.AppendFormat ("昵称: {0}{1}", this.Nick, Environment.NewLine);
			builder.AppendFormat ("性别: {0}{1}", this.Sex.GetDescription (), Environment.NewLine);
			builder.AppendFormat ("年龄: {0}{1}", this.Age, Environment.NewLine);
			return builder.ToString ();
		}
		/// <summary>
		/// 当在派生类中重写时, 处理返回用于发送的字符串
		/// </summary>
		/// <returns>用于发送的字符串</returns>
		public override string ToSendString ()
		{
			return this.ToString ();
		}
		#endregion

		#region --私有方法--
		/// <summary>
		/// 进行当前模型初始化
		/// </summary>
		/// <param name="reader">解析模型的数据源</param>
		protected override void Initialize (BinaryReader reader)
		{
			this.QQ = new QQ (this.CQApi, reader.ReadInt64_Ex ());
			this.Nick = reader.ReadString_Ex (CQApi.DefaultEncoding);
			this.Sex = (QQSex)reader.ReadInt32_Ex ();
			this.Age = reader.ReadInt32_Ex ();
		}
		#endregion
	}
}
