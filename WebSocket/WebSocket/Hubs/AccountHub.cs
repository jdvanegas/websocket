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
    public async Task Save(long account, decimal value)
    {
      CheckFile();
      try
      {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        var filePath = Path.Combine(path, "datos.json");
        if (!Directory.Exists(path))
          Directory.CreateDirectory(path);        
        if (!File.Exists(filePath))
        {
          var file = File.Create(filePath);
          file.Close();
        };

        var accounts = JsonConvert.DeserializeObject<List<Account>>(File.ReadAllText(filePath));
        if (accounts == null) accounts = new List<Account>();
        var iaccount = accounts.FirstOrDefault(x => x.Number == account);
        if (iaccount != null)
        {
          var indexOfAccount = accounts.IndexOf(iaccount);
          iaccount.Registers.Add(value);
          accounts[indexOfAccount] = iaccount;
        }
        else
        {
          accounts.Add(new Account
          {
            Number = account,
            Registers = new List<decimal> { value }
          });
        }

        File.WriteAllText(filePath, JsonConvert.SerializeObject(accounts));
        await Clients.All.SendAsync("ReceiveMessage", "Registro grabado OK");
      }
      catch (Exception)
      {
        await Clients.All.SendAsync("ReceiveMessage", "No-OK");
      }
    }
    public async Task Search(long account)
    {
      CheckFile();
      try
      {        
        var accounts = JsonConvert.DeserializeObject<List<Account>>(File.ReadAllText(_filePath));
        if (accounts == null) accounts = new List<Account>();
        var iaccount = accounts.FirstOrDefault(x => x.Number == account);       
        await Clients.All.SendAsync("ReceiveMessageSearch", JsonConvert.SerializeObject(iaccount, new JsonSerializerSettings
        {
          ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
        }));
      }
      catch (Exception)
      {
        await Clients.All.SendAsync("ReceiveMessageSearch", "No-OK");
      }
    }

    private void CheckFile()
    {
      if (!Directory.Exists(_path))
        Directory.CreateDirectory(_path);
      if (!File.Exists(_filePath))
      {
        var file = File.Create(_filePath);
        file.Close();
      };
    }
  }
}
