﻿namespace Nalix.CrossPlatform.Commands;

public enum Command : System.UInt16
{
    /// <summary>
    /// Lệnh để thiết lập kết nối ban đầu giữa máy khách và máy chủ.
    /// </summary>
    Handshake = 0x01,

    /// <summary>
    /// Lệnh để người chơi đăng nhập vào trò chơi.
    /// </summary>
    Login = 0x64,

    /// <summary>
    /// Lệnh để người chơi đăng xuất khỏi trò chơi.
    /// </summary>
    Logout = 0x65,

    /// <summary>
    /// Lệnh để người chơi đăng ký tài khoản mới.
    /// </summary>
    Register = 0x66,

    /// <summary>
    /// Lệnh để người chơi thay đổi mật khẩu tài khoản.
    /// </summary>
    ChangePassword = 0x67,
}