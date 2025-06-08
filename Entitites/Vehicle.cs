namespace Exercise.Entities;

// Classe de entidade que representa um veículo, apenas os valores necessários
public class Vehicle
{
    public int Id { get; set; }
    public required string Placa { get; set; }
    public required string Placa_Mercosul { get; set; }

}