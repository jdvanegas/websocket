using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebSocket.Models;

namespace WebSocket.Hubs
{
  public class AccountHub : Hub
  {    
    private readonly string _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
    private readonly string _filePath = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data"), "datos.json");

    //METODO PARA GUARDAR EL REGISTRO, SE LLAMA DESDE EL CLIENTE, Recibe el número de cuenta y el valor como parametro
    public async Task Save(long account, decimal value)
    {
      var id = Context.ConnectionId;
      CheckFile(); // Metodo para verificar si el archivo datos.json
      try
      {
        var accounts = JsonConvert.DeserializeObject<List<Account>>(File.ReadAllText(_filePath)); //Deserializa los datos en el archivo datos.json a una Lista
        if (accounts == null) accounts = new List<Account>();
        var iaccount = accounts.FirstOrDefault(x => x.Number == account); // Busca si la cuenta existe
        if (iaccount != null)
        {
          var indexOfAccount = accounts.IndexOf(iaccount);
          iaccount.Registers.Add(value);//Si la cuenta existe, añade un nuevo registro
          accounts[indexOfAccount] = iaccount;// Actualiza la cuenta en la Lista
        }
        else
        {
          accounts.Add(new Account // Si la cuenta no existe crea una y la añade a la Lista
          {
            Number = account,
            Registers = new List<decimal> { value }
          });
        }

        File.WriteAllText(_filePath, JsonConvert.SerializeObject(accounts)); // Se serializa la lista a Json y se remplaza el texto en el archivo
        await Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", "Registro grabado OK"); // Devuelve Ok al cliente
      }
      catch (Exception)
      {
        await Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", "No-OK");//Si hay algun error en el proceso devuelve No-OK
      }
    }

    //METODO PARA BUSCAR UNA CUENTA, Recibe el numero de cuenta como parametro
    public async Task Search(long account)
    {
      CheckFile();// Metodo para verificar si el archivo datos.json
      try
      {
        var accounts = JsonConvert.DeserializeObject<List<Account>>(File.ReadAllText(_filePath)); //Deserializa los datos en el archivo datos.json a una Lista
        if (accounts == null) accounts = new List<Account>();
        var iaccount = accounts.FirstOrDefault(x => x.Number == account);// Busca si la cuenta existe
        await Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessageSearch", 
          JsonConvert.SerializeObject(iaccount, new JsonSerializerSettings //Serializa la cuenta a json y la devuelve como respuesta al cliente
        {
          ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
        }));
      }
      catch (Exception)
      {
        await Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessageSearch", "No-OK");//Si hay algun error en el proceso devuelve No-OK
      }
    }

    private void CheckFile()
    {
      if (!Directory.Exists(_path)) // Verificar si existe el Directorio
        Directory.CreateDirectory(_path); // Si no existe crea uno
      if (!File.Exists(_filePath))// Virificar si existe el Archivo
      {
        var file = File.Create(_filePath);// Si no existe lo crea
        file.Close();
      };
    }
  }
}
