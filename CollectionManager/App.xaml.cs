using System.Diagnostics;
using CollectionManager.Services;

namespace CollectionManager;

public partial class App : Application
{
    public static CollectionStore Store { get; } = new CollectionStore();

    public App()
    {
        InitializeComponent();

        Debug.WriteLine("App Started");

        MainPage = new AppShell();
    }
}