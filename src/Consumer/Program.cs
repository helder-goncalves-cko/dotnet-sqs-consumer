﻿using System;
using System.IO;
using System.Runtime.Loader;
using System.Threading;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Proto;
using Serilog;
using Consumer.Actors;
using StructureMap;
using Queueing.Configuration;
using Shared;
using Consumer.Dependencies;
using Queueing.Dependencies;
using Consumer.Messages;
using Consumer.Factories;

namespace Consumer
{
    class Program
    {
        private static Serilog.ILogger _logger;
        private static IConfiguration _configuration;
        private static AutoResetEvent _closing = new AutoResetEvent(false);
        private static CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        static void Main()
        {
            _configuration = BuildConfiguration();
            _logger = CreateLogger();

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            AssemblyLoadContext.Default.Unloading += OnShutdown;
            Console.CancelKeyPress += OnCancelKeyPress;

            try
            {
                _logger.Information("Starting Consumer. Press Ctrl+C to exit.");
                _logger.Debug(_configuration.Dump());

                var container = new Container(config =>
                {
                    config.AddRegistry(new AppRegistry(_configuration, _logger));
                    config.AddRegistry(new QueueingRegistry(_configuration));
                });

#if DEBUG
                container.AssertConfigurationIsValid();
#endif

                var actorFactory = container.GetInstance<IActorFactory>();
                var dequeuer = actorFactory.GetActor<Dequeuer>();
                dequeuer.Tell(new ReceiveCommands());

                _closing.WaitOne();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error starting Consumer.");
            }
            finally
            {
                Serilog.Log.CloseAndFlush();
            }
        }

        Serilog.ILogger ConfigureLogger()
        {
            var logger = new LoggerConfiguration()
                .WriteTo.ColoredConsole()
                .CreateLogger();

            return Serilog.Log.Logger = logger;
        }

        static void OnShutdown(AssemblyLoadContext context)
        {
            _logger.Information("Shutting down Consumer");
            _cancellationToken.Cancel();
            Serilog.Log.CloseAndFlush();
        }

        static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs args)
        {
            args.Cancel = true;
            _closing.Set();
            _cancellationToken.Cancel();
        }

        static void OnUnhandledException(object sender, UnhandledExceptionEventArgs ex)
        {
            Console.WriteLine(ex.ExceptionObject.ToString());
            Environment.Exit(1);
        }

        /// <summary>
        /// Creates a logger using the application configuration
        /// </summary>
        /// <param name="configuration">The configuration to read from</param>
        /// <returns>An logger instance</returns>
        static Serilog.ILogger CreateLogger()
        {
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(_configuration)
                .CreateLogger()
                .ForContext<Program>();

            return Serilog.Log.Logger = logger;
        }

        /// <summary>
        /// Builds a configuration from file and event variable sources
        /// </summary>
        /// <returns>The built configuration</returns>
        static IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .AddEnvironmentVariables(prefix: "Consumer_")
                .Build();
        }

        private static IAmazonSQS CreateAmazonSQSClient(QueueSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.Endpoint))
                return new AmazonSQSClient();

            // localstack usage
            var config = new AmazonSQSConfig { ServiceURL = settings.Endpoint };
            return new AmazonSQSClient(config);
        }
    }
}