using System;
using SQLite;

namespace Storage
{
	[Table("expenses")]
	public class Expense
	{
		[Column("drive_id")]
		public string DriveId { get; set; }
		[Column("date")]
		public DateTime Date { get; set; }
		[Column("title")]
		public string Title { get; set; }
		[Column("amount")]
		public decimal Amount { get; set; }
	}

	public static class Database
	{
		public static SQLiteConnection GetConnection(string absolutePath)
		{
			var conn = new SQLiteConnection(absolutePath);
			conn.CreateTable<Expense>();
			return conn;
		}
	}
}
