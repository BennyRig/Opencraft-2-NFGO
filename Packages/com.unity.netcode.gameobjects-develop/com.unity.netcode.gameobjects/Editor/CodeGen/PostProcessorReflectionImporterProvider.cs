using Mono.Cecil;

namespace Unity.Netcode.gameobjects.Editor.CodeGen
{
    internal class PostProcessorReflectionImporterProvider : IReflectionImporterProvider
    {
        public IReflectionImporter GetReflectionImporter(ModuleDefinition moduleDefinition)
        {
            return new PostProcessorReflectionImporter(moduleDefinition);
        }
    }
}
