// See https://aka.ms/new-console-template for more information

// Program to take in a file, and output a syntax tree of the file

using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Transactions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

internal class Program
{
    public static void Main(string[] args)
    {
        const string invalidType = "obj??";

        var simpleHelloWorldCode = """
                                   public class MainWindow
                                   {
                                       public void OnClick()
                                       {
                                           if (!IPAddress.TryParse("192.168.1.1", out var ipAddress))
                                           {
                                               MessageBox.Show("invalid IP address");
                                               return;
                                           }
                                           
                                           var bytes = ipAddress.GetAddressBytes();
                                           
                                           var sb = new StringBuilder();
                                           foreach (var b in bytes)
                                           {
                                               sb.Append(b.ToString("X2"));
                                           }
                                           
                                           MessageBox.Show(sb.ToString());
                                       }
                                   }
                                   """;

        var tree = CSharpSyntaxTree.ParseText(simpleHelloWorldCode);
        var root = tree.GetRoot();
        var sb = new StringBuilder();
        foreach (var node in root.DescendantNodes())
        {
            sb.AppendLine(node.GetType().Name);
        }


// var compilation = CSharpCompilation.Create("HelloWorld")
//     .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
//     .AddSyntaxTrees(tree);
//
// var model = compilation.GetSemanticModel(tree);
// var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

//making my own method into roslyn

// var compliationunit = SyntaxFactory.CompilationUnit()
        var a = new byte[5];

/* DEMO NODE GRAPH CODE GENERATOR
 *
 *
 *
 *
 */

        var nodeStrings = "System.Net.IPAddress.TryParseSystem.StringstrSystem.BooleanRETSystem.Net.IPAddressaddress";

        var hash = MD5.HashData(Encoding.UTF8.GetBytes(nodeStrings));
        var guid = new Guid(hash);


        var asm = typeof(Console).Assembly;
        var nsj = typeof(Newtonsoft.Json.JsonConvert).Assembly;

        var ty = nsj.GetExportedTypes();

        ty = asm.GetExportedTypes().Concat(ty).ToArray();

        List<FuncNodeDef> f = new();
        foreach (var t in ty) //               class/struct/enum/interface
        {
            var members = t.GetMembers();

            foreach (var member in members)
            {
                if (member.DeclaringType == null) continue;


                var node = new FuncNodeDef
                {
                    FQN = member.DeclaringType.FullName + "." + member.Name
                };

                switch (member)
                {
                    case MethodInfo mi:
                    {
                        if (mi.IsSpecialName) continue; //property getter/setter
                        if (mi.IsGenericMethod) continue; //generic method
                        if (mi.IsGenericMethodDefinition) continue;
                        if (mi.ContainsGenericParameters) continue;
                        if (mi.GetParameters().Any(x => x.ParameterType.FullName == null)) continue; //no name

                        node.MODS = GetMods(mi);
                        node.IN_PINS = GetFuncInPins(mi);
                        node.OUT_PINS = GetFuncOutPins(mi);
                        break;
                    }

                    case ConstructorInfo ci:
                    {
                        if (ci.IsGenericMethod) continue; //generic method
                        if (ci.IsGenericMethodDefinition) continue;
                        if (member.DeclaringType.FullName == null) continue;
                        if (ci.ContainsGenericParameters) continue;


                        node.FQN = "CTor " + member.DeclaringType.FullName;
                        node.IN_PINS = GetFuncInPins(ci);
                        node.OUT_PINS = new PinDef[]
                            { new() { NAME = "INST", TYPE = member.DeclaringType?.FullName ?? invalidType } };
                        node.MODS = GetMods(ci);
                        break;
                    }


                    case FieldInfo fi:
                    case PropertyInfo pi:
                    default:
                        continue;
                        throw new Exception();
                }

                if (!((node.MODS & Mods.Static) != 0)) continue;


                node.FN_ID = new Guid(GetNodeId(node));


                f.Add(node);
            }
        }

        var nonDistinct =
            from nodeDef in f
            group nodeDef by nodeDef.FN_ID
            into g
            where g.Count() > 1
            select g;


//find any node that has any pin of type InvalidType
        var invalidNodes = f.Where(x => x.IN_PINS.Any(y => y.TYPE == invalidType) ||
                                        x.OUT_PINS.Any(y => y.TYPE == invalidType));



        var progInfo = new ProgInfo()
        {
            AvalFunc = f.ToDictionary(x => x.FN_ID),
            Nodes = new Dictionary<Guid, DocumentNode>()
        };

        progInfo.Nodes.Add(Guid.NewGuid(), new DocumentNode()
        {
            FN_ID = f[0].FN_ID
        });

        JArray invalid = JArray.FromObject(invalidNodes);
        JArray nd = JArray.FromObject(nonDistinct);
        JArray arr = JArray.FromObject(f);
        JObject o = JObject.FromObject(progInfo);
        
        File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "aval_funcs.json"),
            o.ToString(Formatting.Indented));


        return;

        byte[] GetNodeId(FuncNodeDef funcNodeDef)
        {
            var funcNodeString = "";

            funcNodeString += funcNodeDef.FQN + ",";
            funcNodeString += funcNodeDef.MODS + ",";

            funcNodeString += "IN:";
            foreach (var pin in funcNodeDef.IN_PINS)
            {
                funcNodeString += pin.PinStr() + ",";
            }

            funcNodeString += "OUT:";
            foreach (var pin in funcNodeDef.OUT_PINS)
            {
                funcNodeString += pin.PinStr() + ",";
            }

            return MD5.HashData(Encoding.UTF8.GetBytes(funcNodeString));
        }


        Mods GetMods(MethodBase mi)
        {
            var m = (Mods)0;

            if (mi.IsPublic) m |= Mods.Public;
            if (mi.IsStatic) m |= Mods.Static;
            if (mi.IsConstructor) m |= Mods.CTor;

            return m;
        }

        PinDef[] GetFuncOutPins(MethodInfo mi)
        {
            var ret = new List<PinDef>();

            var retType = mi.ReturnType;
            if (retType != typeof(void))
            {
                ret.Add(new PinDef()
                {
                    NAME = "RET",
                    TYPE = retType.FullName ?? invalidType,
                    IsOptional = true
                });
            }

            var inputs = mi.GetParameters();
            for (var index = 0; index < inputs.Length; index++)
            {
                var i = inputs[index];
                if (!i.IsOut) continue;
                ret.Add(new PinDef()
                {
                    NAME = i.Name ?? $"out arg {index}",
                    TYPE = i.ParameterType.FullName ?? invalidType
                });
            }


            return ret.ToArray();
        }

        PinDef[] GetFuncInPins(MethodBase mb)
        {
            var ret = new List<PinDef>();

            var inputs = mb.GetParameters();
            for (var index = 0; index < inputs.Length; index++)
            {
                var i = inputs[index];
                if (i.IsOut) continue;

                var pin = new PinDef();
                pin.IsOptional = i.IsOptional;
                pin.NAME = i.Name ?? $"arg {index}";
                pin.TYPE = i.ParameterType.FullName ?? invalidType;
                ret.Add(pin);
            }


            return ret.ToArray();
        }
    }
}

