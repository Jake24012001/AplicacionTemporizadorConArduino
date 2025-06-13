// Importación de bibliotecas necesarias
using InTheHand.Net.Bluetooth; // Para trabajar con dispositivos Bluetooth
using InTheHand.Net.Sockets;  // Cliente Bluetooth (RFCOMM)
using Microsoft.Maui.Controls; // Controles visuales en MAUI
using System.Diagnostics;      // Para Stopwatch (temporizador interno)
using System.IO;               // Para manejar el Stream de datos

namespace AplicacionTemporizadorConArduino
{
    public partial class MainPage : ContentPage
    {
        // Variables de control para el temporizador
        private int countdownTime = 0;          // Tiempo en segundos
        private bool isTimerRunning = false;    // Estado del temporizador
        private Stopwatch timer = new();        // No usado directamente, pero útil si expandes funcionalidad

        // Bluetooth
        private BluetoothClient _client;        // Cliente para conectar al HC-05
        private Stream _stream;                 // Flujo de datos hacia/desde el módulo

        public MainPage()
        {
            InitializeComponent(); // Inicializa componentes gráficos
        }

        [Obsolete]
        private async void StartTimerBtn_Clicked(object sender, EventArgs e)
        {
            // Verifica que exista una conexión Bluetooth activa
            if (_stream == null || !_stream.CanWrite)
            {
                await DisplayAlert("Bluetooth", "Primero debes conectarte al HC-05", "OK");
                return;
            }

            // Si no hay tiempo configurado, no hace nada
            if (countdownTime <= 0) return;

            isTimerRunning = true;
            await SendCommandToArduino("START"); // Instrucción al Arduino

            // Lógica para descontar tiempo cada segundo
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                if (!isTimerRunning || countdownTime <= 0)
                {
                    isTimerRunning = false;
                    return false; // Detiene el temporizador
                }

                countdownTime--; // Resta un segundo
                TimerLabel.Text = $"00:{countdownTime:D2}"; // Actualiza la etiqueta en pantalla
                return true; // Continúa
            });
        }

        private async void PauseTimerBtn_Clicked(object sender, EventArgs e)
        {
            if (isTimerRunning)
            {
                isTimerRunning = false;
                await SendCommandToArduino("PAUSE"); // Detiene temporizador en Arduino
            }
        }

        private async void ResetTimerBtn_Clicked(object sender, EventArgs e)
        {
            countdownTime = 0;
            TimerLabel.Text = "00:00"; // Reinicia el display
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

        // Envía un comando al Arduino a través del stream Bluetooth
        private async Task SendCommandToArduino(string command)
        {
            if (_stream != null && _stream.CanWrite)
            {
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(command);
                await _stream.WriteAsync(buffer, 0, buffer.Length);
            }
        }

        // Conecta al módulo HC-05 vía Bluetooth clásico
        private async void ConnectBluetoothBtn_Clicked(object sender, EventArgs e)
        {
            try
            {
                StatusLabel.Text = "Buscando...";
                StatusLabel.TextColor = Colors.DarkOrange;

                // Escanea dispositivos emparejados
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

                // Conecta al canal RFCOMM y abre stream
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

        // Cierra la conexión Bluetooth y libera recursos
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