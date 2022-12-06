using JBPPP2;
using JBPPP2.Controllers;
using JBPPP2.Model;

Config.Load();

var window = new Window();

window.RegisterController<DataController>();
window.WaitForExit();