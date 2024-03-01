using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class Particle
{
    public List<int> position;
    public List<int> velocity;
    public double fitness;

    public Particle(List<int> position, List<int> velocity)
    {
        this.position = position;
        this.velocity = velocity;
        this.fitness = CalculateFitness();
        Console.WriteLine(fitness.ToString());
    }

    public double CalculateFitness()
    {
        // Inicializar la distancia total como 0
        double totalDistance = 0;

        // Para cada cliente
        foreach (var client in Program.clients)
        {
            double minDistance = double.MaxValue;

            // Encontrar el servidor más cercano al cliente
            foreach (var server in Program.servers)
            {
                double distance = Program.Distance(client, server);
                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }

            // Sumar la distancia mínima al total
            totalDistance += minDistance;
        }

        // Penalizar soluciones no factibles
        foreach (var server in Program.servers)
        {
            int capacity = server[3]; // Capacidad del servidor
            int demand = 0;

            // Calcular la demanda total de los clientes asignados a este servidor
            foreach (var assignedClientIndex in position)
            {
                List<int> assignedClient = Program.clients[assignedClientIndex - 1]; // -1 para obtener el índice correcto
                if (assignedClient[0] == server[0]) // Si el servidor es asignado a esta partícula
                {
                    demand += assignedClient[3]; // Sumar la demanda del cliente
                }
            }           

            // Si la demanda excede la capacidad, penalizar la solución
            if (demand > capacity)
            {
                totalDistance += 1000; // Valor de penalización arbitrario
            }
        }

        return totalDistance;
    }    
}

public class Program
{
    public static List<List<int>> clients;
    public static List<List<int>> servers;
    public static int p;

    public static void Main(string[] args)
    {
        string[] lines = File.ReadAllLines("datos.txt");
        string[] firstLine = lines[0].Split(' ');
        int n = int.Parse(firstLine[0]); // Número de clientes
        p = int.Parse(firstLine[1]); // Número de servidores
        int capacity = int.Parse(firstLine[2]); // Capacidad de cada servidor

        clients = new List<List<int>>();
        servers = new List<List<int>>();

        for (int i = 1; i <= n; i++)
        {
            string[] parts = lines[i].Split(' ');
            List<int> client = new List<int>();
            for (int j = 1; j < parts.Length; j++)
            {
                client.Add(int.Parse(parts[j]));
            }
            clients.Add(client);
        }

        for (int i = n + 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(' ');
            List<int> server = new List<int>();
            for (int j = 1; j < parts.Length; j++)
            {
                server.Add(int.Parse(parts[j]));
            }
            servers.Add(server);
        }

        // PSO
        int numParticles = 20; //Numero de soluciones que dara el programa
        int numIterations = 100;
        List<Particle> swarm = InitializeSwarm(numParticles);

        Particle globalBest = swarm[0];
        foreach (var particle in swarm)
        {
            if (particle.fitness < globalBest.fitness)
            {
                globalBest = particle;
            }
        }

        for (int iteration = 0; iteration < numIterations; iteration++)
        {
            foreach (var particle in swarm)
            {
                UpdateVelocity(particle, globalBest);
                UpdatePosition(particle);
                particle.fitness = particle.CalculateFitness();
            }

            foreach (var particle in swarm)
            {
                if (particle.fitness < globalBest.fitness)
                {
                    globalBest = particle;
                }
            }
        }

        
        Console.WriteLine("Global best solution:");
        Console.WriteLine("Position: " + string.Join(", ", globalBest.position));
        Console.WriteLine("Fitness: " + globalBest.fitness);
    }

    public static List<Particle> InitializeSwarm(int numParticles)
    {
        Random rand = new Random();
        List<Particle> swarm = new List<Particle>();

        for (int i = 0; i < numParticles; i++)
        {
            List<int> position = new List<int>();
            List<int> velocity = new List<int>();

            for (int j = 0; j < p; j++)
            {
                position.Add(rand.Next(1, clients.Count + 1)); // Asigna un cliente aleatorio a cada servidor
                velocity.Add(rand.Next(-1, 2)); // Inicializa la velocidad aleatoriamente
            }

            Particle particle = new Particle(position, velocity);
            swarm.Add(particle);
        }

        return swarm;
    }

    public static void UpdateVelocity(Particle particle, Particle globalBest)
    {
        Random rand = new Random();
        double w = 0.5; // Inertia weight
        double c1 = 2; // Cognitive weight
        double c2 = 2; // Social weight

        for (int i = 0; i < p; i++)
        {
            int r1 = rand.Next(0, 2);
            int r2 = rand.Next(0, 2);

            particle.velocity[i] = (int)(w * particle.velocity[i] +
                                    c1 * r1 * (globalBest.position[i] - particle.position[i]) +
                                    c2 * r2 * (particle.position[i] - globalBest.position[i]));
        }
    }

    public static void UpdatePosition(Particle particle)
    {
        for (int i = 0; i < p; i++)
        {
            // Actualiza la posición del servidor sumando la velocidad
            particle.position[i] += particle.velocity[i];

            // Si la posición excede el número de clientes, se ajusta al rango válido
            if (particle.position[i] < 1)
            {
                particle.position[i] = 1;
            }
            else if (particle.position[i] > clients.Count)
            {
                particle.position[i] = clients.Count;
            }
        }
    }

    public static double Distance(List<int> point1, List<int> point2)
    {
        // Distancia euclidiana entre dos puntos
        double sum = 0;
        for (int i = 0; i < point1.Count; i++)
        {
            sum += Math.Pow(point1[i] - point2[i], 2);
        }
        return Math.Sqrt(sum);
    }
}
