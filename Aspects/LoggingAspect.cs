using System;
using System.Diagnostics;
using System.Linq;
using ArxOne.MrAdvice.Advice;
using ArxOne.MrAdvice.Annotation;

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

		private Logger log; // was Common.Logging.ILog

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
				isReturnTypeSimple = m.ReturnType.IsPrimitive || m.ReturnType == typeof(decimal) || m.ReturnType == typeof(string) 
					                || m.ReturnType == typeof(DateTime) || m.ReturnType == typeof(Guid);
				methodName = m.Name;
			}
			
			log = new Logger(); // init logging stuff, removed for brevity
		}

		private void Log(string message, bool info = false)
		{
			if (isPublicMethod)
			{
				log.Debug(message);
			}
		}


		public void Advise(MethodAdviceContext context)
		{
			Initialize(context);
			Log("Methodname: " + context.TargetMethod.Name);

			try
			{
				context.Proceed();
			} catch (Exception e)
			{
				string exceptionMessage = string.Format("| Exception in Class.Method {0}.{1} with Parameters :" + Environment.NewLine + e.Message);
				Log(exceptionMessage, true);
				throw;
			}

			string successMessage = string.Format("| Success in Class.Method: {0}.{1} with ReturnType: {2}", className, methodName, returnType);
			Log(successMessage);
		}
	}

	public class Logger
	{
		public void Debug(string message)
		{
			Console.WriteLine("Debug logging: " + message);
		}
	}
}
