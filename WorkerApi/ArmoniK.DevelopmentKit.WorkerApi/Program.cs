// This file is part of the ArmoniK project
// 
// Copyright (C) ANEO, 2021-2021. All rights reserved.
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

using System.IO;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Events;

namespace ArmoniK.DevelopmentKit.WorkerApi
{
  public class Program
  {
    private static readonly string SocketPath = "/cache/armonik.sock";

    public static void Main(string[] args)
    {
      CreateHostBuilder(args).Build().Run();
    }

    // Additional configuration is required to successfully run gRPC on macOS.
    // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
    public static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
          .UseSerilog((context, services, configuration) => configuration
                                                            .ReadFrom.Configuration(context.Configuration)
                                                            .ReadFrom.Services(services)
                                                            .MinimumLevel
                                                            .Override("Microsoft.AspNetCore",
                                                                      LogEventLevel.Debug)
                                                            .Enrich.FromLogContext())
          .ConfigureWebHostDefaults(webBuilder =>
          {
            webBuilder.UseStartup<Startup>()
                      .ConfigureKestrel(options =>
                      {
                        options.Limits.MaxRequestBodySize = 2097152000;
                        if (File.Exists(SocketPath))
                        {
                          File.Delete(SocketPath);
                        }

                        options.ListenUnixSocket(SocketPath,
                                                 listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; });
                      });
          });
  }
}
