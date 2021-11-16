using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestGenerator.MembersData;

namespace TestGenerator
{
    public class TestsGenerator
    {
        private readonly SyntaxToken PublicModifier = SyntaxFactory.Token(SyntaxKind.PublicKeyword);
        private readonly TypeSyntax VoidReturnType = SyntaxFactory.ParseTypeName("void");
        private readonly AttributeSyntax SetupAttribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("SetUp"));
        private readonly AttributeSyntax MethodAttribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("Test"));
        private readonly AttributeSyntax ClassAttribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("TestFixture"));

        public Dictionary<string, string> GenerateTests(FileData fileData)
        {
            var fileNameCode = new Dictionary<string, string>();

            foreach (var classData in fileData.Classes)
            {
                var classDeclaration = GenerateClass(classData);
                var compilationUnit = SyntaxFactory.CompilationUnit()
                    .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
                    .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("NUnit.Framework")))
                    .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("ConsoleApp.Files")))
                    .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Moq")))
                    .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic")))
                    .AddMembers(classDeclaration);
                fileNameCode.Add(classData.Name + "Test", compilationUnit.NormalizeWhitespace().ToFullString());
            }

            return fileNameCode;
        }

        private ClassDeclarationSyntax GenerateClass(ClassData classData)
        {
            var fields = new List<FieldDeclarationSyntax>();
            VariableDeclarationSyntax variable;
            var interfaces = new Dictionary<string, string>();
            ConstructorData constructor = null;
            if (classData.Constructors.Count > 0)
            {
                constructor = FindLargestConstructor(classData.Constructors);
                interfaces = GetCustomTypeVariables(constructor.Parameters);
                foreach (var custom in interfaces)
                {
                    variable = GenerateVariable("_" + custom.Key, $"Mock<{custom.Value}>");
                    fields.Add(GenerateField(variable));
                }
            }

            variable = GenerateVariable(GetClassVariableName(classData.Name), classData.Name);
            fields.Add(GenerateField(variable));
            var methods = new List<MethodDeclarationSyntax>();
            methods.Add(GenerateSetUpMethod(constructor, classData.Name));
            foreach (var methodInfo in classData.Methods)
            {
                methods.Add(GenerateMethod(methodInfo, classData.Name));
            }

            return SyntaxFactory.ClassDeclaration(classData.Name + "Test")
                .AddMembers(fields.ToArray())
                .AddMembers(methods.ToArray())
                .AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.AttributeList().Attributes.Add(ClassAttribute)));
        }

        private ConstructorData FindLargestConstructor(List<ConstructorData> constructors)
        {
            var constructor = constructors[0];
            foreach (var temp in constructors)
            {
                if (constructor.Parameters.Count < temp.Parameters.Count)
                {
                    constructor = temp;
                }
            }
            return constructor;
        }

        private Dictionary<string, string> GetBaseTypeVariables(Dictionary<string, string> parameters)
        {
            var res = new Dictionary<string, string>();
            foreach (var parameter in parameters)
            {
                if (parameter.Value[0] != 'I')
                {
                    res.Add(parameter.Key, parameter.Value);
                }
            }

            return res;
        }

        private Dictionary<string, string> GetCustomTypeVariables(Dictionary<string, string> parameters)
        {
            var res = new Dictionary<string, string>();

            foreach (var parameter in parameters)
            {
                if (parameter.Value[0] == 'I')
                {
                    res.Add(parameter.Key, parameter.Value);
                }
            }

            return res;
        }

        private string ConvertParametersToStringRepresentation(Dictionary<string, string> parameters)
        {
            var s = "";
            foreach (var pair in parameters)
            {
                s += pair.Value[0] == 'I' ? $"_{pair.Key}.Object" : $"{pair.Key}";
                s += ", ";
            }

            return s.Length > 0 ? s.Remove(s.Length - 2, 2) : "";
        }

        private string GetClassVariableName(string className)
        {
            return "_" + className[0].ToString().ToLower() + className.Remove(0, 1);
        }

        private StatementSyntax GenerateBasesTypesAssignStatement(string varName, string varType)
        {
            return SyntaxFactory.ParseStatement(string.Format
            (
                "var {0} = default({1});",
                varName,
                varType
            ));
        }

        private StatementSyntax GenerateCustomsTypesAssignStatement(string varName, string constructorName, string invokeArgs = "")
        {
            return SyntaxFactory.ParseStatement(string.Format
            (
                "{0} = new {1}{2};",
                varName,
                constructorName,
                $"({invokeArgs})"
            ));
        }

        private StatementSyntax GenerateFunctionCall(string varName, string funcName, string invokeArgs = "")
        {
            return SyntaxFactory.ParseStatement(string.Format
            (
                "var {0} = {1}{2};",
                varName,
                funcName,
                $"({invokeArgs})"
            ));
        }

        private StatementSyntax GenerateVoidFunctionCall(string funcName, string invokeArgs = "")
        {
            return SyntaxFactory.ParseStatement(string.Format
            (
                "{0}{1};",
                funcName,
                $"({invokeArgs})"
            ));
        }

        private VariableDeclarationSyntax GenerateVariable(string varName, string typeName)
        {
            return SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(typeName))
                .AddVariables(SyntaxFactory.VariableDeclarator(varName));
        }

        private FieldDeclarationSyntax GenerateField(VariableDeclarationSyntax var)
        {
            return SyntaxFactory.FieldDeclaration(var)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
        }

        private void GenerateArrangePart(List<StatementSyntax> body, Dictionary<string, string> parameters)
        {
            var baseTypeVars = GetBaseTypeVariables(parameters);
            foreach (var var in baseTypeVars)
            {
                body.Add(GenerateBasesTypesAssignStatement(var.Key, var.Value));
            }
        }

        private void GenerateActPart(List<StatementSyntax> body, MethodData methodInfo, string checkedClassVariable)
        {
            if (methodInfo.ReturnValueType != "void")
            {
                body.Add(GenerateFunctionCall("actual", GetClassVariableName(checkedClassVariable) + "." + methodInfo.Name, ConvertParametersToStringRepresentation(methodInfo.Parameters)));
            }
            else
            {
                body.Add(GenerateVoidFunctionCall(GetClassVariableName(checkedClassVariable) + "." + methodInfo.Name, ConvertParametersToStringRepresentation(methodInfo.Parameters)));
            }
        }

        private InvocationExpressionSyntax GenerateExpression(string firstCall, string secondCall)
        {
            return SyntaxFactory.InvocationExpression(
                       SyntaxFactory.MemberAccessExpression(
                           SyntaxKind.SimpleMemberAccessExpression,
                           SyntaxFactory.IdentifierName(firstCall),
                           SyntaxFactory.IdentifierName(secondCall)));
        }

        private void GenerateAssertPart(List<StatementSyntax> body, string returnType)
        {
            body.Add(GenerateBasesTypesAssignStatement("expected", returnType));
            var invocationExpression = GenerateExpression("Assert", "That");
            var secondPart = GenerateExpression("Is", "EqualTo").WithArgumentList(ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(
                new SyntaxNodeOrToken[] {
                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName("expected"))})));
            var argList = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(
                new SyntaxNodeOrToken[] {
                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName("actual")),
                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName(secondPart.ToString()))}));

            var s = ExpressionStatement(invocationExpression.WithArgumentList(argList));
            body.Add(s);
        }

        private MethodDeclarationSyntax GenerateSetUpMethod(ConstructorData constructorInfo, string className)
        {
            List<StatementSyntax> body = new List<StatementSyntax>();
            if (constructorInfo != null)
            {
                var baseTypeVars = GetBaseTypeVariables(constructorInfo.Parameters);
                foreach (var var in baseTypeVars)
                {
                    body.Add(GenerateBasesTypesAssignStatement(var.Key, var.Value));
                }

                var customVars = GetCustomTypeVariables(constructorInfo.Parameters);
                foreach (var var in customVars)
                {
                    body.Add(GenerateCustomsTypesAssignStatement("_" + var.Key, $"Mock<{var.Value}>", ""));
                }
            }

            body.Add(GenerateCustomsTypesAssignStatement(
                GetClassVariableName(className),
                className,
                constructorInfo != null ? ConvertParametersToStringRepresentation(constructorInfo.Parameters) : ""));
            return SyntaxFactory.MethodDeclaration(VoidReturnType, "SetUp")
                .AddModifiers(PublicModifier)
                .AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.AttributeList().Attributes.Add(SetupAttribute)))
                .WithBody(SyntaxFactory.Block(body)); ;
        }

        private MethodDeclarationSyntax GenerateMethod(MethodData methodInfo, string checkedClassVar)
        {
            List<StatementSyntax> body = new List<StatementSyntax>();
            GenerateArrangePart(body, methodInfo.Parameters);
            GenerateActPart(body, methodInfo, checkedClassVar);
            if (methodInfo.ReturnValueType != "void")
            {
                GenerateAssertPart(body, methodInfo.ReturnValueType);
            }

            body.Add(CreateFailExpression());
            return SyntaxFactory.MethodDeclaration(VoidReturnType, methodInfo.Name)
                .AddModifiers(PublicModifier)
                .AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.AttributeList().Attributes.Add(MethodAttribute)))
                .WithBody(SyntaxFactory.Block(body)); ;
        }

        private ExpressionStatementSyntax CreateFailExpression()
        {
            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("Assert"),
                            SyntaxFactory.IdentifierName("Fail")))
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                SyntaxFactory.Argument(
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.Literal("autogenerated")))))));
        }
    }
}
