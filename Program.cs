// Biblioteca para a interação com o banco de dados
using Microsoft.EntityFrameworkCore;
// Namespace com o contexto de dados de EF Core
using Exercise.Data;
// Biblioteca para criação de requisições HTTP em JSON
using System.Net.Http.Json;
// Biblioteca para manipulação de JSON
using System.Text.Json.Nodes;
// Biblioteca do Regex, para validar strings
using System.Text.RegularExpressions;
// Namespace com a entidade Vehicle
using Exercise.Entities;

// Classe para facilitar a leitura dos tokens de autenticação com System.Net.Http
class TokenMessage
{
    public string? Token { get; set; }
}


class Program
{
    // Formata a placa antiga no padrão Mercosul
    public static string FormatPlaca(string placa)
    {
        int quartoNum = (int)char.GetNumericValue(placa[4]);
        char quartoNumFormatado = (char)(quartoNum + 'A');
        string placaFormatada = placa.Substring(0, 4) +
        quartoNumFormatado +
        placa.Substring(5, 2);
        return placaFormatada;
    }

    // Loga a placa e a placa Mercosul do primeiro veículo encontrado na consulta
    public static async Task PlacaLog(IQueryable<Vehicle> query)
    {
        Vehicle first = await query.FirstAsync();

        Console.WriteLine($"ID: {first.Id}, Placa: {first.Placa}, Placa Mercosul: {first.Placa_Mercosul}");
    }

