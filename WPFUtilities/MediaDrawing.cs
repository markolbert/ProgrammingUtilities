#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'WPFViewModel' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System.Drawing;
using System.Runtime.CompilerServices;

namespace J4JSoftware.WPFUtilities
{
    // thanx to Eric Ouellet for this!
    // https://stackoverflow.com/questions/4662193/how-to-convert-from-system-drawing-color-to-system-windows-media-color/4662224
    public static class MediaDrawing
    {
        [ MethodImpl( MethodImplOptions.AggressiveInlining ) ]
        public static Color ToDrawingColor( this System.Windows.Media.Color mediaColor )
        {
            return Color.FromArgb( mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B );
        }

        /// <summary>
        ///     Convert Drawing Color (WPF) to Media Color (WinForm)
        /// </summary>
        /// <param name="drawingColor"></param>
        /// <returns></returns>
        [ MethodImpl( MethodImplOptions.AggressiveInlining ) ]
        public static System.Windows.Media.Color ToMediaColor( this Color drawingColor )
        {
            return System.Windows.Media.Color.FromArgb( drawingColor.A,
                                                       drawingColor.R,
                                                       drawingColor.G,
                                                       drawingColor.B );
        }
    }
}
