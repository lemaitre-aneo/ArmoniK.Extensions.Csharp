// This file is part of the ArmoniK project
// 
// Copyright (C) ANEO, 2021-2022. All rights reserved.
//   W. Kirschenmann   <wkirschenmann@aneo.fr>
//   J. Gurhem         <jgurhem@aneo.fr>
//   D. Dubuc          <ddubuc@aneo.fr>
//   L. Ziane Khodja   <lzianekhodja@aneo.fr>
//   F. Lemaitre       <flemaitre@aneo.fr>
//   S. Djebbar        <sdjebbar@aneo.fr>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using ArmoniK.DevelopmentKit.Common;
using ArmoniK.DevelopmentKit.Common.Exceptions;
using ArmoniK.DevelopmentKit.SymphonyApi;
using ArmoniK.DevelopmentKit.SymphonyApi.api;
using ArmoniK.DevelopmentKit.WorkerApi.Common;
using ArmoniK.EndToEndTests.Common;

using Microsoft.Extensions.Logging;

namespace ArmoniK.EndToEndTests.Tests.SimpleComputeNSubtasking
{
  public class ServiceContainer : ServiceContainerBase
  {
    public override void OnCreateService(ServiceContext serviceContext)
    {
      //END USER PLEASE FIXME
    }

    public override void OnSessionEnter(SessionContext sessionContext)
    {
      //END USER PLEASE FIXME
    }

    public byte[] ComputeSquare(TaskContext taskContext, ClientPayload clientPayload)
    {
      Logger.LogInformation($"Enter in function : ComputeSquare with taskId {taskContext.TaskId}");

      if (clientPayload.Numbers.Count == 0)
        return new ClientPayload
          {
            Type   = ClientPayload.TaskType.Result,
            Result = 0,
          }
          .Serialize(); // Nothing to do

      if (clientPayload.Numbers.Count == 1)
      {
        var value = clientPayload.Numbers[0] * clientPayload.Numbers[0];
        Logger.LogInformation($"Compute {value}             with taskId {taskContext.TaskId}");

        return new ClientPayload
          {
            Type   = ClientPayload.TaskType.Result,
            Result = value,
          }
          .Serialize();
      }
      else // if (clientPayload.numbers.Count > 1)
      {
        var value  = clientPayload.Numbers[0];
        var square = value * value;

        var subTaskPayload = new ClientPayload();
        clientPayload.Numbers.RemoveAt(0);
        subTaskPayload.Numbers = clientPayload.Numbers;
        subTaskPayload.Type    = clientPayload.Type;
        Logger.LogInformation($"Compute {value} in                 {taskContext.TaskId}");

        Logger.LogInformation($"Submitting subTask from task          : {taskContext.TaskId} from Session {SessionId}");
        var subTaskId = this.SubmitTask(subTaskPayload.Serialize());
        Logger.LogInformation($"Submitted  subTask                    : {subTaskId} with ParentTask {this.TaskId}");

        ClientPayload aggPayload = new()
        {
          Type   = ClientPayload.TaskType.Aggregation,
          Result = square,
        };

        Logger.LogInformation($"Submitting aggregate task             : {taskContext.TaskId} from Session {SessionId}");

        var aggTaskId = this.SubmitTaskWithDependencies(aggPayload.Serialize(),
                                                        new[] { subTaskId },
                                                        true);

        Logger.LogInformation($"Submitted  SubmitTaskWithDependencies : {aggTaskId} with task dependencies      {subTaskId}");

        //return new ClientPayload
        //  {
        //    Type      = ClientPayload.TaskType.Aggregation,
        //    SubTaskId = aggTaskId,
        //  }
        //  .Serialize(); //nothing to do
        return null;
      }
    }

    private void Job_of_N_Tasks(byte[] payload, int nbTasks)
    {
      var payloads = new List<byte[]>(nbTasks);
      for (var i = 0; i < nbTasks; i++)
        payloads.Add(payload);

      var sw      = Stopwatch.StartNew();
      var taskIds = SubmitTasks(payloads);

      ClientPayload aggPayload = new()
      {
        Type = ClientPayload.TaskType.AggregationNTask,
      };
      ;
      this.SubmitTaskWithDependencies(aggPayload.Serialize(),
                                      taskIds.ToList());

      var elapsedMilliseconds = sw.ElapsedMilliseconds;
      Logger.LogInformation($"Server called {nbTasks} tasks in {elapsedMilliseconds} ms");
    }


