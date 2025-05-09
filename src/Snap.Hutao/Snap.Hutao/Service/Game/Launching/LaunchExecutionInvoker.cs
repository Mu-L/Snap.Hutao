// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using CommunityToolkit.Mvvm.Messaging;
using Snap.Hutao.Core;
using Snap.Hutao.Core.ExceptionService;
using Snap.Hutao.Service.Game.Launching.Handler;
using System.Collections.Concurrent;

namespace Snap.Hutao.Service.Game.Launching;

internal abstract class LaunchExecutionInvoker
{
    private static readonly ConcurrentDictionary<LaunchExecutionInvoker, Void> Invokers = [];

    private bool invoked;

    protected Queue<ILaunchExecutionDelegateHandler> Handlers { get; } = [];

    public static bool IsAnyLaunchExecutionInvoking()
    {
        return !Invokers.IsEmpty;
    }

    public async ValueTask<LaunchExecutionResult> InvokeAsync(LaunchExecutionContext context)
    {
        HutaoException.ThrowIf(Interlocked.Exchange(ref invoked, true), "The invoker has been invoked");

        try
        {
            Invokers.TryAdd(this, default);
            context.ServiceProvider.GetRequiredService<IMessenger>().Send(new LaunchExecutionGameFileSystemExclusiveAccessChangedMessage(false));
            await RecursiveInvokeHandlerAsync(context, 0).ConfigureAwait(false);
            return context.Result;
        }
        finally
        {
            await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);

            Invokers.TryRemove(this, out _);
            if (Invokers.IsEmpty)
            {
                unsafe
                {
                    SpinWaitPolyfill.SpinWhile(&LaunchExecutionEnsureGameNotRunningHandler.IsGameRunning);
                }

                context.ServiceProvider.GetRequiredService<IMessenger>().Send<LaunchExecutionProcessStatusChangedMessage>();
                context.ServiceProvider.GetRequiredService<IMessenger>().Send(new LaunchExecutionGameFileSystemExclusiveAccessChangedMessage(true));
            }
        }
    }

    private async ValueTask RecursiveInvokeHandlerAsync(LaunchExecutionContext context, int index)
    {
        if (Handlers.TryDequeue(out ILaunchExecutionDelegateHandler? handler))
        {
            string typeName = TypeNameHelper.GetTypeDisplayName(handler, false);
            context.Logger.LogInformation("Handler {Index} [{Handler}] begin execution", index, typeName);
            await handler.OnExecutionAsync(context, () => RecursiveInvokeHandlerAsync(context, index + 1)).ConfigureAwait(false);
            context.Logger.LogInformation("Handler {Index} [{Handler}] end execution", index, typeName);
        }
    }
}