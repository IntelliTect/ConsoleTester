﻿using IntelliTect.TestTools.Selenate.Examples.Pages;
using OpenQA.Selenium;
using System;
using Xunit;

namespace IntelliTect.TestTools.Selenate.Examples
{
    public class BasicElementInteractions : IDisposable
    {
        public BasicElementInteractions()
        {
            _WebDriver = new WebDriverFactory(BrowserType.Chrome).GetDriver();
            _DriverHandler = new DriverHandler(_WebDriver);
            _DynamicLoadingPage = new DynamicLoadingPages(_WebDriver);
            _DynamicControlsPage = new DynamicControlsPage(_WebDriver);
        }

        private readonly IWebDriver _WebDriver;
        private readonly DriverHandler _DriverHandler;
        private readonly DynamicLoadingPages _DynamicLoadingPage;
        private readonly DynamicControlsPage _DynamicControlsPage;


        // Below two tests should functionally operate the same
        [Fact]
        public void FindElementThatIsUnhiddenAfterPageLoad()
        {
            _DriverHandler.NavigateToPage("http://the-internet.herokuapp.com/dynamic_loading/1");

            _DynamicLoadingPage.StartButton.Click();

            Assert.True(
                _DynamicLoadingPage.HelloWorldLabel
                    .SetTimeoutSeconds(8)
                    .WaitForVisibleState(),
                "Hello World label did not appear when we expected it to.");
        }

        [Fact]
        public void FindElementThatIsCreatedAfterPageLoad()
        {
            _DriverHandler.NavigateToPage("http://the-internet.herokuapp.com/dynamic_loading/2");

            _DynamicLoadingPage.StartButton.Click();

            Assert.True(
                _DynamicLoadingPage.HelloWorldLabel
                    .SetTimeoutSeconds(8)
                    .WaitForVisibleState(),
                "Hello World label did not appear when we expected it to.");
        }

        [Fact]
        public void VerifyElementsThatDisappearCanBeTracked()
        {
            _DriverHandler.NavigateToPage("http://the-internet.herokuapp.com/dynamic_controls");

            Assert.True(_DynamicControlsPage.Checkbox.WaitForVisibleState());
            _DynamicControlsPage.RemoveAddButton.Click();
            Assert.True(_DynamicControlsPage.Checkbox.WaitForInvisibleState());
            _DynamicControlsPage.RemoveAddButton.Click(); // Bug here. Button does not become enabled in time
            Assert.True(_DynamicControlsPage.Checkbox.WaitForVisibleState());
        }

        [Fact]
        public void CheckForElementEnabledStates()
        {
            _DriverHandler.NavigateToPage("http://the-internet.herokuapp.com/dynamic_controls");

            Assert.True(_DynamicControlsPage.TextBox.WaitForEnabledState());
            _DynamicControlsPage.EnableDisableButton.Click();
            Assert.True(_DynamicControlsPage.TextBox.WaitForInvisibleState());
            _DynamicControlsPage.EnableDisableButton.Click();
            Assert.True(_DynamicControlsPage.TextBox.WaitForVisibleState());
        }

        public void Dispose()
        {
            _WebDriver.Dispose();
        }
    }
}