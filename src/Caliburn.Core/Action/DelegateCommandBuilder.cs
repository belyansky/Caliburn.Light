﻿using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Weakly;

namespace Caliburn.Light
{
    /// <summary>
    /// Builds an <see cref="IDelegateCommand"/> in a strongly typed fashion.
    /// </summary>
    /// <typeparam name="TTarget">The type of the command target.</typeparam>
    public sealed class DelegateCommandBuilder<TTarget>
        where TTarget : class
    {
        private readonly TTarget _target;
        private Action<TTarget> _execute;
        private Func<TTarget, bool> _canExecute;
        private string _propertyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommandBuilder&lt;TTarget&gt;"/> class.
        /// </summary>
        /// <param name="target">The command target.</param>
        public DelegateCommandBuilder(TTarget target)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            _target = target;
        }

        /// <summary>
        /// Sets the command execute function.
        /// </summary>
        /// <param name="execute">The execute function.</param>
        /// <returns>Itself</returns>
        public DelegateCommandBuilder<TTarget> Execute(Action<TTarget> execute)
        {
            VerifyExecute(execute);
            _execute = execute;
            return this;
        }

        /// <summary>
        /// Sets the command execute function.
        /// </summary>
        /// <param name="execute">The execute function.</param>
        /// <returns>Itself</returns>
        public DelegateCommandBuilder<TTarget> Execute(Func<TTarget, Task> execute)
        {
            VerifyExecute(execute);
            _execute = t => execute(t).ObserveException().Watch();
            return this;
        }

        // ReSharper disable once UnusedParameter.Local
        private void VerifyExecute(Delegate execute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            if (execute.Target != null)
                throw new ArgumentException("An instance method or closure cannot be used.", "execute");

            if (_execute != null)
                throw new InvalidOperationException("Execute already set.");
        }

        /// <summary>
        /// Sets the command canExecute function.
        /// </summary>
        /// <param name="canExecute">The canExecute function.</param>
        /// <returns>Itself</returns>
        public DelegateCommandBuilder<TTarget> CanExecute(Func<TTarget, bool> canExecute)
        {
            if ((Delegate) canExecute == null)
                throw new ArgumentNullException("canExecute");
            if (canExecute.Target != null)
                throw new ArgumentException("An instance method or closure cannot be used.", "canExecute");

            if (_canExecute != null)
                throw new InvalidOperationException("CanExecute already set.");

            _canExecute = canExecute;
            return this;
        }

        // ReSharper disable once UnusedParameter.Local

        /// <summary>
        /// Sets the property to listen for change notifications.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>Itself</returns>
        public DelegateCommandBuilder<TTarget> PropertyChanged(Expression<Func<TTarget, bool>> property)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            if (!string.IsNullOrEmpty(_propertyName))
                throw new InvalidOperationException("PropertyName already set.");

