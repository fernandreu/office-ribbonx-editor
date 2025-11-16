using System.Drawing;
using ScintillaNET;

namespace OfficeRibbonXEditor.Lexers;

public class VbaLexer : ScintillaLexer
{
    private const string Keywords = 
        """
        debug
        release
        addhandler
        addressof
        aggregate
        alias
        and
        andalso
        ansi
        as
        assembly
        auto
        binary
        boolean
        byref
        byte
        byval
        call
        case
        catch
        cbool
        cbyte
        cchar
        cdate
        cdbl
        cdec
        char
        cint
        class
        clng
        cobj
        compare
        const
        continue
        csbyte
        cshort
        csng
        cstr
        ctype
        cuint
        culng
        cushort
        custom
        date
        decimal
        declare
        default
        delegate
        dim
        directcast
        distinct
        do
        double
        each
        else
        elseif
        end
        endif
        enum
        equals
        erase
        error
        event
        exit
        explicit
        false
        finally
        for
        friend
        from
        function
        get
        gettype
        getxmlnamespace
        global
        gosub
        goto
        group
        handles
        if
        implements
        imports
        in
        inherits
        integer
        interface
        into
        is
        isfalse
        isnot
        istrue
        join
        key
        let
        lib
        like
        long
        loop
        me
        mid
        mod
        module
        mustinherit
        mustoverride
        my
        mybase
        myclass
        namespace
        narrowing
        new
        next
        not
        nothing
        notinheritable
        notoverridable
        object
        of
        off
        on
        operator
        option
        optional
        or
        order
        orelse
        overloads
        overridable
        overrides
        paramarray
        partial
        preserve
        private
        property
        protected
        public
        raiseevent
        readonly
        redim
        rem
        removehandler
        resume
        return
        sbyte
        select
        set
        shadows
        shared
        short
        single
        skip
        static
        step
        stop
        strict
        string
        structure
        sub
        synclock
        take
        text
        then
        throw
        to
        true
        try
        trycast
        typeof
        uinteger
        ulong
        unicode
        until
        ushort
        using
        variant
        wend
        when
        where
        while
        widening
        with
        withevents
        writeonly
        xor
        """;

    private const string Literals =
        """
        !
        #
        %
        @
        &amp;
        i
        d
        f
        l
        r
        s
        ui
        ul
        us
        """;

    private const string Other = "_";
        
    protected override void UpdateImplementation()
    {
        if (Editor == null)
        {
            return;
        }

        var editor = Editor;
            
        editor.TabWidth = Properties.Settings.Default.TabWidth;
        editor.WrapMode = Properties.Settings.Default.WrapMode;
        editor.ViewEol = Properties.Settings.Default.ShowWhitespace;
        editor.ViewWhitespace = Properties.Settings.Default.ShowWhitespace ? WhitespaceMode.VisibleAlways : WhitespaceMode.Invisible;

        // Set the VB Lexer
        editor.Lexer = Lexer.Vb;

        // Enable folding
        editor.SetProperty("fold", "1");
        editor.SetProperty("fold.compact", "1");
        editor.SetProperty("fold.html", "1");

        // Use Margin 2 for fold markers
        editor.Margins[2].Type = MarginType.Symbol;
        editor.Margins[2].Mask = Marker.MaskFolders;
        editor.Margins[2].Sensitive = true;
        editor.Margins[2].Width = 20;

        // Reset folder markers
        for (int i = Marker.FolderEnd; i <= Marker.FolderOpen; i++)
        {
            editor.Markers[i].SetForeColor(SystemColors.ControlLightLight);
            editor.Markers[i].SetBackColor(SystemColors.ControlDark);
        }

        // Style the folder markers
        editor.Markers[Marker.Folder].Symbol = MarkerSymbol.BoxPlus;
        editor.Markers[Marker.Folder].SetBackColor(SystemColors.ControlText);
        editor.Markers[Marker.FolderOpen].Symbol = MarkerSymbol.BoxMinus;
        editor.Markers[Marker.FolderEnd].Symbol = MarkerSymbol.BoxPlusConnected;
        editor.Markers[Marker.FolderEnd].SetBackColor(SystemColors.ControlText);
        editor.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
        editor.Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.BoxMinusConnected;
        editor.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
        editor.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

        // Enable automatic folding
        editor.AutomaticFold = AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change;

        // Set the Styles
        editor.StyleResetDefault();

        editor.Styles[Style.Default].Font = "Consolas";
        editor.Styles[Style.Default].Size = Properties.Settings.Default.EditorFontSize;
        editor.Styles[Style.Default].ForeColor = Color.Black;
        editor.Styles[Style.Default].BackColor = Color.White;
        editor.StyleClearAll();
            
        editor.Styles[Style.Vb.BinNumber].ForeColor = Color.OrangeRed;
        editor.Styles[Style.Vb.Comment].ForeColor = Color.Green;
        editor.Styles[Style.Vb.CommentBlock].ForeColor = Color.Green;
        editor.Styles[Style.Vb.Constant].ForeColor = Color.DarkMagenta;
        editor.Styles[Style.Vb.Date].ForeColor = Color.DarkMagenta;
        editor.Styles[Style.Vb.Default].ForeColor = Color.Black;
        editor.Styles[Style.Vb.DocBlock].ForeColor = Color.Green;
        editor.Styles[Style.Vb.DocKeyword].ForeColor = Color.FromArgb(255, 64, 47, 241);
        editor.Styles[Style.Vb.DocLine].ForeColor = Color.Green;
        editor.Styles[Style.Vb.Error].ForeColor = Color.DarkRed;
        editor.Styles[Style.Vb.Error].Bold = true;
        editor.Styles[Style.Vb.HexNumber].ForeColor = Color.DarkOrange;
        editor.Styles[Style.Vb.Identifier].ForeColor = Color.Black;
        editor.Styles[Style.Vb.Keyword].ForeColor = Color.FromArgb(255, 72, 64, 213);
        editor.Styles[Style.Vb.Keyword2].ForeColor = Color.DarkCyan;
        editor.Styles[Style.Vb.Keyword3].ForeColor = Color.OrangeRed;
        editor.Styles[Style.Vb.Keyword4].ForeColor = Color.DarkRed;
        editor.Styles[Style.Vb.Label].ForeColor = Color.FromArgb(255, 72, 64, 213);
        editor.Styles[Style.Vb.Number].ForeColor = Color.OrangeRed;
        editor.Styles[Style.Vb.Operator].ForeColor = Color.Gray;
        editor.Styles[Style.Vb.Operator].Bold = true;
        editor.Styles[Style.Vb.Preprocessor].ForeColor = Color.Gray;
        editor.Styles[Style.Vb.String].ForeColor = Color.Brown;
        editor.Styles[Style.Vb.StringEol].ForeColor = Color.Black;
        editor.Styles[Style.Vb.StringEol].FillLine = true;

        editor.SetKeywords(0, Keywords);
        editor.SetKeywords(2, Literals);
        editor.SetKeywords(3, Other);
    }
}