class FuncNodeDef
{
    /// <summary>
    ///     ID is based on the FQN, MODS, and IN/OUT pins names off of the type
    /// </summary>
    public Guid FN_ID;


    /// <summary>
    ///     Fully Qualified Name of the function
    /// </summary>
    public string FQN;

    /// <summary>
    ///     Modifiers of the function
    /// </summary>
    public Mods MODS;

    /// <summary>
    ///     Input pins of the function
    /// </summary>
    public PinDef[] IN_PINS;

    /// <summary>
    ///     Output pins of the function
    /// </summary>
    public PinDef[] OUT_PINS;
}

[Flags]
public enum Mods
{
    Public = 1 << 0,
    Static = 1 << 1,
    CTor = 1 << 2,
}


class PinDef
{
    public string TYPE;
    public string NAME;
    public bool IsOptional;

    public string PinStr() => $"{NAME}:{TYPE}";
}


class ProgInfo
{
    /// <summary>
    ///     This is  all of the lib functions that are available to the user
    /// </summary>
    public Dictionary<Guid /*FN_ID*/, FuncNodeDef> AvalFunc;

    /// <summary>
    ///     This is all of the nodes that are in the program
    /// </summary>
    public Dictionary<Guid /*Node.ID*/, DocumentNode> Nodes;
}

internal class DocumentNode
{
    public Guid FN_ID;
}