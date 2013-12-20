using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace SitefinityWebApp.Mvc.Models
{
	/// <summary>
	/// Represents the model for the CsvContent MVC widget.
	/// </summary>
	public class CsvContentModel
	{
		#region Construction

		/// <summary>
		/// Creates a new instance of the <see cref="CsvContentModel"/> with
		/// default yes marker being "x".
		/// </summary>
		/// <param name="csvFileUrl">
		/// The url of the CSV file to be visualized.
		/// </param>
		public CsvContentModel(string csvFileUrl)
			: this(csvFileUrl, "x", null)
		{
		}

		/// <summary>
		/// Creates a new instance of the <see cref="CsvContentModel"/> with
		/// specified CSV file url and yes marker.
		/// </summary>
		/// <param name="csvFileUrl">
		/// The url of the CSV file to be visualized.
		/// </param>
		/// <param name="yesMarker">
		/// The character which is to be recognized as the yes marker.
		/// </param>
		public CsvContentModel(string csvFileUrl, string yesMarker)
			: this(csvFileUrl, yesMarker, null)
		{

		}

		/// <summary>
		/// Creates a new instance of the <see cref="CsvContentModel"/> with the
		/// specified CSV file url (which is being ignored though), yes marker
		/// and the contents of the CSV file.
		/// </summary>
		/// <param name="csvFileUrl">
		/// The url of the CSV file to be visualized.
		/// </param>
		/// <param name="yesMarker">
		/// The character which is to be recognized as the yes marker.
		/// </param>
		/// <param name="fileContents">
		/// The contents of the CSV file which will be used instead of
		/// downloading a file from the CSV file url.
		/// </param>
		/// <remarks>
		/// This constructor is used either for testing or when you have loaded
		/// CSV file through some other means and have it readily available.
		/// </remarks>
		public CsvContentModel(string csvFileUrl, string yesMarker, string fileContents)
		{
			if (string.IsNullOrEmpty(csvFileUrl))
				this.ErrorMessage = "You must specify the CsvFileUrl property.";

			this.fileContents = fileContents;
			this.csvFileUrl = csvFileUrl;
			this.yesMarker = yesMarker;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the error message. Use this property
		/// to display error in the view. If this property is not
		/// an empty string, there were errors in the execution of the model
		/// and this property will contain the information about these errors.
		/// </summary>
		public string ErrorMessage { get; set; }

		/// <summary>
		/// Gets the structured object which represents the CSV table.
		/// </summary>
		/// <remarks>
		/// If you have initialized the model with the fileContents argument,
		/// that value will be used when constructing the table; otherwise the
		/// csvFileUrl will be used to download the CSV file.
		/// </remarks>
		public CsvTable Table
		{
			get
			{
				if (this.table == null)
				{
					this.fileContents = this.fileContents ?? this.DownloadFile();
					this.table = this.BuildTable(this.fileContents);
				}
				return this.table;
			}
		}

		#endregion

		#region Non-public methods

		private string DownloadFile()
		{
			if (this.csvFileUrl.StartsWith("~"))
			{
				this.csvFileUrl = string.Format("{0}://{1}{2}",
					HttpContext.Current.Request.Url.Scheme,
					HttpContext.Current.Request.Url.Authority,
					VirtualPathUtility.ToAbsolute(this.csvFileUrl));
			}

			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(this.csvFileUrl);
			HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

			StreamReader sr = new StreamReader(resp.GetResponseStream());
			string results = sr.ReadToEnd();
			sr.Close();

			return results;
		}

		private CsvTable BuildTable(string str)
		{
			if (string.IsNullOrEmpty(this.fileContents))
				return null;

			CsvTable table = new CsvTable();

			CsvReader reader = new CsvReader(this.fileContents);
			foreach (string[] line in reader.RowEnumerator)
			{
				var row = new CsvRow() { IsHeader = true };
				foreach (var col in line)
				{
					row.Columns.Add(new CsvColumn()
					{
						Value = this.RemoveComments(col),
						IsMarked = this.IsColumnMarked(col)
					});
				}

				row.IsHeader = this.IsHeaderRow(row);

				table.Rows.Add(row);
			}

			this.EnsureTableSymmetry(table);

			return table;
		}
  
		private void EnsureTableSymmetry(CsvTable table)
		{
			var maxColCount = table.Rows.Max(r => r.Columns.Count);
			foreach (var row in table.Rows)
			{
				for (var i = row.Columns.Count; i < maxColCount; i++)
				{
					row.Columns.Add(new CsvColumn() { IsPlaceholder = true });
				}
			}
		}
  
		private bool IsHeaderRow(CsvRow row)
		{
			for (var i = 1; i < row.Columns.Count; i++)
			{
				if (!string.IsNullOrEmpty(row.Columns[i].Value))
				{
					return false;
				}
			}
			return true;
		}
  
		private string RemoveComments(string columnValue)
		{
			return this.commentsRegex.Replace(columnValue.Trim(), "");
		}
  
		private bool IsColumnMarked(string columnValue)
		{
			return columnValue.Trim().Equals(this.yesMarker, StringComparison.InvariantCultureIgnoreCase);
		}

		#endregion

		#region CSV view model types

		/// <summary>
		/// Represents a Csv table that was built from the file.
		/// </summary>
		public class CsvTable
		{
			/// <summary>
			/// Gets a list of all <see cref="CsvRow"/> instances
			/// that represent the rows of the table.
			/// </summary>
			public List<CsvRow> Rows
			{
				get
				{
					return this.rows = this.rows ?? new List<CsvRow>();
				}
			}

			private List<CsvRow> rows;
		}

		/// <summary>
		/// Represents a single row of the table.
		/// </summary>
		public class CsvRow
		{
			/// <summary>
			/// Gets or sets the value indicating if the row is a header.
			/// </summary>
			public bool IsHeader { get; set; }

			/// <summary>
			/// Gets a list of all <see cref="CsvColumn"/> instances
			/// that represent the columns of a single row.
			/// </summary>
			public List<CsvColumn> Columns 
			{
				get
				{
					return this.columns = this.columns ?? new List<CsvColumn>();
				}
			}

			private List<CsvColumn> columns;
		}

		/// <summary>
		/// Represents a single column of a row of the table.
		/// </summary>
		public class CsvColumn
		{
			/// <summary>
			/// If the value of the column mathched the "yes marker" pattern
			/// returns true; otherwise false.
			/// </summary>
			public bool IsMarked { get; set; }

			/// <summary>
			/// If the column was added only for symmetry, this will be true;
			/// otherwise false.
			/// </summary>
			public bool IsPlaceholder { get; set; }

			/// <summary>
			/// Gets or sets the value of the column.
			/// </summary>
			public string Value { get; set; }
		}

		#endregion

		#region CSV parser

		private sealed class CsvReader : IDisposable
		{
			public CsvReader(String contents)
			{
				this.reader = new StringReader(contents);
			}

			public IEnumerable RowEnumerator
			{
				get
				{
					this.rowNumber = 0;
					string sLine;
					string sNextLine;

					while (null != (sLine = this.reader.ReadLine()))
					{
						while (rexRunOnLine.IsMatch(sLine) && null != (sNextLine = reader.ReadLine()))
							sLine += "\n" + sNextLine;

						this.rowNumber++;
						string[] values = rexCsvSplitter.Split(sLine);

						for (int i = 0; i < values.Length; i++)
							values[i] = Csv.Unescape(values[i]);

						yield return values;
					}

					this.reader.Close();
				}
			}

			public long RowIndex
			{
				get
				{
					return this.rowNumber;
				}
			}

			public void Dispose()
			{
				if (null != this.reader)
					this.reader.Dispose();
			}

			private long rowNumber = 0;
			private TextReader reader;
			private static Regex rexCsvSplitter = new Regex(@",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))");
			private static Regex rexRunOnLine = new Regex(@"^[^""]*(?:""[^""]*""[^""]*)*""[^""]*$");
		}

		private static class Csv
		{
			public static string Escape(string s)
			{
				if (s.Contains(QUOTE))
					s = s.Replace(QUOTE, ESCAPED_QUOTE);

				if (s.IndexOfAny(CHARACTERS_THAT_MUST_BE_QUOTED) > -1)
					s = QUOTE + s + QUOTE;

				return s;
			}

			public static string Unescape(string s)
			{
				if (s.StartsWith(QUOTE) && s.EndsWith(QUOTE))
				{
					s = s.Substring(1, s.Length - 2);

					if (s.Contains(ESCAPED_QUOTE))
						s = s.Replace(ESCAPED_QUOTE, QUOTE);
				}

				return s;
			}

			private const string QUOTE = "\"";
			private const string ESCAPED_QUOTE = "\"\"";
			private static char[] CHARACTERS_THAT_MUST_BE_QUOTED = { ',', '"', '\n' };
		}

		#endregion

		#region Private fields and constants

		private string csvFileUrl;
		private string fileContents;
		private readonly string yesMarker;
		private CsvTable table;
		private readonly Regex commentsRegex = new Regex(@"/\*((?!\*/).)*\*/", RegexOptions.Singleline | RegexOptions.Compiled);

		#endregion
	}
}