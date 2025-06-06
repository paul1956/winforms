﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System;

/// <summary>
///  Scope for temporarily setting an <see cref="AppContext"/> switch. Use in a <see langword="using"/> statement.
/// </summary>
/// <remarks>
///  <para>
///   It is recommended to create wrappers for this struct for both simplicity and to allow adding synchronization.
///   See <see cref="BinaryFormatterScope"/> for an example of doing this.
///  </para>
/// </remarks>
public readonly ref struct AppContextSwitchScope
{
    private readonly string _switchName;
    private readonly bool _originalState;

    public AppContextSwitchScope(string switchName, bool enable)
        : this(switchName, getDefaultValue: null, enable)
    {
    }

    public AppContextSwitchScope(string switchName, Func<bool>? getDefaultValue, bool enable)
    {
        if (!AppContext.TryGetSwitch(AppContextSwitchNames.LocalAppContext_DisableCaching, out bool isEnabled)
            || !isEnabled)
        {
            // It doesn't make sense to try messing with AppContext switches if they are going to be cached.
            throw new InvalidOperationException("LocalAppContext switch caching is not disabled.");
        }

        // AppContext won't have any switch value until it is explicitly set.
        if (!AppContext.TryGetSwitch(switchName, out _originalState))
        {
            _originalState = getDefaultValue is not null && getDefaultValue();
        }

        AppContext.SetSwitch(switchName, enable);
        if (!AppContext.TryGetSwitch(switchName, out isEnabled) || isEnabled != enable)
        {
            throw new InvalidOperationException($"Could not set {switchName} to {enable}.");
        }

        _switchName = switchName;
    }

    public void Dispose()
    {
        AppContext.SetSwitch(_switchName, _originalState);
        if (!AppContext.TryGetSwitch(_switchName, out bool isEnabled) || isEnabled != _originalState)
        {
            throw new InvalidOperationException($"Could not reset {_switchName} to {_originalState}.");
        }
    }

    /// <summary>
    ///  Gets the default value for a switch via reflection.
    /// </summary>
    public static bool GetDefaultValueForSwitchInAssembly(string switchName, string assemblyName, string typeName)
    {
        Type type = Type.GetType($"{typeName}, {assemblyName}")
            ?? throw new InvalidOperationException($"Could not find {typeName} type in {assemblyName} assembly.");

        return type.TestAccessor().Dynamic.GetSwitchDefaultValue(switchName);
    }
}
