using Core;
using Nest;

var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
    .DefaultIndex("person");
var client = new ElasticClient(settings);
var searchResponse = client.Where<Person>(y => y.Age >= 18);
Console.WriteLine(searchResponse.Documents.Count);