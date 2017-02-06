using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;


namespace Shared.Helpers
{
	public class ServiceExecutionContext<T> : WCFClient<T> where T : class
	{
		private static ServiceExecutionContext<T> _instance;
		public static ServiceExecutionContext<T> Instance
		{
			get
			{
				if (_instance == null)
					_instance = new ServiceExecutionContext<T>();
				return _instance;
			}
		}

		private ServiceExecutionContext() : base("*", AutoShowErrorMessageMode.None) { }

		private static ChannelFactory<T> _factory;
		private static ChannelFactory<T> Factory
		{
			get
			{
				if (_factory == null)
				{
					_factory = new ChannelFactory<T>("*");
				}
				return _factory;
			}
		}

		public static TResult Execute<TResult>(Func<T, TResult> remoteCall, out ICommunicationObject comObject)
		{
			DateTime start = DateTime.Now;
			if (remoteCall == null)
				throw new ArgumentNullException("RemoteCall");
			comObject = null;
			TResult retVal = default(TResult);
			ICommunicationObject comunicationObject = null;
			try
			{


				T currentChanel = Factory.CreateChannel();
				comunicationObject = currentChanel as ICommunicationObject;
				comObject = comunicationObject;
				using (new OperationContextScope(currentChanel as IContextChannel))
				{
					retVal = remoteCall(currentChanel);
					return retVal;
				}
			}
			catch (TimeoutException timeProblem)
			{
				//todo: timeout exception
			}
			catch (EndpointNotFoundException enfe)
			{
				//// enfe.Message.Contains("Could not connect") && enfe.Message.Contains("TCP error code"))
				//AbortCall(comunicationObject, AbortReasonEnum.TransportConnection, enfe.ToString());
				//// Clear the aborted state async
				//new Thread(new ThreadStart(AbortRecover)).Start();

				//todo: end point not found
			}
			catch (FaultException unknownFault)
			{
				var mf = unknownFault.CreateMessageFault();
			}
			catch (CommunicationException ce)
			{
				//AbortCall(comunicationObject, AbortReasonEnum.Communication, ce.ToString());
				//// Clear the aborted state async
				//new Thread(new ThreadStart(AbortRecover)).Start();
				//todo: communication exception
				var v = ce.ToString();
			}
			catch (Exception e)
			{
				var v = e.ToString();
				//AbortCall(comunicationObject, AbortReasonEnum.UnknownException, e.ToString());
				//// Clear the aborted state async
				//new Thread(new ThreadStart(AbortRecover)).Start();
				//todo: msg for unknown exception
			}
			finally
			{
				if (comunicationObject != null && comunicationObject.State != System.ServiceModel.CommunicationState.Faulted)
				{
					comunicationObject.Close();
				}
			}
			//IsAborted = false;
			//lock (ServiceExecutionContextManager.LockingObject)
			//    ServiceExecutionContextManager.IsAborted = false;

			return retVal;
		}

		public static TResult Execute<TResult>(Func<T, TResult> remoteCall)
		{
			ICommunicationObject comObj;
			return Execute(remoteCall, out comObj);
		}
	}
}
