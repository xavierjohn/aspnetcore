﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Server.Kestrel.Networking;

namespace Microsoft.AspNet.Server.Kestrel.Http
{
    public class ConnectionContext : ListenerContext
    {
        public ConnectionContext()
        {
        }

        public ConnectionContext(ListenerContext context) : base(context)
        {
        }

        public ConnectionContext(ConnectionContext context) : base(context)
        {
            SocketInput = context.SocketInput;
            SocketOutput = context.SocketOutput;
            ConnectionControl = context.ConnectionControl;
        }

        public SocketInput SocketInput { get; set; }
        public ISocketOutput SocketOutput { get; set; }

        public IConnectionControl ConnectionControl { get; set; }
    }

    public interface IConnectionControl
    {
        void Pause();
        void Resume();
        void End(ProduceEndType endType);
    }

    public class Connection : ConnectionContext, IConnectionControl
    {
        private static readonly Action<UvStreamHandle, int, object> _readCallback = ReadCallback;
        private static readonly Func<UvStreamHandle, int, object, Libuv.uv_buf_t> _allocCallback = AllocCallback;

        private static Libuv.uv_buf_t AllocCallback(UvStreamHandle handle, int suggestedSize, object state)
        {
            return ((Connection)state).OnAlloc(handle, suggestedSize);
        }

        private static void ReadCallback(UvStreamHandle handle, int nread, object state)
        {
            ((Connection)state).OnRead(handle, nread);
        }

        private readonly UvStreamHandle _socket;
        private Frame _frame;

        public Connection(ListenerContext context, UvStreamHandle socket) : base(context)
        {
            _socket = socket;
            ConnectionControl = this;
        }

        public void Start()
        {
            SocketInput = new SocketInput(Memory);
            SocketOutput = new SocketOutput(Thread, _socket);
            _frame = new Frame(this);
            _socket.ReadStart(_allocCallback, _readCallback, this);
        }

        private Libuv.uv_buf_t OnAlloc(UvStreamHandle handle, int suggestedSize)
        {
            return new Libuv.uv_buf_t
            {
                memory = SocketInput.Pin(2048),
                len = 2048
            };
        }

        private void OnRead(UvStreamHandle handle, int nread)
        {
            SocketInput.Unpin(nread);

            if (nread == 0)
            {
                SocketInput.RemoteIntakeFin = true;
            }

            _frame.Consume();
        }

        void IConnectionControl.Pause()
        {
            _socket.ReadStop();
        }

        void IConnectionControl.Resume()
        {
            _socket.ReadStart(_allocCallback, _readCallback, this);
        }

        void IConnectionControl.End(ProduceEndType endType)
        {
            switch (endType)
            {
                case ProduceEndType.SocketShutdownSend:
                    Thread.Post(
                        x =>
                        {
                            var self = (Connection)x;
                            var shutdown = new UvShutdownReq();
                            shutdown.Init(self.Thread.Loop);
                            shutdown.Shutdown(self._socket, (req, status, state) => req.Dispose(), null);
                        },
                        this);
                    break;
                case ProduceEndType.ConnectionKeepAlive:
                    _frame = new Frame(this);
                    Thread.Post(
                        x => ((Frame)x).Consume(),
                        _frame);
                    break;
                case ProduceEndType.SocketDisconnect:
                    Thread.Post(
                        x => ((UvHandle)x).Dispose(),
                        _socket);
                    break;
            }
        }
    }
}
