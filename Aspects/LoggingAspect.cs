using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using ArxOne.MrAdvice.Advice;
using ArxOne.MrAdvice.Annotation;
using Common.Logging;
using Common.Logging.NLog;

namespace Aspects
{
	[Priority(2)] //Higher priority is executed first in contrast to PostSharp.
	[IncludePointcut(Scope = VisibilityScope.AnyAccessibility)]
	public sealed class LoggingAspect : Attribute, IMethodAdvice
	{
		private string className;
		private string methodName;
		private string returnType;
		private bool isPublicMethod;
		private bool isReturnTypeSimple;
		private ILog log;

		private void Initialize(MethodAdviceContext context)
		{
			if (log != null)
				return;

			var method = context.TargetMethod;
			isPublicMethod = method.IsPublic;
			if (method.DeclaringType != null)
				className = method.DeclaringType.FullName;
			System.Reflection.MethodInfo m = method as System.Reflection.MethodInfo;
			if (m != null)
			{
				returnType = m.ReturnType.ToString();
				isReturnTypeSimple = m.ReturnType.IsPrimitive || m.ReturnType == typeof(decimal) || m.ReturnType == typeof(string) ||
					m.ReturnType == typeof(DateTime) || m.ReturnType == typeof(Guid);
				methodName = m.Name;
			}
			LogManager.Adapter = new NLogLoggerFactoryAdapter(null);
			log = LogManager.GetLogger(className);
		}

		
		[DebuggerHidden]
		public static string UserName
		{
			get
			{
				string name = System.Environment.UserName;
				
				return name;
			}
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
		private void Log(string message, bool info = false)
		{
			if (isPublicMethod)
			{
				log.Debug(message);
			} else if (log.IsTraceEnabled)
			{
				if (info)
					log.Info(message);
				else
					log.Trace(message);
			}
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
		private string Serialize(MethodAdviceContext context)
		{
			string result = " null ";
			try
			{
				if (context.HasReturnValue && context.ReturnValue != null)
				{
					var obj = context.ReturnValue;

					if (isReturnTypeSimple)
					{
						return obj.ToString();
					}

					decimal retNum;
					if (obj is string || obj is char || obj is bool || obj is float ||
						(decimal.TryParse(Convert.ToString(obj), System.Globalization.NumberStyles.Any,
										  System.Globalization.NumberFormatInfo.InvariantInfo, out retNum)))
					{
						result = obj.ToString();
					}

					if (obj.GetType().IsArray)
					{
						result = ((Array) obj).Cast<object>().Aggregate("",
																			 (current, item) =>
																			 current + (item + " , "));
					}
					var listFields = (from item in obj.GetType().GetFields()
									  select item.GetValue(obj)).ToList();

					result = listFields.Aggregate<object, string>(null,
												(current, item) =>
												current +
												(" FieldType: " + item.GetType() + " FieldValue: " + (item != null ? item : "<null>")));
				}
			} catch (Exception exception)
			{
				result += "error during serialize: " + exception.Message;
			}

			return result;
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
		public void Advise(MethodAdviceContext context)
		{
			Initialize(context);
			if (!(log.IsTraceEnabled || log.IsDebugEnabled))
			{
				context.Proceed();
				return;
			}

			string parameters = "";
			if (context.Arguments != null && context.Arguments.Any())
			{
				parameters = string.Join("; ", context.Arguments.ToList().ConvertAll(a => (a ?? "null").ToString()).ToArray());
			}
			string entryMessage = string.Format(UserName + "| Entering Class.Method: {0}.{1} with Parameters: {2}", className, methodName, parameters);
			Log(entryMessage);

			try
			{
				context.Proceed();
			} catch (Exception e)
			{
				string exceptionMessage = string.Format(UserName + "| Exception in Class.Method {0}.{1} with Parameters :" + Environment.NewLine + e.Message);
				Log(exceptionMessage, true);
				throw;
			}

			string returnValue = Serialize(context);
			string successMessage = string.Format(UserName + "| Success in Class.Method: {0}.{1} with ReturnType: {2} and ReturnValue: {3}", className, methodName, returnType, returnValue);
			Log(successMessage);
		}
	}
}
