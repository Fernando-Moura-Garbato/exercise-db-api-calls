# Exercício de DB e requisições HTTP
Este é um exercício simples executado em linha de comando sobre a manipulação de pacotes JSON e sua relação com o banco de dados. Ele possui algoritmos de verificação de integridade dos dados, e comunica cada passo ou erro ao usuário.
 O exercício foi feito Utilizando o ambiente .NET 9.0 e a linguagem C#, com o banco de dados SQLite.

# Dependências e execução
Para executar o código, é preciso apenas de:
* .NET SDK 9.0, disponível em https://dotnet.microsoft.com/pt-br/download/dotnet/9.0

Utilize o comando `dotnet run` na pasta principal para executar. Os comandos dotnet `dotnet restore` e `dotnet build` serão executados implicitamente, mas podem ser usados pelo usuário para gerar o arquivo executável.
O código pode ser compilado e executado em Windows, Linux e MacOS, mas foi testado apenas em Windows.

## Estruturação do banco de dados
Utilizando a forma simplificada de formatação dos dados do SQLite, o schema seguinte foi utilizado:
CREATE TABLE vehicles(
id integer primary key not null, 
placa text not null, 
placa_mercosul text not null);

Esses são os únicos campos requiridos no exercícios, e foram criados da de forma que:
* O type "TEXT" possui alocação de memória variável, de acordo com o dado inserido;
* Tanto `placa` quanto `placa_mercosul` precisam ser informadas no momento de inserção, o que é seguido no programa;
* `id` não precisa de AUTOINCREMENT, já que é a única primary key.
