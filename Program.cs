using System;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var sensorData = Enumerable.Range(1, 10)
            .Select(i => new SensorData { SensorId = i, Value = new Random().NextDouble() * 100 })
            .ToList();

        var parentTask = Task.Run(async () =>
        {
            var tasks = sensorData.Select(data =>
                Task.Run(() => ProcessSensorData(data))
                    .ContinueWith(t => Console.WriteLine($"Procesamiento completado para el sensor {data.SensorId}"), TaskContinuationOptions.OnlyOnRanToCompletion)
                    .ContinueWith(t => Console.WriteLine($"Procesamiento cancelado para el sensor {data.SensorId}"), TaskContinuationOptions.OnlyOnCanceled)
                    .ContinueWith(t => Console.WriteLine($"Error en el sensor {data.SensorId}: {t.Exception?.InnerException?.Message}"), TaskContinuationOptions.OnlyOnFaulted)
            ).ToArray();

            await Task.WhenAll(tasks); 
        });

        var completedTask = await Task.WhenAny(parentTask, Task.Delay(7000));

        if (completedTask == parentTask)
            Console.WriteLine("Todas las tareas de procesamiento se completaron.");
        else
            Console.WriteLine("Tiempo de espera agotado antes de que todas las tareas se completaran.");

        await parentTask; 
    }

    static async Task ProcessSensorData(SensorData data)
    {
        try
        {
            await Task.Delay(new Random().Next(800, 2000));
            Console.WriteLine($"Procesando datos del sensor {data.SensorId} con valor {data.Value:F2}");

            if (data.Value > 80)
                Console.WriteLine($"ALERTA: Valor anómalo detectado en el sensor {data.SensorId} ({data.Value:F2})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en el procesamiento del sensor {data.SensorId}: {ex.Message}");
        }
    }
}

class SensorData
{
    public int SensorId { get; set; }
    public double Value { get; set; }
}
