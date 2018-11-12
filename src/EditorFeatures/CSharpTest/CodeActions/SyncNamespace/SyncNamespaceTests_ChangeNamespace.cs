﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Test.Utilities;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.CodeActions.SyncNamespace
{
    public partial class SyncNamespaceTests : CSharpSyncNamespaceTestsBase
    {
        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeNamespace_InvalidFolderName1()
        {
            var defaultNamespace = "A";
            var declaredNamespace = "Foo.Bar";

            // No change namespace action because the folder name is not valid identifier
            var documentPath = CreateDocumentFilePath(new[] { "3B", "C" }, "File1.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath.folder}"" FilePath=""{documentPath.filePath}""> 
namespace [||]{declaredNamespace}
{{    
    class Class1
    {{
    }}
}}  
        </Document>
    </Project>
</Workspace>";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal: null);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeNamespace_InvalidFolderName2()
        {
            var defaultNamespace = "A";
            var declaredNamespace = "Foo.Bar";

            // No change namespace action because the folder name is not valid identifier
            var documentPath = CreateDocumentFilePath(new[] { "B.3C", "D" }, "File1.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath.folder}"" FilePath=""{documentPath.filePath}""> 
namespace [||]{declaredNamespace}
{{    
    class Class1
    {{
    }}
}}  
        </Document>
    </Project>
</Workspace>";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal: null);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeNamespace_SingleDocumentNoReference()
        {
            var defaultNamespace = "A";
            var declaredNamespace = "Foo.Bar";

            var documentPath = CreateDocumentFilePath(new[] { "B", "C" }, "File1.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath.folder}"" FilePath=""{documentPath.filePath}""> 
namespace [||]{declaredNamespace}
{{
    class Class1
    {{
    }}
}}</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"namespace A.B.C
{
    class Class1
    {
    }
}";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeNamespace_SingleDocumentLocalReference()
        {
            var defaultNamespace = "A";
            var declaredNamespace = "Foo.Bar";

            var documentPath = CreateDocumentFilePath(new[] { "B", "C" }, "File1.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath.folder}"" FilePath=""{documentPath.filePath}""> 
namespace [||]{declaredNamespace}
{{
    delegate void D1;

    interface Class1
    {{
        void M1();
    }}

    class Class2 : {declaredNamespace}.Class1
    {{
        {declaredNamespace}.D1 d;  

        void {declaredNamespace}.Class1.M1(){{}}
    }}
}}</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"namespace A.B.C
{
    delegate void D1;

    interface Class1
    {
        void M1();
    }

    class Class2 : Class1
    {
        D1 d;

        void Class1.M1() { }
    }
}";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeNamespace_WithCrefReference()
        {
            var defaultNamespace = "A";
            var declaredNamespace = "Foo.Bar.Baz";

            var documentPath1 = CreateDocumentFilePath(new[] { "B", "C" }, "File1.cs");
            var documentPath2 = CreateDocumentFilePath(Array.Empty<string>(), "File2.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath1.folder}"" FilePath=""{documentPath1.filePath}""> 
namespace [||]{declaredNamespace}
{{
    /// &lt;summary&gt;
    /// See &lt;see cref=""Class1""/&gt;
    /// See &lt;see cref=""{declaredNamespace}.Class1""/&gt;
    /// &lt;/summary&gt;
    public class Class1
    {{
    }}
}}</Document>
<Document Folders=""{documentPath2.folder}"" FilePath=""{documentPath2.filePath}""> 
namespace Foo
{{
    using {declaredNamespace};

    /// &lt;summary&gt;
    /// See &lt;see cref=""Class1""/&gt;
    /// See &lt;see cref=""{declaredNamespace}.Class1""/&gt;
    /// &lt;/summary&gt;
    class RefClass
    {{
    }}
}}</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"namespace A.B.C
{
    /// <summary>
    /// See <see cref=""Class1""/>
    /// See <see cref=""Class1""/>
    /// </summary>
    public class Class1
    {
    }
}";
            var expectedSourceReference =
@"
namespace Foo
{
    using A.B.C;

    /// <summary>
    /// See <see cref=""Class1""/>
    /// See <see cref=""A.B.C.Class1""/>
    /// </summary>
    class RefClass
    {
    }
}";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal, expectedSourceReference);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeNamespace_WithCrefReferencesInVB()
        {
            var defaultNamespace = "A.B.C";
            var declaredNamespace = "A.B.C.D";

            var documentPath1 = CreateDocumentFilePath(Array.Empty<string>(), "File1.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath1.folder}"" FilePath=""{documentPath1.filePath}""> 
namespace [||]{declaredNamespace}
{{
    /// &lt;summary&gt;
    /// See &lt;see cref=""Class1""/&gt;
    /// See &lt;see cref=""{declaredNamespace}.Class1""/&gt;
    /// &lt;/summary&gt;
    public class Class1
    {{
    }}
}}</Document>
    </Project>    
<Project Language=""Visual Basic"" AssemblyName=""Assembly2"" CommonReferences=""true"">
        <Document> 
Imports {declaredNamespace}

''' &lt;summary&gt;
''' See &lt;see cref=""Class1""/&gt;
''' See &lt;see cref=""{declaredNamespace}.Class1""/&gt;
''' &lt;/summary&gt;
Public Class VBClass
    Public ReadOnly Property C1 As Class1
End Class</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"namespace A.B.C
{
    /// <summary>
    /// See <see cref=""Class1""/>
    /// See <see cref=""Class1""/>
    /// </summary>
    public class Class1
    {
    }
}";
            var expectedSourceReference =
@"
Imports A.B.C

''' <summary>
''' See <see cref=""Class1""/>
''' See <see cref=""Class1""/>
''' </summary>
Public Class VBClass
    Public ReadOnly Property C1 As Class1
End Class";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal, expectedSourceReference);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeNamespace_ReferencingTypesDeclaredInOtherDocument()
        {
            var defaultNamespace = "A";
            var declaredNamespace = "Foo.Bar.Baz";

            var documentPath1 = CreateDocumentFilePath(new[] { "B", "C" }, "File1.cs");
            var documentPath2 = CreateDocumentFilePath(Array.Empty<string>(), "File2.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath1.folder}"" FilePath=""{documentPath1.filePath}""> 
namespace [||]{declaredNamespace}
{{
    class Class1 
    {{ 
        private Class2 c2;
        private Class3 c3;
        private Class4 c4;
    }}
}}</Document>
<Document Folders=""{documentPath2.folder}"" FilePath=""{documentPath2.filePath}""> 
namespace Foo
{{
    class Class2 {{}}

    namespace Bar
    {{
        class Class3 {{}}
        
        namespace Baz
        {{
            class Class4 {{}}    
        }}
    }}
}}</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"
using Foo;
using Foo.Bar;
using Foo.Bar.Baz;

namespace A.B.C
{
    class Class1
    {
        private Class2 c2;
        private Class3 c3;
        private Class4 c4;
    }
}";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeNamespace_ReferencingQualifiedTypesDeclaredInOtherDocument()
        {
            var defaultNamespace = "A";
            var declaredNamespace = "Foo.Bar.Baz";

            var documentPath1 = CreateDocumentFilePath(new[] { "B", "C" }, "File1.cs");
            var documentPath2 = CreateDocumentFilePath(Array.Empty<string>(), "File2.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath1.folder}"" FilePath=""{documentPath1.filePath}""> 
namespace [||]{declaredNamespace}
{{
    class Class1 
    {{ 
        private Foo.Class2 c2;
        private Foo.Bar.Class3 c3;
        private Foo.Bar.Baz.Class4 c4;
    }}
}}</Document>
<Document Folders=""{documentPath2.folder}"" FilePath=""{documentPath2.filePath}""> 
namespace Foo
{{
    class Class2 {{}}

    namespace Bar
    {{
        class Class3 {{}}
        
        namespace Baz
        {{
            class Class4 {{}}    
        }}
    }}
}}</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"
using Foo;
using Foo.Bar;
using Foo.Bar.Baz;

namespace A.B.C
{
    class Class1
    {
        private Class2 c2;
        private Class3 c3;
        private Class4 c4;
    }
}";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeNamespace_WithReferencesInOtherDocument()
        {
            var defaultNamespace = "A";
            var declaredNamespace = "Foo.Bar.Baz";

            var documentPath1 = CreateDocumentFilePath(new[] { "B", "C" }, "File1.cs");
            var documentPath2 = CreateDocumentFilePath(Array.Empty<string>(), "File2.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath1.folder}"" FilePath=""{documentPath1.filePath}""> 
namespace [||]{declaredNamespace}
{{
    class Class1 
    {{ 
    }}
    
    class Class2 
    {{ 
    }}
}}</Document>
<Document Folders=""{documentPath2.folder}"" FilePath=""{documentPath2.filePath}""> 
using Foo.Bar.Baz;

namespace Foo
{{
    class RefClass
    {{
        private Class1 c1;

        void M1()
        {{
            Bar.Baz.Class2 c2 = null;
        }}
    }}
}}</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"namespace A.B.C
{
    class Class1
    {
    }

    class Class2
    {
    }
}";
            var expectedSourceReference =
@"
using A.B.C;

namespace Foo
{
    class RefClass
    {
        private Class1 c1;

        void M1()
        {
            Class2 c2 = null;
        }
    }
}";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal, expectedSourceReference);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeNamespace_WithQualifiedReferencesInOtherDocument()
        {
            var defaultNamespace = "A";
            var declaredNamespace = "Foo.Bar.Baz";

            var documentPath1 = CreateDocumentFilePath(new[] { "B", "C" }, "File1.cs");
            var documentPath2 = CreateDocumentFilePath(Array.Empty<string>(), "File2.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath1.folder}"" FilePath=""{documentPath1.filePath}""> 
namespace [||]{declaredNamespace}
{{
    interface Interface1 
    {{
        void M1(Interface1 c1);   
    }}
}}</Document>
<Document Folders=""{documentPath2.folder}"" FilePath=""{documentPath2.filePath}""> 
namespace Foo
{{
    using {declaredNamespace};

    class RefClass : Interface1
    {{
        void {declaredNamespace}.Interface1.M1(Interface1 c1){{}}
    }}
}}</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"namespace A.B.C
{
    interface Interface1
    {
        void M1(Interface1 c1);
    }
}";
            var expectedSourceReference =
@"
namespace Foo
{
    using A.B.C;

    class RefClass : Interface1
    {
        void Interface1.M1(Interface1 c1){}
    }
}";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal, expectedSourceReference);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeNamespace_ChangeUsingsInMultipleContainers()
        {
            var defaultNamespace = "A";
            var declaredNamespace = "Foo.Bar.Baz";

            var documentPath1 = CreateDocumentFilePath(new[] { "B", "C" }, "File1.cs");
            var documentPath2 = CreateDocumentFilePath(Array.Empty<string>(), "File2.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath1.folder}"" FilePath=""{documentPath1.filePath}""> 
namespace [||]{declaredNamespace}
{{
    class Class1
    {{
    }}
}}</Document>
<Document Folders=""{documentPath2.folder}"" FilePath=""{documentPath2.filePath}""> 
namespace NS1
{{
    using Foo.Bar.Baz;

    class Class2
    {{
        Class1 c2;
    }}

    namespace NS2
    {{
        using Foo.Bar.Baz;

        class Class2
        {{
            Class1 c1;
        }}
    }}
}}</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"namespace A.B.C
{
    class Class1
    {
    }
}";
            var expectedSourceReference =
@"
namespace NS1
{
    using A.B.C;

    class Class2
    {
        Class1 c2;
    }

    namespace NS2
    {
        class Class2
        {
            Class1 c1;
        }
    }
}";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal, expectedSourceReference);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeNamespace_WithAliasReferencesInOtherDocument()
        {
            var defaultNamespace = "A";
            var declaredNamespace = "Foo.Bar.Baz";

            var documentPath1 = CreateDocumentFilePath(new[] { "B", "C" }, "File1.cs");
            var documentPath2 = CreateDocumentFilePath(Array.Empty<string>(), "File2.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath1.folder}"" FilePath=""{documentPath1.filePath}""> 
namespace [||]{declaredNamespace}
{{
    class Class1 
    {{ 
    }}
    
    class Class2 
    {{ 
    }}
}}</Document>
<Document Folders=""{documentPath2.folder}"" FilePath=""{documentPath2.filePath}"">
using System;
using Class1Alias = Foo.Bar.Baz.Class1;

namespace Foo
{{
    class RefClass
    {{
        private Class1Alias c1;

        void M1()
        {{
            Bar.Baz.Class2 c2 = null;
        }}
    }}
}}</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"namespace A.B.C
{
    class Class1
    {
    }

    class Class2
    {
    }
}";
            var expectedSourceReference =
@"
using System;
using A.B.C;
using Class1Alias = A.B.C.Class1;

namespace Foo
{
    class RefClass
    {
        private Class1Alias c1;

        void M1()
        {
            Class2 c2 = null;
        }
    }
}";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal, expectedSourceReference);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeToGlobalNamespace_SingleDocumentNoRef()
        {
            var defaultNamespace = "";
            var declaredNamespace = "Foo.Bar";

            var documentPath = CreateDocumentFilePath(Array.Empty<string>(), "File1.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath.folder}"" FilePath=""{documentPath.filePath}""> 
using System;

// Comments before declaration.
namespace [||]{declaredNamespace}
{{  // Comments after opening brace
    class Class1
    {{
    }}
    // Comments before closing brace
}} // Comments after declaration.
</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"
using System;

// Comments before declaration.
// Comments after opening brace
class Class1
{
}
// Comments before closing brace
// Comments after declaration.
";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeToGlobalNamespace_SingleDocumentLocalRef()
        {
            var defaultNamespace = "";
            var declaredNamespace = "Foo.Bar";

            var documentPath = CreateDocumentFilePath(Array.Empty<string>(), "File1.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath.folder}"" FilePath=""{documentPath.filePath}""> 
namespace [||]{declaredNamespace}
{{
    delegate void D1;

    interface Class1
    {{
        void M1();
    }}

    class Class2 : {declaredNamespace}.Class1
    {{
        global::{declaredNamespace}.D1 d;  

        void {declaredNamespace}.Class1.M1() {{ }}
    }}
}}</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"delegate void D1;

interface Class1
{
    void M1();
}

class Class2 : Class1
{
    global::D1 d;

    void Class1.M1() { }
}
";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeToGlobalNamespace_WithReferencesInOtherDocument()
        {
            var defaultNamespace = "";
            var declaredNamespace = "Foo.Bar.Baz";

            var documentPath1 = CreateDocumentFilePath(Array.Empty<string>(), "File1.cs");
            var documentPath2 = CreateDocumentFilePath(Array.Empty<string>(), "File2.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath1.folder}"" FilePath=""{documentPath1.filePath}""> 
namespace [||]{declaredNamespace}
{{
    class Class1 
    {{ 
    }}
    
    class Class2 
    {{ 
    }}
}}</Document>
<Document Folders=""{documentPath2.folder}"" FilePath=""{documentPath2.filePath}""> 
using Foo.Bar.Baz;

namespace Foo
{{
    class RefClass
    {{
        private Class1 c1;

        void M1()
        {{
            Bar.Baz.Class2 c2 = null;
        }}
    }}
}}</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"class Class1
{
}

class Class2
{
}
";
            var expectedSourceReference =
@"namespace Foo
{
    class RefClass
    {
        private Class1 c1;

        void M1()
        {
            Class2 c2 = null;
        }
    }
}";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal, expectedSourceReference);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeToGlobalNamespace_WithQualifiedReferencesInOtherDocument()
        {
            var defaultNamespace = "";
            var declaredNamespace = "Foo.Bar.Baz";

            var documentPath1 = CreateDocumentFilePath(Array.Empty<string>(), "File1.cs");
            var documentPath2 = CreateDocumentFilePath(Array.Empty<string>(), "File2.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath1.folder}"" FilePath=""{documentPath1.filePath}""> 
namespace [||]{declaredNamespace}
{{
    interface Interface1 
    {{
        void M1(Interface1 c1);   
    }}
}}</Document>
<Document Folders=""{documentPath2.folder}"" FilePath=""{documentPath2.filePath}""> 
namespace Foo
{{
    using {declaredNamespace};

    class RefClass : Interface1
    {{
        void {declaredNamespace}.Interface1.M1(Interface1 c1){{}}
    }}
}}</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"interface Interface1
{
    void M1(Interface1 c1);
}
";
            var expectedSourceReference =
@"
namespace Foo
{
    class RefClass : Interface1
    {
        void Interface1.M1(Interface1 c1){}
    }
}";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal, expectedSourceReference);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeToGlobalNamespace_WithReferenceAndConflictDeclarationInOtherDocument()
        {
            var defaultNamespace = "";
            var declaredNamespace = "Foo.Bar.Baz";

            var documentPath1 = CreateDocumentFilePath(Array.Empty<string>(), "File1.cs");
            var documentPath2 = CreateDocumentFilePath(Array.Empty<string>(), "File2.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath1.folder}"" FilePath=""{documentPath1.filePath}""> 
namespace [||]{declaredNamespace}
{{
    class MyClass 
    {{
    }}
}}</Document>
<Document Folders=""{documentPath2.folder}"" FilePath=""{documentPath2.filePath}""> 
namespace Foo
{{
    using {declaredNamespace};

    class RefClass
    {{
        Foo.Bar.Baz.MyClass c;
    }}

    class MyClass
    {{
    }}
}}</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"class MyClass
{
}
";
            var expectedSourceReference =
@"
namespace Foo
{
    class RefClass
    {
        global::MyClass c;
    }

    class MyClass
    {
    }
}";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal, expectedSourceReference);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeToGlobalNamespace_ReferencingTypesDeclaredInOtherDocument()
        {
            var defaultNamespace = "";
            var declaredNamespace = "Foo.Bar.Baz";

            var documentPath1 = CreateDocumentFilePath(Array.Empty<string>(), "File1.cs");
            var documentPath2 = CreateDocumentFilePath(Array.Empty<string>(), "File2.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath1.folder}"" FilePath=""{documentPath1.filePath}""> 
namespace [||]{declaredNamespace}
{{
    class Class1 
    {{ 
        private Class2 c2;
        private Class3 c3;
        private Class4 c4;
    }}
}}</Document>
<Document Folders=""{documentPath2.folder}"" FilePath=""{documentPath2.filePath}""> 
namespace Foo
{{
    class Class2 {{}}

    namespace Bar
    {{
        class Class3 {{}}
        
        namespace Baz
        {{
            class Class4 {{}}    
        }}
    }}
}}</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"
using Foo;
using Foo.Bar;
using Foo.Bar.Baz;

class Class1
{
    private Class2 c2;
    private Class3 c3;
    private Class4 c4;
}
";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeToGlobalNamespace_ChangeUsingsInMultipleContainers()
        {
            var defaultNamespace = "";
            var declaredNamespace = "Foo.Bar.Baz";

            var documentPath1 = CreateDocumentFilePath(Array.Empty<string>(), "File1.cs");
            var documentPath2 = CreateDocumentFilePath(Array.Empty<string>(), "File2.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath1.folder}"" FilePath=""{documentPath1.filePath}""> 
namespace [||]{declaredNamespace}
{{
    class Class1
    {{
    }}
}}</Document>
<Document Folders=""{documentPath2.folder}"" FilePath=""{documentPath2.filePath}""> 
namespace NS1
{{
    using Foo.Bar.Baz;

    class Class2
    {{
        Class1 c2;
    }}

    namespace NS2
    {{
        using Foo.Bar.Baz;

        class Class2
        {{
            Class1 c1;
        }}
    }}
}}</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"class Class1
{
}
";
            var expectedSourceReference =
@"
namespace NS1
{
    class Class2
    {
        Class1 c2;
    }

    namespace NS2
    {
        class Class2
        {
            Class1 c1;
        }
    }
}";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal, expectedSourceReference);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeToGlobalNamespace_WithAliasReferencesInOtherDocument()
        {
            var defaultNamespace = "";
            var declaredNamespace = "Foo.Bar.Baz";

            var documentPath1 = CreateDocumentFilePath(Array.Empty<string>(), "File1.cs");
            var documentPath2 = CreateDocumentFilePath(Array.Empty<string>(), "File2.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath1.folder}"" FilePath=""{documentPath1.filePath}""> 
namespace [||]{declaredNamespace}
{{
    class Class1 
    {{ 
    }}
    
    class Class2 
    {{ 
    }}
}}</Document>
<Document Folders=""{documentPath2.folder}"" FilePath=""{documentPath2.filePath}"">
using System;
using Class1Alias = Foo.Bar.Baz.Class1;

namespace Foo
{{
    class RefClass
    {{
        private Class1Alias c1;

        void M1()
        {{
            Bar.Baz.Class2 c2 = null;
        }}
    }}
}}</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"class Class1
{
}

class Class2
{
}
";
            var expectedSourceReference =
@"using System;
using Class1Alias = Class1;

namespace Foo
{
    class RefClass
    {
        private Class1Alias c1;

        void M1()
        {
            Class2 c2 = null;
        }
    }
}";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal, expectedSourceReference);
        }













        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeFromGlobalNamespace_SingleDocumentNoRef()
        {
            var defaultNamespace = "A";
            var documentPath = CreateDocumentFilePath(new[] { "B", "C" }, "File1.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath.folder}"" FilePath=""{documentPath.filePath}""> 
using System;

class [||]Class1
{{
}}
</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"using System;

namespace A.B.C
{
    class Class1
    {
    }
}";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeFromGlobalNamespace_SingleDocumentLocalRef()
        {
            var defaultNamespace = "A";
            var documentPath = CreateDocumentFilePath(new[] { "B", "C" }, "File1.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath.folder}"" FilePath=""{documentPath.filePath}""> 
delegate void [||]D1;

interface Class1
{{
    void M1();
}}

class Class2 : Class1
{{
    D1 d;  

    void Class1.M1() {{ }}
}}</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"namespace A.B.C
{
    delegate void D1;

    interface Class1
    {
        void M1();
    }

    class Class2 : Class1
    {
        D1 d;

        void Class1.M1() { }
    }
}";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeFromGlobalNamespace_WithReferencesInOtherDocument()
        {
            var defaultNamespace = "A";

            var documentPath1 = CreateDocumentFilePath(new[] { "B", "C" }, "File1.cs");
            var documentPath2 = CreateDocumentFilePath(Array.Empty<string>(), "File2.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath1.folder}"" FilePath=""{documentPath1.filePath}""> 
class [||]Class1 
{{ 
}}
    
class Class2 
{{ 
}}</Document>
<Document Folders=""{documentPath2.folder}"" FilePath=""{documentPath2.filePath}""> 
namespace Foo
{{
    class RefClass
    {{
        private Class1 c1;

        void M1()
        {{
            Class2 c2 = null;
        }}
    }}
}}</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"namespace A.B.C
{
    class Class1
    {
    }

    class Class2
    {
    }
}";
            var expectedSourceReference =
@"
using A.B.C;

namespace Foo
{
    class RefClass
    {
        private Class1 c1;

        void M1()
        {
            Class2 c2 = null;
        }
    }
}";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal, expectedSourceReference);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeFromGlobalNamespace_WithQualifiedReferencesInOtherDocument()
        {
            var defaultNamespace = "A";
            var documentPath1 = CreateDocumentFilePath(new[] { "B", "C" }, "File1.cs");
            var documentPath2 = CreateDocumentFilePath(Array.Empty<string>(), "File2.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath1.folder}"" FilePath=""{documentPath1.filePath}""> 
interface [||]Interface1 
{{
    void M1(Interface1 c1);   
}}</Document>
<Document Folders=""{documentPath2.folder}"" FilePath=""{documentPath2.filePath}""> 
namespace Foo
{{
    class RefClass : Interface1
    {{
        void Interface1.M1(Interface1 c1){{}}
    }}
}}</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"namespace A.B.C
{
    interface Interface1
    {
        void M1(Interface1 c1);
    }
}";
            var expectedSourceReference =
@"
using A.B.C;

namespace Foo
{
    class RefClass : Interface1
    {
        void Interface1.M1(Interface1 c1){}
    }
}";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal, expectedSourceReference);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeFromGlobalNamespace_ReferencingQualifiedTypesDeclaredInOtherDocument()
        {
            var defaultNamespace = "A";
            var documentPath1 = CreateDocumentFilePath(new[] { "B", "C" }, "File1.cs");
            var documentPath2 = CreateDocumentFilePath(Array.Empty<string>(), "File2.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath1.folder}"" FilePath=""{documentPath1.filePath}""> 
class [||]Class1 
{{ 
    private A.Class2 c2;
    private A.B.Class3 c3;
    private A.B.C.Class4 c4;
}}</Document>

<Document Folders=""{documentPath2.folder}"" FilePath=""{documentPath2.filePath}""> 
namespace A
{{
    class Class2 {{}}

    namespace B
    {{
        class Class3 {{}}
        
        namespace C
        {{
            class Class4 {{}}    
        }}
    }}
}}</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"namespace A.B.C
{
    class Class1
    {
        private Class2 c2;
        private Class3 c3;
        private Class4 c4;
    }
}";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeFromGlobalNamespace_ChangeUsingsInMultipleContainers()
        {
            var defaultNamespace = "A";

            var documentPath1 = CreateDocumentFilePath(new[] { "B", "C" }, "File1.cs");
            var documentPath2 = CreateDocumentFilePath(Array.Empty<string>(), "File2.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath1.folder}"" FilePath=""{documentPath1.filePath}""> 
class [||]Class1
{{
}}</Document>
<Document Folders=""{documentPath2.folder}"" FilePath=""{documentPath2.filePath}""> 
namespace NS1
{{
    using System;

    class Class2
    {{
        Class1 c2;
    }}

    namespace NS2
    {{
        using System;

        class Class2
        {{
            Class1 c1;
        }}
    }}
}}</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"namespace A.B.C
{
    class Class1
    {
    }
}";
            var expectedSourceReference =
@"
namespace NS1
{
    using System;
    using A.B.C;

    class Class2
    {
        Class1 c2;
    }

    namespace NS2
    {
        using System;

        class Class2
        {
            Class1 c1;
        }
    }
}";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal, expectedSourceReference);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeFromGlobalNamespace_WithAliasReferencesInOtherDocument()
        {
            var defaultNamespace = "A";

            var documentPath1 = CreateDocumentFilePath(new[] { "B", "C" }, "File1.cs");
            var documentPath2 = CreateDocumentFilePath(Array.Empty<string>(), "File2.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath1.folder}"" FilePath=""{documentPath1.filePath}""> 
class [||]Class1 
{{ 
}}
    
class Class2 
{{ 
}}</Document>
<Document Folders=""{documentPath2.folder}"" FilePath=""{documentPath2.filePath}"">
using Class1Alias = Class1;

namespace Foo
{{
    using System;

    class RefClass
    {{
        private Class1Alias c1;

        void M1()
        {{
            Class2 c2 = null;
        }}
    }}
}}</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"namespace A.B.C
{
    class Class1
    {
    }

    class Class2
    {
    }
}";
            var expectedSourceReference =
@"
using A.B.C;
using Class1Alias = Class1;

namespace Foo
{
    using System;

    class RefClass
    {
        private Class1Alias c1;

        void M1()
        {
            Class2 c2 = null;
        }
    }
}";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal, expectedSourceReference);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeNamespace_WithReferencesInVBDocument()
        {
            var defaultNamespace = "A.B.C";
            var declaredNamespace = "A.B.C.D";

            var documentPath1 = CreateDocumentFilePath(Array.Empty<string>(), "File1.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath1.folder}"" FilePath=""{documentPath1.filePath}""> 
namespace [||]{declaredNamespace}
{{
    public class Class1
    {{ 
    }}
}}</Document>
    </Project>    
<Project Language=""Visual Basic"" AssemblyName=""Assembly2"" CommonReferences=""true"">
        <Document> 
Imports {declaredNamespace}

Public Class VBClass
    Public ReadOnly Property C1 As Class1
End Class</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"namespace A.B.C
{
    public class Class1
    {
    }
}";
            var expectedSourceReference =
@"
Imports A.B.C

Public Class VBClass
    Public ReadOnly Property C1 As Class1
End Class";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal, expectedSourceReference);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeNamespace_WithQualifiedReferencesInVBDocument()
        {
            var defaultNamespace = "A.B.C";
            var declaredNamespace = "A.B.C.D";

            var documentPath1 = CreateDocumentFilePath(Array.Empty<string>(), "File1.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath1.folder}"" FilePath=""{documentPath1.filePath}""> 
namespace [||]{declaredNamespace}
{{
    public class Class1
    {{ 
    }}
}}</Document>
    </Project>    
<Project Language=""Visual Basic"" AssemblyName=""Assembly2"" CommonReferences=""true"">
        <Document>
Public Class VBClass
    Public ReadOnly Property C1 As A.B.C.D.Class1
End Class</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"namespace A.B.C
{
    public class Class1
    {
    }
}";
            var expectedSourceReference =
@"Public Class VBClass
    Public ReadOnly Property C1 As A.B.C.Class1
End Class";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal, expectedSourceReference);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeFromGlobalNamespace_WithReferencesInVBDocument()
        {
            var defaultNamespace = "A";

            var documentPath1 = CreateDocumentFilePath(new[] { "B", "C" }, "File1.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath1.folder}"" FilePath=""{documentPath1.filePath}""> 
public class [||]Class1
{{ 
}}
</Document>
    </Project>    
<Project Language=""Visual Basic"" AssemblyName=""Assembly2"" CommonReferences=""true"">
        <Document> 
Public Class VBClass
    Public ReadOnly Property C1 As Class1
End Class</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"namespace A.B.C
{
    public class Class1
    {
    }
}";
            var expectedSourceReference =
@"
Imports A.B.C

Public Class VBClass
    Public ReadOnly Property C1 As Class1
End Class";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal, expectedSourceReference);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeFromGlobalNamespace_WithCredReferences()
        {
            var defaultNamespace = "A";
            var documentPath1 = CreateDocumentFilePath(new[] { "B", "C" }, "File1.cs");
            var documentPath2 = CreateDocumentFilePath(Array.Empty<string>(), "File2.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath1.folder}"" FilePath=""{documentPath1.filePath}""> 
/// &lt;summary&gt;
/// See &lt;see cref=""Class1""/&gt;
/// &lt;/summary&gt;
class [||]Class1 
{{
}}</Document>
<Document Folders=""{documentPath2.folder}"" FilePath=""{documentPath2.filePath}""> 
namespace Foo
{{
    /// &lt;summary&gt;
    /// See &lt;see cref=""Class1""/&gt;
    /// &lt;/summary&gt;
    class Bar
    {{
    }}
}}</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"namespace A.B.C
{
    /// <summary>
    /// See <see cref=""Class1""/>
    /// </summary>
    class Class1
    {
    }
}";
            var expectedSourceReference =
@"
using A.B.C;

namespace Foo
{
    /// <summary>
    /// See <see cref=""Class1""/>
    /// </summary>
    class Bar
    {
    }
}";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal, expectedSourceReference);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeToGlobalNamespace_WithReferencesInVBDocument()
        {
            var defaultNamespace = "";
            var declaredNamespace = "Foo.Bar.Baz";

            var documentPath1 = CreateDocumentFilePath(Array.Empty<string>(), "File1.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath1.folder}"" FilePath=""{documentPath1.filePath}""> 
namespace [||]{declaredNamespace}
{{
    public class Class1
    {{ 
    }}
}}</Document>
    </Project>    
<Project Language=""Visual Basic"" AssemblyName=""Assembly2"" CommonReferences=""true"">
        <Document> 
Imports {declaredNamespace}

Public Class VBClass
    Public ReadOnly Property C1 As Class1
End Class</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"public class Class1
{
}
";
            var expectedSourceReference =
@"Public Class VBClass
    Public ReadOnly Property C1 As Class1
End Class";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal, expectedSourceReference);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeToGlobalNamespace_WithReferenceAndConflictDeclarationInVBDocument()
        {
            var defaultNamespace = "";
            var declaredNamespace = "Foo.Bar.Baz";

            var documentPath1 = CreateDocumentFilePath(Array.Empty<string>(), "File1.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath1.folder}"" FilePath=""{documentPath1.filePath}""> 
namespace [||]{declaredNamespace}
{{
    public class MyClass
    {{ 
    }}
}}</Document>
    </Project>    
<Project Language=""Visual Basic"" AssemblyName=""Assembly2"" CommonReferences=""true"">
        <Document>
Namespace Foo
    Public Class VBClass
        Public ReadOnly Property C1 As Foo.Bar.Baz.MyClass
    End Class

    Public Class MyClass
    End Class
End Namespace</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"public class MyClass
{
}
";
            var expectedSourceReference =
@"Namespace Foo
    Public Class VBClass
        Public ReadOnly Property C1 As Global.MyClass
    End Class

    Public Class MyClass
    End Class
End Namespace";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal, expectedSourceReference);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsSyncNamespace)]
        public async Task ChangeToGlobalNamespace_WithCredReferences()
        {
            var defaultNamespace = "";
            var declaredNamespace = "Foo.Bar.Baz";

            var documentPath1 = CreateDocumentFilePath(Array.Empty<string>(), "File1.cs");
            var documentPath2 = CreateDocumentFilePath(Array.Empty<string>(), "File2.cs");
            var code =
$@"
<Workspace>
    <Project Language=""C#"" AssemblyName=""Assembly1"" FilePath=""{ProjectFilePath}"" DefaultNamespace=""{defaultNamespace}"" CommonReferences=""true"">
        <Document Folders=""{documentPath1.folder}"" FilePath=""{documentPath1.filePath}"">
namespace [||]{declaredNamespace}
{{
    /// &lt;summary&gt;
    /// See &lt;see cref=""Class1""/&gt;
    /// See &lt;see cref=""{declaredNamespace}.Class1""/&gt;
    /// &lt;/summary&gt;
    public class Class1
    {{
    }}
}}</Document>
<Document Folders=""{documentPath2.folder}"" FilePath=""{documentPath2.filePath}""> 
namespace Foo
{{
    using {declaredNamespace};

    /// &lt;summary&gt;
    /// See &lt;see cref=""Class1""/&gt;
    /// See &lt;see cref=""{declaredNamespace}.Class1""/&gt;
    /// &lt;/summary&gt;
    class RefClass
    {{
    }}
}}</Document>
    </Project>
</Workspace>";

            var expectedSourceOriginal =
@"/// <summary>
/// See <see cref=""Class1""/>
/// See <see cref=""Class1""/>
/// </summary>
public class Class1
{
}
";
            var expectedSourceReference =
@"
namespace Foo
{
    /// <summary>
    /// See <see cref=""Class1""/>
    /// See <see cref=""Class1""/>
    /// </summary>
    class RefClass
    {
    }
}";
            await TestChangeNamespaceAsync(code, expectedSourceOriginal, expectedSourceReference);
        }
    }
}
