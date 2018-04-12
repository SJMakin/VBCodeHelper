Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions

Module Extensions

	Private ReadOnly generatedCodeAttributes As String() = {"DebuggerNonUserCode", "GeneratedCode", NameOf(DebuggerNonUserCodeAttribute), "GeneratedCodeAttribute"}

	<Extension()>
	Public Function FirstAncestorOfType(Of T As SyntaxNode)(node As SyntaxNode) As T
		Dim currentNode = node
		While True
			Dim parent = currentNode.Parent
			If (parent Is Nothing) Then Exit While
			Dim convertedPerent = TryCast(parent, T)
			If (convertedPerent IsNot Nothing) Then Return convertedPerent
			currentNode = parent
		End While

		Return Nothing
	End Function

	<Extension()>
	Public Function FirstAncestorOfType(node As SyntaxNode, ParamArray types As Type()) As SyntaxNode
		Dim currentNode = node
		While True
			Dim parent = currentNode.Parent
			If (parent Is Nothing) Then Exit While
			For Each type In types
				If (parent.GetType() Is type) Then Return parent
			Next

			currentNode = parent
		End While

		Return Nothing
	End Function

	<Extension()>
	Public Function FirstAncestorOrSelfOfType(Of T As SyntaxNode)(node As SyntaxNode) As T
		Return CType(node.FirstAncestorOrSelfOfType(GetType(T)), T)
	End Function

	<Extension()>
	Public Function FirstAncestorOrSelfOfType(node As SyntaxNode, ParamArray types As Type()) As SyntaxNode
		Dim currentNode = node
		While True
			If (currentNode Is Nothing) Then Exit While
			For Each type In types
				If (currentNode.GetType() Is type) Then Return currentNode
			Next

			currentNode = currentNode.Parent
		End While

		Return Nothing
	End Function

	<Extension>
	Public Function HasAttribute(attributeLists As SyntaxList(Of AttributeListSyntax), attributeName As String) As Boolean
		Return attributeLists.SelectMany(Function(a) a.Attributes).Any(Function(a) a.Name.ToString().EndsWith(attributeName, StringComparison.OrdinalIgnoreCase))
	End Function

	<Extension>
	Public Function HasAttributeOnAncestorOrSelf(node As SyntaxNode, attributeName As String) As Boolean
		Dim vbNode = TryCast(node, VisualBasicSyntaxNode)
		If (vbNode Is Nothing) Then Throw New Exception("Node is not a VB node.")
		Return vbNode.HasAttributeOnAncestorOrSelf(attributeName)
	End Function

	<Extension>
	Public Function HasAttributeOnAncestorOrSelf(node As SyntaxNode, ParamArray attributeNames As String()) As Boolean
		Dim vbNode = TryCast(node, VisualBasicSyntaxNode)
		If (vbNode Is Nothing) Then Throw New Exception("Node is not a VB node.")
		For Each attributeName In attributeNames
			If (vbNode.HasAttributeOnAncestorOrSelf(attributeName)) Then Return True
		Next
		Return False
	End Function

	<Extension>
	Public Function HasAttributeOnAncestorOrSelf(node As VisualBasicSyntaxNode, attributeName As String) As Boolean
		Dim parentMethod = DirectCast(node.FirstAncestorOrSelfOfType(GetType(MethodBlockSyntax), GetType(ConstructorBlockSyntax)), MethodBlockBaseSyntax)
		If If(parentMethod?.BlockStatement.AttributeLists.HasAttribute(attributeName), False) Then Return True
		Dim type = DirectCast(node.FirstAncestorOrSelfOfType(GetType(ClassBlockSyntax), GetType(StructureBlockSyntax)), TypeBlockSyntax)
		While (type IsNot Nothing)
			If type.BlockStatement.AttributeLists.HasAttribute(attributeName) Then Return True
			type = DirectCast(type.FirstAncestorOfType(GetType(ClassBlockSyntax), GetType(StructureBlockSyntax)), TypeBlockSyntax)
		End While
		Dim propertyBlock = node.FirstAncestorOrSelfOfType(Of PropertyBlockSyntax)()
		If If(propertyBlock?.PropertyStatement.AttributeLists.HasAttribute(attributeName), False) Then Return True
		Dim accessor = node.FirstAncestorOrSelfOfType(Of AccessorBlockSyntax)()
		If If(accessor?.AccessorStatement.AttributeLists.HasAttribute(attributeName), False) Then Return True
		Dim anInterface = node.FirstAncestorOrSelfOfType(Of InterfaceBlockSyntax)()
		If If(anInterface?.InterfaceStatement.AttributeLists.HasAttribute(attributeName), False) Then Return True
		Dim anEnum = node.FirstAncestorOrSelfOfType(Of EnumBlockSyntax)()
		If If(anEnum?.EnumStatement.AttributeLists.HasAttribute(attributeName), False) Then Return True
		Dim theModule = node.FirstAncestorOrSelfOfType(Of ModuleBlockSyntax)()
		If If(theModule?.ModuleStatement.AttributeLists.HasAttribute(attributeName), False) Then Return True
		Dim eventBlock = node.FirstAncestorOrSelfOfType(Of EventBlockSyntax)()
		If If(eventBlock?.EventStatement.AttributeLists.HasAttribute(attributeName), False) Then Return True
		Dim theEvent = TryCast(node, EventStatementSyntax)
		If If(theEvent?.AttributeLists.HasAttribute(attributeName), False) Then Return True
		Dim theProperty = TryCast(node, PropertyStatementSyntax)
		If If(theProperty?.AttributeLists.HasAttribute(attributeName), False) Then Return True
		Dim field = TryCast(node, FieldDeclarationSyntax)
		If If(field?.AttributeLists.HasAttribute(attributeName), False) Then Return True
		Dim parameter = TryCast(node, ParameterSyntax)
		If If(parameter?.AttributeLists.HasAttribute(attributeName), False) Then Return True
		Dim theDelegate = TryCast(node, DelegateStatementSyntax)
		If If(theDelegate?.AttributeLists.HasAttribute(attributeName), False) Then Return True
		Return False
	End Function

	<Extension>
	Public Function HasAutoGeneratedComment(tree As SyntaxTree) As Boolean
		Dim root = tree.GetRoot()
		If (root Is Nothing) Then Return False
		Dim firstToken = root.GetFirstToken()
		Dim trivia As SyntaxTriviaList
		If (firstToken = Nothing) Then
			Dim token = DirectCast(root, CompilationUnitSyntax).EndOfFileToken
			If (token.HasLeadingTrivia = False) Then Return False
			trivia = token.LeadingTrivia
		Else
			If (firstToken.HasLeadingTrivia = False) Then Return False
			trivia = firstToken.LeadingTrivia
		End If
		Dim commentLines = trivia.Where(Function(t As SyntaxTrivia) t.IsKind(SyntaxKind.CommentTrivia)).Take(2).ToList()
		If (commentLines.Count <> 2) Then Return False
		Return String.Equals(commentLines(1).ToString(), "' <auto-generated>", StringComparison.Ordinal)
	End Function

	<Extension>
	Public Function IsGenerated(context As SyntaxNodeAnalysisContext) As Boolean
		Return If(context.SemanticModel?.SyntaxTree?.IsGenerated(), False) OrElse If(context.Node?.IsGenerated(), False)
	End Function

	<Extension>
	Public Function IsGenerated(node As SyntaxNode) As Boolean
		Return node.HasAttributeOnAncestorOrSelf(generatedCodeAttributes)
	End Function

	<Extension>
	Public Function IsGenerated(context As SyntaxTreeAnalysisContext) As Boolean
		Return If(context.Tree?.IsGenerated(), False)
	End Function

	<Extension>
	Public Function IsGenerated(context As SymbolAnalysisContext) As Boolean
		If (context.Symbol Is Nothing) Then Return False
		For Each syntaxReference In context.Symbol.DeclaringSyntaxReferences
			If (syntaxReference.SyntaxTree.IsGenerated()) Then Return True
			Dim root = syntaxReference.SyntaxTree.GetRoot()
			Dim node = root?.FindNode(syntaxReference.Span)
			If (node.IsGenerated()) Then Return True
		Next
		Return False
	End Function

	<Extension>
	Public Function IsGenerated(tree As SyntaxTree) As Boolean
		Return If(tree.FilePath?.IsOnGeneratedFile(), False) OrElse tree.HasAutoGeneratedComment()
	End Function

	<Extension>
	Public Function IsOnGeneratedFile(filePath As String) As Boolean
		Return Regex.IsMatch(filePath, "(\\service|\\TemporaryGeneratedFile_.*|\\assemblyinfo|\\assemblyattributes|\.(g\.i|g|designer|generated|assemblyattributes))\.(cs|vb)$", RegexOptions.IgnoreCase)
	End Function
End Module
