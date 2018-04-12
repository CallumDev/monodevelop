//
// BraceCompletionSessionProvider.cs
//
// Author:
//       Mike Krüger <mikkrg@microsoft.com>
//
// Copyright (c) 2018 Microsoft Corporation. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using System.Threading;
using Microsoft.CodeAnalysis.Editor.Shared.Utilities;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.BraceCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using Microsoft.CodeAnalysis.Editor.Implementation.AutomaticCompletion;
using MonoDevelop.Ide;

namespace MonoDevelop.CSharp.BraceCompletion
{
	[Export (typeof (IBraceCompletionSessionProvider))]
	[ContentType (Microsoft.CodeAnalysis.Editor.ContentTypeNames.RoslynContentType)]
	[BracePair (CurlyBrace.OpenCharacter, CurlyBrace.CloseCharacter)]
	[BracePair (Bracket.OpenCharacter, Bracket.CloseCharacter)]
	[BracePair (SingleQuote.OpenCharacter, SingleQuote.CloseCharacter)]
	[BracePair (DoubleQuote.OpenCharacter, DoubleQuote.CloseCharacter)]
	[BracePair (Parenthesis.OpenCharacter, Parenthesis.CloseCharacter)]
	[BracePair (LessAndGreaterThan.OpenCharacter, LessAndGreaterThan.CloseCharacter)]
	internal partial class BraceCompletionSessionProvider : ForegroundThreadAffinitizedObject, IBraceCompletionSessionProvider
	{
		private readonly ITextBufferUndoManagerProvider _undoManager;
		private readonly IEditorOperationsFactoryService _editorOperationsFactoryService;

		[ImportingConstructor]
		public BraceCompletionSessionProvider (
			ITextBufferUndoManagerProvider undoManager,
			IEditorOperationsFactoryService editorOperationsFactoryService)
		{
			_undoManager = undoManager;
			_editorOperationsFactoryService = editorOperationsFactoryService;
		}

		public bool TryCreateSession (ITextView textView, SnapshotPoint openingPoint, char openingBrace, char closingBrace, out IBraceCompletionSession session)
		{
			this.AssertIsForeground ();

			var textSnapshot = openingPoint.Snapshot;
			var document = IdeApp.Workbench.ActiveDocument?.AnalysisDocument;
			if (document != null) {
				var editorSessionFactory = document.GetLanguageService<IEditorBraceCompletionSessionFactory> ();
				if (editorSessionFactory != null) {
					// Brace completion is (currently) not cancellable.
					var cancellationToken = CancellationToken.None;

					var editorSession = editorSessionFactory.TryCreateSession (document, openingPoint, openingBrace, cancellationToken);
					if (editorSession != null) {
						var undoHistory = _undoManager.GetTextBufferUndoManager (textView.TextBuffer).TextBufferUndoHistory;
						session = new BraceCompletionSession (
							textView, openingPoint.Snapshot.TextBuffer, openingPoint, openingBrace, closingBrace,
							undoHistory, _editorOperationsFactoryService,
							editorSession);
						return true;
					}
				}
			}

			session = null;
			return false;
		}

		public static class CurlyBrace
		{
			public const char OpenCharacter = '{';
			public const char CloseCharacter = '}';
		}

		public static class Parenthesis
		{
			public const char OpenCharacter = '(';
			public const char CloseCharacter = ')';
		}

		public static class Bracket
		{
			public const char OpenCharacter = '[';
			public const char CloseCharacter = ']';
		}

		public static class LessAndGreaterThan
		{
			public const char OpenCharacter = '<';
			public const char CloseCharacter = '>';
		}

		public static class DoubleQuote
		{
			public const char OpenCharacter = '"';
			public const char CloseCharacter = '"';
		}

		public static class SingleQuote
		{
			public const char OpenCharacter = '\'';
			public const char CloseCharacter = '\'';
		}
	}
}
