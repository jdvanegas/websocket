(function () {
  "use strict";

  var connection = new signalR.HubConnectionBuilder().withUrl("/accountHub").build();

  //Disable send button until connection is established
  document.getElementById("save").disabled = true;
  document.getElementById("search").disabled = true;

  connection.on("ReceiveMessage", function (message) {
    document.getElementById("receive").innerHTML = message;
  });

  connection.on("ReceiveMessageSearch", function (message) {    
    document.getElementById("registers").innerHTML = "";
    var account = JSON.parse(message);
    if (account) {
      document.getElementById("registersLabel").style.display = "block"; 
      document.getElementById("accountNumber").innerText = "Numero de cuenta: " + account.number;
      var items = "";
      for (var i = 0; i < account.registers.length; i++) {
        var item = account.registers[i];
        items += "<li>" + item + "</li>";        
      }
      document.getElementById("registers").innerHTML = items;
    }
  });

  connection.start().then(function () {
    document.getElementById("save").disabled = false;
    document.getElementById("search").disabled = false;
  }).catch(function (err) {
    return console.error(err.toString());
  });

  document.getElementById("save").addEventListener("click", function (event) {
    var number = document.getElementById("number").value;
    var message = document.getElementById("value").value;

    connection.invoke("Save", number, message).catch(function (err) {
      return console.error(err.toString());
    });

    event.preventDefault();
  });

  document.getElementById("search").addEventListener("click", function (event) {
    var number = document.getElementById("account").value;    
    connection.invoke("Search", number).catch(function (err) {
      return console.error(err.toString());
    });

    event.preventDefault();
  });
}());