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
    }

    public double CalculateFitness()
    {
        double totalDistance = 0;
        foreach (var client in Program.clients)
        {
            int closestServerIndex = -1;
            double minDistance = double.MaxValue;
            for (int i = 0; i < Program.servers.Count; i++)
            {
                double distance = Program.Distance(client, Program.servers[i]);
                if (distance < minDistance && !position.Contains(i))
                {
                    minDistance = distance;
                    closestServerIndex = i;
                }
            }
            if (closestServerIndex != -1)
            {
                totalDistance += minDistance;
                position[closestServerIndex] = closestServerIndex;
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
        int n = int.Parse(firstLine[0]);
        p = int.Parse(firstLine[1]);
        int capacity = int.Parse(firstLine[2]);

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

        int numParticles = 20;
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
        Console.WriteLine("------------------------------------");
        for (int i = 0; i < globalBest.position.Count; i++)
        {
            Console.WriteLine($"Servidor {i + 1}: Cliente {globalBest.position[i] + 1}");
            var assignedClients = Program.clients[globalBest.position[i]];
            Console.WriteLine($"Clientes asignados al Servidor {i + 1}:");
            Console.WriteLine($"Cliente {assignedClients[0]}");
            Console.WriteLine(".............................");
        }
        Console.WriteLine("------------------------------------");
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
                position.Add(-1); // Inicializa con -1 para representar que ningún cliente ha sido asignado aún
                velocity.Add(rand.Next(-1, 2));
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
        Random rand = new Random();

        for (int i = 0; i < p; i++)
        {
            if (rand.NextDouble() < 0.5) // Probabilidad de 0.5 de moverse
            {
                particle.position[i] += particle.velocity[i];
                if (particle.position[i] < 0 || particle.position[i] >= clients.Count)
                {
                    particle.position[i] = rand.Next(0, clients.Count);
                }
            }
        }
    }

    public static double Distance(List<int> point1, List<int> point2)
    {
        double sum = 0;
        for (int i = 0; i < point1.Count; i++)
        {
            sum += Math.Pow(point1[i] - point2[i], 2);
        }
        return Math.Sqrt(sum);
    }
}
