using PM2E2GRUPO1.Models;
namespace PM2E2GRUPO1.Views;

public partial class siteView : ContentPage
{
	private string? base64video;
	private string? base64audio;
    private string? Titulo;
    private string? Lugar;

	public siteView(sitioModel item, string lugar)
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
		base64video = item.videoDigital;
		base64audio = item.audioFile;
        Titulo = item.descripcion;
        Lugar = lugar;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        //Convierte a byte array
        byte[] videoBytes = Convert.FromBase64String(base64video);
        byte[] audioBytes = Convert.FromBase64String(base64audio);

        string videoFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tempVideo.mp4");
        string audioFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tempAudio.mp3");

        //Crea ubicaciones temporales para guardar los archivos y poder verlos en los mediaElement
        File.WriteAllBytes(videoFilePath, videoBytes);
        File.WriteAllBytes(audioFilePath, audioBytes);

        labelTitulo.Text = Titulo;
        labelLugar.Text = Lugar;
        mediaElementVideo.Source = videoFilePath;
        mediaElementAudio.Source = audioFilePath;
    }
}