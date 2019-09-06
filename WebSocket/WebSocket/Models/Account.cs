using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebSocket.Models
{
  public class Account
  {
    public long Number { get; set; }
    public List<string> Registers { get; set; } = new List<string>();
  }
}
