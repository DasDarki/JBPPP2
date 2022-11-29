using JBPPP2;
using JBPPP2.Controllers;

var window = new Window();

window.RegisterController<DataController>();

window.WaitForExit();