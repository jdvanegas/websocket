﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;

namespace WebSocket
{
  public class Startup
  {
    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddLogging(builder =>
      {
        builder.AddConsole().AddDebug()
        .AddFilter<ConsoleLoggerProvider>(null, LogLevel.Debug)
        .AddFilter<DebugLoggerProvider>(null, LogLevel.Debug);
      });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      var webSocketOptions = new WebSocketOptions()
      {
        KeepAliveInterval = TimeSpan.FromSeconds(120),
        ReceiveBufferSize = 4 * 1024
      };

      app.UseWebSockets(webSocketOptions);

      app.Use(async (context, next) =>
      {
        if (context.Request.Path == "/ws")
        {
          if (context.WebSockets.IsWebSocketRequest)
          {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await Echo(context, webSocket);
          }
          else
            context.Response.StatusCode = 400;
        }
        else
          await next();
      });

      app.UseFileServer();
      //app.Run(async (context) =>
      //{
      //  await context.Response.WriteAsync("WebSocket Application!");
      //});
    }

    private async Task Echo(HttpContext context, System.Net.WebSockets.WebSocket webSocket)
    {
      var buffer = new byte[1024 * 4];
      var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
      while (!result.CloseStatus.HasValue)
      {
        await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
      }

      await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }
  }
}
