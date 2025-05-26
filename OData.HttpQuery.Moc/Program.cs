
using OData.HttpQuery.Linq.Factory;
using OData.HttpQuery.Moc.Models;

var teste = new { BomNome = new { Nome = "Nome de Jogador" }, Nome2 = "Nome do 2" };
string nomeTeste = teste.BomNome.Nome;

var factory = new ODataFactory<Contact>();

factory.Filter(x => x.Name == nomeTeste && (x.IsLead == true || x.Salary > 100.10));
Console.WriteLine();