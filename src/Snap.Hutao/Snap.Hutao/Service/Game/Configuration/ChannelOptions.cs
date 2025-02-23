// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Model.Intrinsic;

namespace Snap.Hutao.Service.Game.Configuration;

internal readonly struct ChannelOptions
{
    public const string ChannelName = "channel";
    public const string SubChannelName = "sub_channel";

    public readonly ChannelType Channel;

    public readonly SubChannelType SubChannel;

    public readonly bool IsOversea;

    public readonly ChannelOptionsErrorKind ErrorKind;

    public readonly string? FilePath;

    public ChannelOptions(ChannelType channel, SubChannelType subChannel, bool isOversea)
    {
        Channel = channel;
        SubChannel = subChannel;
        IsOversea = isOversea;
    }

    public ChannelOptions(string? channel, string? subChannel, bool isOversea)
    {
        _ = Enum.TryParse(channel, out Channel);
        _ = Enum.TryParse(subChannel, out SubChannel);
        IsOversea = isOversea;
    }

    private ChannelOptions(ChannelOptionsErrorKind errorKind, string? filePath)
    {
        ErrorKind = errorKind;
        FilePath = filePath;
    }

    public static ChannelOptions ConfigurationFileNotFound(string filePath)
    {
        return new(ChannelOptionsErrorKind.ConfigurationFileNotFound, filePath);
    }

    public static ChannelOptions GamePathNullOrEmpty()
    {
        return new(ChannelOptionsErrorKind.GamePathNullOrEmpty, string.Empty);
    }

    public static ChannelOptions GameContentCorrupted(string directory)
    {
        return new(ChannelOptionsErrorKind.GameContentCorrupted, directory);
    }

    public override string ToString()
    {
        return $$"""
            { ChannelType: {{Channel}}, SubChannel: {{SubChannel}}, IsOversea: {{IsOversea}}}
            """;
    }

    // DO NOT DELETE, used in HashSet
    public override int GetHashCode()
    {
        return HashCode.Combine(Channel, SubChannel, IsOversea);
    }
}