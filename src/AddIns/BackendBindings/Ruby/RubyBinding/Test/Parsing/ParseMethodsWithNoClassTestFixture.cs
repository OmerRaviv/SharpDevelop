﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Matthew Ward" email="mrward@users.sourceforge.net"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Collections.Generic;
using ICSharpCode.RubyBinding;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor.Document;
using NUnit.Framework;
using RubyBinding.Tests;

namespace RubyBinding.Tests.Parsing
{
	/// <summary>
	/// Support folding when no classes are defined.
	/// </summary>
	[TestFixture]
	public class ParseMethodsWithNoClassTestFixture
	{
		ICompilationUnit compilationUnit;
		FoldMarker fooMethodMarker;
		FoldMarker barMethodMarker;
		IClass globalClass;
		IMethod fooMethod;
		IMethod barMethod;
		
		[TestFixtureSetUp]
		public void SetUpFixture()
		{
			string Ruby = "def foo\r\n" +
							"end\r\n" +
							"\r\n" +
							"def bar(i)\r\n" +
							"end";
			
			DefaultProjectContent projectContent = new DefaultProjectContent();
			RubyParser parser = new RubyParser();
			compilationUnit = parser.Parse(projectContent, @"C:\test.rb", Ruby);			
			
			if (compilationUnit.Classes.Count > 0) {
				globalClass = compilationUnit.Classes[0];
				if (globalClass.Methods.Count > 1) {
					fooMethod = globalClass.Methods[0];
					barMethod = globalClass.Methods[1];
				}
			}
	
			// Get folds.
			ParserFoldingStrategy foldingStrategy = new ParserFoldingStrategy();
			ParseInformation parseInfo = new ParseInformation();
			parseInfo.SetCompilationUnit(compilationUnit);
		
			DocumentFactory docFactory = new DocumentFactory();
			IDocument doc = docFactory.CreateDocument();
			doc.TextContent = Ruby;
			List<FoldMarker> markers = foldingStrategy.GenerateFoldMarkers(doc, @"C:\Temp\test.rb", parseInfo);
		
			if (markers.Count > 1) {
				fooMethodMarker = markers[0];
				barMethodMarker = markers[1];
			}
		}
		
		[Test]
		public void OneClass()
		{
			Assert.AreEqual(1, compilationUnit.Classes.Count);
		}
		
		[Test]
		public void GlobalClassName()
		{
			Assert.AreEqual("test", globalClass.Name);
		}
		
		[Test]
		public void GlobalClassHasTwoMethods()
		{
			Assert.AreEqual(2, globalClass.Methods.Count);
		}
		
		[Test]
		public void FooMethodName()
		{
			Assert.AreEqual("foo", fooMethod.Name);
		}

		[Test]
		public void BarMethodName()
		{
			Assert.AreEqual("bar", barMethod.Name);
		}
		
		[Test]
		public void FooMethodDefaultReturnType()
		{
			Assert.AreEqual(globalClass, fooMethod.ReturnType.GetUnderlyingClass());
		}
		
		[Test]
		public void BarMethodDefaultReturnType()
		{
			Assert.AreEqual(globalClass, barMethod.ReturnType.GetUnderlyingClass());
		}		
		
		[Test]
		public void FooMethodDeclaringType()
		{
			Assert.AreEqual(globalClass, fooMethod.DeclaringType);
		}
		
		[Test]
		public void FooMethodBodyRegion()
		{
			int startLine = 1;
			int startColumn = 10;
			int endLine = 2;
			int endColumn = 4;
			DomRegion region = new DomRegion(startLine, startColumn, endLine, endColumn);
			Assert.AreEqual(region.ToString(), fooMethod.BodyRegion.ToString());
		}
		
		[Test]
		public void FooMethodRegion()
		{
			int startLine = 1;
			int startColumn = 1;
			int endLine = 1;
			int endColumn = 10;
			DomRegion region = new DomRegion(startLine, startColumn, endLine, endColumn);
			Assert.AreEqual(region.ToString(), fooMethod.Region.ToString());
		}

		[Test]
		public void BarMethodBodyRegion()
		{
			int startLine = 4;
			int startColumn = 11;
			int endLine = 5;
			int endColumn = 4;
			DomRegion region = new DomRegion(startLine, startColumn, endLine, endColumn);
			Assert.AreEqual(region.ToString(), barMethod.BodyRegion.ToString());
		}

		[Test]
		public void BarMethodRegion()
		{
			int startLine = 4;
			int startColumn = 1;
			int endLine = 4;
			int endColumn = 11;
			DomRegion region = new DomRegion(startLine, startColumn, endLine, endColumn);
			Assert.AreEqual(region.ToString(), barMethod.Region.ToString());
		}
		
		[Test]
		public void BarMethodHasOneParameter()
		{
			Assert.AreEqual(1, barMethod.Parameters.Count);
		}
				
		[Test]
		public void FooMethodFoldMarkerInnerText()
		{
			Assert.AreEqual("\r\nend", fooMethodMarker.InnerText);
		}
		
		[Test]
		public void BarMethodFoldMarkerInnerText()
		{
			Assert.AreEqual("\r\nend", barMethodMarker.InnerText);
		}
		
		[Test]
		public void FooMethodCollapsedFoldText()
		{
			Assert.AreEqual("...", fooMethodMarker.FoldText);
		}
		
		[Test]
		public void BarMethodCollapsedFoldText()
		{
			Assert.AreEqual("...", barMethodMarker.FoldText);
		}
	}
}