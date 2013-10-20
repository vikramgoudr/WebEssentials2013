﻿using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.Web.Editor;

namespace MadsKristensen.EditorExtensions
{
    internal class ReferenceTagGoToDefinition : CommandTargetBase
    {
        public ReferenceTagGoToDefinition(IVsTextView adapter, IWpfTextView textView)
            : base(adapter, textView, typeof(VSConstants.VSStd97CmdID).GUID, (uint)VSConstants.VSStd97CmdID.GotoDefn)
        {
        }

        protected override bool Execute(uint commandId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            var path = FindReferencedPath();
            if (path == null)
                return false;

            var referencedPath = Path.Combine(Path.GetDirectoryName(TextView.TextBuffer.GetFileName()), path);

            if (referencedPath != null)
            {
                FileHelpers.OpenFileInPreviewTab(Path.GetFullPath(referencedPath));
                return true;
            }

            EditorExtensionsPackage.DTE.StatusBar.Text = "Couldn't find " + path;

            return false;
        }


        static readonly Regex regex = new Regex(@"///\s*<reference\s+path=(['""])(?<path>[^'""]+)\1(\s*/>)?");
        private string FindReferencedPath()
        {
            int position = TextView.Caret.Position.BufferPosition.Position;
            var line = TextView.TextBuffer.CurrentSnapshot.Lines.SingleOrDefault(l => l.Start <= position && l.End >= position);
            int linePos = position - line.Start.Position;

            var match = regex.Matches(line.GetText()).Cast<Match>().FirstOrDefault(m => m.Index <= linePos && m.Index + m.Length >= linePos);
            if (match == null) return null;

            return match.Groups["path"].Value;
        }

        protected override bool IsEnabled()
        {
            return FindReferencedPath() != null;
        }
    }
}