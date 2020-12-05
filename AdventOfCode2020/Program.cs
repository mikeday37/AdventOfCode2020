// This is just to try C# 9's new "Top-level statements"
// Would be a simple one-liner except for that fact that Program.Main needs the STAThread attribute.
// See: https://stackoverflow.com/questions/64946371/how-to-handle-stathread-in-c-sharp-9-using-top-level-program-cs

using System.Threading;
using AdventOfCodeScaffolding;

var t = new Thread(()=>
	new AdventOfCodeApp().Run()
);
t.SetApartmentState(ApartmentState.STA);
t.Start();