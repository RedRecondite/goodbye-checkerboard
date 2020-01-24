using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodbyeCheckerboard
{
    class Program
    {
        static void Main( string[] args )
        {
            // Search for all .bmp recursively from current working directory.
            foreach ( var lFile in Directory.EnumerateFiles( ".", "*.bmp", SearchOption.AllDirectories ) )
            {
                using ( var lOrig = new Bitmap( lFile ) )
                {
                    using ( var lBitmap = new Bitmap( lOrig.Width, lOrig.Height, PixelFormat.Format32bppArgb ) )
                    {
                        using ( var lGraphics = Graphics.FromImage( lBitmap ) )
                        {
                            lGraphics.DrawImage( lOrig, new Rectangle( 0, 0, lOrig.Width, lOrig.Height ) );
                        }

                        // Goal: Find a 'checkerboard'-like pixel (a solid pixel that has white pixels at top/bottom/left/right)
                        // Then, make it white as well.
                        if ( lBitmap.Height > 1 && lBitmap.Width > 1 )
                        {
                            for ( int y = 0; y < lBitmap.Height; y++ )
                            {
                                for ( int x = 0; x < lBitmap.Width; x++ )
                                {
                                    // Find a solid pixel
                                    if ( IsCheckerboardSolidShadowPixel( lBitmap, x, y ) )
                                    {
                                        // Make this pixel white
                                        lBitmap.SetPixel( x, y, Color.White );
                                    }
                                }
                            }
                        }

                        var lPngFile = Path.ChangeExtension( lFile, "png" );
                        Console.WriteLine( $"Saved: {lPngFile}" );
                        lBitmap.Save( lPngFile );
                    }
                }
            }
            return;
        }

        static bool IsCheckerboardSolidShadowPixel( Bitmap aBitmap, int x, int y )
        {
            bool lResult = false;
            // Find a black pixel
            var lPixel = aBitmap.GetPixel( x, y );
            if ( lPixel.ToArgb() == Color.Black.ToArgb() )
            {
                int? lTopPixel = null;
                if ( y != 0 ) lTopPixel = aBitmap.GetPixel( x, y - 1 ).ToArgb();
                bool lTopPixelAlpha = lTopPixel == Color.White.ToArgb();

                int? lLeftPixel = null;
                if ( x != 0 ) lLeftPixel = aBitmap.GetPixel( x - 1, y ).ToArgb();
                bool lLeftPixelAlpha = lLeftPixel == Color.White.ToArgb();

                int? lBottomPixel = null;
                if ( y != ( aBitmap.Height - 1 ) ) lBottomPixel = aBitmap.GetPixel( x, y + 1 ).ToArgb();
                bool lBottomPixelAlpha = lBottomPixel == Color.White.ToArgb();

                int? lRightPixel = null;
                if ( x != ( aBitmap.Width - 1 ) ) lRightPixel = aBitmap.GetPixel( x + 1, y ).ToArgb();
                bool lRightPixelAlpha = lRightPixel == Color.White.ToArgb();

                int lAdjacentAlphaPixelCount = 0;
                if ( lTopPixelAlpha )
                {
                    lAdjacentAlphaPixelCount += 1;
                }
                if ( lLeftPixelAlpha )
                {
                    lAdjacentAlphaPixelCount += 1;
                }
                if ( lBottomPixelAlpha )
                {
                    lAdjacentAlphaPixelCount += 1;
                }
                if ( lRightPixelAlpha )
                {
                    lAdjacentAlphaPixelCount += 1;
                }

                if ( lAdjacentAlphaPixelCount == 4 )
                {
                    // If we're surrounded by alpha pixels, that means that we're a shadow pixel!
                    lResult = true;
                }
                else if ( lAdjacentAlphaPixelCount == 3 )
                {
                    // Let's imagine where a shadow meets a sprite:
                    // P = Sprite
                    // X = Shadow
                    // 
                    //   ABCDEFGHIJKL
                    // 1   PPPPPPPPP
                    // 2  PPPPPPPPPP
                    // 3  X X X X X X
                    // 4 X X X X X X
                    //
                    // We would want all pixels on row 3 to be identified as shadow pixels.
                    // But, the 'P' pixels at B2 and K2 should not.
                    // 
                    // We can do this by checking to see if we have any kiddy-corner shadow pixels.
                    int? lTopLeftPixel = null;
                    if ( !( y == 0 || x == 0 ) ) lTopLeftPixel = aBitmap.GetPixel( x - 1, y - 1 ).ToArgb();
                    bool lTopLeftPixelAlpha = ( lTopLeftPixel == null || lTopLeftPixel == Color.White.ToArgb() );

                    int? lTopRightPixel = null;
                    if ( !( y == 0 || x == ( aBitmap.Width - 1 ) ) ) lTopRightPixel = aBitmap.GetPixel( x + 1, y - 1 ).ToArgb();
                    bool lTopRightPixelAlpha = ( lTopRightPixel == null || lTopRightPixel == Color.White.ToArgb() );

                    int? lBottomLeftPixel = null;
                    if ( !( y == ( aBitmap.Height - 1 ) || x == 0 ) ) lBottomLeftPixel = aBitmap.GetPixel( x - 1, y + 1 ).ToArgb();
                    bool lBottomLeftPixelAlpha = ( lBottomLeftPixel == null || lBottomLeftPixel == Color.White.ToArgb() );

                    int? lBottomRightPixel = null;
                    if ( !( y == ( aBitmap.Height - 1 ) || x == ( aBitmap.Width - 1 ) ) ) lBottomRightPixel = aBitmap.GetPixel( x + 1, y + 1 ).ToArgb();
                    bool lBottomRightPixelAlpha = ( lBottomRightPixel == null || lBottomRightPixel == Color.White.ToArgb() );

                    if ( lTopPixelAlpha && !lTopLeftPixelAlpha && !lTopRightPixelAlpha )
                    {
                        lResult = true;
                    }
                    else if ( lLeftPixelAlpha && !lTopLeftPixelAlpha && !lBottomLeftPixelAlpha )
                    {
                        lResult = true;
                    }
                    else if ( lRightPixelAlpha && !lTopRightPixelAlpha && !lBottomRightPixelAlpha )
                    {
                        lResult = true;
                    }
                    else if ( lBottomPixelAlpha && !lBottomLeftPixelAlpha && !lBottomRightPixelAlpha )
                    {
                        lResult = true;
                    }
                    // We're on an edge
                    else if ( y == 0 || y == aBitmap.Height - 1 || x == 0 || x == aBitmap.Width - 1 )
                    {
                        lResult = true;
                    }
                }
            }
            return lResult;
        }
    }
}
