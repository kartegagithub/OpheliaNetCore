using System;
using System.Collections.Generic;
using System.Text;

namespace Ophelia.Web.UI
{
    //
    // Summary:
    //     Specifies the HTML tags that can be passed to an System.Web.UI.TextWriter
    //     or System.Web.UI.Html32TextWriter object output stream.
    public enum TextWriterTag
    {
        //
        // Summary:
        //     The string passed as an HTML tag is not recognized.
        Unknown = 0,
        //
        // Summary:
        //     The HTML a element.
        A = 1,
        //
        // Summary:
        //     The HTML acronym element.
        Acronym = 2,
        //
        // Summary:
        //     The HTML address element.
        Address = 3,
        //
        // Summary:
        //     The HTML area element.
        Area = 4,
        //
        // Summary:
        //     The HTML b element.
        B = 5,
        //
        // Summary:
        //     The HTML base element.
        Base = 6,
        //
        // Summary:
        //     The HTML basefont element.
        Basefont = 7,
        //
        // Summary:
        //     The HTML bdo element.
        Bdo = 8,
        //
        // Summary:
        //     The HTML bgsound element.
        Bgsound = 9,
        //
        // Summary:
        //     The HTML big element.
        Big = 10,
        //
        // Summary:
        //     The HTML blockquote element.
        Blockquote = 11,
        //
        // Summary:
        //     The HTML body element.
        Body = 12,
        //
        // Summary:
        //     The HTML br element.
        Br = 13,
        //
        // Summary:
        //     The HTML button element.
        Button = 14,
        //
        // Summary:
        //     The HTML caption element.
        Caption = 15,
        //
        // Summary:
        //     The HTML center element.
        Center = 16,
        //
        // Summary:
        //     The HTML cite element.
        Cite = 17,
        //
        // Summary:
        //     The HTML code element.
        Code = 18,
        //
        // Summary:
        //     The HTML col element.
        Col = 19,
        //
        // Summary:
        //     The HTML colgroup element.
        Colgroup = 20,
        //
        // Summary:
        //     The HTML dd element.
        Dd = 21,
        //
        // Summary:
        //     The HTML del element.
        Del = 22,
        //
        // Summary:
        //     The HTML dfn element.
        Dfn = 23,
        //
        // Summary:
        //     The HTML dir element.
        Dir = 24,
        //
        // Summary:
        //     The HTML div element.
        Div = 25,
        //
        // Summary:
        //     The HTML dl element.
        Dl = 26,
        //
        // Summary:
        //     The HTML dt element.
        Dt = 27,
        //
        // Summary:
        //     The HTML em element.
        Em = 28,
        //
        // Summary:
        //     The HTML embed element.
        Embed = 29,
        //
        // Summary:
        //     The HTML fieldset element.
        Fieldset = 30,
        //
        // Summary:
        //     The HTML font element.
        Font = 31,
        //
        // Summary:
        //     The HTML form element.
        Form = 32,
        //
        // Summary:
        //     The HTML frame element.
        Frame = 33,
        //
        // Summary:
        //     The HTML frameset element.
        Frameset = 34,
        //
        // Summary:
        //     The HTML H1 element.
        H1 = 35,
        //
        // Summary:
        //     The HTML H2 element.
        H2 = 36,
        //
        // Summary:
        //     The HTML H3 element.
        H3 = 37,
        //
        // Summary:
        //     The HTML H4 element.
        H4 = 38,
        //
        // Summary:
        //     The HTML H5 element.
        H5 = 39,
        //
        // Summary:
        //     The HTML H6 element.
        H6 = 40,
        //
        // Summary:
        //     The HTML head element.
        Head = 41,
        //
        // Summary:
        //     The HTML hr element.
        Hr = 42,
        //
        // Summary:
        //     The HTML html element.
        Html = 43,
        //
        // Summary:
        //     The HTML i element.
        I = 44,
        //
        // Summary:
        //     The HTML iframe element.
        Iframe = 45,
        //
        // Summary:
        //     The HTML img element.
        Img = 46,
        //
        // Summary:
        //     The HTML input element.
        Input = 47,
        //
        // Summary:
        //     The HTML ins element.
        Ins = 48,
        //
        // Summary:
        //     The HTML isindex element.
        Isindex = 49,
        //
        // Summary:
        //     The HTML kbd element.
        Kbd = 50,
        //
        // Summary:
        //     The HTML label element.
        Label = 51,
        //
        // Summary:
        //     The HTML legend element.
        Legend = 52,
        //
        // Summary:
        //     The HTML li element.
        Li = 53,
        //
        // Summary:
        //     The HTML link element.
        Link = 54,
        //
        // Summary:
        //     The HTML map element.
        Map = 55,
        //
        // Summary:
        //     The HTML marquee element.
        Marquee = 56,
        //
        // Summary:
        //     The HTML menu element.
        Menu = 57,
        //
        // Summary:
        //     The HTML meta element.
        Meta = 58,
        //
        // Summary:
        //     The HTML nobr element.
        Nobr = 59,
        //
        // Summary:
        //     The HTML noframes element.
        Noframes = 60,
        //
        // Summary:
        //     The HTML noscript element.
        Noscript = 61,
        //
        // Summary:
        //     The HTML object element.
        Object = 62,
        //
        // Summary:
        //     The HTML ol element.
        Ol = 63,
        //
        // Summary:
        //     The HTML option element.
        Option = 64,
        //
        // Summary:
        //     The HTML p element.
        P = 65,
        //
        // Summary:
        //     The HTML param element.
        Param = 66,
        //
        // Summary:
        //     The HTML pre element.
        Pre = 67,
        //
        // Summary:
        //     The HTML q element.
        Q = 68,
        //
        // Summary:
        //     The DHTML rt element, which specifies text for the ruby element.
        Rt = 69,
        //
        // Summary:
        //     The DHTML ruby element.
        Ruby = 70,
        //
        // Summary:
        //     The HTML s element.
        S = 71,
        //
        // Summary:
        //     The HTML samp element.
        Samp = 72,
        //
        // Summary:
        //     The HTML script element.
        Script = 73,
        //
        // Summary:
        //     The HTML select element.
        Select = 74,
        //
        // Summary:
        //     The HTML small element.
        Small = 75,
        //
        // Summary:
        //     The HTML span element.
        Span = 76,
        //
        // Summary:
        //     The HTML strike element.
        Strike = 77,
        //
        // Summary:
        //     The HTML strong element.
        Strong = 78,
        //
        // Summary:
        //     The HTML style element.
        Style = 79,
        //
        // Summary:
        //     The HTML sub element.
        Sub = 80,
        //
        // Summary:
        //     The HTML sup element.
        Sup = 81,
        //
        // Summary:
        //     The HTML table element.
        Table = 82,
        //
        // Summary:
        //     The HTML tbody element.
        Tbody = 83,
        //
        // Summary:
        //     The HTML td element.
        Td = 84,
        //
        // Summary:
        //     The HTML textarea element.
        Textarea = 85,
        //
        // Summary:
        //     The HTML tfoot element.
        Tfoot = 86,
        //
        // Summary:
        //     The HTML th element.
        Th = 87,
        //
        // Summary:
        //     The HTML thead element.
        Thead = 88,
        //
        // Summary:
        //     The HTML title element.
        Title = 89,
        //
        // Summary:
        //     The HTML tr element.
        Tr = 90,
        //
        // Summary:
        //     The HTML tt element.
        Tt = 91,
        //
        // Summary:
        //     The HTML u element.
        U = 92,
        //
        // Summary:
        //     The HTML ul element.
        Ul = 93,
        //
        // Summary:
        //     The HTML var element.
        Var = 94,
        //
        // Summary:
        //     The HTML wbr element.
        Wbr = 95,
        //
        // Summary:
        //     The HTML xml element.
        Xml = 96
    }
}
