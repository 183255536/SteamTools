// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.HttpServer/TcpReverseProxyHandler.cs

using Microsoft.AspNetCore.Connections;
using System.Net.Sockets;
using System.Net;
using System.IO.Pipelines;

namespace System.Application.Services.Implementation.HttpServer;

/// <summary>
/// TCP 反射代理处理者
/// </summary>
abstract class TcpReverseProxyHandler : ConnectionHandler
{
    private readonly IDomainResolver domainResolver;
    private readonly DnsEndPoint endPoint;
    private readonly TimeSpan connectTimeout = TimeSpan.FromSeconds(10d);

    public TcpReverseProxyHandler(IDomainResolver domainResolver, DnsEndPoint endPoint)
    {
        this.domainResolver = domainResolver;
        this.endPoint = endPoint;
    }

    /// <summary>
    /// TCP 连接后
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task OnConnectedAsync(ConnectionContext context)
    {
        var cancellationToken = context.ConnectionClosed;
        using var connection = await CreateConnectionAsync(cancellationToken);
        var task1 = connection.CopyToAsync(context.Transport.Output, cancellationToken);
        var task2 = context.Transport.Input.CopyToAsync(connection, cancellationToken);
        await Task.WhenAny(task1, task2);
    }

    /// <summary>
    /// 创建连接
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="AggregateException"></exception>
    private async Task<Stream> CreateConnectionAsync(CancellationToken cancellationToken)
    {
        var innerExceptions = new List<Exception>();
        await foreach (var address in domainResolver.ResolveAsync(endPoint, cancellationToken))
        {
            var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                using var timeoutTokenSource = new CancellationTokenSource(connectTimeout);
                using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutTokenSource.Token);
                await socket.ConnectAsync(address, endPoint.Port, linkedTokenSource.Token);
                return new NetworkStream(socket, ownsSocket: false);
            }
            catch (Exception ex)
            {
                socket.Dispose();
                cancellationToken.ThrowIfCancellationRequested();
                innerExceptions.Add(ex);
            }
        }
        throw new AggregateException(
            $"Unable to connect to {endPoint.Host}:{endPoint.Port}.", innerExceptions);
    }
}
