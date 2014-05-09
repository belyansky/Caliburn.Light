using System;
using System.Collections.Generic;
using System.Reflection;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Caliburn.Light
{
    /// <summary>
    /// A basic implementation of <see cref="INavigationService" /> designed to adapt the <see cref="Frame" /> control.
    /// </summary>
    public class FrameAdapter : INavigationService
    {

        private static readonly ILogger Log = LogManager.GetLogger(typeof (FrameAdapter));
        private const string FrameStateKey = "FrameState";
        private const string ParameterKey = "ParameterKey";

        private readonly Frame _frame;
        private event NavigatingCancelEventHandler ExternalNavigatingHandler = delegate { };
        private object _currentParameter;

        /// <summary>
        /// Creates an instance of <see cref="FrameAdapter" />.
        /// </summary>
        /// <param name="frame">The frame to represent as a <see cref="INavigationService" />.</param>
        public FrameAdapter(Frame frame)
        {
            _frame = frame;

            _frame.Navigating += OnNavigating;
            _frame.Navigated += OnNavigated;

#if WINDOWS_PHONE_APP
            _frame.Loaded += (sender, args) => { Windows.Phone.UI.Input.HardwareButtons.BackPressed += OnHardwareBackPressed; };
            _frame.Unloaded += (sender, args) => { Windows.Phone.UI.Input.HardwareButtons.BackPressed -= OnHardwareBackPressed; };
#endif
        }

        /// <summary>
        ///   Occurs before navigation
        /// </summary>
        /// <param name="sender"> The event sender. </param>
        /// <param name="e"> The event args. </param>
        protected virtual void OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            ExternalNavigatingHandler(sender, e);

            if (e.Cancel)
                return;

            var view = _frame.Content as FrameworkElement;

            if (view == null)
                return;

            var guard = view.DataContext as ICloseGuard;

            if (guard != null)
            {
                var shouldCancel = false;
                guard.CanClose(result => { shouldCancel = !result; });

                if (shouldCancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            var deactivator = view.DataContext as IDeactivate;

            if (deactivator != null)
            {
                deactivator.Deactivate(CanCloseOnNavigating(sender, e));
            }
        }

        /// <summary>
        ///   Occurs after navigation
        /// </summary>
        /// <param name="sender"> The event sender. </param>
        /// <param name="e"> The event args. </param>
        protected virtual void OnNavigated(object sender, NavigationEventArgs e)
        {

            if (e.Content == null)
                return;

            _currentParameter = e.Parameter;

            var view = e.Content as Page;

            if (view == null)
            {
                throw new ArgumentException("View '" + e.Content.GetType().FullName +
                                            "' should inherit from Page or one of its descendents.");
            }

            BindViewModel(view);
        }

        /// <summary>
        /// Binds the view model.
        /// </summary>
        /// <param name="view">The view.</param>
        protected virtual void BindViewModel(DependencyObject view)
        {
            ViewLocator.InitializeComponent(view);

            var viewModel = ViewModelLocator.LocateForView(view);
            if (viewModel == null)
                return;

            TryInjectParameters(viewModel, _currentParameter);
            ViewModelBinder.Bind(viewModel, view, null);

            var activator = viewModel as IActivate;
            if (activator != null)
            {
                activator.Activate();
            }

            GC.Collect(); // Why?
        }

        /// <summary>
        ///   Attempts to inject query string parameters from the view into the view model.
        /// </summary>
        /// <param name="viewModel"> The view model.</param>
        /// <param name="parameter"> The parameter.</param>
        protected virtual void TryInjectParameters(object viewModel, object parameter)
        {
            var viewModelType = viewModel.GetType();

            if (parameter is string && ((string) parameter).StartsWith("caliburn://"))
            {
                var uri = new Uri((string) parameter);

                if (!String.IsNullOrEmpty(uri.Query))
                {
                    var decorder = new WwwFormUrlDecoder(uri.Query);

                    foreach (var pair in decorder)
                    {
                        var property = viewModelType.GetRuntimeProperty(pair.Name);

                        if (property == null)
                        {
                            continue;
                        }

                        property.SetValue(viewModel,
                            ParameterBinder.CoerceValue(property.PropertyType, pair.Value, null));
                    }
                }
            }
            else
            {
                var property = viewModelType.GetRuntimeProperty("Parameter");

                if (property == null)
                    return;

                property.SetValue(viewModel, ParameterBinder.CoerceValue(property.PropertyType, parameter, null));
            }
        }

        /// <summary>
        /// Called to check whether or not to close current instance on navigating.
        /// </summary>
        /// <param name="sender"> The event sender from OnNavigating event. </param>
        /// <param name="e"> The event args from OnNavigating event. </param>
        protected virtual bool CanCloseOnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            return false;
        }

        /// <summary>
        ///   Raised after navigation.
        /// </summary>
        public event NavigatedEventHandler Navigated
        {
            add { _frame.Navigated += value; }
            remove { _frame.Navigated -= value; }
        }

        /// <summary>
        ///   Raised prior to navigation.
        /// </summary>
        public event NavigatingCancelEventHandler Navigating
        {
            add { ExternalNavigatingHandler += value; }
            remove { ExternalNavigatingHandler -= value; }
        }

        /// <summary>
        ///   Raised when navigation fails.
        /// </summary>
        public event NavigationFailedEventHandler NavigationFailed
        {
            add { _frame.NavigationFailed += value; }
            remove { _frame.NavigationFailed -= value; }
        }

        /// <summary>
        ///   Raised when navigation is stopped.
        /// </summary>
        public event NavigationStoppedEventHandler NavigationStopped
        {
            add { _frame.NavigationStopped += value; }
            remove { _frame.NavigationStopped -= value; }
        }

        /// <summary>
        /// Gets or sets the data type of the current content, or the content that should be navigated to.
        /// </summary>
        public Type SourcePageType
        {
            get { return _frame.SourcePageType; }
            set { _frame.SourcePageType = value; }
        }

        /// <summary>
        /// Gets the data type of the content that is currently displayed.
        /// </summary>
        public Type CurrentSourcePageType
        {
            get { return _frame.CurrentSourcePageType; }
        }

        /// <summary>
        ///   Navigates to the specified content.
        /// </summary>
        /// <param name="sourcePageType"> The <see cref="System.Type" /> to navigate to. </param>
        /// <returns> Whether or not navigation succeeded. </returns>
        public bool Navigate(Type sourcePageType)
        {
            return _frame.Navigate(sourcePageType);
        }

        /// <summary>
        ///   Navigates to the specified content.
        /// </summary>
        /// <param name="sourcePageType"> The <see cref="System.Type" /> to navigate to. </param>
        /// <param name="parameter">The object parameter to pass to the target.</param>
        /// <returns> Whether or not navigation succeeded. </returns>
        public bool Navigate(Type sourcePageType, object parameter)
        {
            return _frame.Navigate(sourcePageType, parameter);
        }

        /// <summary>
        ///   Navigates forward.
        /// </summary>
        public void GoForward()
        {
            _frame.GoForward();
        }

        /// <summary>
        ///   Navigates back.
        /// </summary>
        public void GoBack()
        {
            _frame.GoBack();
        }

        /// <summary>
        ///   Indicates whether the navigator can navigate forward.
        /// </summary>
        public bool CanGoForward
        {
            get { return _frame.CanGoForward; }
        }

        /// <summary>
        ///   Indicates whether the navigator can navigate back.
        /// </summary>
        public bool CanGoBack
        {
            get { return _frame.CanGoBack; }
        }

        /// <summary>
        /// Gets a collection of PageStackEntry instances representing the backward navigation history of the Frame.
        /// </summary>
        public IList<PageStackEntry> BackStack
        {
            get { return _frame.BackStack; }
        }

        /// <summary>
        /// Gets a collection of PageStackEntry instances representing the forward navigation history of the Frame.
        /// </summary>
        public IList<PageStackEntry> ForwardStack
        {
            get { return _frame.ForwardStack; }
        }

        /// <summary>
        /// Stores the frame navigation state in local settings if it can.
        /// </summary>
        /// <returns>Whether the suspension was sucessful</returns>
        public bool SuspendState()
        {
            try
            {
                var container = GetSettingsContainer();

                container.Values[FrameStateKey] = _frame.GetNavigationState();
                container.Values[ParameterKey] = _currentParameter;

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to suspend state. {0}", ex);
            }

            return false;
        }

        /// <summary>
        /// Tries to restore the frame navigation state from local settings.
        /// </summary>
        /// <returns>Whether the restoration of successful.</returns>
        public bool ResumeState()
        {
            var container = GetSettingsContainer();

            if (!container.Values.ContainsKey(FrameStateKey))
                return false;

            var frameState = (string) container.Values[FrameStateKey];

            _currentParameter = container.Values.ContainsKey(ParameterKey)
                ? container.Values[ParameterKey]
                : null;

            if (String.IsNullOrEmpty(frameState))
                return false;

            _frame.SetNavigationState(frameState);

            var view = _frame.Content as Page;
            if (view == null)
            {
                return false;
            }

            BindViewModel(view);

            if (!ReferenceEquals(Window.Current.Content, _frame))
                Window.Current.Content = _frame;

            Window.Current.Activate();

            return true;
        }

#if WINDOWS_PHONE_APP
        private void OnHardwareBackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            if (CanGoBack)
            {
                e.Handled = true;
                GoBack();
            }
        }
#endif

        private static ApplicationDataContainer GetSettingsContainer()
        {
            return ApplicationData.Current.LocalSettings.CreateContainer("Caliburn.Micro",
                ApplicationDataCreateDisposition.Always);
        }
    }
}