    public byte[] ComputeCube(TaskContext taskContext, ClientPayload clientPayload)
    {
      var value = clientPayload.Numbers[0] * clientPayload.Numbers[0] * clientPayload.Numbers[0];
      return new ClientPayload
        {
          Type   = ClientPayload.TaskType.Result,
          Result = value,
        }
        .Serialize(); //nothing to do
    }

    public override byte[] OnInvoke(SessionContext sessionContext, TaskContext taskContext)
    {
      var clientPayload = ClientPayload.Deserialize(taskContext.TaskInput);

      if (clientPayload.Type == ClientPayload.TaskType.ComputeSquare)
        return ComputeSquare(taskContext,
                             clientPayload);

      if (clientPayload.Type == ClientPayload.TaskType.ComputeCube)
      {
        return ComputeCube(taskContext,
                           clientPayload);
      }

      if (clientPayload.Type == ClientPayload.TaskType.Sleep)
      {
        Logger.LogInformation($"Empty task, sessionId : {sessionContext.SessionId}, taskId : {taskContext.TaskId}, sessionId from task : {taskContext.SessionId}");
        Thread.Sleep(clientPayload.Sleep * 1000);
      }
      else if (clientPayload.Type == ClientPayload.TaskType.JobOfNTasks)
      {
        var newPayload = new ClientPayload
        {
          Type  = ClientPayload.TaskType.Sleep,
          Sleep = clientPayload.Sleep,
        };

        var bytePayload = newPayload.Serialize();

        Job_of_N_Tasks(bytePayload,
                       clientPayload.Numbers[0] - 1);

        return new ClientPayload
          {
            Type   = ClientPayload.TaskType.Result,
            Result = 42,
          }
          .Serialize(); //nothing to do
      }
      else if (clientPayload.Type == ClientPayload.TaskType.AggregationNTask)
      {
        return AggregateValuesNTasks(taskContext,
                                     clientPayload);
      }
      else if (clientPayload.Type == ClientPayload.TaskType.Aggregation)
      {
        return AggregateValues(taskContext,
                               clientPayload);
      }
      else
      {
        Logger.LogInformation($"Task type is unManaged {clientPayload.Type}");
        throw new WorkerApiException($"Task type is unManaged {clientPayload.Type}");
      }

      return new ClientPayload
        {
          Type   = ClientPayload.TaskType.Result,
          Result = 42,
        }
        .Serialize(); //nothing to do
    }

    private byte[] AggregateValuesNTasks(TaskContext taskContext, ClientPayload clientPayload)
    {
      var finalResult = 0;

      foreach (var pair in taskContext.DataDependencies)
      {
        var taskResult = pair.Value;
        finalResult += BitConverter.ToInt32(taskResult,
                                            0);
      }

      ClientPayload childResult = new()
      {
        Type   = ClientPayload.TaskType.Result,
        Result = finalResult,
      };

      return childResult.Serialize();
    }

    private byte[] AggregateValues(TaskContext taskContext, ClientPayload clientPayload)
    {
      Logger.LogInformation($"Aggregate Task {taskContext.TaskId} request result from Dependencies TaskIds : [{string.Join(", ", taskContext.DependenciesTaskIds)}]");
      var parentResult = taskContext.DataDependencies?.Single().Value;

      if (parentResult == null || parentResult.Length == 0)
        throw new WorkerApiException($"Cannot retrieve Result from taskId {taskContext.DependenciesTaskIds?.Single()}");

      var parentResultPayload = ClientPayload.Deserialize(parentResult);
      //if (parentResultPayload.SubTaskId != null)
      //{
      //  //parentResult        = GetResult(parentResultPayload.SubTaskId);
      //  //parentResultPayload = ClientPayload.Deserialize(parentResult);
      //  throw new WorkerApiException($"There should received a new SubTask here");
      //}

      var value = clientPayload.Result + parentResultPayload.Result;

      ClientPayload childResult = new()
      {
        Type   = ClientPayload.TaskType.Result,
        Result = value,
      };

      return childResult.Serialize();
    }

    public override void OnSessionLeave(SessionContext sessionContext)
    {
      //END USER PLEASE FIXME
    }

    public override void OnDestroyService(ServiceContext serviceContext)
    {
      //END USER PLEASE FIXME
    }
  }
}