﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Toolkit.Uwp.UI.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace UnitTests.Extensions
{
    [TestClass]
    public class Test_FontIconExtensionMarkupExtension
    {
        [TestCategory("FontIconExtensionMarkupExtension")]
        [UITestMethod]
        public void Test_FontIconExtension_MarkupExtension_ProvideSegoeMdl2Asset()
        {
            var treeroot = XamlReader.Load(@"<Page
    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
    xmlns:ex=""using:Microsoft.Toolkit.Uwp.UI.Extensions"">
        <AppBarButton x:Name=""Check"" Icon=""{ex:FontIcon Glyph=&#xE14D;}""/>
</Page>") as FrameworkElement;

            var button = treeroot.FindChildByName("Check") as AppBarButton;

            Assert.IsNotNull(button, $"Could not find the {nameof(AppBarButton)} control in tree.");

            var icon = button.Icon as FontIcon;

            Assert.IsNotNull(icon, $"Could not find the {nameof(FontIcon)} element in button.");

            Assert.AreEqual(icon.Glyph, "\uE105", "Expected icon glyph to be E105.");
            Assert.AreEqual(icon.FontFamily.Source, "Segoe MDL2 Assets", "Expected font family to be Segoe MDL2 Assets");
        }

        [TestCategory("FontIconExtensionMarkupExtension")]
        [UITestMethod]
        public void Test_FontIconExtension_MarkupExtension_ProvideSegoeUI()
        {
            var treeroot = XamlReader.Load(@"<Page
    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
    xmlns:ex=""using:Microsoft.Toolkit.Uwp.UI.Extensions"">
        <AppBarButton x:Name=""Check"" Icon=""{ex:FontIcon Glyph=&#xE14D;, FontFamily='Segoe UI'}""/>
</Page>") as FrameworkElement;

            var button = treeroot.FindChildByName("Check") as AppBarButton;

            Assert.IsNotNull(button, $"Could not find the {nameof(AppBarButton)} control in tree.");

            var icon = button.Icon as FontIcon;

            Assert.IsNotNull(icon, $"Could not find the {nameof(FontIcon)} element in button.");

            Assert.AreEqual(icon.Glyph, "\uE105", "Expected icon glyph to be E105.");
            Assert.AreEqual(icon.FontFamily.Source, "Segoe MDL2 Assets", "Expected font family to be Segoe UI");
        }
    }
}
