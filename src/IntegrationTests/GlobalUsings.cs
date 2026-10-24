// Fixie's package-injected entry point (build/Fixie.Main.cs) is compiled into this
// project and uses `Task<int>` without a using directive, relying on implicit usings.
// This project keeps `ImplicitUsings` disabled (all source uses explicit, file-scoped
// using directives), so the single namespace that entry point needs is declared globally
// here instead. Duplicating it with the explicit `using System.Threading.Tasks;` in other
// files is harmless.
global using System.Threading.Tasks;
