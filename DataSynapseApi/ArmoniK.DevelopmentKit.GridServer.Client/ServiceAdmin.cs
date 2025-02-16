﻿using ArmoniK.Api.gRPC.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

//TODO : remove pragma
#pragma warning disable CS1591

namespace ArmoniK.DevelopmentKit.GridServer.Client
{
  public class ServiceAdmin : IDisposable
  {
    private static ServiceAdmin serviceAdmin_;
    public Session SessionId { get; set; }
    public Dictionary<string, Task> TaskWarehouse { get; set; }

    public ArmonikDataSynapseClientService ClientService { get; set; }

    public string ServiceType { get; set; }

    public ServiceAdmin(IConfiguration configuration, ILoggerFactory loggerFactory, Properties properties)
    {
      ClientService = new ArmonikDataSynapseClientService(loggerFactory,
                                                          properties);
      throw new NotImplementedException("Service Admin need to move into Poling agent");

      ServiceType = "ServiceAdmin";
    }

    public void UploadResources(string path)
    {
      //DataSynapsePayload payload = new() { ArmonikRequestType = ArmonikRequestType.Upload };
      //string             taskId  = ClientService.SubmitTask(payload.Serialize());

      //ClientService.WaitCompletion(taskId);
    }

    public void DeployResources()
    {
      throw new NotImplementedException();
    }

    public void DeleteResources()
    {
      throw new NotImplementedException();
    }

    public void DownloadResource(string path)
    {
      throw new NotImplementedException();
    }

    public IEnumerable<string> ListResources()
    {
      throw new NotImplementedException();
    }

    public void GetRegisteredServices()
    {
      throw new NotImplementedException();
    }

    public void RegisterService(string name)
    {
      throw new NotImplementedException();
    }

    public void UnRegisterService(string name)
    {
      throw new NotImplementedException();
    }

    public void GetServiceBinding(string name)
    {
      throw new NotImplementedException();
    }

    public void ResourceExists(string name)
    {
      throw new NotImplementedException();
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
    }

    public static ServiceAdmin CreateInstance(IConfiguration configuration, ILoggerFactory loggerFactory, Properties properties)
    {
      serviceAdmin_ ??= new ServiceAdmin(configuration,
                                         loggerFactory,
                                         properties);
      return serviceAdmin_;
    }

    public string UploadResource(string filepath)
    {
      return Guid.NewGuid().ToString();
    }
  }
}