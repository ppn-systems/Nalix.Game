﻿using Nalix.Common.Connection;
using Nalix.Common.Package.Metadata;
using Nalix.Logging;
using Nalix.Network.Connection;
using Nalix.Network.Dispatch.Core;
using Nalix.Network.Package;
using Nalix.Network.Protocols;
using System;
using System.Threading;

namespace Nalix.Infrastructure.Network;

/// <summary>
/// Lớp `ServerProtocol` xử lý giao thức máy chủ, quản lý kết nối và xử lý dữ liệu.
/// </summary>
/// <param name="packetDispatcher">Bộ điều phối gói tin.</param>
public sealed class ServerProtocol(IPacketDispatch<Packet> packetDispatcher) : Protocol
{
    /// <summary>
    /// Bộ điều phối gói tin được sử dụng để xử lý dữ liệu nhận được.
    /// </summary>
    private readonly IPacketDispatch<Packet> _packetDispatcher = packetDispatcher;

    /// <summary>
    /// Xác định xem kết nối có được giữ mở liên tục hay không.
    /// </summary>
    public override Boolean KeepConnectionOpen => true;

    /// <summary>
    /// Xử lý sự kiện khi chấp nhận một kết nối mới.
    /// </summary>
    /// <param name="connection">Đối tượng kết nối mới.</param>
    /// <param name="cancellationToken">Token hủy kết nối.</param>
    public override void OnAccept(IConnection connection, CancellationToken cancellationToken = default)
    {
        base.OnAccept(connection, cancellationToken);

        // Thêm kết nối vào danh sách quản lý
        _ = ConnectionHub.Instance.RegisterConnection(connection);

        NLogix.Host.Instance.Debug($"[OnAccept] Connection accepted from {connection.RemoteEndPoint}");
    }

    public override void ProcessMessage(ReadOnlySpan<Byte> bytes)
    {
        IConnection connection = ConnectionHub.Instance.GetConnection(bytes[PacketSize.Header..sizeof(UInt32)]);

        try
        {
            NLogix.Host.Instance.Debug($"[ProcessMessage] Received packet from {connection.RemoteEndPoint}");
            _packetDispatcher.HandlePacket(bytes, connection);
            NLogix.Host.Instance.Debug($"[ProcessMessage] Successfully processed packet from {connection.RemoteEndPoint}");
        }
        catch (Exception ex)
        {
            NLogix.Host.Instance.Error($"[ProcessMessage] Error processing packet from {connection.RemoteEndPoint}: {ex}");
            connection.Disconnect();
        }
    }

    /// <summary>
    /// Xử lý tin nhắn nhận được từ kết nối.
    /// </summary>
    /// <param name="sender">Nguồn gửi tin nhắn.</param>
    /// <param name="args">Thông tin sự kiện kết nối.</param>
    public override void ProcessMessage(Object sender, IConnectEventArgs args)
    {
        try
        {
            NLogix.Host.Instance.Debug($"[ProcessMessage] Received packet from {args.Connection.RemoteEndPoint}");
            _packetDispatcher.HandlePacket(args.Connection.IncomingPacket, args.Connection);
            NLogix.Host.Instance.Debug($"[ProcessMessage] Successfully processed packet from {args.Connection.RemoteEndPoint}");
        }
        catch (Exception ex)
        {
            NLogix.Host.Instance.Error($"[ProcessMessage] Error processing packet from {args.Connection.RemoteEndPoint}: {ex}");
            args.Connection.Disconnect();
        }
    }

    /// <summary>
    /// Xử lý lỗi xảy ra trong quá trình kết nối.
    /// </summary>
    /// <param name="connection">Kết nối bị lỗi.</param>
    /// <param name="exception">Ngoại lệ xảy ra.</param>
    protected override void OnConnectionError(IConnection connection, Exception exception)
    {
        base.OnConnectionError(connection, exception);
        NLogix.Host.Instance.Error($"[OnConnectionError] Connection error with {connection.RemoteEndPoint}: {exception}");
    }
}