Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Reflection

Namespace Compatibility

    Public Class MethodReplacements
        Public Shared Property Methods As New ReplacementInfoCollection()

        Public Shared Function IsFound(name As String) As Boolean
            Return Methods.Any(Function(m) String.Equals(m.MethodName, name, StringComparison.OrdinalIgnoreCase))
        End Function

        Public Shared Function GetByName(name As String) As ReplacementInfo
            Return Methods.FirstOrDefault(Function(m) String.Equals(m.MethodName, name, StringComparison.OrdinalIgnoreCase))
        End Function
    End Class

    Public Class ReplacementInfoCollection
        Inherits Collection(Of ReplacementInfo)

        Private Const ResourceFileName As String = "VBCodeStandard.MethodReplacements.xml"

        Public Sub New()
            Using s As Stream = Me.GetType().GetTypeInfo().Assembly.GetManifestResourceStream(ResourceFileName)
                Dim doc = XDocument.Load(s)
                Dim methodsNode = doc.Root

                For Each method In methodsNode.Elements()
                    Dim ri As New ReplacementInfo()
                    ri.MethodName = method.Attribute("name").Value
                    ri.DetectNotExpression = Convert.ToBoolean(method.Attribute("detectNotExpression").Value)
                    ri.NewPattern = method.Element("newPattern").Value
                    If ri.DetectNotExpression Then ri.NewNotPattern = method.Element("newNotPattern").Value
                    Add(ri)
                Next
            End Using
        End Sub
    End Class

    Public Class ReplacementInfo
        Public Property MethodName As String
        Public Property DetectNotExpression As Boolean
        Public Property NewPattern As String
        Public Property NewNotPattern As String
    End Class



End Namespace