    static async Task<int> Main(string[] args)
    {

        string placa;
        try
        {
            placa = args[0];
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine("O valor da placa não foi fornecido. Por favor, forneça um valor válido após o comando.");
            return 1;
        }
        // Regex do formato antigo 
        string oldPattern = @"^[A-Z]{3}[0-9]{4}$";
        // Regex do formato Mercosul
        string mercosulPattern = @"^[A-Z]{3}[0-9]{1}[A-Z]{1}[0-9]{2}$";
        DataContext db = new();
        string? column;

        // Verifica se a placa está no formato antigo, Mercosul ou se é inválida
        if (Regex.IsMatch(placa, oldPattern))
        {
            Console.WriteLine("Placa no formato antigo detectada.");
            column = "placa";
        }
        else if (Regex.IsMatch(placa, mercosulPattern))
        {
            Console.WriteLine("Placa no formato Mercosul detectada.");
            column = "placa_mercosul";
        }
        else
        {
            Console.WriteLine("Placa inválida. Por favor, verifique se o valor está correto.");
            return 1;
        }
        // Verifica se a placa já está cadastrada no banco de dados
        IQueryable<Vehicle> query;
        if (column == "placa")
        {
            query = db.Vehicles.FromSqlInterpolated($"SELECT * FROM vehicles WHERE placa = {placa}");
            if (await query.AnyAsync())
            {
                Console.WriteLine("Placa já cadastrada no banco de dados:");
                await PlacaLog(query);
                return 0;
            }
        }
        else
        {
            query = db.Vehicles.FromSqlInterpolated($"SELECT * FROM vehicles WHERE placa_mercosul = {placa}");
            if (await query.AnyAsync())
            {
                Console.WriteLine("Placa já cadastrada no banco de dados:");
                await PlacaLog(query);
                return 0;
            }
        }

        Console.WriteLine("Prosseguindo com a consulta...");

        Console.WriteLine("Digite o email do usuário:");
        string? email = Console.ReadLine();

        Console.WriteLine("Digite a senha do usuário:");
        string? userpassword = Console.ReadLine();


        HttpClient client = new();

        // Cria a solicitação JSON para obter o token de autenticação
        JsonContent content = JsonContent.Create(new
        {
            username = email,
            password = userpassword
        });

        // Tenta obter o token de autenticação e coloca no cabeçalho da requisição padrão do cliente
        // Retorna um erro em caso de falha
        try
        {
            HttpResponseMessage responseToken = await client.PostAsync("https://service.checkview.com.br/api/token/", content);
            TokenMessage? tokenMessage = await responseToken.Content.ReadFromJsonAsync<TokenMessage>();
            if (tokenMessage?.Token == null)
            {
                Console.WriteLine("Token inválido ou não fornecido.");
                return 1;
            }
            string token = tokenMessage.Token;
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            Console.WriteLine("Token obtido com sucesso.");
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("Erro ao obter o token: " + e.Message);
            return 1;
        }

        // Cria o conteúdo JSON para a consulta de placa
        content = JsonContent.Create(new
        {
            data = new Dictionary<string, object>
            {
                ["placa|bf4016e5-5df9-4cda-9196-31519b50a6d9"] = new
                {
                    value = args[0],
                }
            },
            products = new[]
            {
                "2dcfb38b-25e1-4eba-9fee-34144fd8eb58"
            }
        });

        // Tenta fazer a requisição de consulta de placa e processar a resposta
        try
        {
            HttpResponseMessage responseVehicle = await client.PostAsync("https://service.checkview.com.br/call/query/", content);
            JsonNode? vehicleObject = await responseVehicle.Content.ReadFromJsonAsync<JsonNode>();
            if (vehicleObject == null)
            {
                Console.WriteLine("Resposta da consulta de placa inválida.");
                return 1;
            }

            // Extrai as placas Mercosul e antiga da resposta JSON
            string? placaMercosul = vehicleObject["content"]?
            ["data"]?
            ["placa|bf4016e5-5df9-4cda-9196-31519b50a6d9"]?
            ["value"]?.ToString();
            string? placaAntiga = vehicleObject["content"]?
            ["responses"]?.AsArray()[0]?
            ["response"]?.AsArray()[0]?
            ["RESULTADO"]?
            ["AGREGADOS"]?
            ["nr_placa"]?.ToString();

            Console.WriteLine("Consulta de placa realizada com sucesso.");
            Console.WriteLine("Placa Mercosul: " + placaMercosul);
            Console.WriteLine("Placa Antiga: " + placaAntiga);

            if (placaAntiga == null || placaAntiga == "")
            {
                // Placa antiga não encontrada, mas a Mercosul sim, registrar a Mercosul em placa e placa_mercosul
                if (!(placaMercosul == null || placaMercosul == ""))
                {
                    Console.WriteLine("Placa antiga não encontrada. Registrando a placa Mercosul em ambos os campos...");
                    await db.AddAsync<Vehicle>(new Vehicle
                    {
                        Placa = placaMercosul,
                        Placa_Mercosul = placaMercosul
                    });
                    await db.SaveChangesAsync();
                    Console.WriteLine("Placa Mercosul registrada com sucesso na base de dados.");
                    return 0;
                }
                // Nenhuma placa encontrada, informar ao usuário e retornar
                else
                {
                    Console.WriteLine("Nenhuma placa encontrada. Por favor, verifique se a placa está correta.");
                    return 1;
                }
            }
            // Placa antiga encontrada, mas a Mercosul não, registrar a antiga no campo placa e formatar para o padrão Mercosul para registrar no campo placa_mercosul
            else if (placaMercosul == null || placaMercosul == "")
            {
                Console.WriteLine("Placa mercosul não encontrada. Formatando a antiga no padrão novo e salvando no banco de dados...");

                string placaFormatada = FormatPlaca(placaAntiga);
                await db.AddAsync<Vehicle>(new Vehicle
                {
                    Placa = placaAntiga,
                    Placa_Mercosul = placaFormatada
                });
                await db.SaveChangesAsync();
                Console.WriteLine("Placa registrada com sucesso no banco de dados.");
                return 0;

            }
            // Ambas as placas foram encontradas, se a placa Mercosul estiver no formato antigo, formatar para o novo padrão
            // e registrar ambas as placas no banco de dados
            else
            {
                Console.WriteLine("Placas encontradas.");
                if (Regex.IsMatch(placaMercosul, oldPattern))
                {
                    Console.WriteLine("Placa Mercosul está no formato antigo. Formatando para o novo padrão...");
                    placaMercosul = FormatPlaca(placaMercosul);
                }
                await db.AddAsync<Vehicle>(new Vehicle
                {
                    Placa = placaAntiga,
                    Placa_Mercosul = placaMercosul
                });
                await db.SaveChangesAsync();
                Console.WriteLine("Placas registradas com sucesso na base de dados.");
                return 0;

            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("Erro ao consultar a placa: " + e.Message);
            return 1;
        }
    }
}