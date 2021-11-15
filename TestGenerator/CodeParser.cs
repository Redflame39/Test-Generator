using System;
using TestGenerator.MembersData;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace TestGenerator
{
    public class CodeParser
    {
        public FileData GetFileData(string source)
        {
            CompilationUnitSyntax tree = CSharpSyntaxTree.ParseText(source).GetCompilationUnitRoot();
            List<ClassData> classes = new List<ClassData>();
            foreach (var classDeclaration in tree.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                classes.Add(GetClassData(classDeclaration));
            }

            return new FileData(classes);
        }

        private ClassData GetClassData(ClassDeclarationSyntax syntax)
        {

            List<ConstructorData> ctors = new List<ConstructorData>();
            foreach (var ctor in syntax.DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .Where((cd) => cd.Modifiers
                .Any((c) => c.IsKind(SyntaxKind.PublicKeyword))))
            {
                ctors.Add(GetConstructorData(ctor));
            }

            List<MethodData> methods = new List<MethodData>();
            foreach (var method in syntax.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where((md) => md.Modifiers
                .Any((m) => m.IsKind(SyntaxKind.PublicKeyword))))
            {
                methods.Add(GetMethodData(method));
            }

            return new ClassData(syntax.Identifier.ValueText, ctors, methods);
        }

        private ConstructorData GetConstructorData(ConstructorDeclarationSyntax syntax)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            foreach (var param in syntax.ParameterList.Parameters)
            {
                parameters.Add(param.Identifier.Text, param.Type.ToString());
            }

            return new ConstructorData(syntax.Identifier.Text, parameters);
        }

        private MethodData GetMethodData(MethodDeclarationSyntax syntax)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            foreach (var param in syntax.ParameterList.Parameters)
            {
                parameters.Add(param.Identifier.Text, param.Type.ToString());
            }

            return new MethodData(syntax.Identifier.Text, parameters, syntax.ReturnType.ToString());
        }
    }
}
