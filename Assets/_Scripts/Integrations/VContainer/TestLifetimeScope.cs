﻿using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace CosmicShore.Integrations.VContainer
{
    public class TestLifetimeScope : LifetimeScope
    {
        [SerializeField] TestMenu testMenu;
        protected override void Configure(IContainerBuilder builder)
        {
            // builder.RegisterEntryPoint<TestPresenter>();
            builder.Register<TestService>(Lifetime.Singleton);
            builder.Register<IServiceA, TestServiceA>(Lifetime.Singleton);
            builder.Register<IServiceB, TestServiceB>(Lifetime.Singleton);
            builder.Register<TestComponentA>(Lifetime.Singleton);
            builder.RegisterComponent(testMenu);

            // Use registered entry point with a group
            builder.UseEntryPoints(Lifetime.Singleton, entryPoints =>
            {
                entryPoints.Add<TestPresenter>();
                entryPoints.Add<ModuleA>();
            });
        }
    }
}