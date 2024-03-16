using CommunityToolkit.Maui.Views;
using Plugin.AudioRecorder;
using PM2E2GRUPO1.Controllers;
using PM2E2GRUPO1.Extensions;
using PM2E2GRUPO1.Config;
using Microsoft.Maui.Controls;

namespace PM2E2GRUPO1.Views;

public partial class agregarVideo : ContentPage
{
    private ApiService _apiService;
    
    //video
    private byte[] videoBytes;
    private string base64Video;

    //audio
    private AudioRecorderService audioRecorderService;
    private string audioFilePath;
    private bool isRecording;
    private byte[] audioBytes;
    private string base64Audio;
    private double Latitude = 00.00;
    private double Longitude = 00.00;

    public agregarVideo()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        _apiService = new ApiService();
        audioRecorderService = new AudioRecorderService();
        InitializePage();
    }

    private async void InitializePage()
    {
        try
        {
            // Revisa si el permiso de ubicacion ha sido concedido
            var locationPermissionStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (locationPermissionStatus == PermissionStatus.Granted)
            {
                // Obtiene la ubicacion
                var location = await Geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Default,
                    Timeout = TimeSpan.FromSeconds(10)
                });

                if (location != null)
                {
                    Latitude = location.Latitude;
                    Longitude = location.Longitude;
                    // Coloca la latitude y longitud en los labels
                    labelLatitude.Text = $"{location.Latitude}";
                    labelLongitude.Text = $"{location.Longitude}";
                }
                else if (labelLatitude.Text.Equals("00.00") || labelLongitude.Text.Equals("00.00"))
                {
                    // Cuando la ubicacion es nula
                    await DisplayAlert("Alerta", "El GPS se encuentra desactivado. Porfavor active su GPS y abra la aplicación de nuevo!", "Ok");
                }
            }
            else
            {
                // Cuando el permiso no es otorgado
                await DisplayAlert("Error", "Permiso de Ubicación no otorgado. El Permiso es necesario para utilizar la aplicacion.", "OK");
                Application.Current.Quit();
            }
        }
        catch (FeatureNotEnabledException)
        {
            try
            {
                await Application.Current.MainPage.DisplayAlert("Alerta", "El GPS se encuentra desactivado. Porfavor active su GPS y abra la aplicación de nuevo!", "Ok");
                Application.Current.Quit();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DisplayGpsNotEnabledAlert: {ex.Message}");
            }

        }
    }

    private async void btnAgregar_Clicked(object sender, EventArgs e)
    {
        ShowLoadingDialog();

        if (string.IsNullOrEmpty(base64Video))
        {
            await DisplayAlert("Alerta", "Por favor grabe un video de la ubicación", "OK");
            HideLoadingDialog();
            return;
        }

        if (Latitude == 00.00 || Longitude == 00.00)
        {
            await DisplayAlert("Alerta", "No existen datos de geolocalización", "OK");
            HideLoadingDialog();
            return;
        }

        if (string.IsNullOrEmpty(base64Audio))
        {
            await DisplayAlert("Alerta", "Por favor grabe un audio con la descripción de la ubicación (mantenga presionado el icono del micrófono para grabar)", "OK");
            HideLoadingDialog();
            return;
        }

        if (string.IsNullOrEmpty (entryDescripcion.Text)) 
        {
            await DisplayAlert("Alerta", "Por favor ingrese un título para el lugar", "OK");
            HideLoadingDialog();
            return;
        }

        string sanitizedInput = InputSanitizer.SanitizeInput(entryDescripcion.Text);

        var data = new
        {
            Descripcion = sanitizedInput,
            Latitud = Latitude,
            Longitud = Longitude,
            VideoDigital = base64Video,
            AudioFile = base64Audio
        };

        bool isSuccess = await _apiService.PostSuccessAsync("sitios/", data);


        if (isSuccess)
        {
            HideLoadingDialog();
            await DisplayAlert("¡Guardado!", "El sitio ha sido guardado", "OK");
            mediaElementAudio.Source = null;
            mediaElementVideo.Source = null;
            base64Audio = string.Empty;
            base64Video = string.Empty;
            entryDescripcion.Text = string.Empty;
            await Navigation.PopAsync();
        }
    }

    private async void btnCapture_Clicked(object sender, EventArgs e)
    {
        var status = await Permissions.RequestAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            return;
        }

        var options = new Xamarin.Essentials.MediaPickerOptions
        {
            Title = "Capturar Video"
        };

        try
        {
            var mediaFile = await Xamarin.Essentials.MediaPicker.CaptureVideoAsync(options);

            if (mediaFile != null)
            {
                using (var stream = await mediaFile.OpenReadAsync())
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await stream.CopyToAsync(memoryStream);
                        videoBytes = memoryStream.ToArray();
                    }
                }

                base64Video = Convert.ToBase64String(videoBytes);

                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tempVideo.mp4");

                File.WriteAllBytes(filePath, videoBytes);

                mediaElementVideo.Source = MediaSource.FromFile(filePath);

            }
            else
            {
                Console.WriteLine("Video capture cancelled or failed.");
            }
        }
        catch (FeatureNotSupportedException)
        {
            Console.WriteLine("Video capture is not supported on this device.");
        }
        catch (PermissionException)
        {
            Console.WriteLine("Camera permission denied.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error capturing video: {ex.Message}");
        }
    }

    private async Task StartRecordingAsync()
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.Microphone>();
            if (status != PermissionStatus.Granted)
            {
                return;
            }

            // Empieza a grabar
            var audioRecordTask = await audioRecorderService.StartRecording();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting recording: {ex.Message}");
        }
    }

    private async Task StopRecordingAsync()
    {
        try
        {
            // Para de grabar
            await audioRecorderService.StopRecording();

            audioFilePath = audioRecorderService.GetAudioFilePath();

            audioBytes = File.ReadAllBytes(audioFilePath);
            base64Audio = Convert.ToBase64String(audioBytes);

            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tempAudio.mp3");

            File.WriteAllBytes(filePath, audioBytes);

            mediaElementAudio.Source = MediaSource.FromFile(filePath);

            Console.WriteLine("Base64 audio: " + base64Audio);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error stopping recording: {ex.Message}");
        }
    }

    private async void btnGrabarAudio_Pressed(object sender, EventArgs e)
    {
        if (!isRecording)
        {
            await StartRecordingAsync();
            isRecording = true;
        }
    }

    private async void btnGrabarAudio_Released(object sender, EventArgs e)
    {
        if (isRecording)
        {
            await StopRecordingAsync();
            isRecording = false;
        }
    }

    private void ShowLoadingDialog()
    {
        // Show your loading dialog here
        activityIndicator.IsRunning = true;
        activityIndicator.IsVisible = true;
        framePrincipal.IsVisible = false;
        btnAgregar.IsEnabled = false;
    }

    private void HideLoadingDialog()
    {
        // Hide your loading dialog here
        activityIndicator.IsRunning = false;
        activityIndicator.IsVisible = false;
        framePrincipal.IsVisible= true;
        btnAgregar.IsEnabled = true;
    }
}