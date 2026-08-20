using Modding;            

namespace HkRl
{
    public class HkRlMod : Mod        // what do you inherit from?
    {
        public HkRlMod() : base("HkRl") { }

        public override string GetVersion() => "0.1.0";

        public override void Initialize()
        {
            Log("My first C# code, and my first time creating a mod");               // log
        }
    }
}