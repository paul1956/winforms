﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Drawing;

/// <summary>
///  The BufferedGraphics class can be thought of as a "Token" or "Reference" to the buffer that a
///  BufferedGraphicsContext creates. While a BufferedGraphics is outstanding, the memory associated with the
///  buffer is locked. The general design is such that under normal conditions a single BufferedGraphics will be in
///  use at one time for a given BufferedGraphicsContext.
/// </summary>
public sealed class BufferedGraphics : IDisposable
{
    private readonly Graphics? _targetGraphics;
    private readonly IntPtr _targetDC;
    private Graphics _bufferedGraphicsSurface;
    private BufferedGraphicsContext _context;
    private readonly Point _targetLoc;
    private readonly Size _virtualSize;

    /// <summary>
    ///  Internal constructor, this class is created by BufferedGraphicsContext.
    /// </summary>
    internal BufferedGraphics(Graphics bufferedGraphicsSurface, BufferedGraphicsContext context, Graphics? targetGraphics,
                              IntPtr targetDC, Point targetLoc, Size virtualSize)
    {
        _context = context;
        _bufferedGraphicsSurface = bufferedGraphicsSurface;
        _targetDC = targetDC;
        _targetGraphics = targetGraphics;
        _targetLoc = targetLoc;
        _virtualSize = virtualSize;
    }

    public void Dispose()
    {
        if (_context is not null)
        {
            _context.ReleaseBuffer();

            if (DisposeContext)
            {
                _context.Dispose();
                _context = null!;
            }
        }

        if (_bufferedGraphicsSurface is not null)
        {
            _bufferedGraphicsSurface.Dispose();
            _bufferedGraphicsSurface = null!;
        }
    }

    /// <summary>
    ///  Allows access to the Graphics wrapper for the buffer.
    /// </summary>
    public Graphics Graphics => _bufferedGraphicsSurface;

    /// <summary>
    ///  Renders the buffer to the specified target graphics.
    /// </summary>
    public void Render(Graphics? target)
    {
        if (target is not null)
        {
            IntPtr targetDC = target.GetHdc();

            try
            {
                RenderInternal(new HandleRef(target, targetDC));
            }
            finally
            {
                target.ReleaseHdcInternal(targetDC);
            }
        }
    }

    /// <summary>
    ///  Internal method that renders the specified buffer into the target.
    /// </summary>
    private void RenderInternal(HandleRef refTargetDC)
    {
        IntPtr sourceDC = Graphics.GetHdc();

        try
        {
            PInvokeCore.BitBlt(
                (HDC)refTargetDC.Handle,
                _targetLoc.X,
                _targetLoc.Y,
                _virtualSize.Width,
                _virtualSize.Height,
                (HDC)sourceDC,
                0,
                0,
                ROP_CODE.SRCCOPY);

            GC.KeepAlive(refTargetDC.Wrapper);
        }
        finally
        {
            Graphics.ReleaseHdcInternal(sourceDC);
        }
    }

    /// <summary>
    ///  Determines if we need to dispose of the Context when this is disposed.
    /// </summary>
    internal bool DisposeContext { get; set; }

    /// <summary>
    ///  Renders the buffer to the original graphics used to allocate the buffer.
    /// </summary>
    public void Render()
    {
        if (_targetGraphics is not null)
        {
            Render(_targetGraphics);
        }
        else
        {
            RenderInternal(new HandleRef(Graphics, _targetDC));
        }
    }

    /// <summary>
    ///  Renders the buffer to the specified target HDC.
    /// </summary>
    public void Render(IntPtr targetDC) => RenderInternal(new HandleRef(null, targetDC));
}
