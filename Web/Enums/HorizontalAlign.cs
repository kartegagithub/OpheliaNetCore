using System;
using System.Collections.Generic;
using System.Text;

namespace Ophelia.Web
{
    //
    // Summary:
    //     Specifies the horizontal alignment of items within a container.
    public enum HorizontalAlign
    {
        //
        // Summary:
        //     The horizontal alignment is not set.
        NotSet = 0,
        //
        // Summary:
        //     The contents of a container are left justified.
        Left = 1,
        //
        // Summary:
        //     The contents of a container are centered.
        Center = 2,
        //
        // Summary:
        //     The contents of a container are right justified.
        Right = 3,
        //
        // Summary:
        //     The contents of a container are uniformly spread out and aligned with both the
        //     left and right margins.
        Justify = 4
    }
}
