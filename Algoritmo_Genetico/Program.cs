using System;
using System.IO;
using System.Linq;

public class PSO
{
    // Definición de las variables del problema
    static int n; // número de clientes
    static int p; // número de hubs
    static int Q; // capacidad de los hubs
    static int[,] clientes; // matriz de clientes: [id, x, y, demanda]
    static List<double[]> todasLasSoluciones = new List<double[]>(); //Lista para guardar todas las soluciones
    
    // Definición de las variables del algoritmo PSO
    static int numParticulas = 3;
    static int numIteraciones = 1000;
    static int numMaxSoluciones = 1; // Cambia este valor al número deseado de soluciones
    static double w = 0.7; // inercia
    static double c1 = 1.5; // constante de aprendizaje cognitivo
    static double c2 = 1.5; // constante de aprendizaje social

    // Mejor solución encontrada
    static double[] mejorPosicionGlobal;
    static double mejorValorGlobal = double.MaxValue;

    static void Main(string[] args)
    {
        // Leer datos del archivo
        LeerDatos("datos2.txt");

        // Inicializar PSO
        InicializarPSO();

        // Ejecutar PSO
        EjecutarPSO();

        // Mostrar resultados
        Console.WriteLine("Mejor solución encontrada:");
        MostrarSolucion(mejorPosicionGlobal);
        Console.WriteLine("Valor de la función objetivo: " + mejorValorGlobal);
        Console.WriteLine("............................................");
        // Mostrar asignación de clientes a hubs en la mejor solución
        Console.WriteLine("Asignación de clientes a hubs en la mejor solución encontrada:");
        var asignacion = AsignacionClientesHub(mejorPosicionGlobal);
        foreach (var kvp in asignacion)
        {
            Console.WriteLine($"Hub {kvp.Key + 1}: {string.Join(", ", kvp.Value.Select(x => x + 1))}");
        }
        Console.WriteLine("............................................");
        // Mostrar todas las soluciones encontradas
        Console.WriteLine("\nTodas las soluciones encontradas:");
        for (int i = 0; i < todasLasSoluciones.Count; i++)
        {
            Console.WriteLine($"Solución {i + 1}:");
            MostrarSolucion(todasLasSoluciones[i], FuncionObjetivo(todasLasSoluciones[i]));
        }
    }

    static void MostrarSolucion(double[] solucion, double valorFuncionObjetivo)
    {
        
        for (int i = 0; i < p; i++)
        {
            Console.WriteLine($"Hub {i + 1}: ({solucion[i * 2]}, {solucion[i * 2 + 1]})");
        }
        Console.WriteLine("Valor de la función objetivo: " + valorFuncionObjetivo);
        Console.WriteLine("...........................");
    }

    static void MostrarSolucion(double[] solucion)
    {
        for (int i = 0; i < p; i++)
        {
            Console.WriteLine($"Hub {i + 1}: ({solucion[i * 2]}, {solucion[i * 2 + 1]})");
        }
    }

    static Dictionary<int, List<int>> AsignacionClientesHub(double[] posicion)
    {
        Dictionary<int, List<int>> asignacion = new Dictionary<int, List<int>>();

        // Inicializar asignación
        for (int i = 0; i < p; i++)
        {
            asignacion.Add(i, new List<int>());
        }

        // Asignar clientes al hub más cercano
        for (int i = 0; i < n; i++)
        {
            double distanciaMinima = double.MaxValue;
            int hubAsignado = -1;
            for (int j = 0; j < p; j++)
            {
                double distancia = Distancia(clientes[i, 1], clientes[i, 2], posicion[j * 2], posicion[j * 2 + 1]);
                if (distancia < distanciaMinima)
                {
                    distanciaMinima = distancia;
                    hubAsignado = j;
                }
            }
            asignacion[hubAsignado].Add(i);
        }

        return asignacion;
    }

    static void LeerDatos(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        string[] firstLine = lines[0].Split(' ');
        n = int.Parse(firstLine[0]);
        p = int.Parse(firstLine[1]);
        Q = int.Parse(firstLine[2]);
        clientes = new int[n, 4];
        for (int i = 0; i < n; i++)
        {
            string[] parts = lines[i + 1].Split(' ');
            for (int j = 0; j < 4; j++)
            {
                clientes[i, j] = int.Parse(parts[j]);
            }
        }
    }

    static void InicializarPSO()
    {
        mejorPosicionGlobal = new double[p * 2]; // Posiciones de los hubs (x, y)
        var rnd = new Random();
        for (int i = 0; i < p * 2; i++)
        {
            mejorPosicionGlobal[i] = rnd.Next(100); // Supongamos un espacio de búsqueda de 0 a 100
        }
    }

    static void EjecutarPSO()
    {
        double[][] particulas = new double[numParticulas][];
        double[][] velocidades = new double[numParticulas][];


        // Inicializar partículas
        var rnd = new Random();
        for (int i = 0; i < numParticulas; i++)
        {
            particulas[i] = new double[p * 2];
            velocidades[i] = new double[p * 2];
            for (int j = 0; j < p * 2; j++)
            {
                particulas[i][j] = rnd.Next(100); // Supongamos un espacio de búsqueda de 0 a 100
                velocidades[i][j] = 0;
            }
        }

        for (int iteracion = 0; iteracion < numIteraciones && todasLasSoluciones.Count < numMaxSoluciones; iteracion++)
        {
            for (int i = 0; i < numParticulas; i++)
            {
                double[] posicionActual = particulas[i];
                double[] velocidadActual = velocidades[i];

                // Evaluar la función objetivo
                double valorActual = FuncionObjetivo(posicionActual);

                // Actualizar mejor posición local
                if (valorActual < mejorValorGlobal)
                {
                    mejorValorGlobal = valorActual;
                    Array.Copy(posicionActual, mejorPosicionGlobal, p * 2);
                }

                // Agregar solución actual a la lista de todas las soluciones
                todasLasSoluciones.Add(posicionActual);

                // Actualizar velocidad y posición
                for (int j = 0; j < p * 2; j++)
                {
                    double r1 = rnd.NextDouble();
                    double r2 = rnd.NextDouble();
                    velocidadActual[j] = w * velocidadActual[j] +
                                          c1 * r1 * (mejorPosicionGlobal[j] - posicionActual[j]) +
                                          c2 * r2 * (mejorPosicionGlobal[j] - posicionActual[j]);
                    posicionActual[j] += velocidadActual[j];

                    // Limitar posición a un rango válido (0 a 100)
                    if (posicionActual[j] < 0) posicionActual[j] = 0;
                    if (posicionActual[j] > 100) posicionActual[j] = 100;
                }

                // Actualizar partícula
                particulas[i] = posicionActual;
                velocidades[i] = velocidadActual;
            }
        }
    }

    static double FuncionObjetivo(double[] posicion)
    {
        // Calcular la suma de las distancias de los clientes a los hubs
        double sumaDistancias = 0;
        for (int i = 0; i < n; i++)
        {
            double distanciaMinima = double.MaxValue;
            for (int j = 0; j < p; j++)
            {
                double distancia = Distancia(clientes[i, 1], clientes[i, 2], posicion[j * 2], posicion[j * 2 + 1]);
                if (distancia < distanciaMinima)
                {
                    distanciaMinima = distancia;
                }
            }
            sumaDistancias += distanciaMinima;
        }

        // Verificar restricciones de capacidad

        return sumaDistancias;
    }

    static double Distancia(int x1, int y1, double x2, double y2)
    {
        return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
    }    
}
