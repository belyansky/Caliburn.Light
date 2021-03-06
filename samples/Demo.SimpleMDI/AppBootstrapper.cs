﻿using Caliburn.Light;
using System.Windows;

namespace Demo.SimpleMDI
{
    public class AppBootstrapper : BootstrapperBase
    {
        private SimpleContainer _container;

        public AppBootstrapper()
        {
            LogManager.Initialize(type => new DebugLogger(type));
            Initialize();
        }

        protected override void Configure()
        {
            _container = new SimpleContainer();
            IoC.Initialize(_container);

            _container.RegisterSingleton<IWindowManager, WindowManager>();
            _container.RegisterSingleton<IEventAggregator, EventAggregator>();
            _container.RegisterSingleton<IViewModelLocator, ViewModelLocator>();
            _container.RegisterSingleton<IViewModelBinder, ViewModelBinder>();

            var typeResolver = new ViewModelTypeResolver();
            typeResolver.AddMapping<ShellView, ShellViewModel>();
            typeResolver.AddMapping<TabView, TabViewModel>();
            _container.RegisterInstance<IViewModelTypeResolver>(typeResolver);

            _container.RegisterPerRequest<ShellViewModel>();
            _container.RegisterPerRequest<TabViewModel>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
