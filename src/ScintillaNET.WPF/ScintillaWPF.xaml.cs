using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Markup;
using System.ComponentModel;
using System.Windows.Controls;
using ScintillaNET.WPF.Configuration;

namespace ScintillaNET.WPF
{
    [DefaultProperty("Text")]
    [DefaultEvent("DocumentChanged")]
    [ContentProperty("WPFConfig")]
    public partial class ScintillaWPF : UserControl
    {
        public Scintilla Scintilla { get; private set; }

        public ScintillaWPF()
        {
            InitializeComponent();
            this.Scintilla = new Scintilla();
            this.winFormsHost.Child = this.Scintilla;
            this.mWPFConfig = new ScintillaWPFConfigItemCollection(this);

            this.Scintilla.ZoomChanged += (o, e) =>
            {
                if (this.Zoom != this.Scintilla.Zoom)
                {
                    this.Zoom = this.Scintilla.Zoom;
                }
            };
        }

        private readonly ScintillaWPFConfigItemCollection mWPFConfig;

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScintillaWPFConfigItemCollection WPFConfig => mWPFConfig;

        /// <summary>
        /// A constant used to specify an invalid document position.
        /// </summary>
        public const int InvalidPosition = -1; // NativeMethods.INVALID_POSITION;

        #region Properties

        /// <summary>
        /// Gets or sets the caret foreground color for additional selections.
        /// </summary>
        /// <returns>The caret foreground color in additional selections. The default is (127, 127, 127).</returns>
        [Category("Multiple Selection")]
        [Description("The additional caret foreground color.")]
        public Color AdditionalCaretForeColor
        {
            get => MediaColor(Scintilla.AdditionalCaretForeColor);
            set => Scintilla.AdditionalCaretForeColor = DrawingColor(value);
        }

        /// <summary>
        /// Gets or sets whether the carets in additional selections will blink.
        /// </summary>
        /// <returns>true if additional selection carets should blink; otherwise, false. The default is true.</returns>
        [DefaultValue(true)]
        [Category("Multiple Selection")]
        [Description("Whether the carets in additional selections should blink.")]
        public bool AdditionalCaretsBlink
        {
            get => Scintilla.AdditionalCaretsBlink;
            set => Scintilla.AdditionalCaretsBlink = value;
        }

        /// <summary>
        /// Gets or sets whether the carets in additional selections are visible.
        /// </summary>
        /// <returns>true if additional selection carets are visible; otherwise, false. The default is true.</returns>
        [DefaultValue(true)]
        [Category("Multiple Selection")]
        [Description("Whether the carets in additional selections are visible.")]
        public bool AdditionalCaretsVisible
        {
            get => Scintilla.AdditionalCaretsVisible;
            set => Scintilla.AdditionalCaretsVisible = value;
        }

        /// <summary>
        /// Gets or sets the alpha transparency of additional multiple selections.
        /// </summary>
        /// <returns>
        /// The alpha transparency ranging from 0 (completely transparent) to 255 (completely opaque).
        /// The value 256 will disable alpha transparency. The default is 256.
        /// </returns>
        [DefaultValue(256)]
        [Category("Multiple Selection")]
        [Description("The transparency of additional selections.")]
        public int AdditionalSelAlpha
        {
            get => Scintilla.AdditionalSelAlpha;
            set => Scintilla.AdditionalSelAlpha = value;
        }

        /// <summary>
        /// Gets or sets whether additional typing affects multiple selections.
        /// </summary>
        /// <returns>true if typing will affect multiple selections instead of just the main selection; otherwise, false. The default is false.</returns>
        [DefaultValue(false)]
        [Category("Multiple Selection")]
        [Description("Whether typing, backspace, or delete works with multiple selection simultaneously.")]
        public bool AdditionalSelectionTyping
        {
            get => Scintilla.AdditionalSelectionTyping;
            set => Scintilla.AdditionalSelectionTyping = value;
        }

        /// <summary>
        /// Gets or sets the current anchor position.
        /// </summary>
        /// <returns>The zero-based character position of the anchor.</returns>
        /// <remarks>
        /// Setting the current anchor position will create a selection between it and the <see cref="CurrentPosition" />.
        /// The caret is not scrolled into view.
        /// </remarks>
        /// <seealso cref="ScrollCaret" />
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int AnchorPosition
        {
            get => Scintilla.AnchorPosition;
            set => Scintilla.AnchorPosition = value;
        }

        /// <summary>
        /// Gets or sets the display of annotations.
        /// </summary>
        /// <returns>One of the <see cref="Annotation" /> enumeration values. The default is <see cref="Annotation.Hidden" />.</returns>
        [DefaultValue(Annotation.Hidden)]
        [Category("Appearance")]
        [Description("Display and location of annotations.")]
        public Annotation AnnotationVisible
        {
            get => Scintilla.AnnotationVisible;
            set => Scintilla.AnnotationVisible = value;
        }

        /// <summary>
        /// Gets a value indicating whether there is an autocompletion list displayed.
        /// </summary>
        /// <returns>true if there is an active autocompletion list; otherwise, false.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AutoCActive => Scintilla.AutoCActive;

        /// <summary>
        /// Gets or sets whether to automatically cancel autocompletion when there are no viable matches.
        /// </summary>
        /// <returns>
        /// true to automatically cancel autocompletion when there is no possible match; otherwise, false.
        /// The default is true.
        /// </returns>
        [DefaultValue(true)]
        [Category("Autocompletion")]
        [Description("Whether to automatically cancel autocompletion when no match is possible.")]
        public bool AutoCAutoHide
        {
            get => Scintilla.AutoCAutoHide;
            set => Scintilla.AutoCAutoHide = value;
        }

        /// <summary>
        /// Gets or sets whether to cancel an autocompletion if the caret moves from its initial location,
        /// or is allowed to move to the word start.
        /// </summary>
        /// <returns>
        /// true to cancel autocompletion when the caret moves.
        /// false to allow the caret to move to the beginning of the word without cancelling autocompletion.
        /// </returns>
        [DefaultValue(true)]
        [Category("Autocompletion")]
        [Description("Whether to cancel an autocompletion if the caret moves from its initial location, or is allowed to move to the word start.")]
        public bool AutoCCancelAtStart
        {
            get => Scintilla.AutoCCancelAtStart;
            set => Scintilla.AutoCCancelAtStart = value;
        }

        /// <summary>
        /// Gets the index of the current autocompletion list selection.
        /// </summary>
        /// <returns>The zero-based index of the current autocompletion selection.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int AutoCCurrent => Scintilla.AutoCCurrent;

        /// <summary>
        /// Gets or sets whether to automatically select an item when it is the only one in an autocompletion list.
        /// </summary>
        /// <returns>
        /// true to automatically choose the only autocompletion item and not display the list; otherwise, false.
        /// The default is false.
        /// </returns>
        [DefaultValue(false)]
        [Category("Autocompletion")]
        [Description("Whether to automatically choose an autocompletion item when it is the only one in the list.")]
        public bool AutoCChooseSingle
        {
            get => Scintilla.AutoCChooseSingle;
            set => Scintilla.AutoCChooseSingle = value;
        }

        /// <summary>
        /// Gets or sets whether to delete any word characters following the caret after an autocompletion.
        /// </summary>
        /// <returns>
        /// true to delete any word characters following the caret after autocompletion; otherwise, false.
        /// The default is false.</returns>
        [DefaultValue(false)]
        [Category("Autocompletion")]
        [Description("Whether to delete any existing word characters following the caret after autocompletion.")]
        public bool AutoCDropRestOfWord
        {
            get => Scintilla.AutoCDropRestOfWord;
            set => Scintilla.AutoCDropRestOfWord = value;
        }

        /// <summary>
        /// Gets or sets whether matching characters to an autocompletion list is case-insensitive.
        /// </summary>
        /// <returns>true to use case-insensitive matching; otherwise, false. The default is false.</returns>
        [DefaultValue(false)]
        [Category("Autocompletion")]
        [Description("Whether autocompletion word matching can ignore case.")]
        public bool AutoCIgnoreCase
        {
            get => Scintilla.AutoCIgnoreCase;
            set => Scintilla.AutoCIgnoreCase = value;
        }

        /// <summary>
        /// Gets or sets the maximum height of the autocompletion list measured in rows.
        /// </summary>
        /// <returns>The max number of rows to display in an autocompletion window. The default is 5.</returns>
        /// <remarks>If there are more items in the list than max rows, a vertical scrollbar is shown.</remarks>
        [DefaultValue(5)]
        [Category("Autocompletion")]
        [Description("The maximum number of rows to display in an autocompletion list.")]
        public int AutoCMaxHeight
        {
            get => Scintilla.AutoCMaxHeight;
            set => Scintilla.AutoCMaxHeight = value;
        }

        /// <summary>
        /// Gets or sets the width in characters of the autocompletion list.
        /// </summary>
        /// <returns>
        /// The width of the autocompletion list expressed in characters, or 0 to automatically set the width
        /// to the longest item. The default is 0.
        /// </returns>
        /// <remarks>Any items that cannot be fully displayed will be indicated with ellipsis.</remarks>
        [DefaultValue(0)]
        [Category("Autocompletion")]
        [Description("The width of the autocompletion list measured in characters.")]
        public int AutoCMaxWidth
        {
            get => Scintilla.AutoCMaxWidth;
            set => Scintilla.AutoCMaxWidth = value;
        }

        /// <summary>
        /// Gets or sets the autocompletion list sort order to expect when calling <see cref="AutoCShow" />.
        /// </summary>
        /// <returns>One of the <see cref="Order" /> enumeration values. The default is <see cref="Order.Presorted" />.</returns>
        [DefaultValue(Order.Presorted)]
        [Category("Autocompletion")]
        [Description("The order of words in an autocompletion list.")]
        public Order AutoCOrder
        {
            get => Scintilla.AutoCOrder;
            set => Scintilla.AutoCOrder = value;
        }

        /// <summary>
        /// Gets the document position at the time <see cref="AutoCShow" /> was called.
        /// </summary>
        /// <returns>The zero-based document position at the time <see cref="AutoCShow" /> was called.</returns>
        /// <seealso cref="AutoCShow" />
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int AutoCPosStart => Scintilla.AutoCPosStart;

        /// <summary>
        /// Gets or sets the delimiter character used to separate words in an autocompletion list.
        /// </summary>
        /// <returns>The separator character used when calling <see cref="AutoCShow" />. The default is the space character.</returns>
        /// <remarks>The <paramref name="value" /> specified should be limited to printable ASCII characters.</remarks>
        [DefaultValue(' ')]
        [Category("Autocompletion")]
        [Description("The autocompletion list word delimiter. The default is a space character.")]
        public Char AutoCSeparator
        {
            get => Scintilla.AutoCSeparator;
            set => Scintilla.AutoCSeparator = value;
        }

        /// <summary>
        /// Gets or sets the delimiter character used to separate words and image type identifiers in an autocompletion list.
        /// </summary>
        /// <returns>The separator character used to reference an image registered with <see cref="RegisterRgbaImage" />. The default is '?'.</returns>
        /// <remarks>The <paramref name="value" /> specified should be limited to printable ASCII characters.</remarks>
        [DefaultValue('?')]
        [Category("Autocompletion")]
        [Description("The autocompletion list image type delimiter.")]
        public Char AutoCTypeSeparator
        {
            get => Scintilla.AutoCTypeSeparator;
            set => Scintilla.AutoCTypeSeparator = value;
        }

