using Microsoft.Maui.Controls;

namespace EtiquetadoAuto;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Esto le dice a la app que use el sistema de pestañas
        MainPage = new AppShell();
    }
}