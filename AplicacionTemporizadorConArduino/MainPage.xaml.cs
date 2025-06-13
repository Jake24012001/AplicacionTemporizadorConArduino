using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.IO;

namespace AplicacionTemporizadorConArduino
{
    public partial class MainPage : ContentPage
    {
        private int countdownTime = 0;
        private bool isTimerRunning = false;
        private Stopwatch timer = new();

        private BluetoothClient _client;
        private Stream _stream;

        public MainPage()
        {
            InitializeComponent();
        }

        [Obsolete]
        private async void StartTimerBtn_Clicked(object sender, EventArgs e)
        {
            if (_stream == null || !_stream.CanWrite)
            {
                await DisplayAlert("Bluetooth", "Primero debes conectarte al HC-05", "OK");
                return;
            }

            if (countdownTime <= 0) return;

            isTimerRunning = true;
            await SendCommandToArduino("START");

            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                if (!isTimerRunning || countdownTime <= 0)
                {
                    isTimerRunning = false;
                    return false;
                }

                countdownTime--;
                TimerLabel.Text = $"00:{countdownTime:D2}";
                return true;
            });
        }

        private async void PauseTimerBtn_Clicked(object sender, EventArgs e)
        {
            if (isTimerRunning)
            {
                isTimerRunning = false;
                await SendCommandToArduino("PAUSE");
            }
        }

        private async void ResetTimerBtn_Clicked(object sender, EventArgs e)
        {
            countdownTime = 0;
            TimerLabel.Text = "00:00";
            await SendCommandToArduino("RESET");
        }

        private async void Set14SecTimerBtn_Clicked(object sender, EventArgs e)
        {
            countdownTime = 14;
            TimerLabel.Text = "00:14";
            await SendCommandToArduino("T14");
        }

        private async void Set24SecTimerBtn_Clicked(object sender, EventArgs e)
        {
            countdownTime = 24;
            TimerLabel.Text = "00:24";
            await SendCommandToArduino("T24");
        }

        // Quedó pendiente implementar el método para enviar comandos al Arduino
        private async Task SendCommandToArduino(string command)
        {
            if (_stream != null && _stream.CanWrite)
            {
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(command);
                await _stream.WriteAsync(buffer, 0, buffer.Length);
            }
        }
        private async void ConnectBluetoothBtn_Clicked(object sender, EventArgs e)
        {
            try
            {
                StatusLabel.Text = "Buscando...";
                StatusLabel.TextColor = Colors.DarkOrange;

                // Ejecutar búsqueda en un hilo separado
                var device = await Task.Run(() =>
                {
                    _client = new BluetoothClient();
                    var devices = _client.DiscoverDevices();
                    return devices.FirstOrDefault(d => d.DeviceName.Contains("HC-05"));
                });

                if (device == null)
                {
                    StatusLabel.Text = "HC-05 no encontrado";
                    StatusLabel.TextColor = Colors.Red;
                    await DisplayAlert("Bluetooth", "No se encontró el dispositivo HC-05.", "OK");
                    return;
                }

                // Conectar de forma asíncrona
                await Task.Run(() =>
                {
                    _client.Connect(device.DeviceAddress, BluetoothService.SerialPort);
                    _stream = _client.GetStream();
                });

                StatusLabel.Text = "Conectado";
                StatusLabel.TextColor = Colors.Green;
                await DisplayAlert("Bluetooth", "Conexión establecida con HC-05", "OK");
            }
            catch (Exception ex)
            {
                StatusLabel.Text = "Error al conectar";
                StatusLabel.TextColor = Colors.Red;
                await DisplayAlert("Error", $"Fallo la conexión: {ex.Message}", "OK");
            }
        }
        private async void DisconnectBluetoothBtn_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (_stream != null)
                {
                    _stream.Close();
                    _stream = null;
                }

                if (_client != null)
                {
                    _client.Close();
                    _client = null;
                }

                StatusLabel.Text = "Desconectado";
                StatusLabel.TextColor = Colors.DarkRed;
                await DisplayAlert("Bluetooth", "Desconexión exitosa", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Fallo al desconectar: {ex.Message}", "OK");
            }
        }


    }
}