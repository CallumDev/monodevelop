﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text.BraceCompletion;
using Microsoft.VisualStudio.Text.Editor;
using MonoDevelop.Ide.CodeCompletion;
using MonoDevelop.Ide.Editor.Extension;

namespace MonoDevelop.SourceEditor.Braces
{
	class BraceCompletionEditorExtension : TextEditorExtension
	{
		private IBraceCompletionManager _manager;

		public override bool KeyPress (KeyDescriptor descriptor)
		{
			char typedChar = descriptor.KeyChar;
			Console.WriteLine ("key press !!!");
			if ((descriptor.SpecialKey == SpecialKey.None) &&
				(typedChar != '\0')) {
				// handle closing braces if there is an active session
				if ((Manager.HasActiveSessions && Manager.ClosingBraces.IndexOf (typedChar) > -1)
					|| Manager.OpeningBraces.IndexOf (typedChar) > -1) {
					bool handledCommand = false;
					Manager.PreTypeChar (typedChar, out handledCommand);
					Console.WriteLine ("pre type " + typedChar + " handled:" + handledCommand);

					if (handledCommand) {
						return false;
					}

					bool result = base.KeyPress (descriptor);

					Manager.PostTypeChar (typedChar);

					return result;
				}
			}

			if (Manager.HasActiveSessions) {
				switch (descriptor.SpecialKey) {
				case SpecialKey.Return:
					if (!IsCompletionActive) {
						bool handledCommand = false;

						Manager.PreReturn (out handledCommand);

						if (handledCommand) {
							return false;
						}

						bool result = base.KeyPress (descriptor);

						Manager.PostReturn ();

						return result;
					}
					break;
				case SpecialKey.Tab:
					if (!IsCompletionActive) {
						bool handledCommand = false;

						Manager.PreTab (out handledCommand);

						if (handledCommand) {
							return false;
						}

						bool result = base.KeyPress (descriptor);

						Manager.PostTab ();

						return result;
					}
					break;
				case SpecialKey.BackSpace: {
						bool handledCommand = false;

						Manager.PreBackspace (out handledCommand);

						if (handledCommand) {
							return false;
						}

						bool result = base.KeyPress (descriptor);

						Manager.PostBackspace ();

						return result;
					}
				case SpecialKey.Delete: {
						bool handledCommand = false;

						Manager.PreDelete (out handledCommand);

						if (handledCommand) {
							return false;
						}

						bool result = base.KeyPress (descriptor);

						Manager.PostDelete ();

						return result;
					}
				}
			}

			return base.KeyPress (descriptor);
		}

		private bool IsCompletionActive {
			get {
				return CompletionWindowManager.IsVisible;
				// return CompletionBroker.IsCompletionActive(View);
			}
		}

		private ITextView View {
			get {
				return Editor.TextView;
			}
		}

		private IBraceCompletionManager Manager {
			get {
				if (_manager == null
					&& !View.Properties.TryGetProperty ("BraceCompletionManager", out _manager)) {
					_manager = null;
				}

				return _manager;
			}
		}
	}
}