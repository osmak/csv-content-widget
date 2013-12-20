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
					var colVal = col.Trim();
					colVal = this.commentsRegex.Replace(colVal, "");

					row.Columns.Add(new CsvColumn() { 
						Value = colVal,
						IsMarked = colVal.Equals(this.yesMarker, StringComparison.InvariantCultureIgnoreCase)
					});
				}

				for (var i = 1; i < row.Columns.Count; i++)
				{
					if(!string.IsNullOrEmpty(row.Columns[i].Value))
					{
						row.IsHeader = false;
						break;
					}
				}

				table.Rows.Add(row);
			}

			var maxColCount = table.Rows.Max(r => r.Columns.Count);
			foreach (var row in table.Rows)
			{
				for (var i = row.Columns.Count; i < maxColCount; i++)
				{
					row.Columns.Add(new CsvColumn() { IsPlaceholder = true });
				}
			}

			return table;
		}

		public class CsvTable
		{
			public int ColumnCount { get; set; }

			public List<CsvRow> Rows
			{
				get
				{
					return this.rows = this.rows ?? new List<CsvRow>();
				}
			}

			private List<CsvRow> rows;
		}

		public class CsvRow
		{
			public bool IsHeader { get; set; }

			public List<CsvColumn> Columns 
			{
				get
				{
					return this.columns = this.columns ?? new List<CsvColumn>();
				}
			}

			private List<CsvColumn> columns;
		}

		public class CsvColumn
		{
			public bool IsMarked { get; set; }

			public bool IsPlaceholder { get; set; }

			public string Value { get; set; }
		}
		
		public sealed class CsvReader : System.IDisposable
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

		private string csvFileUrl;
		private string fileContents;
		private string yesMarker;
		private CsvTable table;
		private Regex commentsRegex = new Regex(@"/\*((?!\*/).)*\*/", RegexOptions.Singleline | RegexOptions.Compiled);
	}
}