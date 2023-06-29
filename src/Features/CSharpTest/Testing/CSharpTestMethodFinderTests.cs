﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editor.UnitTests.Workspaces;
using Microsoft.CodeAnalysis.Features.Testing;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests;

[UseExportProvider]
public class CSharpTestMethodFinderTests
{
    #region Xunit

    [Fact]
    public async Task TestFindsXUnitFactMethod()
    {
        var code = """
            using Xunit;
            public class TestClass
            {
                [Fact]
                public void Test$$Method1() { }
            }
            """;
        await TestXunitAsync(code, "TestMethod1");
    }

    [Fact]
    public async Task TestFindsXUnitTheoryMethod()
    {
        var code = """
            using Xunit;
            public class TestClass
            {
                [Theory]
                public void Test$$Method1() { }
            }
            """;
        await TestXunitAsync(code, "TestMethod1");
    }

    [Fact]
    public async Task TestFindsXUnitAliasedFactMethod()
    {
        var code = """
            using Xunit;
            using test = Xunit.FactAttribute;
            public class TestClass
            {
                [test]
                public void Test$$Method1() { }
            }
            """;
        await TestXunitAsync(code, "TestMethod1");
    }

    [Fact]
    public async Task TestFindsXUnitFactOnlySelectedMethod()
    {
        var code = """
            using Xunit;
            public class TestClass
            {
                [Fact]
                public void Test$$Method1() { }

                [Fact]
                public void TestMethod2() { }

                public void NotTestMethod() { }
            }
            """;
        await TestXunitAsync(code, "TestMethod1");
    }

    [Fact]
    public async Task TestFindsXunitMethodsInClassMethod()
    {
        var code = """
            using Xunit;
            public class Test$$Class
            {
                [Fact]
                public void TestMethod1() { }

                [Fact]
                public void TestMethod2() { }

                [Theory]
                public void TestMethod3() { }

                public void NotTestMethod() { }
            }
            """;
        await TestXunitAsync(code, "TestMethod1", "TestMethod2", "TestMethod3");
    }

    [Fact]
    public async Task TestXunitNoMethodsInClass()
    {
        var code = """
            using Xunit;
            public class Test$$Class
            {
                public void NotTestMethod() { }
            }
            """;
        await TestXunitAsync(code);
    }

    [Fact]
    public async Task TestFindsSelectedXunitMethods()
    {
        var code = """
            using Xunit;
            public class TestClass
            {
                [Fact]
                [|public void TestMethod1() { }

                [Fact]
                public void TestMethod2()|] { }

                [Theory]
                public void TestMethod3() { }

                public void NotTestMethod() { }
            }
            """;
        await TestXunitAsync(code, "TestMethod1", "TestMethod2");
    }

    #endregion

    #region NUnit

    [Fact]
    public async Task TestFindsNUnitTestMethod()
    {
        var code = """
            using NUnit.Framework;
            public class TestClass
            {
                [Test]
                public void Test$$Method1() { }
            }
            """;
        await TestNUnitAsync(code, "TestMethod1");
    }

    [Fact]
    public async Task TestFindsNUnitTheoryMethod()
    {
        var code = """
            using NUnit.Framework;
            public class TestClass
            {
                [Theory]
                public void Test$$Method1() { }
            }
            """;
        await TestNUnitAsync(code, "TestMethod1");
    }

    [Fact]
    public async Task TestFindsNUnitTestCaseMethod()
    {
        var code = """
            using NUnit.Framework;
            public class TestClass
            {
                [TestCase]
                public void Test$$Method1() { }
            }
            """;
        await TestNUnitAsync(code, "TestMethod1");
    }

    [Fact]
    public async Task TestFindsNUnitTestCaseSourceMethod()
    {
        var code = """
            using NUnit.Framework;
            public class TestClass
            {
                public static string s_s = "";
                [TestCaseSource(nameof(s_s))]
                public void Test$$Method1() { }
            }
            """;
        await TestNUnitAsync(code, "TestMethod1");
    }

    [Fact]
    public async Task TestFindsNUnitMethodsInClass()
    {
        var code = """
            using NUnit.Framework;
            public class Test$$Class
            {
                [Test]
                public void TestMethod1() { }

                [Theory]
                public void TestMethod2() { }

                [TestCase]
                public void TestMethod3() { }

                public static string s_s = "";
                [TestCaseSource(nameof(s_s))]
                public void TestMethod4() { }

                public void NotTestMethod() { }
            }
            """;
        await TestNUnitAsync(code, "TestMethod1", "TestMethod2", "TestMethod3", "TestMethod4");
    }

    #endregion

    #region MSTest

    [Fact]
    public async Task TestFindsMSTestTestMethod()
    {
        var code = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            public class TestClass
            {
                [TestMethod]
                public void Test$$Method1() { }
            }

            """;
        await TestMSTestAsync(code, "TestMethod1");
    }

    [Fact]
    public async Task TestFindsMSTestMethodsInClass()
    {
        var code = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            public class Test$$Class
            {
                [TestMethod]
                public void TestMethod1() { }

                [TestMethod]
                public void TestMethod2() { }

                public void NotTestMethod() { }
            }
            """;
        await TestMSTestAsync(code, "TestMethod1", "TestMethod2");
    }

    #endregion

    private static Task TestXunitAsync(string code, params string[] expectedTestNames)
    {
        var xunitDefinitions = """
            using System;
            namespace Xunit
            {
                [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
                public class FactAttribute : Attribute { }

                [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
                public class TheoryAttribute : FactAttribute { }
            }
            """;

        return TestAsync(code, xunitDefinitions, expectedTestNames);
    }

    private static Task TestNUnitAsync(string code, params string[] expectedTestNames)
    {
        var nunitDefinitions = """
            using System;
            namespace NUnit.Framework
            {
                [AttributeUsage(AttributeTargets.Method, AllowMultiple=false, Inherited=true)]
                public class TestAttribute : Attribute { }

                [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited=true)]
                public class TheoryAttribute : Attribute { }

                [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited=false)]
                public class TestCaseAttribute : Attribute { }

                [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
                public class TestCaseSourceAttribute : Attribute
                {
                    public TestCaseSourceAttribute(string sourceName) { }
                }
            }
            """;
        return TestAsync(code, nunitDefinitions, expectedTestNames);
    }

    private static Task TestMSTestAsync(string code, params string[] expectedTestNames)
    {
        var nunitDefinitions = """
            using System;
            namespace Microsoft.VisualStudio.TestTools.UnitTesting
            {
                [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
                public class TestMethodAttribute : Attribute { }
            }
            """;
        return TestAsync(code, nunitDefinitions, expectedTestNames);
    }

    private static async Task TestAsync(string code, string testAttributeDefinitionsCode, params string[] expectedTestNames)
    {
        var workspace = TestWorkspace.CreateCSharp(new[] { code, testAttributeDefinitionsCode });

        var testDocument = workspace.Documents.First();
        var span = testDocument.CursorPosition != null ? new TextSpan(testDocument.CursorPosition.Value, 0) : testDocument.SelectedSpans.Single();

        var testMethodFinder = workspace.CurrentSolution.Projects.Single().GetRequiredLanguageService<ITestMethodFinder>();
        var testMethods = await testMethodFinder.GetPotentialTestMethodsAsync(span, workspace.CurrentSolution.GetRequiredDocument(testDocument.Id), CancellationToken.None);
        var testMethodNames = testMethods.Cast<MethodDeclarationSyntax>().Select(m => m.Identifier.Text).ToArray();

        AssertEx.Equal(expectedTestNames, testMethodNames);
    }
}
