using System;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using Microsoft.Maui.Dispatching;
using System.Diagnostics;

namespace MauiApp1 {
    public partial class MainPage : ContentPage {
        public MainPage()
        {
            InitializeComponent();

            FetchDataSequentially();
        }

        private async void FetchDataSequentially()
        {
            await GetData("https://mf.eskisehir.edu.tr/tr/Duyuru", MF_list);
            await GetData("https://matse.eskisehir.edu.tr/tr/Duyuru", Matse_list);
        }

        async Task GetData(string url,VerticalStackLayout list)
        {
            MainThread.BeginInvokeOnMainThread(() => {
                console.Text = $"Fetching {list.ClassId} data...";
                progressbar.IsVisible = true;
                progressbar.Progress = 0.01;
            });
            try {
                FirefoxOptions options = new FirefoxOptions();
                options.AddArgument("--headless"); 
                options.AddArgument("--disable-images");

                var driverService = FirefoxDriverService.CreateDefaultService();
                driverService.HideCommandPromptWindow = true;

                using IWebDriver driver = new FirefoxDriver(driverService, options);
                driver.Navigate().GoToUrl(url);

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight/2);");
                await Task.Delay(2000);

                IWebElement parentDiv = wait.Until(d => d.FindElement(By.XPath("/html/body/div[5]/div/div[2]/div/div/div/div/div[2]/div/div/div[1]")));
                IReadOnlyCollection<IWebElement> divsInsideParent = parentDiv.FindElements(By.XPath(".//div[contains(@class, 'gdlr-core-blog-full-frame gdlr-core-skin-e-background')]"));
                int i = 0;
                foreach (IWebElement div in divsInsideParent) {
                    string text = $"Duyuru{i}: " + div.FindElement(By.XPath("div[1]/div/h3/a")).Text + "\n";
                    CreateElement(text, list);
                    i++;
                    double progressValue = (double)i / 10;
                    MainThread.BeginInvokeOnMainThread(() => {
                        console.Text = $"Data-{i} Fetched";
                        progressbar.ProgressTo(progressValue, 200, Easing.Default);
                    });
                }
                MainThread.BeginInvokeOnMainThread(() => {
                    console.Text = "";
                    progressbar.Progress = 0;
                    progressbar.IsVisible = false;
                });
                driver.Quit();
            } catch (Exception ex) {
                MainThread.BeginInvokeOnMainThread(() => {
                    console.Text = ex.Message;
                });
            }
        }

        public HorizontalStackLayout CreateElement(string text,VerticalStackLayout list)
        {
            HorizontalStackLayout element = new HorizontalStackLayout {
                Spacing = 10,
                ClassId = list.ClassId
            };

            Button button = new Button {
                Text = "link",
            };

            Label label = new Label {
                Text = text,
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            element.Add(label);
            element.Add(button);

            MainThread.BeginInvokeOnMainThread(() => {
                list.Add(element);
            });

            button.Clicked += Button_Clicked;

            return element;
        }
        private void Button_Clicked(object sender, EventArgs e)
        {
            string url = "";
            if(sender is Button btn) {
                url = (btn.Parent.ClassId == "MF") ? "https://mf.eskisehir.edu.tr/tr/Duyuru" : "https://matse.eskisehir.edu.tr/tr/Duyuru";
            }
            OpenLink(url);
        }
        private async void OpenLink(string url)
        {
            if (await Launcher.CanOpenAsync(url)) {
                await Launcher.OpenAsync(url);
            }
        }

        private void Collapse_btn_Clicked(object sender, EventArgs e)
        {
            if(sender is Button button) {
                button.Text = (button.Text == "<") ? ">" : "<";
                VerticalStackLayout list = new VerticalStackLayout();
                list = (button.ClassId == "MF") ? MF_list : Matse_list;
                list.IsVisible = !list.IsVisible;
            }
        }
    }
}
