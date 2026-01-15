using ReservaChro.Mobile.Views;

namespace ReservaChro.Mobile;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// Abre direto a tela de login para validar o layout/imagens
		return new Window(new NavigationPage(new LoginPage()));
	}
}
