// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.IO;

namespace Snap.Hutao.Core.IO;

internal static class FileOperation
{
    public static void Copy(string source, string destination, bool overwrite)
    {
        try
        {
            File.Copy(source, destination, overwrite);
        }
        catch (IOException ex)
        {
            // ERROR_ENCRYPTION_FAILED
            if (ex.HResult is unchecked((int)0x80071770))
            {
                FileSystem.CopyFileAllowDecryptedDestination(Path.GetFullPath(source), Path.GetFullPath(destination), overwrite);
            }
            else
            {
                throw;
            }
        }
    }

    public static bool Move(string sourceFileName, string destFileName, bool overwrite)
    {
        if (!File.Exists(sourceFileName))
        {
            return false;
        }

        if (overwrite)
        {
            try
            {
                File.Move(sourceFileName, destFileName, true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        if (File.Exists(destFileName))
        {
            return false;
        }

        try
        {
            File.Move(sourceFileName, destFileName, false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool Delete(string path)
    {
        if (!File.Exists(path))
        {
            return false;
        }

        File.Delete(path);
        return true;
    }

    public static void UnsafeDelete(string path)
    {
        FileSystem.DeleteItem(path);
    }

    public static void UnsafeMove(string sourceFileName, string destFileName)
    {
        string? destFolder = Path.GetDirectoryName(destFileName);
        ArgumentException.ThrowIfNullOrEmpty(destFolder);
        string fileName = Path.GetFileName(destFileName);
        ArgumentException.ThrowIfNullOrEmpty(fileName);
        FileSystem.MoveItem(sourceFileName, destFolder, fileName);
    }
}