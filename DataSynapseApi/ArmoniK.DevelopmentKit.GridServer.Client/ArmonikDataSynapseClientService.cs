﻿#if NET5_0_OR_GREATER
using Grpc.Net.Client;
#else
using Grpc.Core;
#endif
using System;
using System.Collections.Generic;

using ArmoniK.Api.gRPC.V1;
using ArmoniK.DevelopmentKit.Common;

using Google.Protobuf.WellKnownTypes;

using Microsoft.Extensions.Logging;

namespace ArmoniK.DevelopmentKit.GridServer.Client
{
  /// <summary>
  ///   The main object to communicate with the control Plane from the client side
  ///   The class will connect to the control plane to createSession, SubmitTask,
  ///   Wait for result and get the result.
  ///   See an example in the project ArmoniK.Samples in the sub project
  ///   https://github.com/aneoconsulting/ArmoniK.Samples/tree/main/Samples/GridServerLike
  ///   Samples.ArmoniK.Sample.SymphonyClient
  /// </summary>
  [MarkDownDoc]
  public class ArmonikDataSynapseClientService
  {
    private readonly Properties properties_;
    private ILogger<ArmonikDataSynapseClientService> Logger { get; set; }
    private Submitter.SubmitterClient ControlPlaneService { get; set; }


    /// <summary>
    /// Set or Get TaskOptions with inside MaxDuration, Priority, AppName, VersionName and AppNamespace
    /// </summary>
    private TaskOptions TaskOptions { get; set; }

    private ILoggerFactory LoggerFactory { get; set; }

    /// <summary>
    /// The ctor with IConfiguration and optional TaskOptions
    /// 
    /// </summary>
    /// <param name="loggerFactory">The factory to create the logger for clientService</param>
    /// <param name="properties">Properties containing TaskOption and connection string to the control plane</param>
    public ArmonikDataSynapseClientService(ILoggerFactory loggerFactory, Properties properties)
    {
      properties_   = properties;
      LoggerFactory = loggerFactory;
      Logger        = loggerFactory.CreateLogger<ArmonikDataSynapseClientService>();

      TaskOptions = properties_.TaskOptions;
    }

    /// <summary>
    /// Create the session to submit task
    /// </summary>
    /// <param name="taskOptions">Optional parameter to set TaskOptions during the Session creation</param>
    /// <returns></returns>
    public SessionService CreateSession(TaskOptions taskOptions = null)
    {
      if (taskOptions != null) TaskOptions = taskOptions;

      ControlPlaneConnection();

      Logger.LogDebug("Creating Session... ");

      return new SessionService(LoggerFactory,
                                ControlPlaneService,
                                TaskOptions);
    }

    private void ControlPlaneConnection()
    {
#if NET5_0_OR_GREATER
      var channel = GrpcChannel.ForAddress(properties_.ConnectionString);
#else
      Environment.SetEnvironmentVariable("GRPC_DNS_RESOLVER",
                                         "native");
      var uri = new Uri(properties_.ConnectionString);
      var channel = new Channel($"{uri.Host}:{uri.Port}",
                                ChannelCredentials.Insecure);
#endif
      ControlPlaneService ??= new Submitter.SubmitterClient(channel);
    }

    /// <summary>
    /// Set connection to an already opened Session
    /// </summary>
    /// <param name="sessionId">SessionId previously opened</param>
    /// <param name="clientOptions"></param>
    public SessionService OpenSession(string sessionId, IDictionary<string, string> clientOptions = null)
    {
      ControlPlaneConnection();

      return new SessionService(LoggerFactory,
                                ControlPlaneService,
                                new Session()
                                {
                                  Id = sessionId,
                                },
                                clientOptions);
    }

    /// <summary>
    /// This method is creating a default taskOptions initialization where
    /// MaxDuration is 40 seconds, MaxRetries = 2 The app name is ArmoniK.DevelopmentKit.GridServer
    /// The version is 1.0.0 the namespace ArmoniK.DevelopmentKit.GridServer and simple service FallBackServerAdder 
    /// </summary>
    /// <returns>Return the default taskOptions</returns>
    public static TaskOptions InitializeDefaultTaskOptions()
    {
      TaskOptions taskOptions = new()
      {
        MaxDuration = new Duration
        {
          Seconds = 40,
        },
        MaxRetries = 2,
        Priority   = 1,
      };

      taskOptions.Options.Add(AppsOptions.EngineTypeNameKey,
                              EngineType.DataSynapse.ToString());

      taskOptions.Options.Add(AppsOptions.GridAppNameKey,
                              "ArmoniK.DevelopmentKit.GridServer");

      taskOptions.Options.Add(AppsOptions.GridAppVersionKey,
                              "1.X.X");

      taskOptions.Options.Add(AppsOptions.GridAppNamespaceKey,
                              "ArmoniK.DevelopmentKit.GridServer");

      taskOptions.Options.Add(AppsOptions.GridServiceNameKey,
                              "FallBackServerAdder");

      return taskOptions;
    }
  }
}