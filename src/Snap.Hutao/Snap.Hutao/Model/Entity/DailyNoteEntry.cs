// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using CommunityToolkit.Mvvm.ComponentModel;
using Snap.Hutao.Model.Entity.Abstraction;
using Snap.Hutao.ViewModel.DailyNote;
using Snap.Hutao.ViewModel.User;
using Snap.Hutao.Web.Hoyolab.Takumi.Binding;
using Snap.Hutao.Web.Hoyolab.Takumi.GameRecord.DailyNote;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Snap.Hutao.Model.Entity;

[Table("daily_notes")]
internal sealed partial class DailyNoteEntry : ObservableObject, IAppDbEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid InnerId { get; set; }

    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = default!;

    public string Uid { get; set; } = default!;

    public DailyNote? DailyNote { get; set; }

    public DateTimeOffset RefreshTime { get; set; }

    public int ResinNotifyThreshold { get; set; }

    public bool ResinNotifySuppressed { get; set; }

    public int HomeCoinNotifyThreshold { get; set; }

    public bool HomeCoinNotifySuppressed { get; set; }

    public bool TransformerNotify { get; set; }

    public bool TransformerNotifySuppressed { get; set; }

    public bool DailyTaskNotify { get; set; }

    public bool DailyTaskNotifySuppressed { get; set; }

    public bool ExpeditionNotify { get; set; }

    public bool ExpeditionNotifySuppressed { get; set; }

    [NotMapped]
    public UserGameRole? UserGameRole { get; set; }

    [NotMapped]
    public DailyNoteArchonQuestView? ArchonQuestView { get; set; }

    [NotMapped]
    public string RefreshTimeFormatted
    {
        get
        {
            return RefreshTime == default
                ? SH.ModelEntityDailyNoteNotRefreshed
                : SH.FormatModelEntityDailyNoteRefreshTimeFormat(RefreshTime.ToLocalTime());
        }
    }

    public static DailyNoteEntry From(UserAndUid userAndUid)
    {
        return new()
        {
            UserId = userAndUid.User.InnerId,
            Uid = userAndUid.Uid.Value,
            ResinNotifyThreshold = 120,
            HomeCoinNotifyThreshold = 1800,
        };
    }

    public void Update(DailyNote? dailyNote)
    {
        DailyNote = dailyNote;
        OnPropertyChanged(nameof(DailyNote));

        RefreshTime = DateTimeOffset.UtcNow;
        OnPropertyChanged(nameof(RefreshTimeFormatted));
    }

    public void CopyTo(DailyNoteEntry other)
    {
        other.Update(DailyNote);

        other.ResinNotifySuppressed = ResinNotifySuppressed;
        other.OnPropertyChanged(nameof(ResinNotifySuppressed));

        other.HomeCoinNotifySuppressed = HomeCoinNotifySuppressed;
        other.OnPropertyChanged(nameof(HomeCoinNotifySuppressed));

        other.TransformerNotifySuppressed = TransformerNotifySuppressed;
        other.OnPropertyChanged(nameof(TransformerNotifySuppressed));

        other.DailyTaskNotifySuppressed = DailyTaskNotifySuppressed;
        other.OnPropertyChanged(nameof(DailyTaskNotifySuppressed));

        other.ExpeditionNotifySuppressed = ExpeditionNotifySuppressed;
        other.OnPropertyChanged(nameof(ExpeditionNotifySuppressed));
    }
}