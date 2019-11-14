using System;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media.Effects;
using System.Windows.Media;
using System.Windows.Input;

namespace ScintillaNET.WPF.Configuration
{
	public abstract class ScintillaWPFConfigItem : UIElement
	{
		protected ScintillaWPF mLastAppliedParent;
		internal virtual void ApplyConfig(ScintillaWPF scintilla)
		{
			mLastAppliedParent = scintilla;
		}
		internal abstract void Reset(ScintillaWPF scintilla);

		protected void TryApplyConfig()
		{
			if (mLastAppliedParent != null)
				ApplyConfig(mLastAppliedParent);
		}

		// And now the stuff to hide all the default properties.

		#region Properties

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public bool AllowDrop { get; set; }

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public BitmapEffect BitmapEffect { get; set; }

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public BitmapEffectInput BitmapEffectInput { get; set; }

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public CacheMode CacheMode { get; set; }

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public Geometry Clip { get; set; }

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public bool ClipToBounds { get; set; }

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public Effect Effect { get; set; }

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public bool Focusable { get; set; }

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public bool IsEnabled { get; set; }

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public bool IsHitTestVisible { get; set; }

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public bool IsManipulationEnabled { get; set; }

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public double Opacity { get; set; }

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public Brush OpacityMask { get; set; }

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public Size RenderSize { get; set; }

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public Transform RenderTransform { get; set; }

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public Point RenderTransformOrigin { get; set; }

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public bool SnapsToDevicePixels { get; set; }

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public string Uid { get; set; }

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public Visibility Visibility { get; set; }

		#endregion

		#region Events
#pragma warning disable 67
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event DragEventHandler DragEnter;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event DragEventHandler DragLeave;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event DragEventHandler DragOver;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event DragEventHandler Drop;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event DependencyPropertyChangedEventHandler IsEnabledChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event DependencyPropertyChangedEventHandler FocusableChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event GiveFeedbackEventHandler GiveFeedback;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event RoutedEventHandler GotFocus;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event KeyboardFocusChangedEventHandler GotKeyboardFocus;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event MouseEventHandler GotMouseCapture;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event StylusEventHandler GotStylusCapture;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event EventHandler<TouchEventArgs> GotTouchCapture;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event DependencyPropertyChangedEventHandler IsHitTestVisibleChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event DependencyPropertyChangedEventHandler IsKeyboardFocusedChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event DependencyPropertyChangedEventHandler IsKeyboardFocusWithinChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event DependencyPropertyChangedEventHandler IsMouseCapturedChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event DependencyPropertyChangedEventHandler IsMouseCaptureWithinChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event DependencyPropertyChangedEventHandler IsMouseDirectlyOverChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event DependencyPropertyChangedEventHandler IsStylusCapturedChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event DependencyPropertyChangedEventHandler IsStylusCaptureWithinChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event DependencyPropertyChangedEventHandler IsStylusDirectlyOverChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event DependencyPropertyChangedEventHandler IsVisibleChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event KeyEventHandler KeyDown;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event KeyEventHandler KeyUp;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event EventHandler LayoutUpdated;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event RoutedEventHandler LostFocus;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event KeyboardFocusChangedEventHandler LostKeyboardFocus;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event MouseEventHandler LostMouseCapture;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event StylusEventHandler LostStylusCapture;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event EventHandler<TouchEventArgs> LostTouchCapture;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event EventHandler<ManipulationBoundaryFeedbackEventArgs> ManipulationBoundaryFeedback;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event EventHandler<ManipulationCompletedEventArgs> ManipulationCompleted;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event EventHandler<ManipulationDeltaEventArgs> ManipulationDelta;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event EventHandler<ManipulationInertiaStartingEventArgs> ManipulationInertiaStarting;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event EventHandler<ManipulationStartedEventArgs> ManipulationStarted;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event EventHandler<ManipulationStartingEventArgs> ManipulationStarting;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event MouseButtonEventHandler MouseDown;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event MouseEventHandler MouseEnter;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event MouseEventHandler MouseLeave;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event MouseButtonEventHandler MouseLeftButtonDown;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event MouseButtonEventHandler MouseLeftButtonUp;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event MouseEventHandler MouseMove;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event MouseButtonEventHandler MouseRightButtonDown;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event MouseButtonEventHandler MouseRightButtonUp;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event MouseButtonEventHandler MouseUp;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event MouseWheelEventHandler MouseWheel;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event DragEventHandler PreviewDragEnter;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event DragEventHandler PreviewDragLeave;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event DragEventHandler PreviewDragOver;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event DragEventHandler PreviewDrop;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event GiveFeedbackEventHandler PreviewGiveFeedback;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event KeyboardFocusChangedEventHandler PreviewGotKeyboardFocus;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event KeyEventHandler PreviewKeyDown;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event KeyEventHandler PreviewKeyUp;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event KeyboardFocusChangedEventHandler PreviewLostKeyboardFocus;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event MouseButtonEventHandler PreviewMouseDown;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event MouseButtonEventHandler PreviewMouseLeftButtonDown;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event MouseButtonEventHandler PreviewMouseLeftButtonUp;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event MouseEventHandler PreviewMouseMove;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event MouseButtonEventHandler PreviewMouseRightButtonDown;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event MouseButtonEventHandler PreviewMouseRightButtonUp;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event MouseButtonEventHandler PreviewMouseUp;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event MouseWheelEventHandler PreviewMouseWheel;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event QueryContinueDragEventHandler PreviewQueryContinueDrag;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event StylusButtonEventHandler PreviewStylusButtonDown;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event StylusButtonEventHandler PreviewStylusButtonUp;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event StylusDownEventHandler PreviewStylusDown;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event StylusEventHandler PreviewStylusInAirMove;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event StylusEventHandler PreviewStylusInRange;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event StylusEventHandler PreviewStylusMove;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event StylusEventHandler PreviewStylusOutOfRange;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event StylusSystemGestureEventHandler PreviewStylusSystemGesture;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event StylusEventHandler PreviewStylusUp;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event TextCompositionEventHandler PreviewTextInput;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event EventHandler<TouchEventArgs> PreviewTouchDown;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event EventHandler<TouchEventArgs> PreviewTouchMove;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event EventHandler<TouchEventArgs> PreviewTouchUp;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event QueryContinueDragEventHandler QueryContinueDrag;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event QueryCursorEventHandler QueryCursor;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event StylusButtonEventHandler StylusButtonDown;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event StylusButtonEventHandler StylusButtonUp;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event StylusDownEventHandler StylusDown;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event StylusEventHandler StylusEnter;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event StylusEventHandler StylusInAirMove;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event StylusEventHandler StylusInRange;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event StylusEventHandler StylusLeave;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event StylusEventHandler StylusMove;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event StylusEventHandler StylusOutOfRange;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event StylusSystemGestureEventHandler StylusSystemGesture;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event StylusEventHandler StylusUp;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event TextCompositionEventHandler TextInput;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event EventHandler<TouchEventArgs> TouchDown;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event EventHandler<TouchEventArgs> TouchEnter;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event EventHandler<TouchEventArgs> TouchLeave;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event EventHandler<TouchEventArgs> TouchMove;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property is not valid on this class.", true)]
		new public event EventHandler<TouchEventArgs> TouchUp;

#pragma warning restore 67
		#endregion
	}
}
