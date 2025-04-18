// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Core.Text;
using Snap.Hutao.UI.Xaml.Data.Converter;
using Snap.Hutao.Web.Endpoint.Hutao;

namespace Snap.Hutao.Model.Metadata.Converter;

internal sealed partial class GachaAvatarIconConverter : ValueConverter<string, Uri>, IIconNameToUriConverter
{
    public static Uri IconNameToUri(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return default!;
        }

        string icon = default!;
        Interpolated.Parse(name, $"UI_AvatarIcon_{icon}");
        return StaticResourcesEndpoints.StaticRaw("GachaAvatarIcon", $"UI_Gacha_AvatarIcon_{icon}.png").ToUri();
    }

    public override Uri Convert(string from)
    {
        return IconNameToUri(from);
    }
}