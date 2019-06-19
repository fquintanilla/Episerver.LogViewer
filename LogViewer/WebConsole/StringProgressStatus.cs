﻿using System;
using System.Diagnostics;
using System.Text;
using System.Web;

namespace Kamsar.WebConsole
{
	/// <summary>
	/// Variant of the IProgressStatus that saves the output lines to a StringBuilder for programmatic capture
	/// 
	/// Progress reports are not captured.
	/// </summary>
	public class StringProgressStatus : IProgressStatus
	{
		private readonly StringBuilder _output = new StringBuilder();
		private readonly StringBuilder _errors = new StringBuilder();
		private readonly StringBuilder _warnings = new StringBuilder();

		public virtual void ReportException(Exception exception)
		{
			var exMessage = new StringBuilder();
			exMessage.AppendFormat("ERROR: {0} ({1})", exception.Message, exception.GetType().FullName);
			exMessage.AppendLine();

			exMessage.Append(exception.StackTrace?.Trim() ?? "No stack trace available.");

			exMessage.AppendLine();

			WriteInnerException(exception.InnerException, exMessage);

			ReportStatus(exMessage.ToString(), MessageType.Error);

			if (Debugger.IsAttached) Debugger.Break();
		}

		public virtual void ReportStatus(string statusMessage, params object[] formatParameters)
		{
			ReportStatus(statusMessage, MessageType.Info, formatParameters);
		}

		public virtual void ReportStatus(string statusMessage, MessageType type, params object[] formatParameters)
		{
			var line = new StringBuilder();

			line.AppendFormat("{0}: ", type);

			if (formatParameters.Length > 0)
				line.AppendFormat(statusMessage, formatParameters);
			else
				line.Append(statusMessage);

			_output.AppendLine(HttpUtility.HtmlEncode(line.ToString()));

			if (type == MessageType.Error)
				_errors.AppendLine(HttpUtility.HtmlEncode(line.ToString()));

			if (type == MessageType.Warning)
				_warnings.AppendLine(HttpUtility.HtmlEncode(line.ToString()));
		}

		public virtual void Report(int percent)
		{
			Progress = percent;
		}

		public virtual void ReportTransientStatus(string statusMessage, params object[] formatParameters)
		{
			// do nothing
		}

		/// <summary>
		/// All available console output
		/// </summary>
		public virtual string Output => _output.ToString();

		/// <summary>
		/// All error output from the console
		/// </summary>
		public virtual string Errors => _errors.ToString();

		/// <summary>
		/// All warning output from the console
		/// </summary>
		public virtual string Warnings => _warnings.ToString();

		public virtual bool HasErrors => _errors.Length > 0;
		public virtual bool HasWarnings => _warnings.Length > 0;

		public virtual int Progress { get; private set; }

		protected virtual void WriteInnerException(Exception innerException, StringBuilder exMessage)
		{
			if (innerException == null) return;

			exMessage.AppendLine("INNER EXCEPTION");
			exMessage.AppendFormat("{0} ({1})", innerException.Message, innerException.GetType().FullName);
			exMessage.AppendLine();

			exMessage.Append(innerException.StackTrace?.Trim() ?? "No stack trace available.");

			WriteInnerException(innerException.InnerException, exMessage);

			exMessage.AppendLine();
		}
	}
}
