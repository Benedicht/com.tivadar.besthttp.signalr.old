using System;

using Best.HTTP.Shared;
using Best.HTTP.Shared.Logger;
using Best.SignalR.Messages;
using Best.WebSockets;

namespace Best.SignalR.Transports
{
    public sealed class WebSocketTransport : TransportBase
    {
        #region Overridden Properties

        public override bool SupportsKeepAlive { get { return true; } }
        public override TransportTypes Type { get { return TransportTypes.WebSocket; } }

        #endregion

        private WebSocket wSocket;

        public WebSocketTransport(Connection connection)
            : base("webSockets", connection)
        {
        }

        #region Overrides from TransportBase

        /// <summary>
        /// Websocket transport specific connection logic. It will create a WebSocket instance, and starts to connect to the server.
        /// </summary>
        public override void Connect()
        {
            if (wSocket != null)
            {
                HTTPManager.Logger.Warning("WebSocketTransport", "Start - WebSocket already created!");
                return;
            }

            // Skip the Connecting state if we are reconnecting. If the connect succeeds, we will set the Started state directly
            if (this.State != TransportStates.Reconnecting)
                this.State = TransportStates.Connecting;

            RequestTypes requestType = this.State == TransportStates.Reconnecting ? RequestTypes.Reconnect : RequestTypes.Connect;

            Uri uri = Connection.BuildUri(requestType, this);

            // Create the WebSocket instance
            wSocket = new WebSocket(uri);

            // Set up eventhandlers
            wSocket.OnOpen += WSocket_OnOpen;
            wSocket.OnMessage += WSocket_OnMessage;
            wSocket.OnClosed += WSocket_OnClosed;

#if !UNITY_WEBGL || UNITY_EDITOR
            // prepare the internal http request
            wSocket.OnInternalRequestCreated = (ws, internalRequest) => Connection.PrepareRequest(internalRequest, requestType);
#endif

            // start opening the websocket protocol
            wSocket.Open();
        }

        protected override void SendImpl(string json)
        {
            if (wSocket != null && wSocket.IsOpen)
                wSocket.Send(json);
        }

        public override void Stop()
        {
            if (wSocket != null)
            {
                wSocket.OnOpen = null;
                wSocket.OnMessage = null;
                wSocket.OnClosed = null;
                wSocket.Close();
                wSocket = null;
            }
        }

        protected override void Started()
        {
            // Nothing to be done here for this transport
        }

        /// <summary>
        /// The /abort request successfully finished
        /// </summary>
        protected override void Aborted()
        {
            // if the websocket is still open, close it
            if (wSocket != null && wSocket.IsOpen)
            {
                wSocket.Close();
                wSocket = null;
            }
        }

#endregion

#region WebSocket Events

        void WSocket_OnOpen(WebSocket webSocket)
        {
            if (webSocket != wSocket)
                return;

            HTTPManager.Logger.Information("WebSocketTransport", "WSocket_OnOpen");

            OnConnected();
        }

        void WSocket_OnMessage(WebSocket webSocket, string message)
        {
            if (webSocket != wSocket)
                return;

            IServerMessage msg = TransportBase.Parse(Connection.JsonEncoder, message);

            if (msg != null)
                Connection.OnMessage(msg);
        }

        void WSocket_OnClosed(WebSocket webSocket, WebSocketStatusCodes code, string message)
        {
            if (webSocket != wSocket)
                return;

            string reason = $"{code} : {message}";

            HTTPManager.Logger.Information("WebSocketTransport", "WSocket_OnClosed " + reason);

            if (this.State == TransportStates.Closing)
                this.State = TransportStates.Closed;
            else
                Connection.Error(reason);
        }

#endregion
    }
}
