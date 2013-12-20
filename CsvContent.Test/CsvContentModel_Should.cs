using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SitefinityWebApp.Mvc.Models;
using System.Reflection;
using System.IO;

namespace CsvContent.Test
{
	[TestClass]
	public class CsvContentModel_Should
	{
		[TestMethod]
		public void ErrorMessage_Should_Be_Displayed_If_CsvFileUrl_NotSet()
		{
			var csvContentModel = new CsvContentModel(null);
			Assert.AreEqual("You must specify the CsvFileUrl property.", csvContentModel.ErrorMessage);
		}

		[TestMethod]
		public void All_Rows_Should_Have_Maximum_Amount_Of_Columns_And_Equal_Amount_Of_Columns()
		{
			var csvContentModel = new CsvContentModel("fakefile", "x", this.LoadFile("CsvContent.Test.column_count_example.txt"));
			foreach (var row in csvContentModel.Table.Rows)
			{
				Assert.AreEqual(3, row.Columns.Count);
			}
		}

		[TestMethod]
		public void Set_The_IsMarked_When_YesMarker_Found()
		{
			var csvContentModel = new CsvContentModel("fakefile", "x", this.LoadFile("CsvContent.Test.yes_marker.txt"));
			Assert.IsFalse(csvContentModel.Table.Rows[0].Columns[0].IsMarked);
			Assert.IsFalse(csvContentModel.Table.Rows[0].Columns[1].IsMarked);
			Assert.IsFalse(csvContentModel.Table.Rows[0].Columns[2].IsMarked);
			Assert.IsFalse(csvContentModel.Table.Rows[0].Columns[3].IsMarked);

			Assert.IsFalse(csvContentModel.Table.Rows[1].Columns[0].IsMarked);
			Assert.IsTrue(csvContentModel.Table.Rows[1].Columns[1].IsMarked);
			Assert.IsTrue(csvContentModel.Table.Rows[1].Columns[2].IsMarked);
			Assert.IsFalse(csvContentModel.Table.Rows[1].Columns[3].IsMarked);

			Assert.IsFalse(csvContentModel.Table.Rows[2].Columns[0].IsMarked);
			Assert.IsFalse(csvContentModel.Table.Rows[2].Columns[1].IsMarked);
			Assert.IsTrue(csvContentModel.Table.Rows[2].Columns[2].IsMarked);
			Assert.IsTrue(csvContentModel.Table.Rows[2].Columns[3].IsMarked);

			Assert.IsFalse(csvContentModel.Table.Rows[3].Columns[0].IsMarked);
			Assert.IsFalse(csvContentModel.Table.Rows[3].Columns[1].IsMarked);
			Assert.IsFalse(csvContentModel.Table.Rows[3].Columns[2].IsMarked);
			Assert.IsFalse(csvContentModel.Table.Rows[3].Columns[3].IsMarked);
		}

		[TestMethod]
		public void Set_The_IsHeader_When_Only_First_Value_Is_Present()
		{
			var csvContentModel = new CsvContentModel("fakefile", "x", this.LoadFile("CsvContent.Test.header.txt"));
			Assert.IsTrue(csvContentModel.Table.Rows[0].IsHeader);
			Assert.IsFalse(csvContentModel.Table.Rows[1].IsHeader);
			Assert.IsFalse(csvContentModel.Table.Rows[2].IsHeader);
			Assert.IsTrue(csvContentModel.Table.Rows[3].IsHeader);
			Assert.IsFalse(csvContentModel.Table.Rows[4].IsHeader);
		}

		[TestMethod]
		public void Dont_Render_Comments()
		{
			var csvContentModel = new CsvContentModel("fakefile", "x", this.LoadFile("CsvContent.Test.with_comments.txt"));
			
			Assert.IsTrue(csvContentModel.Table.Rows[0].IsHeader);
			Assert.AreEqual("Features", csvContentModel.Table.Rows[0].Columns[0].Value);
			Assert.AreEqual(string.Empty, csvContentModel.Table.Rows[0].Columns[1].Value);
			Assert.AreEqual(string.Empty, csvContentModel.Table.Rows[0].Columns[2].Value);

			Assert.IsFalse(csvContentModel.Table.Rows[1].IsHeader);
			Assert.AreEqual("Feature 1", csvContentModel.Table.Rows[1].Columns[0].Value);
			Assert.IsTrue(csvContentModel.Table.Rows[1].Columns[1].IsMarked);
			Assert.IsFalse(csvContentModel.Table.Rows[1].Columns[2].IsMarked);

			Assert.IsFalse(csvContentModel.Table.Rows[2].IsHeader);
			Assert.AreEqual("Feature 2", csvContentModel.Table.Rows[2].Columns[0].Value);
			Assert.IsFalse(csvContentModel.Table.Rows[2].Columns[1].IsMarked);
			Assert.IsTrue(csvContentModel.Table.Rows[2].Columns[2].IsMarked);
		}

		private string LoadFile(string name)
		{
			var assembly = typeof(CsvContentModel_Should).Assembly;
			var resourceName = name;

			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			using (StreamReader reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}
	}
}
