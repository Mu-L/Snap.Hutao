﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI.Xaml;

namespace Snap.Hutao.View.Helper;

internal interface IDeferContentLoader
{
    DependencyObject? Load(string name);

    void Unload(DependencyObject @object);
}