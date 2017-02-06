using System;
using System.Reflection;
using System.ServiceModel;
using System.Windows.Threading;
using System.Linq.Expressions;
using System.ServiceModel.Channels;
using System.Collections.Generic;

namespace Shared.Helpers
{
	public class WCFClient<TServiceContract> where TServiceContract : class
	{
		const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod;

		#region CurrentDispatcher

		static Dispatcher currentDispatcher = Dispatcher.CurrentDispatcher;
		public Dispatcher CurrentDispatcher { get { return currentDispatcher; } }

		#endregion

		public delegate void LogoutNeeded();
		public static event LogoutNeeded OnLogoutNeeded;

		string endpointConfigurationName;
		//WCFClientSession clientSession;
		ChannelFactory<TServiceContract> channelFactory;
		AutoShowErrorMessageMode autoShowErrorMessage;


		public WCFClient(string endpointConfigurationName, AutoShowErrorMessageMode autoShowErrorMessage)
		{
			this.autoShowErrorMessage = autoShowErrorMessage;
			//this.clientSession = WCFClientSession.Singleton;
			this.endpointConfigurationName = endpointConfigurationName;
			CreateChannelFactory();
		}

		private void CreateChannelFactory()
		{
			this.channelFactory = new ChannelFactory<TServiceContract>(endpointConfigurationName);
			//channelFactory.Endpoint.Behaviors.Add(new CryptoMessageEndpointBehavior());
		}

		//public void Execute(Expression<Func<TServiceContract, IAsyncResult>> operationCallExpression, EventHandler<ClientEventArgs> callback, bool cancelPreviousCallbacks = false)
		//{
		//	ExecuteCore(operationCallExpression, callback, cancelPreviousCallbacks);
		//}


		public TServiceContract CreateChannel()
		{
			//clientSession.SetCredentials(channelFactory);
			var channel = channelFactory.CreateChannel();
			return channel;
		}

		public void SetUserNameCredentials(string username, string password)
		{
			//if (channelFactory.Credentials.UserName.UserName != null)
			//	CreateChannelFactory();
			//clientSession.SetCredentials(channelFactory, username, password);
		}

		public TResult ExecuteSync<TResult>(Func<TServiceContract, TResult> remoteCall)
		{
			if (remoteCall == null)
				throw new ArgumentNullException("remoteCall");

			TServiceContract currentChanel = CreateChannel();
			try
			{
				using (CreateOperationContextScope(currentChanel))
				{
					//OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader("TokenKey", string.Empty, clientSession.SecurityToken ?? string.Empty));
					return remoteCall(currentChanel);
				}
			}
			finally
			{
				if ((currentChanel as ICommunicationObject) != null)
					((ICommunicationObject)currentChanel).Close();
			}
		}

		Dictionary<string, int> currentCallIDs = new Dictionary<string, int>();
		private int RegisterAsyncCall(MethodInfo call, Delegate callback)
		{
			int callID;
			string key = string.Concat(call, callback.Method, callback.Target != null ? callback.Target.GetHashCode() : 0);
			lock (currentCallIDs)
			{
				if (currentCallIDs.TryGetValue(key, out callID))
					currentCallIDs[key] = ++callID;
				else
					currentCallIDs.Add(key, callID = 1);
			}
			return callID;
		}
		private bool IsLastAsyncCall(MethodInfo call, Delegate callback, int callID)
		{
			string key = string.Concat(call, callback.Method, callback.Target != null ? callback.Target.GetHashCode() : 0);
			lock (currentCallIDs)
				return currentCallIDs[key] == callID;
		}

		private object ExecuteCore(Expression<Func<TServiceContract, IAsyncResult>> operationCallExpression, EventHandler<ClientEventArgs> callback, bool cancelPreviousCallbacks)
		{
			var methodCall = (MethodCallExpression)operationCallExpression.Body;
			int callID = RegisterAsyncCall(methodCall.Method, callback);
			int argumentsCount = methodCall.Arguments.Count;

			object[] parameterArray = new object[argumentsCount];
			for (int i = 0; i < argumentsCount - 2; i++)
				parameterArray[i] = Expression.Lambda(methodCall.Arguments[i]).Compile().DynamicInvoke();

			AsyncCallback directCallback = OnCallback;
			ConstantExpression funcCallBack = (ConstantExpression)methodCall.Arguments[argumentsCount - 2];
			if (funcCallBack.Value != null)
				directCallback += (AsyncCallback)funcCallBack.Value;

