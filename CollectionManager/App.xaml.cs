using System.Diagnostics;
using CollectionManager.Services;

namespace CollectionManager;

public partial class App : Application
{
    public static FileService Store { get; } = new FileService();

    public App()
    {
        InitializeComponent();

        Debug.WriteLine("App Started");

        MainPage = new AppShell();
    }
}