﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MK94.SeeRaw
{
	public abstract class RendererBase
	{
		protected JsonSerializerOptions jsonOptions;
		protected Serializer serializer = new Serializer();

		private Context previousContext;

        protected RendererBase()
        {
			jsonOptions = new JsonSerializerOptions();
			jsonOptions.Converters.Add(new JsonStringEnumConverter());

		}

		public abstract object OnClientConnected(Server server, WebSocket websocket);
		public abstract void OnMessageReceived(object state, Server server, WebSocket websocket, string message);

		protected void ExecuteCallback(Server server, RenderRoot renderRoot, Dictionary<string, Delegate> callbacks, string message)
		{
			var deserialized = JsonSerializer.Deserialize<JsonElement>(message);

			var id = deserialized.GetProperty("id").GetString();
			var type = deserialized.GetProperty("type").GetString();

			if (callbacks.TryGetValue(id, out var @delegate))
			{
				if (type == "link" && @delegate is Action a)
					a();

				else if (type == "form")
				{
					var jsonArgs = deserialized.GetProperty("args");
					var parameters = @delegate.Method.GetParameters();
					var deserializedArgs = new List<object>();

					for (int i = 0; i < parameters.Length; i++)
					{
						// Hacky way to deserialize until https://github.com/dotnet/runtime/issues/31274 is implemented
						var jsonArg = JsonSerializer.Deserialize(jsonArgs[i].GetRawText(), parameters[i].ParameterType, jsonOptions);

						deserializedArgs.Add(jsonArg);
					}

					SetContext(server, renderRoot);
					@delegate.DynamicInvoke(deserializedArgs.ToArray());
					ResetContext();
				}
				else UnknownMessage();
			}
			else UnknownMessage();

			void UnknownMessage()
			{
#if DEBUG
				Console.WriteLine("Unknown message " + message);
#endif
			}
		}

		protected void SetContext(Server server, RenderRoot renderRoot)
        {
			previousContext = SeeRawDefault.localSeeRawContext.Value;

			SeeRawDefault.localSeeRawContext.Value = new Context
			{
				server = server,
				RenderRoot = renderRoot
			};
        }

		protected void ResetContext()
        {
			SeeRawDefault.localSeeRawContext.Value = previousContext;
			previousContext = null;
		}

		public RendererBase WithSerializer<T>(ISerialize serializer) => WithSerializer(typeof(T), serializer);

		public RendererBase WithSerializer(Type type, ISerialize serializer)
		{
			this.serializer.serializers[type] = serializer;
			return this;
		}
	}

	public class Renderer : RendererBase
	{
		private Server server;
		private RenderRoot state;
		private Dictionary<string, Delegate> callbacks = new Dictionary<string, Delegate>();

		public Renderer(Server server, bool setGlobalContext)
		{
			this.server = server;
			state = new RenderRoot(r => Refresh());

			if (setGlobalContext)
				SetContext(server, state);
		}

		public override object OnClientConnected(Server server, WebSocket websocket) { Refresh(); return null; }
		public override void OnMessageReceived(object state, Server server, WebSocket websocket, string message) => ExecuteCallback(server, this.state, callbacks, message);

		public T Render<T>(T o)
		{
			return Render(o, out _);
		}

		public T Render<T>(T o, out RenderTarget target)
		{
			target = new RenderTarget(Refresh, o);

			state.Targets.Add(target);

			return o;
		}

		public void Refresh()
		{
			callbacks.Clear();
			server.Broadcast(serializer.SerializeState(state, callbacks));
		}
    }

	public class PerClientRenderer : RendererBase
	{
		private class ClientState
		{
			public ClientState(RenderRoot state, Dictionary<string, Delegate> callbacks)
			{
				State = state;
				Callbacks = callbacks;
			}

			internal RenderRoot State { get; }

			internal Dictionary<string, Delegate> Callbacks { get; }


		}

		private readonly Action onClientConnected;

        public PerClientRenderer(Action onClientConnected)
		{
			this.onClientConnected = onClientConnected;
		}

        public override object OnClientConnected(Server server, WebSocket websocket)
        {
			var callbacks = new Dictionary<string, Delegate>();
			var renderRoot = new RenderRoot(r =>
			{
				var message = serializer.SerializeState(r, callbacks);
				Task.Run(() => websocket.SendAsync(message, WebSocketMessageType.Text, true, default));
			});

			var state = new ClientState(renderRoot, callbacks);

			SetContext(server, state.State);
			onClientConnected();
			ResetContext();

			return state;
        }

        public override void OnMessageReceived(object state, Server server, WebSocket websocket, string message)
        {
			var clientState = state as ClientState;

			ExecuteCallback(server, clientState.State, clientState.Callbacks, message);
        }
	}
}