			var channel = CreateChannel();
			using (CreateOperationContextScope(channel))
			{
				//OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader("TokenKey", string.Empty, clientSession.SecurityToken ?? string.Empty));

				parameterArray[argumentsCount - 2] = directCallback;
				parameterArray[argumentsCount - 1] = new ObjectClientState
				{
					CallID = callID,
					Channel = channel,
					Callback = callback,
					BeginMethod = methodCall.Method,
					CancelPreviousCallbacks = cancelPreviousCallbacks,
					UserState = Expression.Lambda(methodCall.Arguments[argumentsCount - 1]).Compile().DynamicInvoke(),
				};

				return
					typeof(TServiceContract).InvokeMember(
					methodCall.Method.Name,
					bindingFlags, null,
					channel, parameterArray);
			}
		}

		private void OnCallback(IAsyncResult result)
		{
			ObjectClientState state = (ObjectClientState)result.AsyncState;
			ClientEventArgs eventArgs = CreateClientEventArgs();
			eventArgs.UserState = state;
			eventArgs.IsLastCallback = IsLastAsyncCall(state.BeginMethod, state.Callback, state.CallID);

			// отвергаем колбэк, если используется синхронизация И этот колбэк не по последнему вызову
			if (state.CancelPreviousCallbacks && !eventArgs.IsLastCallback)
				return;


			try
			{
				eventArgs.Object =
					typeof(TServiceContract).InvokeMember(
					"End" + state.BeginMethod.Name.Substring(5),
					bindingFlags, null,
					state.Channel, new object[] { result });
			}
			catch (TargetInvocationException ex)
			{
				eventArgs.Error = ex.InnerException ?? ex;
			}

			FaultException authEx = eventArgs.Error as FaultException;
			if (authEx != null)
			{
				string authExMsg = authEx.Message.ToLower();
				if (authExMsg.Contains("not authoriz") || authExMsg.Contains("не авторизова"))
				{
					if (OnLogoutNeeded != null)
					{
						OnLogoutNeeded();
						return;
					}
				}
			}

			if (state.Callback != null)
			{
				currentDispatcher.BeginInvoke((Action)delegate
				{
					if (autoShowErrorMessage == AutoShowErrorMessageMode.BeforeCallback)
						eventArgs.ShowErrorMessage();

					state.Callback(this, eventArgs);

					if (autoShowErrorMessage == AutoShowErrorMessageMode.AfterCallback)
						eventArgs.ShowErrorMessage();
				});
			}
		}

		protected virtual ClientEventArgs CreateClientEventArgs()
		{
			return new ClientEventArgs { };
		}

		protected virtual OperationContextScope CreateOperationContextScope(TServiceContract currentChanel)
		{
			//if (OperationContext.Current != null)
			//	return new OperationContextScope(OperationContext.Current);
			//else
			return new OperationContextScope((IContextChannel)currentChanel);
		}
	}

	public enum AutoShowErrorMessageMode { None, BeforeCallback, AfterCallback, }

	public class ClientEventArgs : EventArgs
	{
		public ClientEventArgs() { }
		public ClientEventArgs(object value) { this.Object = value; }

		public Exception Error { get; set; }
		public object Object { get; set; }
		public object UserState { get; set; }
		public bool IsLastCallback { get; set; }

		public TResult GetResult<TResult>()
		{
			if (this.Object is TResult)
				return (TResult)this.Object;
			return default(TResult);
		}
		public TResult GetUserState<TResult>()
		{
			if (this.UserState is TResult)
				return (TResult)this.UserState;
			else if (this.UserState is ObjectClientState)
				return (TResult)(((ObjectClientState)this.UserState).UserState);
			return default(TResult);
		}

		public void ShowErrorMessage() { ShowErrorMessage(null, null); }
		public void ShowErrorMessage(string title) { ShowErrorMessage(title, null); }
		public void ShowErrorMessage(string title, string messageFormat)
		{
			if (Error != null)
				ShowErrorMessageCore(title, messageFormat);
		}

		protected virtual void ShowErrorMessageCore(string title, string messageFormat) { }
	}

	internal class ObjectClientState
	{
		public EventHandler<ClientEventArgs> Callback;
		public MethodInfo BeginMethod;
		public object UserState;
		public object Channel;
		public int CallID;
		public bool CancelPreviousCallbacks;
	}
}