        /// <summary>
        /// Gets or sets the automatic folding flags.
        /// </summary>
        /// <returns>
        /// A bitwise combination of the <see cref="ScintillaNET.AutomaticFold" /> enumeration.
        /// The default is <see cref="ScintillaNET.AutomaticFold.None" />.
        /// </returns>
        [DefaultValue(AutomaticFold.None)]
        [Category("Behavior")]
        [Description("Options for allowing the control to automatically handle folding.")]
        [TypeConverter(typeof(FlagsEnumTypeConverter.FlagsEnumConverter))]
        public AutomaticFold AutomaticFold
        {
            get => Scintilla.AutomaticFold;
            set => Scintilla.AutomaticFold = value;
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Color BackColor
        {
            get => MediaColor(Scintilla.BackColor);
            set => Scintilla.BackColor = DrawingColor(value);
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public System.Drawing.Image BackgroundImage
        {
            get => Scintilla.BackgroundImage;
            set => Scintilla.BackgroundImage = value;
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public System.Windows.Forms.ImageLayout BackgroundImageLayout
        {
            get => Scintilla.BackgroundImageLayout;
            set => Scintilla.BackgroundImageLayout = value;
        }

        /// <summary>
        /// Gets or sets the border type of the <see cref="Scintilla" /> control.
        /// </summary>
        /// <returns>A BorderStyle enumeration value that represents the border type of the control. The default is Fixed3D.</returns>
        /// <exception cref="InvalidEnumArgumentException">A value that is not within the range of valid values for the enumeration was assigned to the property.</exception>
        [DefaultValue(System.Windows.Forms.BorderStyle.Fixed3D)]
        [Category("Appearance")]
        [Description("Indicates whether the control should have a border.")]
        public System.Windows.Forms.BorderStyle BorderStyle
        {
            get => Scintilla.BorderStyle;
            set => Scintilla.BorderStyle = value;
        }

        /// <summary>
        /// Gets or sets whether drawing is double-buffered.
        /// </summary>
        /// <returns>
        /// true to draw each line into an offscreen bitmap first before copying it to the screen; otherwise, false.
        /// The default is true.
        /// </returns>
        /// <remarks>Disabling buffer can improve performance but will cause flickering.</remarks>
        [DefaultValue(true)]
        [Category("Misc")]
        [Description("Determines whether drawing is double-buffered.")]
        public bool BufferedDraw
        {
            get => Scintilla.BufferedDraw;
            set => Scintilla.BufferedDraw = value;
        }

        /*
        /// <summary>
        /// Gets or sets the current position of a call tip.
        /// </summary>
        /// <returns>The zero-based document position indicated when <see cref="CallTipShow" /> was called to display a call tip.</returns>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int CallTipPosStart
        {
            get
            {
                var pos = DirectMessage(NativeMethods.SCI_CALLTIPPOSSTART).ToInt32();
                if (pos < 0)
                    return pos;

                return Lines.ByteToCharPosition(pos);
            }
            set
            {
                value = Helpers.Clamp(value, 0, TextLength);
                value = Lines.CharToBytePosition(value);
                DirectMessage(NativeMethods.SCI_CALLTIPSETPOSSTART, new IntPtr(value));
            }
        }
        */

        /// <summary>
        /// Gets a value indicating whether there is a call tip window displayed.
        /// </summary>
        /// <returns>true if there is an active call tip window; otherwise, false.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CallTipActive => Scintilla.CallTipActive;

        /// <summary>
        /// Gets a value indicating whether there is text on the clipboard that can be pasted into the document.
        /// </summary>
        /// <returns>true when there is text on the clipboard to paste; otherwise, false.</returns>
        /// <remarks>The document cannot be <see cref="ReadOnly" />  and the selection cannot contain protected text.</remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CanPaste => Scintilla.CanPaste;

        /// <summary>
        /// Gets a value indicating whether there is an undo action to redo.
        /// </summary>
        /// <returns>true when there is something to redo; otherwise, false.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CanRedo => Scintilla.CanRedo;

        /// <summary>
        /// Gets a value indicating whether there is an action to undo.
        /// </summary>
        /// <returns>true when there is something to undo; otherwise, false.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CanUndo => Scintilla.CanUndo;

        /// <summary>
        /// Gets or sets the caret foreground color.
        /// </summary>
        /// <returns>The caret foreground color. The default is black.</returns>
        [DefaultValue(typeof(Color), "Black")]
        [Category("Caret")]
        [Description("The caret foreground color.")]
        public Color CaretForeColor
        {
            get => MediaColor(Scintilla.CaretForeColor);
            set => Scintilla.CaretForeColor = DrawingColor(value);
            //get { return (mInnerScintilla.CaretForeColor); }
            //set { mInnerScintilla.CaretForeColor = (value); }
        }

        /// <summary>
        /// Gets or sets the caret line background color.
        /// </summary>
        /// <returns>The caret line background color. The default is yellow.</returns>
        [DefaultValue(typeof(Color), "Yellow")]
        [Category("Caret")]
        [Description("The background color of the current line.")]
        public Color CaretLineBackColor
        {
            get => MediaColor(Scintilla.CaretLineBackColor);
            set => Scintilla.CaretLineBackColor = DrawingColor(value);
        }

        /// <summary>
        /// Gets or sets the alpha transparency of the <see cref="CaretLineBackColor" />.
        /// </summary>
        /// <returns>
        /// The alpha transparency ranging from 0 (completely transparent) to 255 (completely opaque).
        /// The value 256 will disable alpha transparency. The default is 256.
        /// </returns>
        [DefaultValue(256)]
        [Category("Caret")]
        [Description("The transparency of the current line background color.")]
        public int CaretLineBackColorAlpha
        {
            get => Scintilla.CaretLineBackColorAlpha;
            set => Scintilla.CaretLineBackColorAlpha = value;
        }

        /// <summary>
        /// Gets or sets whether the caret line is visible (highlighted).
        /// </summary>
        /// <returns>true if the caret line is visible; otherwise, false. The default is false.</returns>
        [DefaultValue(false)]
        [Category("Caret")]
        [Description("Determines whether to highlight the current caret line.")]
        public bool CaretLineVisible
        {
            get => Scintilla.CaretLineVisible;
            set => Scintilla.CaretLineVisible = value;
        }

        /// <summary>
        /// Gets or sets the caret blink rate in milliseconds.
        /// </summary>
        /// <returns>The caret blink rate measured in milliseconds. The default is 530.</returns>
        /// <remarks>A value of 0 will stop the caret blinking.</remarks>
        [DefaultValue(530)]
        [Category("Caret")]
        [Description("The caret blink rate in milliseconds.")]
        public int CaretPeriod
        {
            get => Scintilla.CaretPeriod;
            set => Scintilla.CaretPeriod = value;
        }

        /// <summary>
        /// Gets or sets the caret display style.
        /// </summary>
        /// <returns>
        /// One of the <see cref="ScintillaNET.CaretStyle" /> enumeration values.
        /// The default is <see cref="ScintillaNET.CaretStyle.Line" />.
        /// </returns>
        [DefaultValue(CaretStyle.Line)]
        [Category("Caret")]
        [Description("The caret display style.")]
        public CaretStyle CaretStyle
        {
            get => Scintilla.CaretStyle;
            set => Scintilla.CaretStyle = value;
        }

        /// <summary>
        /// Gets or sets the width in pixels of the caret.
        /// </summary>
        /// <returns>The width of the caret in pixels. The default is 1 pixel.</returns>
        /// <remarks>
        /// The caret width can only be set to a value of 0, 1, 2 or 3 pixels and is only effective
        /// when the <see cref="CaretStyle" /> property is set to <see cref="ScintillaNET.CaretStyle.Line" />.
        /// </remarks>
        [DefaultValue(1)]
        [Category("Caret")]
        [Description("The width of the caret line measured in pixels (between 0 and 3).")]
        public int CaretWidth
        {
            get => Scintilla.CaretWidth;
            set => Scintilla.CaretWidth = value;
        }

        /// <summary>
        /// Gets the current line index.
        /// </summary>
        /// <returns>The zero-based line index containing the <see cref="CurrentPosition" />.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CurrentLine => Scintilla.CurrentLine;

        /// <summary>
        /// Gets or sets the current caret position.
        /// </summary>
        /// <returns>The zero-based character position of the caret.</returns>
        /// <remarks>
        /// Setting the current caret position will create a selection between it and the current <see cref="AnchorPosition" />.
        /// The caret is not scrolled into view.
        /// </remarks>
        /// <seealso cref="ScrollCaret" />
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CurrentPosition
        {
            get => Scintilla.CurrentPosition;
            set => Scintilla.CurrentPosition = value;
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new System.Windows.Forms.Cursor Cursor
        {
            get => Scintilla.Cursor;
            set => Scintilla.Cursor = value;
        }

        /// <summary>
        /// Gets or sets the default cursor for the control.
        /// </summary>
        /// <returns>An object of type Cursor representing the current default cursor.</returns>
        protected System.Windows.Forms.Cursor DefaultCursor => System.Windows.Forms.Cursors.IBeam;

        //return mInnerScintilla.DefaultCursor;
        /// <summary>
        /// Gets or sets the current document used by the control.
        /// </summary>
        /// <returns>The current <see cref="Document" />.</returns>
        /// <remarks>
        /// Setting this property is equivalent to calling <see cref="ReleaseDocument" /> on the current document, and
        /// calling <see cref="CreateDocument" /> if the new <paramref name="value" /> is <see cref="ScintillaNET.Document.Empty" /> or
        /// <see cref="AddRefDocument" /> if the new <paramref name="value" /> is not <see cref="ScintillaNET.Document.Empty" />.
        /// </remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Document Document
        {
            get => Scintilla.Document;
            set => Scintilla.Document = value;
        }

        /// <summary>
        /// Gets or sets the background color to use when indicating long lines with
        /// <see cref="ScintillaNET.EdgeMode.Background" />.
        /// </summary>
        /// <returns>The background Color. The default is Silver.</returns>
        [DefaultValue(typeof(Color), "Silver")]
        [Category("Long Lines")]
        [Description("The background color to use when indicating long lines.")]
        public Color EdgeColor
        {
            get => MediaColor(Scintilla.EdgeColor);
            set => Scintilla.EdgeColor = DrawingColor(value);
        }

        /// <summary>
        /// Gets or sets the column number at which to begin indicating long lines.
        /// </summary>
        /// <returns>The number of columns in a long line. The default is 0.</returns>
        /// <remarks>
        /// When using <see cref="ScintillaNET.EdgeMode.Line"/>, a column is defined as the width of a space character in the <see cref="Style.Default" /> style.
        /// When using <see cref="ScintillaNET.EdgeMode.Background" /> a column is equal to a character (including tabs).
        /// </remarks>
        [DefaultValue(0)]
        [Category("Long Lines")]
        [Description("The number of columns at which to display long line indicators.")]
        public int EdgeColumn
        {
            get => Scintilla.EdgeColumn;
            set => Scintilla.EdgeColumn = value;
        }

        /// <summary>
        /// Gets or sets the mode for indicating long lines.
        /// </summary>
        /// <returns>
        /// One of the <see cref="ScintillaNET.EdgeMode" /> enumeration values.
        /// The default is <see cref="ScintillaNET.EdgeMode.None" />.
        /// </returns>
        [DefaultValue(EdgeMode.None)]
        [Category("Long Lines")]
        [Description("Determines how long lines are indicated.")]
        public EdgeMode EdgeMode
        {
            get => Scintilla.EdgeMode;
            set => Scintilla.EdgeMode = value;
        }

        /// <summary>
        /// Gets or sets whether vertical scrolling ends at the last line or can scroll past.
        /// </summary>
        /// <returns>true if the maximum vertical scroll position ends at the last line; otherwise, false. The default is true.</returns>
        [DefaultValue(true)]
        [Category("Scrolling")]
        [Description("Determines whether the maximum vertical scroll position ends at the last line or can scroll past.")]
        public bool EndAtLastLine
        {
            get => Scintilla.EndAtLastLine;
            set => Scintilla.EndAtLastLine = value;
        }

        /// <summary>
        /// Gets or sets the end-of-line mode, or rather, the characters added into
        /// the document when the user presses the Enter key.
        /// </summary>
        /// <returns>One of the <see cref="Eol" /> enumeration values. The default is <see cref="Eol.CrLf" />.</returns>
        [DefaultValue(Eol.CrLf)]
        [Category("Line Endings")]
        [Description("Determines the characters added into the document when the user presses the Enter key.")]
        public Eol EolMode
        {
            get => Scintilla.EolMode;
            set => Scintilla.EolMode = value;
        }

        /// <summary>
        /// Gets or sets the amount of whitespace added to the ascent (top) of each line.
        /// </summary>
        /// <returns>The extra line ascent. The default is zero.</returns>
        [DefaultValue(0)]
        [Category("Whitespace")]
        [Description("Extra whitespace added to the ascent (top) of each line.")]
        public int ExtraAscent
        {
            get => Scintilla.ExtraAscent;
            set => Scintilla.ExtraAscent = value;
        }

        /// <summary>
        /// Gets or sets the amount of whitespace added to the descent (bottom) of each line.
        /// </summary>
        /// <returns>The extra line descent. The default is zero.</returns>
        [DefaultValue(0)]
        [Category("Whitespace")]
        [Description("Extra whitespace added to the descent (bottom) of each line.")]
        public int ExtraDescent
        {
            get => Scintilla.ExtraDescent;
            set => Scintilla.ExtraDescent = value;
        }

        /// <summary>
        /// Gets or sets the first visible line on screen.
        /// </summary>
        /// <returns>The zero-based index of the first visible screen line.</returns>
        /// <remarks>The value is a visible line, not a document line.</remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int FirstVisibleLine
        {
            get => Scintilla.FirstVisibleLine;
            set => Scintilla.FirstVisibleLine = value;
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public System.Drawing.Font Font
        {
            get => Scintilla.Font;
            set => Scintilla.Font = value;
        }

        /// <summary>
        /// Gets or sets font quality (anti-aliasing method) used to render fonts.
        /// </summary>
        /// <returns>
        /// One of the <see cref="ScintillaNET.FontQuality" /> enumeration values.
        /// The default is <see cref="ScintillaNET.FontQuality.Default" />.
        /// </returns>
        [DefaultValue(FontQuality.Default)]
        [Category("Misc")]
        [Description("Specifies the anti-aliasing method to use when rendering fonts.")]
        public FontQuality FontQuality
        {
            get => Scintilla.FontQuality;
            set => Scintilla.FontQuality = value;
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Color ForeColor
        {
            get => MediaColor(Scintilla.ForeColor);
            set => Scintilla.ForeColor = DrawingColor(value);
        }

        /// <summary>
        /// Gets or sets the column number of the indentation guide to highlight.
        /// </summary>
        /// <returns>The column number of the indentation guide to highlight or 0 if disabled.</returns>
        /// <remarks>Guides are highlighted in the <see cref="Style.BraceLight" /> style. Column numbers can be determined by calling <see cref="GetColumn" />.</remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int HighlightGuide
        {
            get => Scintilla.HighlightGuide;
            set => Scintilla.HighlightGuide = value;
        }

        /// <summary>
        /// Gets or sets whether to display the horizontal scroll bar.
        /// </summary>
        /// <returns>true to display the horizontal scroll bar when needed; otherwise, false. The default is true.</returns>
        [DefaultValue(true)]
        [Category("Scrolling")]
        [Description("Determines whether to show the horizontal scroll bar if needed.")]
        public bool HScrollBar
        {
            get => Scintilla.HScrollBar;
            set => Scintilla.HScrollBar = value;
        }

        /// <summary>
        /// Gets or sets the strategy used to perform styling using application idle time.
        /// </summary>
        /// <returns>
        /// One of the <see cref="ScintillaNET.IdleStyling" /> enumeration values.
        /// The default is <see cref="ScintillaNET.IdleStyling.None" />.
        /// </returns>
        [DefaultValue(IdleStyling.None)]
        [Category("Misc")]
        [Description("Specifies how to use application idle time for styling.")]
        public IdleStyling IdleStyling
        {
            get => Scintilla.IdleStyling;
            set => Scintilla.IdleStyling = value;
        }

        /// <summary>
        /// Gets or sets the size of indentation in terms of space characters.
        /// </summary>
        /// <returns>The indentation size measured in characters. The default is 0.</returns>
        /// <remarks> A value of 0 will make the indent width the same as the tab width.</remarks>
        [DefaultValue(0)]
        [Category("Indentation")]
        [Description("The indentation size in characters or 0 to make it the same as the tab width.")]
        public int IndentWidth
        {
            get => Scintilla.IndentWidth;
            set => Scintilla.IndentWidth = value;
        }

        /// <summary>
        /// Gets or sets whether to display indentation guides.
        /// </summary>
        /// <returns>One of the <see cref="IndentView" /> enumeration values. The default is <see cref="IndentView.None" />.</returns>
        /// <remarks>The <see cref="Style.IndentGuide" /> style can be used to specify the foreground and background color of indentation guides.</remarks>
        [DefaultValue(IndentView.None)]
        [Category("Indentation")]
        [Description("Indicates whether indentation guides are displayed.")]
        public IndentView IndentationGuides
        {
            get => Scintilla.IndentationGuides;
            set => Scintilla.IndentationGuides = value;
        }

        /// <summary>
        /// Gets or sets the indicator used in a subsequent call to <see cref="IndicatorFillRange" /> or <see cref="IndicatorClearRange" />.
        /// </summary>
        /// <returns>The zero-based indicator index to apply when calling <see cref="IndicatorFillRange" /> or remove when calling <see cref="IndicatorClearRange" />.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int IndicatorCurrent
        {
            get => Scintilla.IndicatorCurrent;
            set => Scintilla.IndicatorCurrent = value;
        }

        /// <summary>
        /// Gets a collection of objects for working with indicators.
        /// </summary>
        /// <returns>A collection of <see cref="Indicator" /> objects.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IndicatorCollection Indicators => Scintilla.Indicators;

        /// <summary>
        /// Gets or sets the user-defined value used in a subsequent call to <see cref="IndicatorFillRange" />.
        /// </summary>
        /// <returns>The indicator value to apply when calling <see cref="IndicatorFillRange" />.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int IndicatorValue
        {
            get => Scintilla.IndicatorValue;
            set => Scintilla.IndicatorValue = value;
        }

        /// <summary>
        /// Gets or sets the current lexer.
        /// </summary>
        /// <returns>One of the <see cref="Lexer" /> enumeration values. The default is <see cref="ScintillaNET.Lexer.Container" />.</returns>
        [DefaultValue(Lexer.Container)]
        [Category("Lexing")]
        [Description("The current lexer.")]
        public Lexer Lexer
        {
            get => Scintilla.Lexer;
            set => Scintilla.Lexer = value;
        }

        /// <summary>
        /// Gets or sets the current lexer by name.
        /// </summary>
        /// <returns>A String representing the current lexer.</returns>
        /// <remarks>Lexer names are case-sensitive.</remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string LexerLanguage
        {
            get => Scintilla.LexerLanguage;
            set => Scintilla.LexerLanguage = value;
        }

        /// <summary>
        /// Gets the combined result of the <see cref="LineEndTypesSupported" /> and <see cref="LineEndTypesAllowed" />
        /// properties to report the line end types actively being interpreted.
        /// </summary>
        /// <returns>A bitwise combination of the <see cref="LineEndType" /> enumeration.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public LineEndType LineEndTypesActive => Scintilla.LineEndTypesActive;

        /// <summary>
        /// Gets or sets the line ending types interpreted by the <see cref="Scintilla" /> control.
        /// </summary>
        /// <returns>
        /// A bitwise combination of the <see cref="LineEndType" /> enumeration.
        /// The default is <see cref="LineEndType.Default" />.
        /// </returns>
        /// <remarks>The line ending types allowed must also be supported by the current lexer to be effective.</remarks>
        [DefaultValue(LineEndType.Default)]
        [Category("Line Endings")]
        [Description("Line endings types interpreted by the control.")]
        [TypeConverter(typeof(FlagsEnumTypeConverter.FlagsEnumConverter))]
        public LineEndType LineEndTypesAllowed
        {
            get => Scintilla.LineEndTypesAllowed;
            set => Scintilla.LineEndTypesAllowed = value;
        }

        /// <summary>
        /// Gets the different types of line ends supported by the current lexer.
        /// </summary>
        /// <returns>A bitwise combination of the <see cref="LineEndType" /> enumeration.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public LineEndType LineEndTypesSupported => Scintilla.LineEndTypesSupported;

        /// <summary>
        /// Gets a collection representing lines of text in the <see cref="Scintilla" /> control.
        /// </summary>
        /// <returns>A collection of text lines.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public LineCollection Lines => Scintilla.Lines;

        /// <summary>
        /// Gets the number of lines that can be shown on screen given a constant
        /// line height and the space available.
        /// </summary>
        /// <returns>
        /// The number of screen lines which could be displayed (including any partial lines).
        /// </returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int LinesOnScreen => Scintilla.LinesOnScreen;

        /// <summary>
        /// Gets or sets the main selection when their are multiple selections.
        /// </summary>
        /// <returns>The zero-based main selection index.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int MainSelection
        {
            get => Scintilla.MainSelection;
            set => Scintilla.MainSelection = value;
        }

        /// <summary>
        /// Gets a collection representing margins in a <see cref="Scintilla" /> control.
        /// </summary>
        /// <returns>A collection of margins.</returns>
        [Category("Collections")]
        [Description("The margins collection.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public MarginCollection Margins => Scintilla.Margins;

        /// <summary>
        /// Gets a collection representing markers in a <see cref="Scintilla" /> control.
        /// </summary>
        /// <returns>A collection of markers.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MarkerCollection Markers => Scintilla.Markers;

        /// <summary>
        /// Gets a value indicating whether the document has been modified (is dirty)
        /// since the last call to <see cref="SetSavePoint" />.
        /// </summary>
        /// <returns>true if the document has been modified; otherwise, false.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Modified => Scintilla.Modified;

        /// <summary>
        /// Gets or sets the time in milliseconds the mouse must linger to generate a <see cref="DwellStart" /> event.
        /// </summary>
        /// <returns>
        /// The time in milliseconds the mouse must linger to generate a <see cref="DwellStart" /> event
        /// or <see cref="Scintilla.TimeForever" /> if dwell events are disabled.
        /// </returns>
        [DefaultValue(-1)]
        [Category("Behavior")]
        [Description("The time in milliseconds the mouse must linger to generate a dwell start event. A value of 10000000 disables dwell events.")]
        public int MouseDwellTime
        {
            get => Scintilla.MouseDwellTime;
            set => Scintilla.MouseDwellTime = value;
        }

        /// <summary>
        /// Gets or sets the ability to switch to rectangular selection mode while making a selection with the mouse.
        /// </summary>
        /// <returns>
        /// true if the current mouse selection can be switched to a rectangular selection by pressing the ALT key; otherwise, false.
        /// The default is false.
        /// </returns>
        [DefaultValue(false)]
        [Category("Multiple Selection")]
        [Description("Enable or disable the ability to switch to rectangular selection mode while making a selection with the mouse.")]
        public bool MouseSelectionRectangularSwitch
        {
            get => Scintilla.MouseSelectionRectangularSwitch;
            set => Scintilla.MouseSelectionRectangularSwitch = value;
        }

        // The MouseWheelCaptures property doesn't seem to work correctly in Windows Forms so hiding for now...
        // P.S. I'm avoiding the MouseDownCaptures property (SCI_SETMOUSEDOWNCAPTURES & SCI_GETMOUSEDOWNCAPTURES) for the same reason... I don't expect it to work in Windows Forms.

        /*
        /// <summary>
        /// Gets or sets whether to respond to mouse wheel messages if the control has focus but the mouse is not currently over the control.
        /// </summary>
        /// <returns>
        /// true to respond to mouse wheel messages even when the mouse is not currently over the control; otherwise, false.
        /// The default is true.
        /// </returns>
        /// <remarks>Scintilla will still react to the mouse wheel if the mouse pointer is over the editor window.</remarks>
        [DefaultValue(true)]
        [Category("Mouse")]
        [Description("Enable or disable mouse wheel support when the mouse is outside the control bounds, but the control still has focus.")]
        public bool MouseWheelCaptures
        {
            get
            {
                return DirectMessage(NativeMethods.SCI_GETMOUSEWHEELCAPTURES) != IntPtr.Zero;
            }
            set
            {
                var mouseWheelCaptures = (value ? new IntPtr(1) : IntPtr.Zero);
                DirectMessage(NativeMethods.SCI_SETMOUSEWHEELCAPTURES, mouseWheelCaptures);
            }
        }
        */

        /// <summary>
        /// Gets or sets whether multiple selection is enabled.
        /// </summary>
        /// <returns>
        /// true if multiple selections can be made by holding the CTRL key and dragging the mouse; otherwise, false.
        /// The default is false.
        /// </returns>
        [DefaultValue(false)]
        [Category("Multiple Selection")]
        [Description("Enable or disable multiple selection with the CTRL key.")]
        public bool MultipleSelection
        {
            get => Scintilla.MultipleSelection;
            set => Scintilla.MultipleSelection = value;
        }

        /// <summary>
        /// Gets or sets the behavior when pasting text into multiple selections.
        /// </summary>
        /// <returns>One of the <see cref="ScintillaNET.MultiPaste" /> enumeration values. The default is <see cref="ScintillaNET.MultiPaste.Once" />.</returns>
        [DefaultValue(MultiPaste.Once)]
        [Category("Multiple Selection")]
        [Description("Determines how pasted text is applied to multiple selections.")]
        public MultiPaste MultiPaste
        {
            get => Scintilla.MultiPaste;
            set => Scintilla.MultiPaste = value;
        }

        /// <summary>
        /// Gets or sets whether to write over text rather than insert it.
        /// </summary>
        /// <return>true to write over text; otherwise, false. The default is false.</return>
        [DefaultValue(false)]
        [Category("Behavior")]
        [Description("Puts the caret into overtype mode.")]
        public bool Overtype
        {
            get => Scintilla.Overtype;
            set => Scintilla.Overtype = value;
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new System.Windows.Forms.Padding Padding
        {
            get => Scintilla.Padding;
            set => Scintilla.Padding = value;
        }

        /// <summary>
        /// Gets or sets whether line endings in pasted text are convereted to the document <see cref="EolMode" />.
        /// </summary>
        /// <returns>true to convert line endings in pasted text; otherwise, false. The default is true.</returns>
        [DefaultValue(true)]
        [Category("Line Endings")]
        [Description("Whether line endings in pasted text are converted to match the document end-of-line mode.")]
        public bool PasteConvertEndings
        {
            get => Scintilla.PasteConvertEndings;
            set => Scintilla.PasteConvertEndings = value;
        }

        /// <summary>
        /// Gets or sets the number of phases used when drawing.
        /// </summary>
        /// <returns>One of the <see cref="Phases" /> enumeration values. The default is <see cref="Phases.Two" />.</returns>
        [DefaultValue(Phases.Two)]
        [Category("Misc")]
        [Description("Adjusts the number of phases used when drawing.")]
        public Phases PhasesDraw
        {
            get => Scintilla.PhasesDraw;
            set => Scintilla.PhasesDraw = value;
        }

        /// <summary>
        /// Gets or sets whether the document is read-only.
        /// </summary>
        /// <returns>true if the document is read-only; otherwise, false. The default is false.</returns>
        /// <seealso cref="ModifyAttempt" />
        [DefaultValue(false)]
        [Category("Behavior")]
        [Description("Controls whether the document text can be modified.")]
        public bool ReadOnly
        {
            get => Scintilla.ReadOnly;
            set => Scintilla.ReadOnly = value;
        }

        /// <summary>
        /// Gets or sets the anchor position of the rectangular selection.
        /// </summary>
        /// <returns>The zero-based document position of the rectangular selection anchor.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int RectangularSelectionAnchor
        {
            get => Scintilla.RectangularSelectionAnchor;
            set => Scintilla.RectangularSelectionAnchor = value;
        }

        /// <summary>
        /// Gets or sets the amount of anchor virtual space in a rectangular selection.
        /// </summary>
        /// <returns>The amount of virtual space past the end of the line offsetting the rectangular selection anchor.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int RectangularSelectionAnchorVirtualSpace
        {
            get => Scintilla.RectangularSelectionAnchorVirtualSpace;
            set => Scintilla.RectangularSelectionAnchorVirtualSpace = value;
        }

        /// <summary>
        /// Gets or sets the caret position of the rectangular selection.
        /// </summary>
        /// <returns>The zero-based document position of the rectangular selection caret.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int RectangularSelectionCaret
        {
            get => Scintilla.RectangularSelectionCaret;
            set => Scintilla.RectangularSelectionCaret = value;
        }

        /// <summary>
        /// Gets or sets the amount of caret virtual space in a rectangular selection.
        /// </summary>
        /// <returns>The amount of virtual space past the end of the line offsetting the rectangular selection caret.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int RectangularSelectionCaretVirtualSpace
        {
            get => Scintilla.RectangularSelectionCaretVirtualSpace;
            set => Scintilla.RectangularSelectionCaretVirtualSpace = value;
        }

        /// <summary>
        /// Gets or sets the range of the horizontal scroll bar.
        /// </summary>
        /// <returns>The range in pixels of the horizontal scroll bar. The default is 2000.</returns>
        /// <remarks>The width will automatically increase as needed when <see cref="ScrollWidthTracking" /> is enabled.</remarks>
        [DefaultValue(2000)]
        [Category("Scrolling")]
        [Description("The range in pixels of the horizontal scroll bar.")]
        public int ScrollWidth
        {
            get => Scintilla.ScrollWidth;
            set => Scintilla.ScrollWidth = value;
        }

        /// <summary>
        /// Gets or sets whether the <see cref="ScrollWidth" /> is automatically increased as needed.
        /// </summary>
        /// <returns>
        /// true to automatically increase the horizontal scroll width as needed; otherwise, false.
        /// The default is true.
        /// </returns>
        [DefaultValue(true)]
        [Category("Scrolling")]
        [Description("Determines whether to increase the horizontal scroll width as needed.")]
        public bool ScrollWidthTracking
        {
            get => Scintilla.ScrollWidthTracking;
            set => Scintilla.ScrollWidthTracking = value;
        }

        /// <summary>
        /// Gets or sets the search flags used when searching text.
        /// </summary>
        /// <returns>A bitwise combination of <see cref="ScintillaNET.SearchFlags" /> values. The default is <see cref="ScintillaNET.SearchFlags.None" />.</returns>
        /// <seealso cref="SearchInTarget" />
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SearchFlags SearchFlags
        {
            get => Scintilla.SearchFlags;
            set => Scintilla.SearchFlags = value;
        }

        /// <summary>
        /// Gets the selected text.
        /// </summary>
        /// <returns>The selected text if there is any; otherwise, an empty string.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedText => Scintilla.SelectedText;

        /// <summary>
        /// Gets or sets the end position of the selection.
        /// </summary>
        /// <returns>The zero-based document position where the selection ends.</returns>
        /// <remarks>
        /// When getting this property, the return value is <code>Math.Max(<see cref="AnchorPosition" />, <see cref="CurrentPosition" />)</code>.
        /// When setting this property, <see cref="CurrentPosition" /> is set to the value specified and <see cref="AnchorPosition" /> set to <code>Math.Min(<see cref="AnchorPosition" />, <paramref name="value" />)</code>.
        /// The caret is not scrolled into view.
        /// </remarks>
        /// <seealso cref="SelectionStart" />
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectionEnd
        {
            get => Scintilla.SelectionEnd;
            set => Scintilla.SelectionEnd = value;
        }

        /// <summary>
        /// Gets or sets whether to fill past the end of a line with the selection background color.
        /// </summary>
        /// <returns>true to fill past the end of the line; otherwise, false. The default is false.</returns>
        [DefaultValue(false)]
        [Category("Selection")]
        [Description("Determines whether a selection should fill past the end of the line.")]
        public bool SelectionEolFilled
        {
            get => Scintilla.SelectionEolFilled;
            set => Scintilla.SelectionEolFilled = value;
        }

        /// <summary>
        /// Gets a collection representing multiple selections in a <see cref="Scintilla" /> control.
        /// </summary>
        /// <returns>A collection of selections.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SelectionCollection Selections => Scintilla.Selections;

        /// <summary>
        /// Gets or sets the start position of the selection.
        /// </summary>
        /// <returns>The zero-based document position where the selection starts.</returns>
        /// <remarks>
        /// When getting this property, the return value is <code>Math.Min(<see cref="AnchorPosition" />, <see cref="CurrentPosition" />)</code>.
        /// When setting this property, <see cref="AnchorPosition" /> is set to the value specified and <see cref="CurrentPosition" /> set to <code>Math.Max(<see cref="CurrentPosition" />, <paramref name="value" />)</code>.
        /// The caret is not scrolled into view.
        /// </remarks>
        /// <seealso cref="SelectionEnd" />
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectionStart
        {
            get => Scintilla.SelectionStart;
            set => Scintilla.SelectionStart = value;
        }

        /// <summary>
        /// Gets or sets the last internal error code used by Scintilla.
        /// </summary>
        /// <returns>
        /// One of the <see cref="Status" /> enumeration values.
        /// The default is <see cref="ScintillaNET.Status.Ok" />.
        /// </returns>
        /// <remarks>The status can be reset by setting the property to <see cref="ScintillaNET.Status.Ok" />.</remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Status Status
        {
            get => Scintilla.Status;
            set => Scintilla.Status = value;
        }

        /// <summary>
        /// Gets a collection representing style definitions in a <see cref="Scintilla" /> control.
        /// </summary>
        /// <returns>A collection of style definitions.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public StyleCollection Styles => Scintilla.Styles;

        /// <summary>
        /// Gets or sets how tab characters are represented when whitespace is visible.
        /// </summary>
        /// <returns>
        /// One of the <see cref="ScintillaNET.TabDrawMode" /> enumeration values.
        /// The default is <see cref="TabDrawMode.LongArrow" />.
        /// </returns>
        /// <seealso cref="ViewWhitespace" />
        [DefaultValue(TabDrawMode.LongArrow)]
        [Category("Whitespace")]
        [Description("Style of visible tab characters.")]
        public TabDrawMode TabDrawMode
        {
            get => Scintilla.TabDrawMode;
            set => Scintilla.TabDrawMode = value;
        }

        /// <summary>
        /// Gets or sets the width of a tab as a multiple of a space character.
        /// </summary>
        /// <returns>The width of a tab measured in characters. The default is 4.</returns>
        [DefaultValue(4)]
        [Category("Indentation")]
        [Description("The tab size in characters.")]
        public int TabWidth
        {
            get => Scintilla.TabWidth;
            set => Scintilla.TabWidth = value;
        }

        /// <summary>
        /// Gets or sets the end position used when performing a search or replace.
        /// </summary>
        /// <returns>The zero-based character position within the document to end a search or replace operation.</returns>
        /// <seealso cref="TargetStart"/>
        /// <seealso cref="SearchInTarget" />
        /// <seealso cref="ReplaceTarget" />
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TargetEnd
        {
            get => Scintilla.TargetEnd;
            set => Scintilla.TargetEnd = value;
        }

        /// <summary>
        /// Gets or sets the start position used when performing a search or replace.
        /// </summary>
        /// <returns>The zero-based character position within the document to start a search or replace operation.</returns>
        /// <seealso cref="TargetEnd"/>
        /// <seealso cref="SearchInTarget" />
        /// <seealso cref="ReplaceTarget" />
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TargetStart
        {
            get => Scintilla.TargetStart;
            set => Scintilla.TargetStart = value;
        }

        /// <summary>
        /// Gets the current target text.
        /// </summary>
        /// <returns>A String representing the text between <see cref="TargetStart" /> and <see cref="TargetEnd" />.</returns>
        /// <remarks>Targets which have a start position equal or greater to the end position will return an empty String.</remarks>
        /// <seealso cref="TargetStart" />
        /// <seealso cref="TargetEnd" />
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string TargetText => Scintilla.TargetText;

        /// <summary>
        /// Gets or sets the rendering technology used.
        /// </summary>
        /// <returns>
        /// One of the <see cref="Technology" /> enumeration values.
        /// The default is <see cref="ScintillaNET.Technology.Default" />.
        /// </returns>
        [DefaultValue(Technology.Default)]
        [Category("Misc")]
        [Description("The rendering technology used to draw text.")]
        public Technology Technology
        {
            get => Scintilla.Technology;
            set => Scintilla.Technology = value;
        }

        /// <summary>
        /// Gets or sets the current document text in the <see cref="Scintilla" /> control.
        /// </summary>
        /// <returns>The text displayed in the control.</returns>
        /// <remarks>Depending on the length of text get or set, this operation can be expensive.</remarks>
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design", typeof(System.Drawing.Design.UITypeEditor))]
        public string Text
        {
            get => Scintilla.Text;
            set => Scintilla.Text = value;
        }

        /// <summary>
        /// Gets the length of the text in the control.
        /// </summary>
        /// <returns>The number of characters in the document.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TextLength => Scintilla.TextLength;

        /// <summary>
        /// Gets or sets whether to use a mixture of tabs and spaces for indentation or purely spaces.
        /// </summary>
        /// <returns>true to use tab characters; otherwise, false. The default is true.</returns>
        [DefaultValue(false)]
        [Category("Indentation")]
        [Description("Determines whether indentation allows tab characters or purely space characters.")]
        public bool UseTabs
        {
            get => Scintilla.UseTabs;
            set => Scintilla.UseTabs = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use the wait cursor for the current control.
        /// </summary>
        /// <returns>true to use the wait cursor for the current control; otherwise, false. The default is false.</returns>
        public bool UseWaitCursor
        {
            get => Scintilla.UseWaitCursor;
            set => Scintilla.UseWaitCursor = value;
        }

        /// <summary>
        /// Gets or sets the visibility of end-of-line characters.
        /// </summary>
        /// <returns>true to display end-of-line characters; otherwise, false. The default is false.</returns>
        [DefaultValue(false)]
        [Category("Line Endings")]
        [Description("Display end-of-line characters.")]
        public bool ViewEol
        {
            get => Scintilla.ViewEol;
            set => Scintilla.ViewEol = value;
        }

        /// <summary>
        /// Gets or sets how to display whitespace characters.
        /// </summary>
        /// <returns>One of the <see cref="WhitespaceMode" /> enumeration values. The default is <see cref="WhitespaceMode.Invisible" />.</returns>
        /// <seealso cref="SetWhitespaceForeColor" />
        /// <seealso cref="SetWhitespaceBackColor" />
        [DefaultValue(WhitespaceMode.Invisible)]
        [Category("Whitespace")]
        [Description("Options for displaying whitespace characters.")]
        public WhitespaceMode ViewWhitespace
        {
            get => Scintilla.ViewWhitespace;
            set => Scintilla.ViewWhitespace = value;
        }

        /// <summary>
        /// Gets or sets the ability for the caret to move into an area beyond the end of each line, otherwise known as virtual space.
        /// </summary>
        /// <returns>
        /// A bitwise combination of the <see cref="VirtualSpace" /> enumeration.
        /// The default is <see cref="VirtualSpace.None" />.
        /// </returns>
        [DefaultValue(VirtualSpace.None)]
        [Category("Behavior")]
        [Description("Options for allowing the caret to move beyond the end of each line.")]
        [TypeConverter(typeof(FlagsEnumTypeConverter.FlagsEnumConverter))]
        public VirtualSpace VirtualSpaceOptions
        {
            get => Scintilla.VirtualSpaceOptions;
            set => Scintilla.VirtualSpaceOptions = value;
        }

        /// <summary>
        /// Gets or sets whether to display the vertical scroll bar.
        /// </summary>
        /// <returns>true to display the vertical scroll bar when needed; otherwise, false. The default is true.</returns>
        [DefaultValue(true)]
        [Category("Scrolling")]
        [Description("Determines whether to show the vertical scroll bar when needed.")]
        public bool VScrollBar
        {
            get => Scintilla.VScrollBar;
            set => Scintilla.VScrollBar = value;
        }

        /// <summary>
        /// Gets or sets the size of the dots used to mark whitespace.
        /// </summary>
        /// <returns>The size of the dots used to mark whitespace. The default is 1.</returns>
        /// <seealso cref="ViewWhitespace" />
        [DefaultValue(1)]
        [Category("Whitespace")]
        [Description("The size of whitespace dots.")]
        public int WhitespaceSize
        {
            get => Scintilla.WhitespaceSize;
            set => Scintilla.WhitespaceSize = value;
        }

        /// <summary>
        /// Gets or sets the characters considered 'word' characters when using any word-based logic.
        /// </summary>
        /// <returns>A string of word characters.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string WordChars
        {
            get => Scintilla.WordChars;
            set => Scintilla.WordChars = value;
        }

        /// <summary>
        /// Gets or sets the line wrapping indent mode.
        /// </summary>
        /// <returns>
        /// One of the <see cref="ScintillaNET.WrapIndentMode" /> enumeration values.
        /// The default is <see cref="ScintillaNET.WrapIndentMode.Fixed" />.
        /// </returns>
        [DefaultValue(WrapIndentMode.Fixed)]
        [Category("Line Wrapping")]
        [Description("Determines how wrapped sublines are indented.")]
        public WrapIndentMode WrapIndentMode
        {
            get => Scintilla.WrapIndentMode;
            set => Scintilla.WrapIndentMode = value;
        }

        /// <summary>
        /// Gets or sets the line wrapping mode.
        /// </summary>
        /// <returns>
        /// One of the <see cref="ScintillaNET.WrapMode" /> enumeration values.
        /// The default is <see cref="ScintillaNET.WrapMode.None" />.
        /// </returns>
        [DefaultValue(WrapMode.None)]
        [Category("Line Wrapping")]
        [Description("The line wrapping strategy.")]
        public WrapMode WrapMode
        {
            get => Scintilla.WrapMode;
            set => Scintilla.WrapMode = value;
        }

        /// <summary>
        /// Gets or sets the indented size in pixels of wrapped sublines.
        /// </summary>
        /// <returns>The indented size of wrapped sublines measured in pixels. The default is 0.</returns>
        /// <remarks>
        /// Setting <see cref="WrapVisualFlags" /> to <see cref="ScintillaNET.WrapVisualFlags.Start" /> will add an
        /// additional 1 pixel to the value specified.
        /// </remarks>
        [DefaultValue(0)]
        [Category("Line Wrapping")]
        [Description("The amount of pixels to indent wrapped sublines.")]
        public int WrapStartIndent
        {
            get => Scintilla.WrapStartIndent;
            set => Scintilla.WrapStartIndent = value;
        }

        /// <summary>
        /// Gets or sets the wrap visual flags.
        /// </summary>
        /// <returns>
        /// A bitwise combination of the <see cref="ScintillaNET.WrapVisualFlags" /> enumeration.
        /// The default is <see cref="ScintillaNET.WrapVisualFlags.None" />.
        /// </returns>
        [DefaultValue(WrapVisualFlags.None)]
        [Category("Line Wrapping")]
        [Description("The visual indicator displayed on a wrapped line.")]
        [TypeConverter(typeof(FlagsEnumTypeConverter.FlagsEnumConverter))]
        public WrapVisualFlags WrapVisualFlags
        {
            get => Scintilla.WrapVisualFlags;
            set => Scintilla.WrapVisualFlags = value;
        }

        /// <summary>
        /// Gets or sets additional location options when displaying wrap visual flags.
        /// </summary>
        /// <returns>
        /// One of the <see cref="ScintillaNET.WrapVisualFlagLocation" /> enumeration values.
        /// The default is <see cref="ScintillaNET.WrapVisualFlagLocation.Default" />.
        /// </returns>
        [DefaultValue(WrapVisualFlagLocation.Default)]
        [Category("Line Wrapping")]
        [Description("The location of wrap visual flags in relation to the line text.")]
        public WrapVisualFlagLocation WrapVisualFlagLocation
        {
            get => Scintilla.WrapVisualFlagLocation;
            set => Scintilla.WrapVisualFlagLocation = value;
        }

        /// <summary>
        /// Gets or sets the horizontal scroll offset.
        /// </summary>
        /// <returns>The horizontal scroll offset in pixels.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int XOffset
        {
            get => Scintilla.XOffset;
            set => Scintilla.XOffset = value;
        }

        /// <summary>
        /// Gets or sets the zoom factor.
        /// </summary>
        /// <returns>The zoom factor measured in points.</returns>
        /// <remarks>For best results, values should range from -10 to 20 points.</remarks>
        /// <seealso cref="ZoomIn" />
        /// <seealso cref="ZoomOut" />
        [DefaultValue(0)]
        [Category("Appearance")]
        [Description("Zoom factor in points applied to the displayed text.")]
        public int Zoom
        {
            get => (int) this.GetValue(ZoomProperty);
            set => this.SetValue(ZoomProperty, value);
        }
        
        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(
            nameof(Zoom),
            typeof(int),
            typeof(ScintillaWPF),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnZoomChanged));

        private static void OnZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ScintillaWPF)?.OnZoomChanged(e);
        }

        private void OnZoomChanged(DependencyPropertyChangedEventArgs e)
        {
            var newValue = (int) e.NewValue;
            if (this.Scintilla.Zoom != newValue)
            {
                this.Scintilla.Zoom = newValue;
            }
        }

        #endregion Properties

        #region Events

        /// <summary>
        /// Occurs when an autocompletion list is cancelled.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs when an autocompletion list is cancelled.")]
        public event EventHandler<EventArgs> AutoCCancelled
        {
            add => Scintilla.AutoCCancelled += value;
            remove => Scintilla.AutoCCancelled -= value;
        }

        /// <summary>
        /// Occurs when the user deletes a character while an autocompletion list is active.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs when the user deletes a character while an autocompletion list is active.")]
        public event EventHandler<EventArgs> AutoCCharDeleted
        {
            add => Scintilla.AutoCCharDeleted += value;
            remove => Scintilla.AutoCCharDeleted -= value;
        }

        /// <summary>
        /// Occurs after autocompleted text is inserted.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs after autocompleted text has been inserted.")]
        public event EventHandler<AutoCSelectionEventArgs> AutoCCompleted
        {
            add => Scintilla.AutoCCompleted += value;
            remove => Scintilla.AutoCCompleted -= value;
        }

        /// <summary>
        /// Occurs when a user has selected an item in an autocompletion list.
        /// </summary>
        /// <remarks>Automatic insertion can be cancelled by calling <see cref="AutoCCancel" /> from the event handler.</remarks>
        [Category("Notifications")]
        [Description("Occurs when a user has selected an item in an autocompletion list.")]
        public event EventHandler<AutoCSelectionEventArgs> AutoCSelection
        {
            add => Scintilla.AutoCSelection += value;
            remove => Scintilla.AutoCSelection -= value;
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler BackColorChanged
        {
            add => Scintilla.BackColorChanged += value;
            remove => Scintilla.BackColorChanged -= value;
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler BackgroundImageChanged
        {
            add => Scintilla.BackgroundImageChanged += value;
            remove => Scintilla.BackgroundImageChanged -= value;
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler BackgroundImageLayoutChanged
        {
            add => Scintilla.BackgroundImageLayoutChanged += value;
            remove => Scintilla.BackgroundImageLayoutChanged -= value;
        }

        /// <summary>
        /// Occurs when text is about to be deleted.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs before text is deleted.")]
        public event EventHandler<BeforeModificationEventArgs> BeforeDelete
        {
            add => Scintilla.BeforeDelete += value;
            remove => Scintilla.BeforeDelete -= value;
        }

        /// <summary>
        /// Occurs when text is about to be inserted.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs before text is inserted.")]
        public event EventHandler<BeforeModificationEventArgs> BeforeInsert
        {
            add => Scintilla.BeforeInsert += value;
            remove => Scintilla.BeforeInsert -= value;
        }

        /// <summary>
        /// Occurs when the value of the <see cref="Scintilla.BorderStyle" /> property has changed.
        /// </summary>
        [Category("Property Changed")]
        [Description("Occurs when the value of the BorderStyle property changes.")]
        public event EventHandler BorderStyleChanged
        {
            add => Scintilla.BorderStyleChanged += value;
            remove => Scintilla.BorderStyleChanged -= value;
        }

        /// <summary>
        /// Occurs when an annotation has changed.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs when an annotation has changed.")]
        public event EventHandler<ChangeAnnotationEventArgs> ChangeAnnotation
        {
            add => Scintilla.ChangeAnnotation += value;
            remove => Scintilla.ChangeAnnotation -= value;
        }

        /// <summary>
        /// Occurs when the user enters a text character.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs when the user types a character.")]
        public event EventHandler<CharAddedEventArgs> CharAdded
        {
            add => Scintilla.CharAdded += value;
            remove => Scintilla.CharAdded -= value;
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler CursorChanged
        {
            add => Scintilla.CursorChanged += value;
            remove => Scintilla.CursorChanged -= value;
        }

        /// <summary>
        /// Occurs when text has been deleted from the document.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs when text is deleted.")]
        public event EventHandler<ModificationEventArgs> Delete
        {
            add => Scintilla.Delete += value;
            remove => Scintilla.Delete -= value;
        }

        /// <summary>
        /// Occurs when the <see cref="Scintilla" /> control is double-clicked.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs when the editor is double clicked.")]
        public event EventHandler<DoubleClickEventArgs> DoubleClick
        {
            add => Scintilla.DoubleClick += value;
            remove => Scintilla.DoubleClick -= value;
        }

        /// <summary>
        /// Occurs when the mouse moves or another activity such as a key press ends a <see cref="DwellStart" /> event.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs when the mouse moves from its dwell start position.")]
        public event EventHandler<DwellEventArgs> DwellEnd
        {
            add => Scintilla.DwellEnd += value;
            remove => Scintilla.DwellEnd -= value;
        }

        /// <summary>
        /// Occurs when the mouse is kept in one position (hovers) for the <see cref="MouseDwellTime" />.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs when the mouse is kept in one position (hovers) for a period of time.")]
        public event EventHandler<DwellEventArgs> DwellStart
        {
            add => Scintilla.DwellStart += value;
            remove => Scintilla.DwellStart -= value;
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler FontChanged
        {
            add => Scintilla.FontChanged += value;
            remove => Scintilla.FontChanged -= value;
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler ForeColorChanged
        {
            add => Scintilla.ForeColorChanged += value;
            remove => Scintilla.ForeColorChanged -= value;
        }

        /// <summary>
        /// Occurs when the user clicks on text that is in a style with the <see cref="Style.Hotspot" /> property set.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs when the user clicks text styled with the hotspot flag.")]
        public event EventHandler<HotspotClickEventArgs> HotspotClick
        {
            add => Scintilla.HotspotClick += value;
            remove => Scintilla.HotspotClick -= value;
        }

        /// <summary>
        /// Occurs when the user double clicks on text that is in a style with the <see cref="Style.Hotspot" /> property set.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs when the user double clicks text styled with the hotspot flag.")]
        public event EventHandler<HotspotClickEventArgs> HotspotDoubleClick
        {
            add => Scintilla.HotspotDoubleClick += value;
            remove => Scintilla.HotspotDoubleClick -= value;
        }

        /// <summary>
        /// Occurs when the user releases a click on text that is in a style with the <see cref="Style.Hotspot" /> property set.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs when the user releases a click on text styled with the hotspot flag.")]
        public event EventHandler<HotspotClickEventArgs> HotspotReleaseClick
        {
            add => Scintilla.HotspotReleaseClick += value;
            remove => Scintilla.HotspotReleaseClick -= value;
        }

        /// <summary>
        /// Occurs when the user clicks on text that has an indicator.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs when the user clicks text with an indicator.")]
        public event EventHandler<IndicatorClickEventArgs> IndicatorClick
        {
            add => Scintilla.IndicatorClick += value;
            remove => Scintilla.IndicatorClick -= value;
        }

        /// <summary>
        /// Occurs when the user releases a click on text that has an indicator.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs when the user releases a click on text with an indicator.")]
        public event EventHandler<IndicatorReleaseEventArgs> IndicatorRelease
        {
            add => Scintilla.IndicatorRelease += value;
            remove => Scintilla.IndicatorRelease -= value;
        }

        /// <summary>
        /// Occurs when text has been inserted into the document.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs when text is inserted.")]
        public event EventHandler<ModificationEventArgs> Insert
        {
            add => Scintilla.Insert += value;
            remove => Scintilla.Insert -= value;
        }

        /// <summary>
        /// Occurs when text is about to be inserted. The inserted text can be changed.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs before text is inserted. Permits changing the inserted text.")]
        public event EventHandler<InsertCheckEventArgs> InsertCheck
        {
            add => Scintilla.InsertCheck += value;
            remove => Scintilla.InsertCheck -= value;
        }

        /// <summary>
        /// Occurs when a key is pressed while the control has focus.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs when a key is pressed while the control has focus.")]
        public new event System.Windows.Forms.KeyEventHandler KeyDown
        {
            add => Scintilla.KeyDown += value;
            remove => Scintilla.KeyDown -= value;
        }

        /// <summary>
        /// Occurs when the mouse is over the control and the mouse button is pressed.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs when the mouse is over the control and the mouse button is pressed.")]
        public new event System.Windows.Forms.MouseEventHandler MouseDown
        {
            add => Scintilla.MouseDown += value;
            remove => Scintilla.MouseDown -= value;
        }

        /// <summary>
        /// Occurs when the mouse is over the control and the mouse button is released.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs when the mouse is over the control and the mouse button is released.")]
        public new event System.Windows.Forms.MouseEventHandler MouseUp
        {
            add => Scintilla.MouseUp += value;
            remove => Scintilla.MouseUp -= value;
        }

        /// <summary>
        /// Occurs when the mouse was clicked inside a margin that was marked as sensitive.
        /// </summary>
        /// <remarks>The <see cref="Margin.Sensitive" /> property must be set for a margin to raise this event.</remarks>
        [Category("Notifications")]
        [Description("Occurs when the mouse is clicked in a sensitive margin.")]
        public event EventHandler<MarginClickEventArgs> MarginClick
        {
            add => Scintilla.MarginClick += value;
            remove => Scintilla.MarginClick -= value;
        }

        // TODO This isn't working in my tests. Could be Windows Forms interfering.
        /// <summary>
        /// Occurs when the mouse was right-clicked inside a margin that was marked as sensitive.
        /// </summary>
        /// <remarks>The <see cref="Margin.Sensitive" /> property and <see cref="PopupMode.Text" /> must be set for a margin to raise this event.</remarks>
        /// <seealso cref="UsePopup(PopupMode)" />
        [Category("Notifications")]
        [Description("Occurs when the mouse is right-clicked in a sensitive margin.")]
        public event EventHandler<MarginClickEventArgs> MarginRightClick
        {
            add => Scintilla.MarginRightClick += value;
            remove => Scintilla.MarginRightClick -= value;
        }

        /// <summary>
        /// Occurs when a user attempts to change text while the document is in read-only mode.
        /// </summary>
        /// <seealso cref="ReadOnly" />
        [Category("Notifications")]
        [Description("Occurs when an attempt is made to change text in read-only mode.")]
        public event EventHandler<EventArgs> ModifyAttempt
        {
            add => Scintilla.ModifyAttempt += value;
            remove => Scintilla.ModifyAttempt -= value;
        }

        /// <summary>
        /// Occurs when the control determines hidden text needs to be shown.
        /// </summary>
        /// <remarks>An example of when this event might be raised is if the end of line of a contracted fold point is deleted.</remarks>
        [Category("Notifications")]
        [Description("Occurs when hidden (folded) text should be shown.")]
        public event EventHandler<NeedShownEventArgs> NeedShown
        {
            add => Scintilla.NeedShown += value;
            remove => Scintilla.NeedShown -= value;
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public event System.Windows.Forms.PaintEventHandler Paint
        {
            add => Scintilla.Paint += value;
            remove => Scintilla.Paint -= value;
        }

        /// <summary>
        /// Occurs when painting has just been done.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs when the control is painted.")]
        public event EventHandler<EventArgs> Painted
        {
            add => Scintilla.Painted += value;
            remove => Scintilla.Painted -= value;
        }

        /// <summary>
        /// Occurs when the document becomes 'dirty'.
        /// </summary>
        /// <remarks>The document 'dirty' state can be checked with the <see cref="Modified" /> property and reset by calling <see cref="SetSavePoint" />.</remarks>
        /// <seealso cref="SetSavePoint" />
        /// <seealso cref="SavePointReached" />
        [Category("Notifications")]
        [Description("Occurs when a save point is left and the document becomes dirty.")]
        public event EventHandler<EventArgs> SavePointLeft
        {
            add => Scintilla.SavePointLeft += value;
            remove => Scintilla.SavePointLeft -= value;
        }

        /// <summary>
        /// Occurs when the document 'dirty' flag is reset.
        /// </summary>
        /// <remarks>The document 'dirty' state can be reset by calling <see cref="SetSavePoint" /> or undoing an action that modified the document.</remarks>
        /// <seealso cref="SetSavePoint" />
        /// <seealso cref="SavePointLeft" />
        [Category("Notifications")]
        [Description("Occurs when a save point is reached and the document is no longer dirty.")]
        public event EventHandler<EventArgs> SavePointReached
        {
            add => Scintilla.SavePointReached += value;
            remove => Scintilla.SavePointReached -= value;
        }

        /// <summary>
        /// Occurs when the control is about to display or print text and requires styling.
        /// </summary>
        /// <remarks>
        /// This event is only raised when <see cref="Lexer" /> is set to <see cref="ScintillaNET.Lexer.Container" />.
        /// The last position styled correctly can be determined by calling <see cref="GetEndStyled" />.
        /// </remarks>
        /// <seealso cref="GetEndStyled" />
        [Category("Notifications")]
        [Description("Occurs when the text needs styling.")]
        public event EventHandler<StyleNeededEventArgs> StyleNeeded
        {
            add => Scintilla.StyleNeeded += value;
            remove => Scintilla.StyleNeeded -= value;
        }

        /// <summary>
        /// Occurs when the control UI is updated as a result of changes to text (including styling),
        /// selection, and/or scroll positions.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs when the control UI is updated.")]
        public event EventHandler<UpdateUIEventArgs> UpdateUI
        {
            add => Scintilla.UpdateUI += value;
            remove => Scintilla.UpdateUI -= value;
        }

        /// <summary>
        /// Occurs when the user zooms the display using the keyboard or the <see cref="Zoom" /> property is changed.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs when the control is zoomed.")]
        public event EventHandler<EventArgs> ZoomChanged
        {
            add => Scintilla.ZoomChanged += value;
            remove => Scintilla.ZoomChanged -= value;
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Increases the reference count of the specified document by 1.
        /// </summary>
        /// <param name="document">The document reference count to increase.</param>
        public void AddRefDocument(Document document)
        {
            Scintilla.AddRefDocument(document);
        }

        /// <summary>
        /// Adds an additional selection range to the existing main selection.
        /// </summary>
        /// <param name="caret">The zero-based document position to end the selection.</param>
        /// <param name="anchor">The zero-based document position to start the selection.</param>
        /// <remarks>A main selection must first have been set by a call to <see cref="SetSelection" />.</remarks>
        public void AddSelection(int caret, int anchor)
        {
            Scintilla.AddSelection(caret, anchor);
        }

        /// <summary>
        /// Inserts the specified text at the current caret position.
        /// </summary>
        /// <param name="text">The text to insert at the current caret position.</param>
        /// <remarks>The caret position is set to the end of the inserted text, but it is not scrolled into view.</remarks>
        public new void AddText(string text)
        {
            Scintilla.AddText(text);
        }

        /// <summary>
        /// Removes the annotation text for every <see cref="Line" /> in the document.
        /// </summary>
        public void AnnotationClearAll()
        {
            Scintilla.AnnotationClearAll();
        }

        /// <summary>
        /// Adds the specified text to the end of the document.
        /// </summary>
        /// <param name="text">The text to add to the document.</param>
        /// <remarks>The current selection is not changed and the new text is not scrolled into view.</remarks>
        public void AppendText(string text)
        {
            Scintilla.AppendText(text);
        }

        /// <summary>
        /// Assigns the specified key definition to a <see cref="Scintilla" /> command.
        /// </summary>
        /// <param name="keyDefinition">The key combination to bind.</param>
        /// <param name="sciCommand">The command to assign.</param>
        public void AssignCmdKey(System.Windows.Forms.Keys keyDefinition, Command sciCommand)
        {
            Scintilla.AssignCmdKey(keyDefinition, sciCommand);
        }

        /// <summary>
        /// Cancels any displayed autocompletion list.
        /// </summary>
        /// <seealso cref="AutoCStops" />
        public void AutoCCancel()
        {
            Scintilla.AutoCCancel();
        }

        /// <summary>
        /// Triggers completion of the current autocompletion word.
        /// </summary>
        public void AutoCComplete()
        {
            Scintilla.AutoCComplete();
        }

        /// <summary>
        /// Selects an item in the autocompletion list.
        /// </summary>
        /// <param name="select">
        /// The autocompletion word to select.
        /// If found, the word in the autocompletion list is selected and the index can be obtained by calling <see cref="AutoCCurrent" />.
        /// If not found, the behavior is determined by <see cref="AutoCAutoHide" />.
        /// </param>
        /// <remarks>
        /// Comparisons are performed according to the <see cref="AutoCIgnoreCase" /> property
        /// and will match the first word starting with <paramref name="select" />.
        /// </remarks>
        /// <seealso cref="AutoCCurrent" />
        /// <seealso cref="AutoCAutoHide" />
        /// <seealso cref="AutoCIgnoreCase" />
        public void AutoCSelect(string select)
        {
            Scintilla.AutoCSelect(select);
        }

        /// <summary>
        /// Sets the characters that, when typed, cause the autocompletion item to be added to the document.
        /// </summary>
        /// <param name="chars">A string of characters that trigger autocompletion. The default is null.</param>
        /// <remarks>Common fillup characters are '(', '[', and '.' depending on the language.</remarks>
        public void AutoCSetFillUps(string chars)
        {
            Scintilla.AutoCSetFillUps(chars);
        }

        /// <summary>
        /// Displays an auto completion list.
        /// </summary>
        /// <param name="lenEntered">The number of characters already entered to match on.</param>
        /// <param name="list">A list of autocompletion words separated by the <see cref="AutoCSeparator" /> character.</param>
        public void AutoCShow(int lenEntered, string list)
        {
            Scintilla.AutoCShow(lenEntered, list);
        }

        /// <summary>
        /// Specifies the characters that will automatically cancel autocompletion without the need to call <see cref="AutoCCancel" />.
        /// </summary>
        /// <param name="chars">A String of the characters that will cancel autocompletion. The default is empty.</param>
        /// <remarks>Characters specified should be limited to printable ASCII characters.</remarks>
        public void AutoCStops(string chars)
        {
            Scintilla.AutoCStops(chars);
        }

        /// <summary>
        /// Marks the beginning of a set of actions that should be treated as a single undo action.
        /// </summary>
        /// <remarks>A call to <see cref="BeginUndoAction" /> should be followed by a call to <see cref="EndUndoAction" />.</remarks>
        /// <seealso cref="EndUndoAction" />
        public void BeginUndoAction()
        {
            Scintilla.BeginUndoAction();
        }

        /// <summary>
        /// Styles the specified character position with the <see cref="Style.BraceBad" /> style when there is an unmatched brace.
        /// </summary>
        /// <param name="position">The zero-based document position of the unmatched brace character or <seealso cref="InvalidPosition"/> to remove the highlight.</param>
        public void BraceBadLight(int position)
        {
            Scintilla.BraceBadLight(position);
        }

        /// <summary>
        /// Styles the specified character positions with the <see cref="Style.BraceLight" /> style.
        /// </summary>
        /// <param name="position1">The zero-based document position of the open brace character.</param>
        /// <param name="position2">The zero-based document position of the close brace character.</param>
        /// <remarks>Brace highlighting can be removed by specifying <see cref="InvalidPosition" /> for <paramref name="position1" /> and <paramref name="position2" />.</remarks>
        /// <seealso cref="HighlightGuide" />
        public void BraceHighlight(int position1, int position2)
        {
            Scintilla.BraceHighlight(position1, position2);
        }

        /// <summary>
        /// Finds a corresponding matching brace starting at the position specified.
        /// The brace characters handled are '(', ')', '[', ']', '{', '}', '&lt;', and '&gt;'.
        /// </summary>
        /// <param name="position">The zero-based document position of a brace character to start the search from for a matching brace character.</param>
        /// <returns>The zero-based document position of the corresponding matching brace or <see cref="InvalidPosition" /> it no matching brace could be found.</returns>
        /// <remarks>A match only occurs if the style of the matching brace is the same as the starting brace. Nested braces are handled correctly.</remarks>
        public int BraceMatch(int position)
        {
            return Scintilla.BraceMatch(position);
        }

        /// <summary>
        /// Cancels the display of a call tip window.
        /// </summary>
        public void CallTipCancel()
        {
            Scintilla.CallTipCancel();
        }

        /// <summary>
        /// Sets the color of highlighted text in a call tip.
        /// </summary>
        /// <param name="color">The new highlight text Color. The default is dark blue.</param>
        public void CallTipSetForeHlt(Color color)
        {
            Scintilla.CallTipSetForeHlt(DrawingColor(color));
        }

        /// <summary>
        /// Sets the specified range of the call tip text to display in a highlighted style.
        /// </summary>
        /// <param name="hlStart">The zero-based index in the call tip text to start highlighting.</param>
        /// <param name="hlEnd">The zero-based index in the call tip text to stop highlighting (exclusive).</param>
        public void CallTipSetHlt(int hlStart, int hlEnd)
        {
            Scintilla.CallTipSetHlt(hlStart, hlEnd);
        }

        /// <summary>
        /// Determines whether to display a call tip above or below text.
        /// </summary>
        /// <param name="above">true to display above text; otherwise, false. The default is false.</param>
        public void CallTipSetPosition(bool above)
        {
            Scintilla.CallTipSetPosition(above);
        }

        /// <summary>
        /// Displays a call tip window.
        /// </summary>
        /// <param name="posStart">The zero-based document position where the call tip window should be aligned.</param>
        /// <param name="definition">The call tip text.</param>
        /// <remarks>
        /// Call tips can contain multiple lines separated by '\n' characters. Do not include '\r', as this will most likely print as an empty box.
        /// The '\t' character is supported and the size can be set by using <see cref="CallTipTabSize" />.
        /// </remarks>
        public void CallTipShow(int posStart, string definition)
        {
            Scintilla.CallTipShow(posStart, definition);
        }

        /// <summary>
        /// Sets the call tip tab size in pixels.
        /// </summary>
        /// <param name="tabSize">The width in pixels of a tab '\t' character in a call tip. Specifying 0 disables special treatment of tabs.</param>
        public void CallTipTabSize(int tabSize)
        {
            Scintilla.CallTipTabSize(tabSize);
        }

        /// <summary>
        /// Indicates to the current <see cref="Lexer" /> that the internal lexer state has changed in the specified
        /// range and therefore may need to be redrawn.
        /// </summary>
        /// <param name="startPos">The zero-based document position at which the lexer state change starts.</param>
        /// <param name="endPos">The zero-based document position at which the lexer state change ends.</param>
        public void ChangeLexerState(int startPos, int endPos)
        {
            Scintilla.ChangeLexerState(startPos, endPos);
        }

        /// <summary>
        /// Finds the closest character position to the specified display point.
        /// </summary>
        /// <param name="x">The x pixel coordinate within the client rectangle of the control.</param>
        /// <param name="y">The y pixel coordinate within the client rectangle of the control.</param>
        /// <returns>The zero-based document position of the nearest character to the point specified.</returns>
        public int CharPositionFromPoint(int x, int y)
        {
            return Scintilla.CharPositionFromPoint(x, y);
        }

        /// <summary>
        /// Finds the closest character position to the specified display point or returns -1
        /// if the point is outside the window or not close to any characters.
        /// </summary>
        /// <param name="x">The x pixel coordinate within the client rectangle of the control.</param>
        /// <param name="y">The y pixel coordinate within the client rectangle of the control.</param>
        /// <returns>The zero-based document position of the nearest character to the point specified when near a character; otherwise, -1.</returns>
        public int CharPositionFromPointClose(int x, int y)
        {
            return Scintilla.CharPositionFromPointClose(x, y);
        }

        /// <summary>
        /// Explicitly sets the current horizontal offset of the caret as the X position to track
        /// when the user moves the caret vertically using the up and down keys.
        /// </summary>
        /// <remarks>
        /// When not set explicitly, Scintilla automatically sets this value each time the user moves
        /// the caret horizontally.
        /// </remarks>
        public void ChooseCaretX()
        {
            Scintilla.ChooseCaretX();
        }

        /// <summary>
        /// Removes the selected text from the document.
        /// </summary>
        public void Clear()
        {
            Scintilla.Clear();
        }

        /// <summary>
        /// Deletes all document text, unless the document is read-only.
        /// </summary>
        public void ClearAll()
        {
            Scintilla.ClearAll();
        }

        /// <summary>
        /// Makes the specified key definition do nothing.
        /// </summary>
        /// <param name="keyDefinition">The key combination to bind.</param>
        /// <remarks>This is equivalent to binding the keys to <see cref="Command.Null" />.</remarks>
        public void ClearCmdKey(System.Windows.Forms.Keys keyDefinition)
        {
            Scintilla.ClearCmdKey(keyDefinition);
        }

        /// <summary>
        /// Removes all the key definition command mappings.
        /// </summary>
        public void ClearAllCmdKeys()
        {
            Scintilla.ClearAllCmdKeys();
        }

        /// <summary>
        /// Removes all styling from the document and resets the folding state.
        /// </summary>
        public void ClearDocumentStyle()
        {
            Scintilla.ClearDocumentStyle();
        }

        /// <summary>
        /// Removes all images registered for autocompletion lists.
        /// </summary>
        public void ClearRegisteredImages()
        {
            Scintilla.ClearRegisteredImages();
        }

        /// <summary>
        /// Sets a single empty selection at the start of the document.
        /// </summary>
        public void ClearSelections()
        {
            Scintilla.ClearSelections();
        }

        /// <summary>
        /// Requests that the current lexer restyle the specified range.
        /// </summary>
        /// <param name="startPos">The zero-based document position at which to start styling.</param>
        /// <param name="endPos">The zero-based document position at which to stop styling (exclusive).</param>
        /// <remarks>This will also cause fold levels in the range specified to be reset.</remarks>
        public void Colorize(int startPos, int endPos)
        {
            Scintilla.Colorize(startPos, endPos);
        }

        /// <summary>
        /// Changes all end-of-line characters in the document to the format specified.
        /// </summary>
        /// <param name="eolMode">One of the <see cref="Eol" /> enumeration values.</param>
        public void ConvertEols(Eol eolMode)
        {
            Scintilla.ConvertEols(eolMode);
        }

        /// <summary>
        /// Copies the selected text from the document and places it on the clipboard.
        /// </summary>
        public void Copy()
        {
            Scintilla.Copy();
        }

        /// <summary>
        /// Copies the selected text from the document and places it on the clipboard.
        /// </summary>
        /// <param name="format">One of the <see cref="CopyFormat" /> enumeration values.</param>
        public void Copy(CopyFormat format)
        {
            Scintilla.Copy(format);
        }

        /// <summary>
        /// Copies the selected text from the document and places it on the clipboard.
        /// If the selection is empty the current line is copied.
        /// </summary>
        /// <remarks>
        /// If the selection is empty and the current line copied, an extra "MSDEVLineSelect" marker is added to the
        /// clipboard which is then used in <see cref="Paste" /> to paste the whole line before the current line.
        /// </remarks>
        public void CopyAllowLine()
        {
            Scintilla.CopyAllowLine();
        }

        /// <summary>
        /// Copies the selected text from the document and places it on the clipboard.
        /// If the selection is empty the current line is copied.
        /// </summary>
        /// <param name="format">One of the <see cref="CopyFormat" /> enumeration values.</param>
        /// <remarks>
        /// If the selection is empty and the current line copied, an extra "MSDEVLineSelect" marker is added to the
        /// clipboard which is then used in <see cref="Paste" /> to paste the whole line before the current line.
        /// </remarks>
        public void CopyAllowLine(CopyFormat format)
        {
            Scintilla.CopyAllowLine(format);
        }

        /// <summary>
        /// Copies the specified range of text to the clipboard.
        /// </summary>
        /// <param name="start">The zero-based character position in the document to start copying.</param>
        /// <param name="end">The zero-based character position (exclusive) in the document to stop copying.</param>
        public void CopyRange(int start, int end)
        {
            Scintilla.CopyRange(start, end);
        }

        /// <summary>
        /// Copies the specified range of text to the clipboard.
        /// </summary>
        /// <param name="start">The zero-based character position in the document to start copying.</param>
        /// <param name="end">The zero-based character position (exclusive) in the document to stop copying.</param>
        /// <param name="format">One of the <see cref="CopyFormat" /> enumeration values.</param>
        public void CopyRange(int start, int end, CopyFormat format)
        {
            Scintilla.CopyRange(start, end, format);
        }

        /// <summary>
        /// Create a new, empty document.
        /// </summary>
        /// <returns>A new <see cref="Document" /> with a reference count of 1.</returns>
        /// <remarks>You are responsible for ensuring the reference count eventually reaches 0 or memory leaks will occur.</remarks>
        public Document CreateDocument()
        {
            return Scintilla.CreateDocument();
        }

        /// <summary>
        /// Creates an <see cref="ILoader" /> object capable of loading a <see cref="Document" /> on a background (non-UI) thread.
        /// </summary>
        /// <param name="length">The initial number of characters to allocate.</param>
        /// <returns>A new <see cref="ILoader" /> object, or null if the loader could not be created.</returns>
        public ILoader CreateLoader(int length)
        {
            return Scintilla.CreateLoader(length);
        }

        /// <summary>
        /// Cuts the selected text from the document and places it on the clipboard.
        /// </summary>
        public void Cut()
        {
            Scintilla.Cut();
        }

        /// <summary>
        /// Deletes a range of text from the document.
        /// </summary>
        /// <param name="position">The zero-based character position to start deleting.</param>
        /// <param name="length">The number of characters to delete.</param>
        public void DeleteRange(int position, int length)
        {
            Scintilla.DeleteRange(position, length);
        }

        /// <summary>
        /// Retrieves a description of keyword sets supported by the current <see cref="Lexer" />.
        /// </summary>
        /// <returns>A String describing each keyword set separated by line breaks for the current lexer.</returns>
        public string DescribeKeywordSets()
        {
            return Scintilla.DescribeKeywordSets();
        }

        /// <summary>
        /// Retrieves a brief description of the specified property name for the current <see cref="Lexer" />.
        /// </summary>
        /// <param name="name">A property name supported by the current <see cref="Lexer" />.</param>
        /// <returns>A String describing the lexer property name if found; otherwise, String.Empty.</returns>
        /// <remarks>A list of supported property names for the current <see cref="Lexer" /> can be obtained by calling <see cref="PropertyNames" />.</remarks>
        public string DescribeProperty(string name)
        {
            return Scintilla.DescribeProperty(name);
        }

        /// <summary>
        /// Sends the specified message directly to the native Scintilla window,
        /// bypassing any managed APIs.
        /// </summary>
        /// <param name="msg">The message ID.</param>
        /// <param name="wParam">The message <c>wparam</c> field.</param>
        /// <param name="lParam">The message <c>lparam</c> field.</param>
        /// <returns>An <see cref="IntPtr"/> representing the result of the message request.</returns>
        /// <remarks>This API supports the Scintilla infrastructure and is not intended to be used directly from your code.</remarks>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public virtual IntPtr DirectMessage(int msg, IntPtr wParam, IntPtr lParam)
        {
            return Scintilla.DirectMessage(msg, wParam, lParam);
        }

        /// <summary>
        /// Returns the zero-based document line index from the specified display line index.
        /// </summary>
        /// <param name="displayLine">The zero-based display line index.</param>
        /// <returns>The zero-based document line index.</returns>
        /// <seealso cref="Line.DisplayIndex" />
        public int DocLineFromVisible(int displayLine)
        {
            return Scintilla.DocLineFromVisible(displayLine);
        }

        /// <summary>
        /// If there are multiple selections, removes the specified selection.
        /// </summary>
        /// <param name="selection">The zero-based selection index.</param>
        /// <seealso cref="Selections" />
        public void DropSelection(int selection)
        {
            Scintilla.DropSelection(selection);
        }

        /// <summary>
        /// Clears any undo or redo history.
        /// </summary>
        /// <remarks>This will also cause <see cref="SetSavePoint" /> to be called but will not raise the <see cref="SavePointReached" /> event.</remarks>
        public void EmptyUndoBuffer()
        {
            Scintilla.EmptyUndoBuffer();
        }

        /// <summary>
        /// Marks the end of a set of actions that should be treated as a single undo action.
        /// </summary>
        /// <seealso cref="BeginUndoAction" />
        public void EndUndoAction()
        {
            Scintilla.EndUndoAction();
        }

        /// <summary>
        /// Performs the specified <see cref="Scintilla" />command.
        /// </summary>
        /// <param name="sciCommand">The command to perform.</param>
        public void ExecuteCmd(Command sciCommand)
        {
            Scintilla.ExecuteCmd(sciCommand);
        }

        /// <summary>
        /// Performs the specified fold action on the entire document.
        /// </summary>
        /// <param name="action">One of the <see cref="FoldAction" /> enumeration values.</param>
        /// <remarks>When using <see cref="FoldAction.Toggle" /> the first fold header in the document is examined to decide whether to expand or contract.</remarks>
        public void FoldAll(FoldAction action)
        {
            Scintilla.FoldAll(action);
        }

        /// <summary>
        /// Changes the appearance of fold text tags.
        /// </summary>
        /// <param name="style">One of the <see cref="FoldDisplayText" /> enumeration values.</param>
        /// <remarks>The text tag to display on a folded line can be set using <see cref="Line.ToggleFoldShowText" />.</remarks>
        /// <seealso cref="Line.ToggleFoldShowText" />.
        public void FoldDisplayTextSetStyle(FoldDisplayText style)
        {
            Scintilla.FoldDisplayTextSetStyle(style);
        }

        /// <summary>
        /// Returns the character as the specified document position.
        /// </summary>
        /// <param name="position">The zero-based document position of the character to get.</param>
        /// <returns>The character at the specified <paramref name="position" />.</returns>
        public int GetCharAt(int position)
        {
            return Scintilla.GetCharAt(position);
        }

        /// <summary>
        /// Returns the column number of the specified document position, taking the width of tabs into account.
        /// </summary>
        /// <param name="position">The zero-based document position to get the column for.</param>
        /// <returns>The number of columns from the start of the line to the specified document <paramref name="position" />.</returns>
        public int GetColumn(int position)
        {
            return Scintilla.GetColumn(position);
        }

        /// <summary>
        /// Returns the last document position likely to be styled correctly.
        /// </summary>
        /// <returns>The zero-based document position of the last styled character.</returns>
        public int GetEndStyled()
        {
            return Scintilla.GetEndStyled();
        }

        /// <summary>
        /// Lookup a property value for the current <see cref="Lexer" />.
        /// </summary>
        /// <param name="name">The property name to lookup.</param>
        /// <returns>
        /// A String representing the property value if found; otherwise, String.Empty.
        /// Any embedded property name macros as described in <see cref="SetProperty" /> will not be replaced (expanded).
        /// </returns>
        /// <seealso cref="GetPropertyExpanded" />
        public string GetProperty(string name)
        {
            return Scintilla.GetProperty(name);
        }

        /// <summary>
        /// Lookup a property value for the current <see cref="Lexer" /> and expand any embedded property macros.
        /// </summary>
        /// <param name="name">The property name to lookup.</param>
        /// <returns>
        /// A String representing the property value if found; otherwise, String.Empty.
        /// Any embedded property name macros as described in <see cref="SetProperty" /> will be replaced (expanded).
        /// </returns>
        /// <seealso cref="GetProperty" />
        public string GetPropertyExpanded(string name)
        {
            return Scintilla.GetPropertyExpanded(name);
        }

        /// <summary>
        /// Lookup a property value for the current <see cref="Lexer" /> and convert it to an integer.
        /// </summary>
        /// <param name="name">The property name to lookup.</param>
        /// <param name="defaultValue">A default value to return if the property name is not found or has no value.</param>
        /// <returns>
        /// An Integer representing the property value if found;
        /// otherwise, <paramref name="defaultValue" /> if not found or the property has no value;
        /// otherwise, 0 if the property is not a number.
        /// </returns>
        public int GetPropertyInt(string name, int defaultValue)
        {
            return Scintilla.GetPropertyInt(name, defaultValue);
        }

        /// <summary>
        /// Gets the style of the specified document position.
        /// </summary>
        /// <param name="position">The zero-based document position of the character to get the style for.</param>
        /// <returns>The zero-based <see cref="Style" /> index used at the specified <paramref name="position" />.</returns>
        public int GetStyleAt(int position)
        {
            return Scintilla.GetStyleAt(position);
        }

        /// <summary>
        /// Returns the capture group text of the most recent regular expression search.
        /// </summary>
        /// <param name="tagNumber">The capture group (1 through 9) to get the text for.</param>
        /// <returns>A String containing the capture group text if it participated in the match; otherwise, an empty string.</returns>
        /// <seealso cref="SearchInTarget" />
        public string GetTag(int tagNumber)
        {
            return Scintilla.GetTag(tagNumber);
        }

        /// <summary>
        /// Gets a range of text from the document.
        /// </summary>
        /// <param name="position">The zero-based starting character position of the range to get.</param>
        /// <param name="length">The number of characters to get.</param>
        /// <returns>A string representing the text range.</returns>
        public string GetTextRange(int position, int length)
        {
            return Scintilla.GetTextRange(position, length);
        }

        /// <summary>
        /// Gets a range of text from the document formatted as Hypertext Markup Language (HTML).
        /// </summary>
        /// <param name="position">The zero-based starting character position of the range to get.</param>
        /// <param name="length">The number of characters to get.</param>
        /// <returns>A string representing the text range formatted as HTML.</returns>
        public string GetTextRangeAsHtml(int position, int length)
        {
            return Scintilla.GetTextRangeAsHtml(position, length);
        }

        /// <summary>
        /// Returns the version information of the native Scintilla library.
        /// </summary>
        /// <returns>An object representing the version information of the native Scintilla library.</returns>
        public System.Diagnostics.FileVersionInfo GetVersionInfo()
        {
            return Scintilla.GetVersionInfo();
        }

        ///<summary>
        /// Gets the word from the position specified.
        /// </summary>
        /// <param name="position">The zero-based document character position to get the word from.</param>
        /// <returns>The word at the specified position.</returns>
        public string GetWordFromPosition(int position)
        {
            return Scintilla.GetWordFromPosition(position);
        }

        /// <summary>
        /// Navigates the caret to the document position specified.
        /// </summary>
        /// <param name="position">The zero-based document character position to navigate to.</param>
        /// <remarks>Any selection is discarded.</remarks>
        public void GotoPosition(int position)
        {
            Scintilla.GotoPosition(position);
        }

        /// <summary>
        /// Hides the range of lines specified.
        /// </summary>
        /// <param name="lineStart">The zero-based index of the line range to start hiding.</param>
        /// <param name="lineEnd">The zero-based index of the line range to end hiding.</param>
        /// <seealso cref="ShowLines" />
        /// <seealso cref="Line.Visible" />
        public void HideLines(int lineStart, int lineEnd)
        {
            Scintilla.HideLines(lineStart, lineEnd);
        }

        /// <summary>
        /// Returns a bitmap representing the 32 indicators in use at the specified position.
        /// </summary>
        /// <param name="position">The zero-based character position within the document to test.</param>
        /// <returns>A bitmap indicating which of the 32 indicators are in use at the specified <paramref name="position" />.</returns>
        public uint IndicatorAllOnFor(int position)
        {
            return Scintilla.IndicatorAllOnFor(position);
        }

        /// <summary>
        /// Removes the <see cref="IndicatorCurrent" /> indicator (and user-defined value) from the specified range of text.
        /// </summary>
        /// <param name="position">The zero-based character position within the document to start clearing.</param>
        /// <param name="length">The number of characters to clear.</param>
        public void IndicatorClearRange(int position, int length)
        {
            Scintilla.IndicatorClearRange(position, length);
        }

        /// <summary>
        /// Adds the <see cref="IndicatorCurrent" /> indicator and <see cref="IndicatorValue" /> value to the specified range of text.
        /// </summary>
        /// <param name="position">The zero-based character position within the document to start filling.</param>
        /// <param name="length">The number of characters to fill.</param>
        public void IndicatorFillRange(int position, int length)
        {
            Scintilla.IndicatorFillRange(position, length);
        }

        /// <summary>
        /// Inserts text at the specified position.
        /// </summary>
        /// <param name="position">The zero-based character position to insert the text. Specify -1 to use the current caret position.</param>
        /// <param name="text">The text to insert into the document.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="position" /> less than zero and not equal to -1. -or-
        /// <paramref name="position" /> is greater than the document length.
        /// </exception>
        /// <remarks>No scrolling is performed.</remarks>
        public void InsertText(int position, string text)
        {
            Scintilla.InsertText(position, text);
        }

        /// <summary>
        /// Determines whether the specified <paramref name="start" /> and <paramref name="end" /> positions are
        /// at the beginning and end of a word, respectively.
        /// </summary>
        /// <param name="start">The zero-based document position of the possible word start.</param>
        /// <param name="end">The zero-based document position of the possible word end.</param>
        /// <returns>
        /// true if <paramref name="start" /> and <paramref name="end" /> are at the beginning and end of a word, respectively;
        /// otherwise, false.
        /// </returns>
        /// <remarks>
        /// This method does not check whether there is whitespace in the search range,
        /// only that the <paramref name="start" /> and <paramref name="end" /> are at word boundaries.
        /// </remarks>
        public bool IsRangeWord(int start, int end)
        {
            return Scintilla.IsRangeWord(start, end);
        }

        /// <summary>
        /// Returns the line that contains the document position specified.
        /// </summary>
        /// <param name="position">The zero-based document character position.</param>
        /// <returns>The zero-based document line index containing the character <paramref name="position" />.</returns>
        public int LineFromPosition(int position)
        {
            return Scintilla.LineFromPosition(position);
        }

        /// <summary>
        /// Scrolls the display the number of lines and columns specified.
        /// </summary>
        /// <param name="lines">The number of lines to scroll.</param>
        /// <param name="columns">The number of columns to scroll.</param>
        /// <remarks>
        /// Negative values scroll in the opposite direction.
        /// A column is the width in pixels of a space character in the <see cref="Style.Default" /> style.
        /// </remarks>
        public void LineScroll(int lines, int columns)
        {
            Scintilla.LineScroll(lines, columns);
        }

        /// <summary>
        /// Loads a <see cref="Scintilla" /> compatible lexer from an external DLL.
        /// </summary>
        /// <param name="path">The path to the external lexer DLL.</param>
        public void LoadLexerLibrary(string path)
        {
            Scintilla.LoadLexerLibrary(path);
        }

        /// <summary>
        /// Removes the specified marker from all lines.
        /// </summary>
        /// <param name="marker">The zero-based <see cref="Marker" /> index to remove from all lines, or -1 to remove all markers from all lines.</param>
        public void MarkerDeleteAll(int marker)
        {
            Scintilla.MarkerDeleteAll(marker);
        }

        /// <summary>
        /// Searches the document for the marker handle and deletes the marker if found.
        /// </summary>
        /// <param name="markerHandle">The <see cref="MarkerHandle" /> created by a previous call to <see cref="Line.MarkerAdd" /> of the marker to delete.</param>
        public void MarkerDeleteHandle(MarkerHandle markerHandle)
        {
            Scintilla.MarkerDeleteHandle(markerHandle);
        }

        /// <summary>
        /// Enable or disable highlighting of the current folding block.
        /// </summary>
        /// <param name="enabled">true to highlight the current folding block; otherwise, false.</param>
        public void MarkerEnableHighlight(bool enabled)
        {
            Scintilla.MarkerEnableHighlight(enabled);
        }

        /// <summary>
        /// Searches the document for the marker handle and returns the line number containing the marker if found.
        /// </summary>
        /// <param name="markerHandle">The <see cref="MarkerHandle" /> created by a previous call to <see cref="Line.MarkerAdd" /> of the marker to search for.</param>
        /// <returns>If found, the zero-based line index containing the marker; otherwise, -1.</returns>
        public int MarkerLineFromHandle(MarkerHandle markerHandle)
        {
            return Scintilla.MarkerLineFromHandle(markerHandle);
        }

        /// <summary>
        /// Specifies the long line indicator column number and color when <see cref="EdgeMode" /> is <see cref="EdgeMode.MultiLine" />.
        /// </summary>
        /// <param name="column">The zero-based column number to indicate.</param>
        /// <param name="edgeColor">The color of the vertical long line indicator.</param>
        /// <remarks>A column is defined as the width of a space character in the <see cref="Style.Default" /> style.</remarks>
        /// <seealso cref="MultiEdgeClearAll" />
        public void MultiEdgeAddLine(int column, Color edgeColor)
        {
            Scintilla.MultiEdgeAddLine(column, DrawingColor(edgeColor));
        }

        /// <summary>
        /// Removes all the long line column indicators specified using <seealso cref="MultiEdgeAddLine" />.
        /// </summary>
        /// <seealso cref="MultiEdgeAddLine" />
        public void MultiEdgeClearAll()
        {
            Scintilla.MultiEdgeClearAll();
        }

        /// <summary>
        /// Searches for all instances of the main selection within the <see cref="TargetStart" /> and <see cref="TargetEnd" />
        /// range and adds any matches to the selection.
        /// </summary>
        /// <remarks>
        /// The <see cref="SearchFlags" /> property is respected when searching, allowing additional
        /// selections to match on different case sensitivity and word search options.
        /// </remarks>
        /// <seealso cref="MultipleSelectAddNext" />
        public void MultipleSelectAddEach()
        {
            Scintilla.MultipleSelectAddEach();
        }

        /// <summary>
        /// Searches for the next instance of the main selection within the <see cref="TargetStart" /> and <see cref="TargetEnd" />
        /// range and adds any match to the selection.
        /// </summary>
        /// <remarks>
        /// The <see cref="SearchFlags" /> property is respected when searching, allowing additional
        /// selections to match on different case sensitivity and word search options.
        /// </remarks>
        /// <seealso cref="MultipleSelectAddNext" />
        public void MultipleSelectAddNext()
        {
            Scintilla.MultipleSelectAddNext();
        }

        /// <summary>
        /// Pastes the contents of the clipboard into the current selection.
        /// </summary>
        public void Paste()
        {
            Scintilla.Paste();
        }

        /// <summary>
        /// Returns the X display pixel location of the specified document position.
        /// </summary>
        /// <param name="pos">The zero-based document character position.</param>
        /// <returns>The x-coordinate of the specified <paramref name="pos" /> within the client rectangle of the control.</returns>
        public int PointXFromPosition(int pos)
        {
            return Scintilla.PointXFromPosition(pos);
        }

        /// <summary>
        /// Returns the Y display pixel location of the specified document position.
        /// </summary>
        /// <param name="pos">The zero-based document character position.</param>
        /// <returns>The y-coordinate of the specified <paramref name="pos" /> within the client rectangle of the control.</returns>
        public int PointYFromPosition(int pos)
        {
            return Scintilla.PointYFromPosition(pos);
        }

        /// <summary>
        /// Retrieves a list of property names that can be set for the current <see cref="Lexer" />.
        /// </summary>
        /// <returns>A String of property names separated by line breaks.</returns>
        public string PropertyNames()
        {
            return Scintilla.PropertyNames();
        }

        /// <summary>
        /// Retrieves the data type of the specified property name for the current <see cref="Lexer" />.
        /// </summary>
        /// <param name="name">A property name supported by the current <see cref="Lexer" />.</param>
        /// <returns>One of the <see cref="PropertyType" /> enumeration values. The default is <see cref="ScintillaNET.PropertyType.Boolean" />.</returns>
        /// <remarks>A list of supported property names for the current <see cref="Lexer" /> can be obtained by calling <see cref="PropertyNames" />.</remarks>
        public PropertyType PropertyType(string name)
        {
            return Scintilla.PropertyType(name);
        }

        /// <summary>
        /// Redoes the effect of an <see cref="Undo" /> operation.
        /// </summary>
        public void Redo()
        {
            Scintilla.Redo();
        }

        /// <summary>
        /// Maps the specified image to a type identifier for use in an autocompletion list.
        /// </summary>
        /// <param name="type">The numeric identifier for this image.</param>
        /// <param name="image">The Bitmap to use in an autocompletion list.</param>
        /// <remarks>
        /// The <paramref name="image" /> registered can be referenced by its <paramref name="type" /> identifer in an autocompletion
        /// list by suffixing a word with the <see cref="AutoCTypeSeparator" /> character and the <paramref name="type" /> value. e.g.
        /// "int?2 long?3 short?1" etc....
        /// </remarks>
        /// <seealso cref="AutoCTypeSeparator" />
        public void RegisterRgbaImage(int type, System.Drawing.Bitmap image)
        {
            Scintilla.RegisterRgbaImage(type, image);
        }

        /// <summary>
        /// Decreases the reference count of the specified document by 1.
        /// </summary>
        /// <param name="document">
        /// The document reference count to decrease.
        /// When a document's reference count reaches 0 it is destroyed and any associated memory released.
        /// </param>
        public void ReleaseDocument(Document document)
        {
            Scintilla.ReleaseDocument(document);
        }

        /// <summary>
        /// Replaces the current selection with the specified text.
        /// </summary>
        /// <param name="text">The text that should replace the current selection.</param>
        /// <remarks>
        /// If there is not a current selection, the text will be inserted at the current caret position.
        /// Following the operation the caret is placed at the end of the inserted text and scrolled into view.
        /// </remarks>
        public void ReplaceSelection(string text)
        {
            Scintilla.ReplaceSelection(text);
        }

        /// <summary>
        /// Replaces the target defined by <see cref="TargetStart" /> and <see cref="TargetEnd" /> with the specified <paramref name="text" />.
        /// </summary>
        /// <param name="text">The text that will replace the current target.</param>
        /// <returns>The length of the replaced text.</returns>
        /// <remarks>
        /// The <see cref="TargetStart" /> and <see cref="TargetEnd" /> properties will be updated to the start and end positions of the replaced text.
        /// The recommended way to delete text in the document is to set the target range to be removed and replace the target with an empty string.
        /// </remarks>
        public int ReplaceTarget(string text)
        {
            return Scintilla.SearchInTarget(text);
        }

        /// <summary>
        /// Replaces the target text defined by <see cref="TargetStart" /> and <see cref="TargetEnd" /> with the specified value after first substituting
        /// "\1" through "\9" macros in the <paramref name="text" /> with the most recent regular expression capture groups.
        /// </summary>
        /// <param name="text">The text containing "\n" macros that will be substituted with the most recent regular expression capture groups and then replace the current target.</param>
        /// <returns>The length of the replaced text.</returns>
        /// <remarks>
        /// The "\0" macro will be substituted by the entire matched text from the most recent search.
        /// The <see cref="TargetStart" /> and <see cref="TargetEnd" /> properties will be updated to the start and end positions of the replaced text.
        /// </remarks>
        /// <seealso cref="GetTag" />
        public int ReplaceTargetRe(string text)
        {
            return Scintilla.ReplaceTargetRe(text);
        }

        /// <summary>
        /// Makes the next selection the main selection.
        /// </summary>
        public void RotateSelection()
        {
            Scintilla.RotateSelection();
        }

        /// <summary>
        /// Scrolls the current position into view, if it is not already visible.
        /// </summary>
        public void ScrollCaret()
        {
            Scintilla.ScrollCaret();
        }

        /// <summary>
        /// Scrolls the specified range into view.
        /// </summary>
        /// <param name="start">The zero-based document start position to scroll to.</param>
        /// <param name="end">
        /// The zero-based document end position to scroll to if doing so does not cause the <paramref name="start" />
        /// position to scroll out of view.
        /// </param>
        /// <remarks>This may be used to make a search match visible.</remarks>
        public void ScrollRange(int start, int end)
        {
            Scintilla.ScrollRange(start, end);
        }

        /// <summary>
        /// Searches for the first occurrence of the specified text in the target defined by <see cref="TargetStart" /> and <see cref="TargetEnd" />.
        /// </summary>
        /// <param name="text">The text to search for. The interpretation of the text (i.e. whether it is a regular expression) is defined by the <see cref="SearchFlags" /> property.</param>
        /// <returns>The zero-based start position of the matched text within the document if successful; otherwise, -1.</returns>
        /// <remarks>
        /// If successful, the <see cref="TargetStart" /> and <see cref="TargetEnd" /> properties will be updated to the start and end positions of the matched text.
        /// Searching can be performed in reverse using a <see cref="TargetStart" /> greater than the <see cref="TargetEnd" />.
        /// </remarks>
        public int SearchInTarget(string text)
        {
            return Scintilla.SearchInTarget(text);
        }

        /// <summary>
        /// Selects all the text in the document.
        /// </summary>
        /// <remarks>The current position is not scrolled into view.</remarks>
        public void SelectAll()
        {
            Scintilla.SelectAll();
        }

        /// <summary>
        /// Sets the background color of additional selections.
        /// </summary>
        /// <param name="color">Additional selections background color.</param>
        /// <remarks>Calling <see cref="SetSelectionBackColor" /> will reset the <paramref name="color" /> specified.</remarks>
        public void SetAdditionalSelBack(Color color)
        {
            Scintilla.CallTipSetForeHlt(DrawingColor(color));
        }

        /// <summary>
        /// Sets the foreground color of additional selections.
        /// </summary>
        /// <param name="color">Additional selections foreground color.</param>
        /// <remarks>Calling <see cref="SetSelectionForeColor" /> will reset the <paramref name="color" /> specified.</remarks>
        public void SetAdditionalSelFore(Color color)
        {
            Scintilla.CallTipSetForeHlt(DrawingColor(color));
        }

        /// <summary>
        /// Removes any selection and places the caret at the specified position.
        /// </summary>
        /// <param name="pos">The zero-based document position to place the caret at.</param>
        /// <remarks>The caret is not scrolled into view.</remarks>
        public void SetEmptySelection(int pos)
        {
            Scintilla.SetEmptySelection(pos);
        }

        /// <summary>
        /// Sets additional options for displaying folds.
        /// </summary>
        /// <param name="flags">A bitwise combination of the <see cref="FoldFlags" /> enumeration.</param>
        public void SetFoldFlags(FoldFlags flags)
        {
            Scintilla.SetFoldFlags(flags);
        }

        /// <summary>
        /// Sets a global override to the fold margin color.
        /// </summary>
        /// <param name="use">true to override the fold margin color; otherwise, false.</param>
        /// <param name="color">The global fold margin color.</param>
        /// <seealso cref="SetFoldMarginHighlightColor" />
        public void SetFoldMarginColor(bool use, Color color)
        {
            Scintilla.SetFoldMarginColor(use, DrawingColor(color));
        }

        /// <summary>
        /// Sets a global override to the fold margin highlight color.
        /// </summary>
        /// <param name="use">true to override the fold margin highlight color; otherwise, false.</param>
        /// <param name="color">The global fold margin highlight color.</param>
        /// <seealso cref="SetFoldMarginColor" />
        public void SetFoldMarginHighlightColor(bool use, Color color)
        {
            Scintilla.SetFoldMarginHighlightColor(use, DrawingColor(color));
        }

        /// <summary>
        /// Updates a keyword set used by the current <see cref="Lexer" />.
        /// </summary>
        /// <param name="set">The zero-based index of the keyword set to update.</param>
        /// <param name="keywords">
        /// A list of keywords pertaining to the current <see cref="Lexer" /> separated by whitespace (space, tab, '\n', '\r') characters.
        /// </param>
        /// <remarks>The keywords specified will be styled according to the current <see cref="Lexer" />.</remarks>
        /// <seealso cref="DescribeKeywordSets" />
        public void SetKeywords(int set, string keywords)
        {
            Scintilla.SetKeywords(set, keywords);
        }

        /// <summary>
        /// Passes the specified property name-value pair to the current <see cref="Lexer" />.
        /// </summary>
        /// <param name="name">The property name to set.</param>
        /// <param name="value">
        /// The property value. Values can refer to other property names using the syntax $(name), where 'name' is another property
        /// name for the current <see cref="Lexer" />. When the property value is retrieved by a call to <see cref="GetPropertyExpanded" />
        /// the embedded property name macro will be replaced (expanded) with that current property value.
        /// </param>
        /// <remarks>Property names are case-sensitive.</remarks>
        public void SetProperty(string name, string value)
        {
            Scintilla.SetProperty(name, value);
        }

        /// <summary>
        /// Marks the document as unmodified.
        /// </summary>
        /// <seealso cref="Modified" />
        public void SetSavePoint()
        {
            Scintilla.SetSavePoint();
        }

        /// <summary>
        /// Sets the anchor and current position.
        /// </summary>
        /// <param name="anchorPos">The zero-based document position to start the selection.</param>
        /// <param name="currentPos">The zero-based document position to end the selection.</param>
        /// <remarks>
        /// A negative value for <paramref name="currentPos" /> signifies the end of the document.
        /// A negative value for <paramref name="anchorPos" /> signifies no selection (i.e. sets the <paramref name="anchorPos" />
        /// to the same position as the <paramref name="currentPos" />).
        /// The current position is scrolled into view following this operation.
        /// </remarks>
        public void SetSel(int anchorPos, int currentPos)
        {
            Scintilla.SetSel(anchorPos, currentPos);
        }

        /// <summary>
        /// Sets a single selection from anchor to caret.
        /// </summary>
        /// <param name="caret">The zero-based document position to end the selection.</param>
        /// <param name="anchor">The zero-based document position to start the selection.</param>
        public void SetSelection(int caret, int anchor)
        {
            Scintilla.SetSelection(caret, anchor);
        }

        /// <summary>
        /// Sets a global override to the selection background color.
        /// </summary>
        /// <param name="use">true to override the selection background color; otherwise, false.</param>
        /// <param name="color">The global selection background color.</param>
        /// <seealso cref="SetSelectionForeColor" />
        public void SetSelectionBackColor(bool use, Color color)
        {
            Scintilla.SetSelectionBackColor(use, DrawingColor(color));
        }

        /// <summary>
        /// Sets a global override to the selection foreground color.
        /// </summary>
        /// <param name="use">true to override the selection foreground color; otherwise, false.</param>
        /// <param name="color">The global selection foreground color.</param>
        /// <seealso cref="SetSelectionBackColor" />
        public void SetSelectionForeColor(bool use, Color color)
        {
            Scintilla.SetSelectionForeColor(use, DrawingColor(color));
        }

        /// <summary>
        /// Styles the specified length of characters.
        /// </summary>
        /// <param name="length">The number of characters to style.</param>
        /// <param name="style">The <see cref="Style" /> definition index to assign each character.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="length" /> or <paramref name="style" /> is less than zero. -or-
        /// The sum of a preceeding call to <see cref="StartStyling" /> or <see name="SetStyling" /> and <paramref name="length" /> is greater than the document length. -or-
        /// <paramref name="style" /> is greater than or equal to the number of style definitions.
        /// </exception>
        /// <remarks>
        /// The styling position is advanced by <paramref name="length" /> after each call allowing multiple
        /// calls to <see cref="SetStyling" /> for a single call to <see cref="StartStyling" />.
        /// </remarks>
        /// <seealso cref="StartStyling" />
        public void SetStyling(int length, int style)
        {
            Scintilla.SetStyling(length, style);
        }

        /// <summary>
        /// Sets the <see cref="TargetStart" /> and <see cref="TargetEnd" /> properties in a single call.
        /// </summary>
        /// <param name="start">The zero-based character position within the document to start a search or replace operation.</param>
        /// <param name="end">The zero-based character position within the document to end a search or replace operation.</param>
        /// <seealso cref="TargetStart" />
        /// <seealso cref="TargetEnd" />
        public void SetTargetRange(int start, int end)
        {
            Scintilla.SetTargetRange(start, end);
        }

        /// <summary>
        /// Sets a global override to the whitespace background color.
        /// </summary>
        /// <param name="use">true to override the whitespace background color; otherwise, false.</param>
        /// <param name="color">The global whitespace background color.</param>
        /// <remarks>When not overridden globally, the whitespace background color is determined by the current lexer.</remarks>
        /// <seealso cref="ViewWhitespace" />
        /// <seealso cref="SetWhitespaceForeColor" />
        public void SetWhitespaceBackColor(bool use, Color color)
        {
            Scintilla.SetWhitespaceBackColor(use, DrawingColor(color));
        }

        /// <summary>
        /// Sets a global override to the whitespace foreground color.
        /// </summary>
        /// <param name="use">true to override the whitespace foreground color; otherwise, false.</param>
        /// <param name="color">The global whitespace foreground color.</param>
        /// <remarks>When not overridden globally, the whitespace foreground color is determined by the current lexer.</remarks>
        /// <seealso cref="ViewWhitespace" />
        /// <seealso cref="SetWhitespaceBackColor" />
        public void SetWhitespaceForeColor(bool use, Color color)
        {
            Scintilla.SetWhitespaceForeColor(use, DrawingColor(color));
        }

        /// <summary>
        /// Shows the range of lines specified.
        /// </summary>
        /// <param name="lineStart">The zero-based index of the line range to start showing.</param>
        /// <param name="lineEnd">The zero-based index of the line range to end showing.</param>
        /// <seealso cref="HideLines" />
        /// <seealso cref="Line.Visible" />
        public void ShowLines(int lineStart, int lineEnd)
        {
            Scintilla.ShowLines(lineStart, lineEnd);
        }

        /// <summary>
        /// Prepares for styling by setting the styling <paramref name="position" /> to start at.
        /// </summary>
        /// <param name="position">The zero-based character position in the document to start styling.</param>
        /// <remarks>
        /// After preparing the document for styling, use successive calls to <see cref="SetStyling" />
        /// to style the document.
        /// </remarks>
        /// <seealso cref="SetStyling" />
        public void StartStyling(int position)
        {
            Scintilla.StartStyling(position);
        }

        /// <summary>
        /// Resets all style properties to those currently configured for the <see cref="Style.Default" /> style.
        /// </summary>
        /// <seealso cref="StyleResetDefault" />
        public void StyleClearAll()
        {
            Scintilla.StyleClearAll();
        }

        /// <summary>
        /// Resets the <see cref="Style.Default" /> style to its initial state.
        /// </summary>
        /// <seealso cref="StyleClearAll" />
        public void StyleResetDefault()
        {
            Scintilla.StyleResetDefault();
        }

        /// <summary>
        /// Moves the caret to the opposite end of the main selection.
        /// </summary>
        public void SwapMainAnchorCaret()
        {
            Scintilla.SwapMainAnchorCaret();
        }

        /// <summary>
        /// Sets the <see cref="TargetStart" /> and <see cref="TargetEnd" /> to the start and end positions of the selection.
        /// </summary>
        /// <seealso cref="TargetWholeDocument" />
        public void TargetFromSelection()
        {
            Scintilla.TargetFromSelection();
        }

        /// <summary>
        /// Sets the <see cref="TargetStart" /> and <see cref="TargetEnd" /> to the start and end positions of the document.
        /// </summary>
        /// <seealso cref="TargetFromSelection" />
        public void TargetWholeDocument()
        {
            Scintilla.TargetWholeDocument();
        }

        /// <summary>
        /// Measures the width in pixels of the specified string when rendered in the specified style.
        /// </summary>
        /// <param name="style">The index of the <see cref="Style" /> to use when rendering the text to measure.</param>
        /// <param name="text">The text to measure.</param>
        /// <returns>The width in pixels.</returns>
        public int TextWidth(int style, string text)
        {
            return Scintilla.TextWidth(style, text);
        }

        /// <summary>
        /// Undoes the previous action.
        /// </summary>
        public void Undo()
        {
            Scintilla.Undo();
        }

        /// <summary>
        /// Determines whether to show the right-click context menu.
        /// </summary>
        /// <param name="enablePopup">true to enable the popup window; otherwise, false.</param>
        /// <seealso cref="UsePopup(PopupMode)" />
        public void UsePopup(bool enablePopup)
        {
            Scintilla.UsePopup(enablePopup);
        }

        /// <summary>
        /// Determines the conditions for displaying the standard right-click context menu.
        /// </summary>
        /// <param name="popupMode">One of the <seealso cref="PopupMode" /> enumeration values.</param>
        public void UsePopup(PopupMode popupMode)
        {
            Scintilla.UsePopup(popupMode);
        }

        /// <summary>
        /// Returns the position where a word ends, searching forward from the position specified.
        /// </summary>
        /// <param name="position">The zero-based document position to start searching from.</param>
        /// <param name="onlyWordCharacters">
        /// true to stop searching at the first non-word character regardless of whether the search started at a word or non-word character.
        /// false to use the first character in the search as a word or non-word indicator and then search for that word or non-word boundary.
        /// </param>
        /// <returns>The zero-based document postion of the word boundary.</returns>
        /// <seealso cref="WordStartPosition" />
        public int WordEndPosition(int position, bool onlyWordCharacters)
        {
            return Scintilla.WordEndPosition(position, onlyWordCharacters);
        }

        /// <summary>
        /// Returns the position where a word starts, searching backward from the position specified.
        /// </summary>
        /// <param name="position">The zero-based document position to start searching from.</param>
        /// <param name="onlyWordCharacters">
        /// true to stop searching at the first non-word character regardless of whether the search started at a word or non-word character.
        /// false to use the first character in the search as a word or non-word indicator and then search for that word or non-word boundary.
        /// </param>
        /// <returns>The zero-based document postion of the word boundary.</returns>
        /// <seealso cref="WordEndPosition" />
        public int WordStartPosition(int position, bool onlyWordCharacters)
        {
            return Scintilla.WordStartPosition(position, onlyWordCharacters);
        }

        /// <summary>
        /// Increases the zoom factor by 1 until it reaches 20 points.
        /// </summary>
        /// <seealso cref="Zoom" />
        public void ZoomIn()
        {
            Scintilla.ZoomIn();
        }

        /// <summary>
        /// Decreases the zoom factor by 1 until it reaches -10 points.
        /// </summary>
        /// <seealso cref="Zoom" />
        public void ZoomOut()
        {
            Scintilla.ZoomOut();
        }

        /// <summary>
        /// Converts a WPF color to a Winforms color
        /// </summary>
        /// <param name="color">A WPF color.</param>
        /// <returns>A Winforms color</returns>
        internal System.Drawing.Color DrawingColor(Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        /// <summary>
        /// Converts a Winforms color to a WPF color
        /// </summary>
        /// <param name="color">A Winforms color</param>
        /// <returns>A WPF color.</returns>
        internal Color MediaColor(System.Drawing.Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        #endregion Methods
    }
}