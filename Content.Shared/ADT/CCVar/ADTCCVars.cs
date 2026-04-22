using Robust.Shared.Configuration;
using Content.Shared.Atmos;
using Robust.Shared;
using System.Runtime.InteropServices.Marshalling;
namespace Content.Shared.ADT.CCVar;
[CVarDefs]
public sealed class ADTCCVars
{
    public static readonly CVarDef<string> HeadshotUrl =
    CVarDef.Create("ic.headshot_url", "", CVar.SERVER | CVar.REPLICATED);

}

