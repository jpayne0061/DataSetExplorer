
using Microsoft.AspNetCore.SignalR;

namespace SalaryExplorer.Models
{
  public class SignalRHub : Hub
  {
    //you're going to invoke this method from the client app
    public void Echo(string message)
    {
      //you're going to configure your client app to listen for this
      Clients.All.SendAsync("Send", message);
    }
  }
}
