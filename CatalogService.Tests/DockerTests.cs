using Docker.DotNet;
using Docker.DotNet.Models;
using System.Runtime.InteropServices;

namespace CatalogService.Tests
{
    public class DockerTests
    {
        [Fact]
        public async Task TestDockerContainerRunning()
        {
            // Configurar o cliente Docker
            using var dockerClient = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient();

            // Listar os contêineres
            var containers = await dockerClient.Containers.ListContainersAsync(new ContainersListParameters { All = true });

            // Verificar se há contêineres com o nome esperado
            Assert.NotEmpty(containers);

            var apiContainer = containers.FirstOrDefault(c => c.Names.Any(n => n.Contains("catalogservice")));
            Assert.NotNull(apiContainer); // Verifica se o contêiner da API está presente

            var dbContainer = containers.FirstOrDefault(c => c.Names.Any(n => n.Contains("catalogdb")));
            Assert.NotNull(dbContainer); // Verifica se o contêiner do banco de dados está presente

            // Verificar se os contêineres estão em execução
            Assert.Equal("running", apiContainer.State);
            Assert.Equal("running", dbContainer.State);
        }
    }
}