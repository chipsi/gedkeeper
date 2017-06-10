﻿/*
 *  "GEDKeeper", the personal genealogical database editor.
 *  Copyright (C) 2009-2017 by Sergey V. Zhdanovskih.
 *
 *  This file is part of "GEDKeeper".
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using Eto.Drawing;
using GKCommon;
using GKCore.Interfaces;

namespace GKUI.Components
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ColorHandler: TypeHandler<Color>, IColor
    {
        public ColorHandler(Color handle) : base(handle)
        {
        }

        public IColor Darker(float fraction)
        {
            Color color = this.Handle;
            float factor = (1.0f - fraction);

            int rgb = color.ToArgb();
            int red = (rgb >> 16) & 0xFF;
            int green = (rgb >> 8) & 0xFF;
            int blue = (rgb >> 0) & 0xFF;
            //int alpha = (rgb >> 24) & 0xFF;

            red = (int) (red * factor);
            green = (int) (green * factor);
            blue = (int) (blue * factor);

            red = (red < 0) ? 0 : red;
            green = (green < 0) ? 0 : green;
            blue = (blue < 0) ? 0 : blue;

            return new ColorHandler(Color.FromArgb(red, green, blue));
        }

        public IColor Lighter(float fraction)
        {
            Color color = this.Handle;
            float factor = (1.0f + fraction);

            int rgb = color.ToArgb();
            int red = (rgb >> 16) & 0xFF;
            int green = (rgb >> 8) & 0xFF;
            int blue = (rgb >> 0) & 0xFF;
            //int alpha = (rgb >> 24) & 0xFF;

            red = (int) (red * factor);
            green = (int) (green * factor);
            blue = (int) (blue * factor);

            if (red < 0) {
                red = 0;
            } else if (red > 255) {
                red = 255;
            }
            if (green < 0) {
                green = 0;
            } else if (green > 255) {
                green = 255;
            }
            if (blue < 0) {
                blue = 0;
            } else if (blue > 255) {
                blue = 255;
            }

            //int alpha = color.getAlpha();

            return new ColorHandler(Color.FromArgb(red, green, blue));
        }

        public string GetName()
        {
            Color color = this.Handle;
            return color.ToString(); // Name;
        }

        public int ToArgb()
        {
            int result = this.Handle.ToArgb();
            return result;
        }

        public string GetCode()
        {
            int argb = ToArgb() & 0xFFFFFF;
            string result = argb.ToString("X6");
            return result;
        }

        public byte GetR()
        {
            return (byte)Handle.Rb;
        }

        public byte GetG()
        {
            return (byte)Handle.Gb;
        }

        public byte GetB()
        {
            return (byte)Handle.Bb;
        }

        public bool IsTransparent()
        {
            return (Handle == Colors.Transparent);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public sealed class GfxPathHandler: TypeHandler<GraphicsPath>, IGfxPath
    {
        public GfxPathHandler(GraphicsPath handle) : base(handle)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                Handle.Dispose();
            }
            base.Dispose(disposing);
        }

        public void AddEllipse(float x, float y, float width, float height)
        {
            Handle.AddEllipse(x, y, width, height);
        }

        public void CloseFigure()
        {
            Handle.CloseFigure();
        }

        public void StartFigure()
        {
            Handle.StartFigure();
        }

        public bool IsVisible(float x, float y)
        {
            return false; //Handle.IsVisible(x, y);
        }

        public ExtRectF GetBounds()
        {
            RectangleF rect = Handle.Bounds;
            return ExtRectF.CreateBounds(rect.Left, rect.Top, rect.Width, rect.Height);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public sealed class PenHandler: TypeHandler<Pen>, IPen
    {
        public IColor Color
        {
            get { return UIHelper.ConvertColor(Handle.Color); }
        }

        public float Width
        {
            get { return Handle.Thickness; }
        }

        public PenHandler(Pen handle) : base(handle)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                Handle.Dispose();
            }
            base.Dispose(disposing);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public sealed class BrushHandler: TypeHandler<Brush>, IBrush
    {
        public IColor Color
        {
            get { return UIHelper.ConvertColor(((SolidBrush)Handle).Color); }
        }

        public BrushHandler(Brush handle) : base(handle)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                Handle.Dispose();
            }
            base.Dispose(disposing);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public sealed class FontHandler: TypeHandler<Font>, IFont
    {
        public string FontFamilyName
        {
            get { return Handle.FamilyName; }
        }

        public string Name
        {
            get { return Handle.FamilyName; }
        }

        public float Size
        {
            get { return Handle.Size; }
        }

        public FontHandler(Font handle) : base(handle)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                Handle.Dispose();
            }
            base.Dispose(disposing);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public sealed class ImageHandler: TypeHandler<Image>, IImage
    {
        public int Height
        {
            get { return Handle.Height; }
        }

        public int Width
        {
            get { return Handle.Width; }
        }

        public ImageHandler(Image handle) : base(handle)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                Handle.Dispose();
            }
            base.Dispose(disposing);
        }

        public byte[] GetBytes()
        {
            using (var stream = new MemoryStream())
            {
                ((Bitmap)Handle).Save(stream, ImageFormat.Bitmap);
                return stream.ToArray();
            }
        }
    }
}