            var propertyName = ExpressionHelper.GetMemberInfo(property).Name;
            _propertyName = propertyName;
            return this;
        }

        /// <summary>
        /// Builds the command.
        /// </summary>
        /// <returns>The newly build command.</returns>
        public IDelegateCommand Build()
        {
            if (_target == null)
                throw new InvalidOperationException("Target not set.");
            if (_execute == null)
                throw new InvalidOperationException("Execute not set.");
            if (!string.IsNullOrEmpty(_propertyName) && _canExecute == null)
                throw new InvalidOperationException("PropertyName is set but CanExecute is not.");

            Func<TTarget, object, bool> canExecute = null;
            if (_canExecute != null)
                canExecute = (t, p) => _canExecute(t);

            return new DelegateCommandImpl<TTarget, object>(_target, (t, p) => _execute(t), canExecute, _propertyName);
        }
    }

    /// <summary>
    /// Builds an <see cref="IDelegateCommand"/> in a strongly typed fashion.
    /// </summary>
    /// <typeparam name="TTarget">The type of the command target.</typeparam>
    /// <typeparam name="TParameter">The type of the command parameter.</typeparam>
    public sealed class DelegateCommandBuilder<TTarget, TParameter>
        where TTarget : class
    {
        private readonly TTarget _target;
        private Action<TTarget, TParameter> _execute;
        private Func<TTarget, TParameter, bool> _canExecute;
        private string _propertyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommandBuilder&lt;TTarget, TParameter&gt;"/> class.
        /// </summary>
        /// <param name="target">The command target.</param>
        public DelegateCommandBuilder(TTarget target)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            _target = target;
        }

        /// <summary>
        /// Sets the command execute function.
        /// </summary>
        /// <param name="execute">The execute function.</param>
        /// <returns>Itself</returns>
        public DelegateCommandBuilder<TTarget, TParameter> Execute(Action<TTarget, TParameter> execute)
        {
            VerifyExecute(execute);
            _execute = execute;
            return this;
        }

        /// <summary>
        /// Sets the command execute function.
        /// </summary>
        /// <param name="execute">The execute function.</param>
        /// <returns>Itself</returns>
        public DelegateCommandBuilder<TTarget, TParameter> Execute(Func<TTarget, TParameter, Task> execute)
        {
            VerifyExecute(execute);
            _execute = (t, p) => execute(t, p).ObserveException().Watch();
            return this;
        }

        // ReSharper disable once UnusedParameter.Local
        private void VerifyExecute(Delegate execute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            if (execute.Target != null)
                throw new ArgumentException("An instance method or closure cannot be used.", "execute");

            if (_execute != null)
                throw new InvalidOperationException("Execute already set.");
        }

        /// <summary>
        /// Sets the command canExecute function.
        /// </summary>
        /// <param name="canExecute">The canExecute function.</param>
        /// <returns>Itself</returns>
        public DelegateCommandBuilder<TTarget, TParameter> CanExecute(Func<TTarget, bool> canExecute)
        {
            VerifyCanExecute(canExecute);
            _canExecute = (t, p) => canExecute(t);
            return this;
        }

        /// <summary>
        /// Sets the command canExecute function.
        /// </summary>
        /// <param name="canExecute">The canExecute function.</param>
        /// <returns>Itself</returns>
        public DelegateCommandBuilder<TTarget, TParameter> CanExecute(Func<TTarget, TParameter, bool> canExecute)
        {
            VerifyCanExecute(canExecute);
            _canExecute = canExecute;
            return this;
        }

        // ReSharper disable once UnusedParameter.Local
        private void VerifyCanExecute(Delegate canExecute)
        {
            if (canExecute == null)
                throw new ArgumentNullException("canExecute");
            if (canExecute.Target != null)
                throw new ArgumentException("An instance method or closure cannot be used.", "canExecute");

            if (_canExecute != null)
                throw new InvalidOperationException("CanExecute already set.");
        }

        /// <summary>
        /// Sets the property to listen for change notifications.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>Itself</returns>
        public DelegateCommandBuilder<TTarget, TParameter> PropertyChanged(Expression<Func<TTarget, bool>> property)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            if (!string.IsNullOrEmpty(_propertyName))
                throw new InvalidOperationException("PropertyName already set.");

            var propertyName = ExpressionHelper.GetMemberInfo(property).Name;
            _propertyName = propertyName;
            return this;
        }

        /// <summary>
        /// Builds the command.
        /// </summary>
        /// <returns>The newly build command.</returns>
        public IDelegateCommand Build()
        {
            if (_target == null)
                throw new InvalidOperationException("Target not set.");
            if (_execute == null)
                throw new InvalidOperationException("Execute not set.");
            if (!string.IsNullOrEmpty(_propertyName) && _canExecute == null)
                throw new InvalidOperationException("PropertyName is set but CanExecute is not.");

            return new DelegateCommandImpl<TTarget, TParameter>(_target, _execute, _canExecute, _propertyName);
        }
    }
}