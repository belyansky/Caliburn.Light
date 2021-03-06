﻿using System;
using Microsoft.Xaml.Interactivity;
using Windows.UI.Xaml;

namespace Caliburn.Light
{
    /// <summary>
    /// Represents an attachable object that encapsulates a unit of functionality.
    /// </summary>
    public abstract class TriggerAction : DependencyObject, IAction, IAttachedObject
    {
        private DependencyObject _associatedObject;

        /// <summary>
        /// Attaches to the specified object.
        /// </summary>
        /// <param name="associatedObject">The <see cref="DependencyObject" /> to which the <seealso cref="TriggerAction" /> will be attached.</param>
        public void Attach(DependencyObject associatedObject)
        {
            _associatedObject = associatedObject;
            OnAttached();
        }

        /// <summary>
        /// Detaches this instance from its associated object.
        /// </summary>
        public void Detach()
        {
            OnDetaching();
            _associatedObject = null;
        }

        /// <summary>
        /// Gets the <see cref="DependencyObject" /> to which the <seealso cref="TriggerAction" /> is attached.
        /// </summary>
        protected DependencyObject AssociatedObject
        {
            get { return _associatedObject; }
        }

        DependencyObject IAttachedObject.AssociatedObject
        {
            get { return AssociatedObject; }
        }

        /// <summary>
        ///  Called after the action is attached to an AssociatedObject.
        /// </summary>
        protected virtual void OnAttached() { }

        /// <summary>
        /// Called when the action is being detached from its AssociatedObject, but before it has actually occurred.
        /// </summary>
        protected virtual void OnDetaching() { }

        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="sender">The object that is passed to the action by the behavior. Generally this is <seealso cref="IBehavior.AssociatedObject" /> or a target object.</param>
        /// <param name="parameter">The value of this parameter is determined by the caller.</param>
        /// <returns>
        /// Returns the result of the action.
        /// </returns>
        public object Execute(object sender, object parameter)
        {
            if (AssociatedObject == null)
                throw new InvalidOperationException("AssociatedObject was not set before Execute.");

            Invoke(parameter);
            return null;
        }

        /// <summary>
        /// Invokes the action.
        /// </summary>
        /// <param name="parameter">The parameter to the action. If the action does not require a parameter, the parameter may be set to a null reference.</param>
        protected abstract void Invoke(object parameter);
    }

    /// <summary>
    /// Represents an attachable object that encapsulates a unit of functionality.
    /// </summary>
    /// <typeparam name="T">The type to which this action can be attached.</typeparam>
    public abstract class TriggerAction<T> : TriggerAction
        where T : DependencyObject
    {
        /// <summary>
        /// Gets the object to which this <see cref="TriggerAction&lt;T&gt;"/> is attached.
        /// </summary>
        protected new T AssociatedObject
        {
            get { return (T) base.AssociatedObject; }
        }
    }
}
