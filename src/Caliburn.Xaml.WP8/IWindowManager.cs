﻿using System.Collections.Generic;

namespace Caliburn.Xaml
{
    /// <summary>
    /// A service that manages windows.
    /// </summary>
    public interface IWindowManager {
        /// <summary>
        ///   Shows a modal dialog for the specified model.
        /// </summary>
        /// <param name = "rootModel">The root model.</param>
        /// <param name="settings">The optional dialog settings.</param>
        /// <param name = "context">The context.</param>
        void ShowDialog(object rootModel, object context = null, IDictionary<string, object> settings = null);

        /// <summary>
        ///   Shows a popup at the current mouse position.
        /// </summary>
        /// <param name = "rootModel">The root model.</param>
        /// <param name = "context">The view context.</param>
        /// <param name = "settings">The optional popup settings.</param>
        void ShowPopup(object rootModel, object context = null, IDictionary<string, object> settings = null);
    }
}
