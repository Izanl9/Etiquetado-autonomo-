using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using InTheHand.Net;

namespace EtiquetadoAuto.Services;

public class PrinterService
{
    public async Task Imprimir(string nombre, string codigo)
    {
        try {
            var client = new BluetoothClient();
            var device = client.PairedDevices.FirstOrDefault(d => d.DeviceName.Contains("Printer")); // Ajusta al nombre de tu impresora
            if (device == null) return;

            client.Connect(device.DeviceAddress, BluetoothService.SerialPort);
            using var stream = client.GetStream();
            
            // Generamos ZPL con prefijo "LOG-" para detectar salidas luego
            string zpl = $"^XA^CF0,50^FO50,50^FD{nombre}^FS^BY3,3,100^FO50,120^BCN,100,Y,N,N^FDLOG-{codigo}^FS^XZ";
            byte[] buffer = System.Text.Encoding.ASCII.GetBytes(zpl);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        } catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error de impresión: {ex.Message}");
            }
        }
    
}