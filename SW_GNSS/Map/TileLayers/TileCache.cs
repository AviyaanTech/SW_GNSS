using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwCad.TileLayers
{
	public class TileCache
	{
		public readonly string Path;
		SQLiteConnection db;
		public TileCache(string path)
		{
			Path = path;
			db = new SQLiteConnection($"Data Source = {path};Version = 3;");
			var tileSql = "CREATE TABLE  IF NOT EXISTS tiles(X INTEGER, Y INTEGER, Z INTEGER, data BLOB);";
			db.Open();
			var cmd = new SQLiteCommand(tileSql, db);
			cmd.ExecuteNonQuery();

			cmd = new SQLiteCommand("CREATE INDEX IF NOT EXISTS tile_index ON tiles(Z,X,Y)", db);
			cmd.ExecuteNonQuery();
		}


		public byte[] GetTile(int x, int y, int z)
		{
			using (var cmd = new SQLiteCommand($"Select * from tiles where X={x} and Y={y} and Z={z}; ", db))
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					byte[] data = null;
					if (reader["data"] != null && !Convert.IsDBNull(reader["data"]))
					{
						data = (byte[])reader["data"];
					}
					return data;
				}
			}
			return null;
		}

		public async Task<Tile> GetTileAsync(int x, int y, int z)
		{
			using (var cmd = new SQLiteCommand($"Select * from tiles where X={x} and Y={y} and Z={z}; ", db))
			using (var reader = await cmd.ExecuteReaderAsync())
			{
				if (reader.Read())
				{
					byte[] data = null;
					if (reader["data"] != null && !Convert.IsDBNull(reader["data"]))
					{
						data = (byte[])reader["data"];
					}
					return new Tile(x, y, z, data);
				}
			}
			return null;
		}


		public async Task<List<Tile>> GetTilesAsync(int minX, int maxX, int minY, int maxY, int Z)
		{
			var ret = new List<Tile>();

			using (var cmd = new SQLiteCommand($"Select * from tiles where X>={minX} and X<={maxX} and Y>={minY} and Y<={maxY} and Z={Z}; ", db))
			using (var reader = await cmd.ExecuteReaderAsync())
			{
				while (reader.Read())
				{
					var x = Convert.ToInt32(reader[0]);
					var y = Convert.ToInt32(reader[1]);
					var z = Convert.ToInt32(reader[2]);
					byte[] data = null;
					if (reader[3] != null && !Convert.IsDBNull(reader[3]))
					{
						data = (byte[])reader["data"];
					}
					ret.Add(new Tile(x, y, z, data));
				}
			}
			return ret;
		}


		public async Task SaveTileAsync(Tile t)
		{
			var sql = $"INSERT INTO tiles(x,y,z,data) VALUES ({t.X},{t.Y},{t.Z},@data);";
			var parameter = new SQLiteParameter("@data", System.Data.DbType.Binary);
			parameter.Value = t.data;
			var cmd = new SQLiteCommand(sql, db);
			cmd.Parameters.Add(parameter);
			await cmd.ExecuteNonQueryAsync();
		}

		public async Task SaveTilesAsync(List<Tile> ti)
		{
			using (var transaction = db.BeginTransaction())
			{
				foreach (var t in ti)
				{
					if (t == null) continue;
					var sql = $"INSERT INTO tiles(x,y,z,data) VALUES ({t.X},{t.Y},{t.Z},@data);";
					var parameter = new SQLiteParameter("@data", System.Data.DbType.Binary);
					parameter.Value = t.data;
					var cmd = new SQLiteCommand(sql, db);
					cmd.Parameters.Add(parameter);
					await cmd.ExecuteNonQueryAsync();
				}
				transaction.Commit();
			}
		}

	}
}
