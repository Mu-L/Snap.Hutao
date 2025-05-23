// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI.Xaml.Controls;
using Snap.Hutao.Core.IO;
using Snap.Hutao.Core.Logging;
using Snap.Hutao.Factory.ContentDialog;
using Snap.Hutao.Factory.Picker;
using Snap.Hutao.Model.InterChange.GachaLog;
using Snap.Hutao.Service;
using Snap.Hutao.Service.GachaLog;
using Snap.Hutao.Service.Notification;
using Snap.Hutao.Service.UIGF;
using Snap.Hutao.UI.Xaml.View.Dialog;
using System.Collections.Immutable;

namespace Snap.Hutao.ViewModel.Setting;

[ConstructorGenerated]
[Injection(InjectAs.Scoped)]
internal sealed partial class SettingGachaLogViewModel : Abstraction.ViewModel
{
    private readonly IFileSystemPickerInteraction fileSystemPickerInteraction;
    private readonly IContentDialogFactory contentDialogFactory;
    private readonly IGachaLogRepository gachaLogRepository;
    private readonly JsonSerializerOptions jsonOptions;
    private readonly IServiceProvider serviceProvider;
    private readonly IInfoBarService infoBarService;
    private readonly IUIGFService uigfService;

    public partial AppOptions AppOptions { get; }

    [Command("ImportUIGFJsonCommand")]
    private async Task ImportUIGFJsonAsync()
    {
        SentrySdk.AddBreadcrumb(BreadcrumbFactory.CreateUI("Import UIGF file", "SettingGachaLogViewModel.Command"));

        (bool isOk, ValueFile file) = fileSystemPickerInteraction.PickFile(
            SH.ViewModelGachaUIGFImportPickerTitile,
            SH.ViewModelGachaLogExportFileType,
            "*.json");

        if (!isOk)
        {
            return;
        }

        ValueResult<bool, UIGF?> result = await file.DeserializeFromJsonAsync<UIGF>(jsonOptions).ConfigureAwait(false);
        if (!result.TryGetValue(out UIGF? uigf))
        {
            infoBarService.Error(SH.ViewModelImportWarningTitle, SH.ViewModelImportWarningMessage);
            return;
        }

        if (uigf.Hk4e.IsDefaultOrEmpty)
        {
            infoBarService.Warning(SH.ViewModelUIGFImportNoHk4eEntry);
            return;
        }

        if (uigf.Hk4e.Select(entry => entry.Uid).ToHashSet().Count != uigf.Hk4e.Length)
        {
            infoBarService.Warning(SH.ViewModelUIGFImportDuplicatedHk4eEntry);
            return;
        }

        UIGFImportDialog importDialog = await contentDialogFactory.CreateInstanceAsync<UIGFImportDialog>(serviceProvider, uigf).ConfigureAwait(false);
        (bool isOk2, HashSet<uint> uids) = await importDialog.GetSelectedUidsAsync().ConfigureAwait(false);
        if (!isOk2)
        {
            return;
        }

        if (uids is null or { Count: 0 })
        {
            infoBarService.Warning(SH.ViewModelUIGFImportNoSelectedEntry);
            return;
        }

        UIGFImportOptions options = new()
        {
            UIGF = uigf,
            GachaArchiveUids = uids,
        };

        ContentDialog dialog = await contentDialogFactory
            .CreateForIndeterminateProgressAsync(SH.ViewModelUIGFImportingProgressTitle)
            .ConfigureAwait(false);

        using (await contentDialogFactory.BlockAsync(dialog).ConfigureAwait(false))
        {
            try
            {
                await uigfService.ImportAsync(options).ConfigureAwait(false);
                infoBarService.Success(SH.ViewModelUIGFImportSuccess);
            }
            catch (Exception ex)
            {
                infoBarService.Error(ex, SH.ViewModelUIGFImportError);
            }
        }
    }

    [Command("ExportUIGFJsonCommand")]
    private async Task ExportUIGFJsonAsync()
    {
        SentrySdk.AddBreadcrumb(BreadcrumbFactory.CreateUI("Export UIGF file", "SettingGachaLogViewModel.Command"));

        (bool isOk, ValueFile file) = fileSystemPickerInteraction.SaveFile(
            SH.ViewModelGachaLogUIGFExportPickerTitle,
            $"Snap Hutao UIGF.json",
            SH.ViewModelGachaLogExportFileType,
            "*.json");

        if (!isOk)
        {
            return;
        }

        ImmutableArray<uint> allUids = gachaLogRepository.GetGachaArchiveUidImmutableArray().SelectAsArray(uint.Parse);
        UIGFExportDialog exportDialog = await contentDialogFactory.CreateInstanceAsync<UIGFExportDialog>(serviceProvider, allUids).ConfigureAwait(false);

        (bool isOk2, ImmutableArray<uint> uids) = await exportDialog.GetSelectedUidsAsync().ConfigureAwait(false);
        if (!isOk2)
        {
            return;
        }

        UIGFExportOptions options = new()
        {
            FilePath = file,
            GachaArchiveUids = uids,
        };

        try
        {
            await uigfService.ExportAsync(options).ConfigureAwait(false);
            infoBarService.Success(SH.ViewModelUIGFExportSuccess);
        }
        catch (Exception ex)
        {
            infoBarService.Error(ex, SH.ViewModelUIGFExportError);
        }
    }
}