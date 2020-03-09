// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace dnc
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    using Grpc.Core;
    using Grpc.Core.Logging;

    using Helloworld;

    class Program
    {
        const int InsecurePort = 12485;
        const int SecurePort = 22485;

        static async Task Main(string[] args)
        {
            Environment.SetEnvironmentVariable("GRPC_TRACE", "all,-timer,-timer_check");
            Environment.SetEnvironmentVariable("GRPC_VERBOSITY", "INFO");

            GrpcEnvironment.SetLogger(new ConsoleLogger());

            var server = new Server();
            server.Ports.Add("127.0.0.1", InsecurePort, ServerCredentials.Insecure);
            server.Ports.Add("127.0.0.1", SecurePort, MakeBadSslServerCredentials());
            server.Services.Add(Greeter.BindService(new GreeterImpl()));

            try
            {
                Console.WriteLine("Starting....");
                server.Start();
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Caught {ex}");
            }

            foreach (var p in server.Ports)
            {
                Console.WriteLine($"{p.Host} @ {p.Port} bound to {p.BoundPort}");
            }

            // Uncomment the following line to clean up the one bound port & see
            // the expected RpcException.

            //await server.ShutdownAsync();

            var channel = new Channel("127.0.0.1", InsecurePort, ChannelCredentials.Insecure);
            var client = new Greeter.GreeterClient(channel);

            try
            {
                Console.WriteLine("Making call. Ideally we'll get a RpcException with a connection failure.");
                var call = client.SayHelloAsync(new HelloRequest { Name = "This is the outgoing request name." });
                HelloReply reply = await call;
                Console.WriteLine($"Got: {reply.Message}");
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"Client RPC exception: {ex}");
            }
        }

        private static SslServerCredentials MakeBadSslServerCredentials()
        {
            var serverCert = new[] { new KeyCertificatePair("this is a bad certificate chain", "this is a bad private key") };
            return new SslServerCredentials(serverCert, "this is a bad root set", forceClientAuth: false);
        }
    }

    class GreeterImpl : Greeter.GreeterBase
    {
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            var reply = new HelloReply
            {
                Message = "'sup?",
            };

            return Task.FromResult(reply);
        }
    }

    class ConsoleLogger : ILogger
    {
        private Type _forType;

        private enum Level
        {
            Debug,
            Info,
            Warning,
            Error,
        }

        public ConsoleLogger()
        {
        }

        private ConsoleLogger(Type forType)
        {
            _forType = forType;
        }

        public void Debug(string message)
        {
            WriteLog(Level.Debug, "{0}", null, message);
        }

        public void Debug(string format, params object[] formatArgs)
        {
            WriteLog(Level.Debug, format, null, formatArgs);
        }

        public void Error(string message)
        {
            WriteLog(Level.Error, "{0}", null, message);
        }

        public void Error(string format, params object[] formatArgs)
        {
            WriteLog(Level.Error, format, null, formatArgs);
        }

        public void Error(Exception exception, string message)
        {
            WriteLog(Level.Error, "{0}", exception, message);
        }

        public ILogger ForType<T>()
        {
            return new ConsoleLogger(typeof(T));
        }

        public void Info(string message)
        {
            WriteLog(Level.Info, "{0}", null, message);
        }

        public void Info(string format, params object[] formatArgs)
        {
            WriteLog(Level.Info, format, null, formatArgs);
        }

        public void Warning(string message)
        {
            WriteLog(Level.Warning, "{0}", null, message);
        }

        public void Warning(string format, params object[] formatArgs)
        {
            WriteLog(Level.Warning, format, null, formatArgs);
        }

        public void Warning(Exception exception, string message)
        {
            WriteLog(Level.Warning, "{0}", exception, message);
        }

        private void WriteLog(Level level, string format, Exception ex, params object[] formatArgs)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0}: ", level);
            if (_forType != null)
            {
                sb.AppendFormat("{0}: ", _forType.Name);
            }

            sb.AppendFormat(format, formatArgs);

            if (ex != null)
            {
                sb.AppendFormat("ex={0}", ex);
            }

            Console.WriteLine(sb.ToString());
        }
    }
}
